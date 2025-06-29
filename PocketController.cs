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
    private readonly IAiService _geminiService;
    private readonly IAiService _claudeService;
    private readonly IFileDecoderService _fileDecoderService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    public PocketController(ILogger<PocketController> logger,
        [FromKeyedServices("gemini")] IAiService geminiService,
        IFileDecoderService fileDecoderService,
        [FromKeyedServices("claude")] IAiService claudeService,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService)
    {
        _logger = logger;
        _geminiService = geminiService;
        _fileDecoderService = fileDecoderService;
        _claudeService = claudeService;
        _configuration = configuration;
        _environment = environment;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
    }

    [HttpGet("/test")]
    public async Task<ActionResult<KvDetectionTestResultDto>> Test()
    {
        _logger.LogDebug("Entering {Method}", nameof(Test));

        string contractContent;
        try
        {
            contractContent = await _kvService.GetSampleContractAsync();
            _logger.LogDebug("Loaded sample contract");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to load samples");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldatei oder der Kollektivverträge.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "No permission to access samples");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldatei oder der Kollektivverträge.");
        }

        KvDetectionResult detection;
        try
        {
            detection = await _kvDetectionService.DetectAsync(contractContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI service failed while processing the prompt");
            return StatusCode(StatusCodes.Status502BadGateway, "Fehler bei der Anfrage an den KI-Dienst.");
        }

        var resultDto = new KvDetectionTestResultDto
        {
            AllEqual = detection.AllEqual
        };

        return Ok(resultDto);
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

        var buffer = new byte[8192];
        var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            return new EmptyResult();
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
            return new EmptyResult();
        }
        if (upload == null)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "invalid", CancellationToken.None);
            return new EmptyResult();
        }

        byte[] fileBytes = Convert.FromBase64String(upload.Data);

        IFormFile CreateFormFile() => new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "file", upload.FileName);

        string? kvName = null;
        try
        {
            var requestContent = await PreValidate(CreateFormFile());
            KvDetectionResult kvResult = await _kvDetectionService.DetectAsync(requestContent.FileContent);
            if (kvResult.AllEqual && kvResult.KvName != "Keiner" && kvResult.KvName != "Multiple")
            {
                kvName = kvResult.KvName;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KV detection failed during validation");
        }

        ContractValidationDto validationDto;
        try
        {
            validationDto = await ProcessContractRequest<ContractValidationDto>(
                CreateFormFile(),
                BuildValidationPrompt,
                GeminiService.PreAnalyzerSchemaDefinition,
                true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pre-validation failed");
            await SendStringAsync(webSocket, JsonSerializer.Serialize(new { error = ex.Message }), CancellationToken.None);
            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            return new EmptyResult();
        }

        validationDto.Kollektivvertrag = kvName;
        if (!string.IsNullOrWhiteSpace(kvName))
        {
            validationDto.Result = string.Concat(
                validationDto.Result.TrimEnd(),
                " Es wurde der Kollektivvertrag \"",
                kvName,
                "\" erkannt.");
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
            analysisDto = await ProcessContractRequest<ContractAnalysisDto>(
                CreateFormFile(),
                _geminiService.PromptBuilder,
                GeminiService.AnalyzerSchemaDefinition,
                false,
                string.IsNullOrWhiteSpace(kvName),
                kvName);
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

    private static Task SendStringAsync(WebSocket socket, string data, CancellationToken token)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
    }


    private static string BuildValidationPrompt(string filename, string fileContent)
    {
        var sb = new StringBuilder();
        sb.Append("Ich sende dir eine Datei. Analysiere, ob der Inhalt der Datei lesbar ist. Schreibe das Ergebnis in die boolean Variable \"isReadable\".");
        sb.Append("Wenn der Text lesbar ist, analysiere ob es sich dabei um einen Arbeitsvertrag handelt. Unabhängig von der Sprache der Datei soll seine Antwort auf deutsch sein!");
        sb.Append("Schreibe das Ergebnis in die bool Variable \"isContract\". ");
        sb.Append("Gib eine Begründung deiner Ergebnisse und falls es sich um einen Arbeitsvertrag handelt, eine überblicksmäßige Zusammenfassung vom Vertrag in die Variable \"result\". ");
        sb.Append("Verwende keine Markdown-Formatierungen! Formatiere die Antwort in HTML! Hier die Datei:\n\n Dateiname: \n");
        sb.Append(filename);
        sb.Append("\nDateiinhalt: ");
        sb.Append(fileContent);
        return sb.ToString();
    }

    private async Task<T> ProcessContractRequest<T>(IFormFile file,
        Func<string, string, string> promptBuilder,
        object schema,
        bool useAltKey,
        bool detectKv = false,
        string? kvName = null)
    {
        FileContentDto requestContent = await PreValidate(file);

        string prompt = promptBuilder(requestContent.FileName, requestContent.FileContent);

        if (!string.IsNullOrWhiteSpace(kvName) || detectKv)
        {
            try
            {

                if (detectKv && string.IsNullOrWhiteSpace(kvName))
                {
                    KvDetectionResult kvResult = await _kvDetectionService.DetectAsync(requestContent.FileContent);
                    _logger.LogInformation("KV detection result: {Result}", kvResult.KvName);

                    if (kvResult.AllEqual && kvResult.KvName != "Keiner" && kvResult.KvName != "Multiple")
                    {
                        kvName = kvResult.KvName;
                    }
                }

                if (!string.IsNullOrWhiteSpace(kvName))
                {
                    string[] kvFiles = await _kvService.GetKvFileContentsAsync(kvName);
                    var sbPrompt = new StringBuilder(prompt);
                    sbPrompt.AppendLine();

                    sbPrompt.AppendLine($"Der folgende Text enthält den dazugehörigen Kollektivvertrag \"{kvName}\" und mögliche Anhänge. " +
                        $"Diese Informationen sind laut Gesetz zwingend Grundlage der Vertragsanalyse. Wenn der Vertrag in einem Punkt so etwas schreibt wie \"laut Kollektivvertrag\" soll die entsprechende Regelung" +
                        $"aus dem Kollektivvetrag angegeben werden mit dem Zusatz \"(lt. Kollektivvertrag)\"");
                    sbPrompt.AppendLine(string.Join("\n\n", kvFiles));
                    prompt = sbPrompt.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kollektivvertrag detection failed");
            }
        }
        string answer = await _geminiService.SendPrompt(prompt, schema, useAltKey);

        T? dto;
        try
        {
            dto = JsonSerializer.Deserialize<T>(answer);
        }
        catch (JsonException ex)
        {
            string truncated = answer.Length > 200 ? answer[..200] + "..." : answer;
            _logger.LogError(ex, "Failed to parse AI response: {Snippet}", truncated);
            throw new InvalidOperationException($"Fehler beim Interpretieren der KI-Antwort: {truncated}", ex);
        }
        if (dto == null)
            throw new InvalidOperationException("Invalid AI response");

        if (!string.IsNullOrWhiteSpace(kvName))
        {
            switch (dto)
            {
                case ContractValidationDto v:
                    v.Kollektivvertrag = kvName;
                    break;
                case ContractAnalysisDto a:
                    a.Kollektivvertrag = kvName;
                    break;
            }
        }

        return dto;
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
