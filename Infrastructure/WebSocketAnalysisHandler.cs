using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Agents;
using PocketLawyer.Domain.Dtos.Shared;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PocketLawyer.Infrastructure;

/// <summary>
/// Handles WebSocket connections for contract analysis.
/// Orchestrates the complete analysis workflow from file reception to result delivery.
/// </summary>
public class WebSocketAnalysisHandler
{
    private readonly ILogger<WebSocketAnalysisHandler> _logger;
    private readonly IFileValidatorService _fileValidatorService;

    public WebSocketAnalysisHandler(
        ILogger<WebSocketAnalysisHandler> logger,
        IFileValidatorService fileValidatorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileValidatorService = fileValidatorService ?? throw new ArgumentNullException(nameof(fileValidatorService));
    }

    /// <summary>
    /// Handles the complete WebSocket analysis workflow.
    /// </summary>
    /// <param name="context">The HTTP context containing the WebSocket connection</param>
    /// <param name="agent">The contract analysis agent to use for this analysis</param>
    public async Task HandleAsync(HttpContext context, IContractAnalysisAgent agent)
    {
        var agentId = agent.AgentId;
        _logger.LogDebug("[Agent {AgentId}] Entering {Method}", agentId, nameof(HandleAsync));
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        
        // Create a cancellation token that triggers when the socket is closed
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        try
        {
            // Receive file (contract type is determined by the endpoint/agent)
            var (fileBytes, fileName) = await ReceiveFileAsync(socket, cancellationToken);
            _logger.LogInformation("[Agent {AgentId}] Received file: {FileName}, Size: {Size} bytes", 
                agentId, fileName, fileBytes.Length);

            // Check if client disconnected
            if (socket.State != WebSocketState.Open)
            {
                _logger.LogWarning("[Agent {AgentId}] Client disconnected before analysis started", agentId);
                return;
            }

            // Validate and decode
            IFormFile CreateFile() => new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "file", fileName);
            var fileContent = await _fileValidatorService.ValidateAndDecodeAsync(CreateFile());
            _logger.LogInformation("[Agent {AgentId}] File validated and decoded successfully", agentId);

            // Check if client disconnected
            if (socket.State != WebSocketState.Open)
            {
                _logger.LogWarning("[Agent {AgentId}] Client disconnected after file validation", agentId);
                return;
            }

            // Initialize agent with contract text
            var initResult = await agent.InitializeAsync(fileContent.FileContent);

            if (!initResult.Success)
            {
                _logger.LogWarning("[Agent {AgentId}] Initialization failed: {Error}", 
                    agentId, initResult.ErrorMessage);
                
                // Only send error if socket is still open
                if (socket.State == WebSocketState.Open)
                {
                    await SendErrorAsync(socket, initResult.ErrorMessage ?? "Initialization failed");
                    await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, initResult.ErrorMessage, CancellationToken.None);
                }
                return;
            }

            // Check if client disconnected
            if (socket.State != WebSocketState.Open)
            {
                _logger.LogWarning("[Agent {AgentId}] Client disconnected after initialization", agentId);
                return;
            }

            // Send confirmation
            _logger.LogInformation("[Agent {AgentId}] Sending confirmation message", agentId);
            await SendConfirmationAsync(socket, "Vertrag akzeptiert, Analyse gestartet");

            // Check if client disconnected
            if (socket.State != WebSocketState.Open)
            {
                _logger.LogWarning("[Agent {AgentId}] Client disconnected after confirmation", agentId);
                return;
            }

            // Execute analyses
            _logger.LogInformation("[Agent {AgentId}] Starting basic analysis", agentId);
            var basicResult = await agent.AnalyzeBasicAsync();
            
            // Check if client disconnected before sending result
            if (socket.State != WebSocketState.Open)
            {
                _logger.LogWarning("[Agent {AgentId}] Client disconnected during basic analysis", agentId);
                return;
            }
            
            await SendResultAsync(socket, basicResult);
            _logger.LogInformation("[Agent {AgentId}] Basic analysis result sent", agentId);

            // Check if client disconnected
            if (socket.State != WebSocketState.Open)
            {
                _logger.LogWarning("[Agent {AgentId}] Client disconnected after basic analysis", agentId);
                return;
            }

            _logger.LogInformation("[Agent {AgentId}] Starting premium analysis", agentId);
            var premiumResult = await agent.AnalyzePremiumAsync();
            
            // Check if client disconnected before sending result
            if (socket.State != WebSocketState.Open)
            {
                _logger.LogWarning("[Agent {AgentId}] Client disconnected during premium analysis", agentId);
                return;
            }
            
            await SendResultAsync(socket, premiumResult);
            _logger.LogInformation("[Agent {AgentId}] Premium analysis result sent", agentId);

            // Close connection
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Analysis complete", CancellationToken.None);
                _logger.LogInformation("[Agent {AgentId}] Connection closed gracefully", agentId);
            }
        }
        catch (FileValidationException ex)
        {
            _logger.LogWarning(ex, "[Agent {AgentId}] File validation failed", agentId);
            if (socket.State == WebSocketState.Open)
            {
                await SendErrorAsync(socket, ex.Message);
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, ex.Message, CancellationToken.None);
            }
        }
        catch (ContractAnalysisException ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] Contract analysis failed", agentId);
            if (socket.State == WebSocketState.Open)
            {
                await SendErrorAsync(socket, ex.Message);
                await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning(ex, "[Agent {AgentId}] WebSocket exception (client likely disconnected)", agentId);
            // Don't try to send error or close socket - it's already closed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] Unknown exception during analysis", agentId);
            if (socket.State == WebSocketState.Open)
            {
                await SendErrorAsync(socket, ex.Message);
                await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// Receives file data from WebSocket.
    /// </summary>
    private async Task<(byte[] fileBytes, string fileName)> ReceiveFileAsync(WebSocket socket, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Entering {Method}", nameof(ReceiveFileAsync));

        var buffer = new byte[8192];
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        
        do
        {
            result = await socket.ReceiveAsync(buffer, cancellationToken);
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            throw new FileValidationException("Socket closed before upload finished");
        }

        WebSocketFileDto? upload;
        try
        {
            upload = JsonSerializer.Deserialize<WebSocketFileDto>(Encoding.UTF8.GetString(ms.ToArray()));
        }
        catch (Exception ex)
        {
            throw new FileValidationException("Invalid upload payload", null, ex);
        }

        if (upload == null)
            throw new FileValidationException("Invalid upload payload");

        return (Convert.FromBase64String(upload.Data), upload.FileName);
    }



    /// <summary>
    /// Sends a confirmation message over WebSocket.
    /// </summary>
    private async Task SendConfirmationAsync(WebSocket socket, string message)
    {
        var confirmation = new { type = "confirmation", message };
        await SendJsonAsync(socket, confirmation);
    }

    /// <summary>
    /// Sends an analysis result over WebSocket.
    /// </summary>
    private async Task SendResultAsync<T>(WebSocket socket, T result) where T : notnull
    {
        // Extract the actual result from the AnalysisDto wrapper
        // The result parameter is AnalysisDto<IAnalysisDto>, we need to extract the Result property
        var resultProperty = result.GetType().GetProperty("Result");
        var metadataProperty = result.GetType().GetProperty("Metadata");
        
        if (resultProperty != null && metadataProperty != null)
        {
            var actualResult = resultProperty.GetValue(result);
            var metadata = metadataProperty.GetValue(result);
            
            var message = new { 
                type = "result", 
                data = new {
                    result = actualResult,
                    metadata = metadata
                }
            };
            await SendJsonAsync(socket, message);
        }
        else
        {
            // Fallback: send as-is
            var message = new { type = "result", data = result };
            await SendJsonAsync(socket, message);
        }
    }

    /// <summary>
    /// Sends an error message over WebSocket.
    /// </summary>
    private async Task SendErrorAsync(WebSocket socket, string error)
    {
        var message = new { type = "error", error = error };
        await SendJsonAsync(socket, message);
    }

    /// <summary>
    /// Sends a JSON message over WebSocket.
    /// </summary>
    private async Task SendJsonAsync<T>(WebSocket socket, T data)
    {
        // Configure serialization to include all properties from concrete types, not just interface properties
        var options = new JsonSerializerOptions
        {
            // This ensures that properties from the actual runtime type are serialized,
            // not just the properties defined in the interface
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            // Use the actual object type for serialization, not the declared type
            WriteIndented = false
        };
        
        var json = JsonSerializer.Serialize(data, data?.GetType() ?? typeof(T), options);
        var bytes = Encoding.UTF8.GetBytes(json);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
