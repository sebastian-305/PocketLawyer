using System.Net.WebSockets;

// Kümmert sich um den Ablauf der Analyse über eine WebSocket-Verbindung.
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Infrastructure;

// Liest hochgeladene Dateien über WebSockets ein und führt
// Voranalyse sowie ausführliche Analyse durch.
public class WebSocketAnalysisService : IWebSocketAnalysisService
{
    private readonly ILogger<WebSocketAnalysisService> _logger;
    private readonly IFileValidatorService _fileValidatorService;
    private readonly IKvDetectionService _kvDetectionService;
    private readonly IContractAnalyzerService _analyzeContractUseCase;

    public WebSocketAnalysisService(
        ILogger<WebSocketAnalysisService> logger,
        IFileValidatorService fileValidatorService,
        IKvDetectionService kvDetectionService,
        IContractAnalyzerService analyzeContractUseCase)
    {
        _logger = logger;
        _fileValidatorService = fileValidatorService;
        _kvDetectionService = kvDetectionService;
        _analyzeContractUseCase = analyzeContractUseCase;
    }

    public async Task HandleAsync(HttpContext context)
    {
        // WebSocket akzeptieren.
        using var socket = await context.WebSockets.AcceptWebSocketAsync();

        try
        {
            // Datei empfangen und in ein IFormFile überführen.
            var (bytes, name) = await ReceiveFileAsync(socket);
            IFormFile CreateFile() => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name);
            // Validate/Decode via FileValidatorService.
            FileContentDto content = await _fileValidatorService.ValidateAndDecodeAsync(CreateFile());
            // Kollektivvertrag erkennen.
            string? kvName = await DetectKvAsync(content);
            // Voranalyse senden und Ergebnis verschicken.
            ContractValidationDto validation = await ValidateContractAsync(content);

            await WebSocketHelpers.SendStringAsync(socket, JsonSerializer.Serialize(validation), CancellationToken.None);
            if (!validation.IsReadable || !validation.IsContract)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                return;
            }

            // Bei positivem Ergebnis Hauptanalyse ausführen.
            string? kvName = await DetectKvAsync(content);

            ContractAnalysisDto analysis = await AnalyzeContractAsync(content, kvName);
            await WebSocketHelpers.SendStringAsync(socket, JsonSerializer.Serialize(analysis), CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        }
        // Fehlerbehandlung und Socket schließen.
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
    }

    private async Task<(byte[] fileBytes, string fileName)> ReceiveFileAsync(WebSocket socket)
    {
        // Empfang der gesamten Datei vom WebSocket.
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

    private async Task<string?> DetectKvAsync(FileContentDto content)
    {
        // Versucht den Kollektivvertrag aus dem Text zu ermitteln.
        try
        {
            KvDetectionResult result = await _kvDetectionService.DetectAsync(content.FileContent);
            if (result.AllEqual && result.KvName != "Keiner" && result.KvName != "Multiple")
            {
                return result.KvName;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KV detection failed");
        }
        return null;
    }

    private Task<ContractValidationDto> ValidateContractAsync(FileContentDto content)
    {
        // Löst die KI-Voranalyse des Vertrags aus.
        return _analyzeContractUseCase.ProcessContractRequest<ContractValidationDto>(
            content,
            PromptBuilder.BuildValidationPrompt,
            GeminiService.PreAnalyzerSchemaDefinition,
            true,
            false);
    }

    private Task<ContractAnalysisDto> AnalyzeContractAsync(FileContentDto content, string? kvName)
    {
        // Führt die eigentliche Hauptanalyse durch.
        return _analyzeContractUseCase.ProcessContractRequest<ContractAnalysisDto>(
            content,
            PromptBuilder.BuildAnalysisPrompt,
            GeminiService.AnalyzerSchemaDefinition,
            false,
            false,
            kvName);
    }

}
