using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Service;
using System.Text.Json;
using System.Text;

namespace PocketLawyer;

// Controller, der die vom Frontend genutzte HTTP-API bereitstellt.
// Er empfängt hochgeladene Vertragsdateien, führt Vorprüfungen durch
// und übergibt die eigentliche Analyse an die KI-Dienste.
[ApiController]
public class PocketController : ControllerBase
{
    // Gibt detaillierte Laufzeitinformationen aus
    private readonly ILogger<PocketController> _logger;
    // KI-Dienst basierend auf Googles Gemini API
    private readonly IAiService _geminiService;
    // Alternativer KI-Dienst, der auf Claudes API basiert
    private readonly IAiService _claudeService;
    // Wandelt hochgeladene Dateien in reinen Text um
    private readonly IFileDecoderService _fileDecoderService;
    // Zugriff auf die Anwendungskonfiguration
    private readonly IConfiguration _configuration;
    // Liefert den Pfad zum Content-Root für Dateizugriffe
    private readonly IWebHostEnvironment _environment;
    // Dienst zum Laden der Kollektivverträge von der Festplatte
    private readonly IKollektivvertragService _kvService;
    // Dienst, der versucht, den passenden Kollektivvertrag zu erkennen
    private readonly IKvDetectionService _kvDetectionService;

    /// <summary>
    /// Alle Abhängigkeiten werden vom Framework aus dem Service-Container
    /// aufgelöst und diesem Konstruktor übergeben.
    /// </summary>
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

    // Einfacher Endpunkt, um während der Entwicklung die KV-Erkennung zu testen
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

    // Liefert ein zufälliges Analyseergebnis aus dem Responses-Ordner.
    // Nützlich, wenn keine Verbindung zur KI besteht oder für UI-Tests.
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


    // Erster Schritt nach dem Upload: Der Vertrag wird validiert und bei Bedarf
    // per OCR ausgelesen. Zudem wird versucht, den passenden Kollektivvertrag zu ermitteln.
    [HttpPost("/preanalysis")]
    public async Task<ActionResult<ContractValidationDto>> ValidateBinaryContract([FromForm] FileUploadDto request)
    {
        _logger.LogDebug("Entering {Method}", nameof(ValidateBinaryContract));

        string? kvName = null;
        try
        {
            var requestContent = await PreValidate(request.UploadedFile!);
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

        ContractValidationDto resultDto;
        try
        {
            resultDto = await ProcessContractRequest<ContractValidationDto>(
                request.UploadedFile!,
                BuildValidationPrompt,
                GeminiService.PreAnalyzerSchemaDefinition,
                true);
        }
        catch (BadHttpRequestException ex)
        {
            _logger.LogError(ex, "Pre-validation failed");
            return BadRequest(new ContractValidationDto
            {
                IsReadable = false,
                IsContract = false,
                Result = ex.Message
            });
        }
        catch (AiServiceException ex)
        {
            _logger.LogError(ex, "AI service failed while processing the prompt");
            return StatusCode(StatusCodes.Status502BadGateway,
                "Fehler bei der Anfrage an den KI-Dienst.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        resultDto.Kollektivvertrag = kvName;
        if (!string.IsNullOrWhiteSpace(kvName))
        {
            resultDto.Result = string.Concat(
                resultDto.Result.TrimEnd(),
                " Es wurde der Kollektivvertrag \"",
                kvName,
                "\" erkannt.");
        }

        _logger.LogDebug("Result: {result}", resultDto.Result);

        return Ok(resultDto);
    }

    // Führt die vollständige Vertragsanalyse durch, indem der vorbereitete Text
    // an den Gemini-Dienst gesendet wird. Optional kann ein Kollektivvertrag
    // mitgegeben werden, um die Analyse zu verbessern.
    [HttpPost("/mainanalysis")]
    public async Task<ActionResult<ContractAnalysisDto>> PostBinaryContract([FromForm] FileUploadDto request)
    {
        _logger.LogDebug("Entering {Method}", nameof(PostBinaryContract));

        string? kvName = request.Kollektivvertrag;
        ContractAnalysisDto dto;
        try
        {
            dto = await ProcessContractRequest<ContractAnalysisDto>(
                request.UploadedFile!,
                _geminiService.PromptBuilder,
                GeminiService.AnalyzerSchemaDefinition,
                false,
                string.IsNullOrWhiteSpace(kvName),
                kvName);
        }
        catch (BadHttpRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (AiServiceException ex)
        {
            _logger.LogError(ex, "AI service failed while processing the prompt");
            return StatusCode(StatusCodes.Status502BadGateway,
                "Fehler bei der Anfrage an den KI-Dienst.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok(dto);
    }

    // Erstellt das Prompt für die Vorvalidierung. Die KI soll prüfen,
    // ob der Text lesbar ist und ob es sich um einen Arbeitsvertrag handelt.
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

    /// <summary>
    /// Gemeinsame Hilfsmethode, die das Prompt erstellt, es optional mit
    /// Kollektivvertragsdaten anreichert und anschließend an den KI-Dienst sendet.
    /// </summary>
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

    // Liest die hochgeladene Datei ein und prüft Größe sowie Dateiendung,
    // bevor der Inhalt an den KI-Dienst weitergegeben wird.
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
