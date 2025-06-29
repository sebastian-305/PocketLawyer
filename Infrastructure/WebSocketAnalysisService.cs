using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Infrastructure;

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
        using var socket = await context.WebSockets.AcceptWebSocketAsync();

        try
        {
            var (bytes, name) = await ReceiveFileAsync(socket);
            IFormFile CreateFile() => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name);
            FileContentDto content = await _fileValidatorService.ValidateAndDecodeAsync(CreateFile());
            string? kvName = await DetectKvAsync(content);
            ContractValidationDto validation = await ValidateContractAsync(content, kvName);
            await SendStringAsync(socket, JsonSerializer.Serialize(validation), CancellationToken.None);
            if (!validation.IsReadable || !validation.IsContract)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                return;
            }
            ContractAnalysisDto analysis = await AnalyzeContractAsync(content, kvName);
            await SendStringAsync(socket, JsonSerializer.Serialize(analysis), CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        }
        catch (FileValidationException ex)
        {
            _logger.LogWarning(ex, "File validation failed");
            await SendStringAsync(socket, JsonSerializer.Serialize(new { error = ex.Message }), CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, ex.Message, CancellationToken.None);
        }
        catch (AnalysisException ex)
        {
            _logger.LogError(ex, "Analysis failed at {Step}", ex.AnalysisStep);
            await SendStringAsync(socket, JsonSerializer.Serialize(new { error = ex.Message }), CancellationToken.None);
            await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
        }
    }

    private async Task<(byte[] fileBytes, string fileName)> ReceiveFileAsync(WebSocket socket)
    {
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
            _logger.LogError(ex, "KV detection failed during validation");
        }
        return null;
    }

    private Task<ContractValidationDto> ValidateContractAsync(FileContentDto content, string? kvName)
    {
        return _analyzeContractUseCase.ProcessContractRequest<ContractValidationDto>(
            content,
            PromptBuilder.BuildValidationPrompt,
            GeminiService.PreAnalyzerSchemaDefinition,
            true,
            false,
            kvName);
    }

    private Task<ContractAnalysisDto> AnalyzeContractAsync(FileContentDto content, string? kvName)
    {
        return _analyzeContractUseCase.ProcessContractRequest<ContractAnalysisDto>(
            content,
            PromptBuilder.BuildAnalysisPrompt,
            GeminiService.AnalyzerSchemaDefinition,
            false,
            string.IsNullOrWhiteSpace(kvName),
            kvName);
    }

    private static Task SendStringAsync(WebSocket socket, string data, CancellationToken token)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
    }
}
