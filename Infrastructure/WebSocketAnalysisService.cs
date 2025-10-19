using Microsoft.AspNetCore.Mvc.ModelBinding;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.AiServices.Nebius;
using PocketLawyer.Web;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PocketLawyer.Infrastructure;

public class WebSocketAnalysisService : IWebSocketAnalysisService
{
    private readonly ILogger<WebSocketAnalysisService> _logger;
    private readonly IFileValidatorService _fileValidatorService;
    private readonly IKvDetectionService _kvDetectionService;
    private readonly IContractAnalyzerService _analyzeEmployment;
    private readonly IContractAnalyzerService _analyzeRent;
    private IContractAnalyzerService _usedUseCase = null;

    public WebSocketAnalysisService(
        ILogger<WebSocketAnalysisService> logger,
        IFileValidatorService fileValidatorService,
        IKvDetectionService kvDetectionService,
        [FromKeyedServices("employment")] IContractAnalyzerService analyzeEmploymentUseCase,
        [FromKeyedServices("rent")] IContractAnalyzerService analyzeRentUseCase
        )
    {
        _logger = logger;
        _fileValidatorService = fileValidatorService;
        _kvDetectionService = kvDetectionService;
        _analyzeEmployment = analyzeEmploymentUseCase;
        _analyzeRent = analyzeRentUseCase;
    }

    public async Task HandleAsync(HttpContext context, AnalysisType type)
    {
        _logger.LogDebug("Entering {Method}", nameof(HandleAsync));
        using var socket = await context.WebSockets.AcceptWebSocketAsync();

        switch (type)
        {
            case AnalysisType.Employment:
                _usedUseCase = _analyzeEmployment;
                break;
            case AnalysisType.Rent:
                _usedUseCase = _analyzeRent;
                break;
            default:
                throw new NotImplementedException(type.ToString());
        }

        try
        {
            var (bytes, name) = await ReceiveFileAsync(socket);
            IFormFile CreateFile() => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name);
            FileContentDto content = await _fileValidatorService.ValidateAndDecodeAsync(CreateFile());
            ContractValidationDto validation = await ValidateContractAsync(content);
            await WebSocketHelpers.SendStringAsync(socket, JsonSerializer.Serialize(validation), CancellationToken.None);
            if (!validation.IsReadable || !validation.IsContract)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                return;
            }


            var analysis = await AnalyzeContractAsync(content, null, type);           
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

    private async Task<ContractValidationDto> ValidateContractAsync(FileContentDto content)
    {
        _logger.LogDebug("Entering {Method}", nameof(ValidateContractAsync));
        return await _usedUseCase.ProcessContractRequest<ContractValidationDto>(
            content,
            PromptBuilder.BuildValidationPrompt,
            NebiusService.PreAnalyzerSchemaDefinition,
            true
            );
    }

    private async Task<IAnalysisDto> AnalyzeContractAsync(FileContentDto content, string? kvName, AnalysisType type)
    {
        _logger.LogDebug("Entering {Method}", nameof(AnalyzeContractAsync));


        return type switch
        {
            AnalysisType.Employment => await _usedUseCase.ProcessContractRequest<EmploymentContractAnalysisDto>(
                content,
                PromptBuilder.BuildEmploymentAnalysisPrompt,
                NebiusService.AnalyzerSchemaDefinition,
                false,
                kvName),

            AnalysisType.Rent => await _usedUseCase.ProcessContractRequest<RentContractAnalysisDto>(
                content,
                PromptBuilder.BuildRentAnalysisPrompt,
                NebiusService.AnalyzerSchemaDefinition,
                false,
                kvName),

            _ => throw new NotImplementedException(type.ToString())
        };

    }

}
