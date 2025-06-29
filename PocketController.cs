using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Service;
using System.Text.Json;
using System.Text;
using System.Net.WebSockets;

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
            _configuration["ResponsesPath"] ?? "Responses");

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
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest();

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var upload = await ReceiveFileAsync(webSocket);
        if (upload == null)
            return new EmptyResult();

        var (fileBytes, fileName) = upload.Value;
        IFormFile CreateFormFile() => new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "file", fileName);

        FileContentDto requestContent = await _fileValidatorService.ValidateAndDecodeAsync(CreateFormFile());

        string? kvName = await DetectKvAsync(requestContent);

        ContractValidationDto validationDto;
        try
        {
            validationDto = await ValidateContractAsync(requestContent, kvName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pre-validation failed");
            await SendStringAsync(webSocket, JsonSerializer.Serialize(new { error = ex.Message }), CancellationToken.None);
            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            return new EmptyResult();
        }

        await SendStringAsync(webSocket, JsonSerializer.Serialize(validationDto), CancellationToken.None);

        if (!validationDto.IsReadable || !validationDto.IsContract)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            return new EmptyResult();
        }

        ContractAnalysisDto analysisDto;
        try
        {
            analysisDto = await AnalyzeContractAsync(requestContent, kvName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed");
            await SendStringAsync(webSocket, JsonSerializer.Serialize(new { error = ex.Message }), CancellationToken.None);
            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            return new EmptyResult();
        }

        await SendStringAsync(webSocket, JsonSerializer.Serialize(analysisDto), CancellationToken.None);
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        return new EmptyResult();
    }

    private async Task<(byte[] fileBytes, string fileName)?> ReceiveFileAsync(WebSocket webSocket)
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
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            return null;
        }

        WebSocketFileDto? upload;
        try
        {
            upload = JsonSerializer.Deserialize<WebSocketFileDto>(Encoding.UTF8.GetString(ms.ToArray()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse websocket request");
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "invalid", CancellationToken.None);
            return null;
        }

        if (upload == null)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "invalid", CancellationToken.None);
            return null;
        }

        byte[] fileBytes = Convert.FromBase64String(upload.Data);
        return (fileBytes, upload.FileName);
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
