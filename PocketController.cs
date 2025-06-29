using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Service;
using System.Text.Json;
using System.Text;
using System.Net.WebSockets;
using PocketLawyer.Infrastructure;

namespace PocketLawyer;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;
    private readonly IContractAnalyzerService _contractAnalyzerService;
    private readonly IFileValidatorService _fileValidatorService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    public PocketController(ILogger<PocketController> logger,

        IFileValidatorService fileValidatorService,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService,
        IContractAnalyzerService contractAnalyzerService)
    {
        _logger = logger;

        _fileValidatorService = fileValidatorService;
        _configuration = configuration;
        _environment = environment;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
        _contractAnalyzerService = contractAnalyzerService;
    }

    [HttpGet("/mockdata")]
    public async Task<ActionResult<ContractAnalysisDto>> GetMockData()
    {
        _logger.LogDebug("Entering {Method}", nameof(GetMockData));

        string responsesPath = Path.Combine(
            _environment.ContentRootPath,
            _configuration["ResponsesPath"] ?? Path.Combine("data", "Responses"));

        Directory.CreateDirectory(responsesPath);

        string mockContent;
        try
        {
            var files = Directory.GetFiles(responsesPath);
            if (files.Length == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "Keine Beispieldaten gefunden.");

            string filePath = files[Random.Shared.Next(files.Length)];
            mockContent = await System.IO.File.ReadAllTextAsync(filePath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to read mock data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldaten.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to access mock data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldaten.");
        }

        ContractAnalysisDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<ContractAnalysisDto>(mockContent);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse mock data");
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Fehler beim Lesen der Beispieldaten.");
        }
        if (dto == null)
            return BadRequest("Invalid mock data");

        return Ok(dto);
    }


    [HttpGet("/analysis")]
    public async Task<ActionResult> AnalyzeViaWebSocket()
    {
        WebSocket webSocket;
        try
        {
            webSocket = await AcceptWebSocketAsync();
        }
        catch (FileReceiveException)
        {
            return BadRequest();
        }

        using (webSocket)
        {
            try
            {
                var (content, kvName, validation) = await ValidateFileAndPreValidateAsync(webSocket);
                await SendStringAsync(webSocket, JsonSerializer.Serialize(validation), CancellationToken.None);

                if (!validation.IsReadable || !validation.IsContract)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                    return new EmptyResult();
                }

                ContractAnalysisDto analysis = await PerformAnalysisAsync(content, kvName);
                await SendStringAsync(webSocket, JsonSerializer.Serialize(analysis), CancellationToken.None);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            catch (FileReceiveException ex)
            {
                _logger.LogError(ex, "Failed to receive file");
                await SendStringAsync(webSocket, JsonSerializer.Serialize(new { error = ex.Message }), CancellationToken.None);
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, ex.Message, CancellationToken.None);
            }
            catch (PreValidationException ex)
            {
                _logger.LogError(ex, "Pre-validation failed");
                await SendStringAsync(webSocket, JsonSerializer.Serialize(new { error = ex.Message }), CancellationToken.None);
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            }
            catch (ContractAnalysisException ex)
            {
                _logger.LogError(ex, "Analysis failed");
                await SendStringAsync(webSocket, JsonSerializer.Serialize(new { error = ex.Message }), CancellationToken.None);
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            }
        }

        return new EmptyResult();
    }

    private async Task<(byte[] fileBytes, string fileName)> ReceiveFileAsync(WebSocket webSocket)
    {
        var buffer = new byte[8192];
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        if (result.MessageType == WebSocketMessageType.Close)
            throw new FileReceiveException("Connection closed while receiving file");

        WebSocketFileDto? upload;
        try
        {
            upload = JsonSerializer.Deserialize<WebSocketFileDto>(Encoding.UTF8.GetString(ms.ToArray()));
        }
        catch (Exception ex)
        {
            throw new FileReceiveException("Failed to parse websocket request", ex);
        }

        if (upload == null)
            throw new FileReceiveException("Invalid websocket payload");

        byte[] fileBytes = Convert.FromBase64String(upload.Data);
        return (fileBytes, upload.FileName);
    }

    private Task<WebSocket> AcceptWebSocketAsync()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            throw new FileReceiveException("WebSocket request expected");

        return HttpContext.WebSockets.AcceptWebSocketAsync();
    }

    private async Task<(FileContentDto content, string? kvName, ContractValidationDto validation)> ValidateFileAndPreValidateAsync(WebSocket socket)
    {
        var (fileBytes, fileName) = await ReceiveFileAsync(socket);
        IFormFile form = new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "file", fileName);

        FileContentDto requestContent;
        try
        {
            requestContent = await _fileValidatorService.ValidateAndDecodeAsync(form);
        }
        catch (Exception ex)
        {
            throw new PreValidationException("Datei konnte nicht verarbeitet werden", ex);
        }

        string? kvName = await DetectKvAsync(requestContent);

        ContractValidationDto validation;
        try
        {
            validation = await ValidateContractAsync(requestContent, kvName);
        }
        catch (Exception ex)
        {
            throw new PreValidationException("Vorvalidierung fehlgeschlagen", ex);
        }

        return (requestContent, kvName, validation);
    }

    private async Task<ContractAnalysisDto> PerformAnalysisAsync(FileContentDto content, string? kvName)
    {
        try
        {
            return await AnalyzeContractAsync(content, kvName);
        }
        catch (Exception ex)
        {
            throw new ContractAnalysisException("Analyse fehlgeschlagen", ex);
        }
    }

    private async Task<string?> DetectKvAsync(FileContentDto content)
    {
        try
        {
            KvDetectionResult kvResult = await _kvDetectionService.DetectAsync(content.FileContent);
            if (kvResult.AllEqual && kvResult.KvName != "Keiner" && kvResult.KvName != "Multiple")
            {
                return kvResult.KvName;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KV detection failed during validation");
        }
        return null;
    }

    private async Task<ContractValidationDto> ValidateContractAsync(FileContentDto content, string? kvName)
    {
        var dto = await _contractAnalyzerService.ProcessContractRequest<ContractValidationDto>(
            content,
            PromptBuilder.BuildValidationPrompt,
            GeminiService.PreAnalyzerSchemaDefinition,
            true);

        dto.Kollektivvertrag = kvName;
        if (!string.IsNullOrWhiteSpace(kvName))
        {
            dto.Result = string.Concat(
                dto.Result.TrimEnd(),
                " Es wurde der Kollektivvertrag \"",
                kvName,
                "\" erkannt.");
        }

        return dto;
    }

    private Task<ContractAnalysisDto> AnalyzeContractAsync(FileContentDto content, string? kvName)
    {
        return _contractAnalyzerService.ProcessContractRequest<ContractAnalysisDto>(
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
