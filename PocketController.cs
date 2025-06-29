using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure;
using PocketLawyer.Service;
using System.Text.Json;
using System.Text;
using System.Net.WebSockets;

namespace PocketLawyer;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;
    private readonly IAiService _geminiService;
    private readonly IContractAnalyzerService _contractAnalyzerService;
    private readonly IFileDecoderService _fileDecoderService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    public PocketController(ILogger<PocketController> logger,
        [FromKeyedServices("gemini")] IAiService geminiService,
        IFileDecoderService fileDecoderService,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService,
        IContractAnalyzerService contractAnalyzerService)
    {
        _logger = logger;
        _geminiService = geminiService;
        _fileDecoderService = fileDecoderService;
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

        string? kvName = await DetectKvAsync(CreateFormFile);

        ContractValidationDto validationDto;
        try
        {
            validationDto = await ValidateContractAsync(CreateFormFile, kvName);
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
            analysisDto = await AnalyzeContractAsync(CreateFormFile, kvName);
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

    private async Task<string?> DetectKvAsync(Func<IFormFile> createFormFile)
    {
        try
        {
            var requestContent = await PreValidate(createFormFile());
            KvDetectionResult kvResult = await _kvDetectionService.DetectAsync(requestContent.FileContent);
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

    private async Task<ContractValidationDto> ValidateContractAsync(Func<IFormFile> createFormFile, string? kvName)
    {
        var dto = await _contractAnalyzerService.ProcessContractRequest<ContractValidationDto>(
            createFormFile(),
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

    private Task<ContractAnalysisDto> AnalyzeContractAsync(Func<IFormFile> createFormFile, string? kvName)
    {
        return _contractAnalyzerService.ProcessContractRequest<ContractAnalysisDto>(
            createFormFile(),
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



    private async Task<FileContentDto> PreValidate(IFormFile file)
    {
        _logger.LogDebug("Entering {Method}", nameof(PreValidate));

        string filename = file.FileName;
        string fileContent;

        var extension = Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
        if (!_fileDecoderService.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Fileextension not allowed: {FileName}", filename);
            throw new BadHttpRequestException($"Fileextension not recognised! Recognised extensions are {_fileDecoderService.AllowedExtensionsString}.");
        }

        long maxUploadSizeMb = _configuration.GetValue<long>("MaxUploadSizeMB");
        long maxBytes = maxUploadSizeMb * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new BadHttpRequestException($"File too large. Maximum {maxUploadSizeMb} MB allowed.");
        }

        await using var stream = file.OpenReadStream();

        try
        {
            fileContent = await _fileDecoderService.GetFileContentAsync(filename, stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding file {FileName}", filename);
            throw new BadHttpRequestException(ex.Message);
        }

        return new FileContentDto(filename, fileContent);
    }

}
