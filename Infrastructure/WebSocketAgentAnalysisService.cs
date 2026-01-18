using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain.Dtos.Shared;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PocketLawyer.Infrastructure;

public class WebSocketAgentAnalysisService : IWebSocketAnalysisService
{
    private readonly ILogger<WebSocketAgentAnalysisService> _logger;
    private readonly IFileValidatorService _fileValidatorService;


    public WebSocketAgentAnalysisService(
        ILogger<WebSocketAgentAnalysisService> logger,
        IFileValidatorService fileValidatorService
        )
    {
        _logger = logger;
        _fileValidatorService = fileValidatorService;
    }

    public async Task HandleAsync(HttpContext context, IContractAnalyzerAgent agent)
    {
        _logger.LogDebug("Entering {Method}", nameof(HandleAsync));
        using var socket = await context.WebSockets.AcceptWebSocketAsync();

        try
        {
            //Receive File
            var (bytes, name) = await ReceiveFileAsync(socket);
            IFormFile CreateFile() => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name);
            FileContentDto content = await _fileValidatorService.ValidateAndDecodeAsync(CreateFile());

            //Pre-Analysis
            ContractValidationDto validation = await agent.ProcessContractRequest<ContractValidationDto>(
            content,
            AgentAction.Validate
            );
            await WebSocketHelpers.SendStringAsync(socket, JsonSerializer.Serialize(validation), CancellationToken.None);
            if (!validation.IsReadable || !validation.IsContract)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                return;
            }


            //Analysis
            var analysis = await agent.ProcessContractRequest<IAnalysisDto>(content, AgentAction.Analyze, validation);

            await WebSocketHelpers.SendStringAsync(socket, JsonSerializer.Serialize(analysis, analysis.GetType()), CancellationToken.None);

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            _logger.LogInformation("Connection closed gracefully");



        }
        catch (FileValidationException ex)
        {
            _logger.LogWarning(ex, "File validation failed");
            await WebSocketHelpers.SendErrorAsync(socket, ex.Message, CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, ex.Message, CancellationToken.None);
        }
        catch (AnalysisException ex)
        {
            _logger.LogError(ex, "Analysis failed at {Step}", ex.AnalysisStep);
            await WebSocketHelpers.SendErrorAsync(socket, ex.Message, CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unknown Exception {ex}", ex.Message);
            await WebSocketHelpers.SendErrorAsync(socket, ex.Message, CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);

        }
    }

    private async Task<(byte[] fileBytes, string fileName)> ReceiveFileAsync(WebSocket socket)
    {
        _logger.LogDebug("Entering {Method}", nameof(ReceiveFileAsync));

        var buffer = new byte[8192];
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(buffer, CancellationToken.None);
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

}
