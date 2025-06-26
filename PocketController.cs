using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Service;
using System.Text.Json;
using System.Text;

namespace PocketLawyer;

// Dieser Controller stellt die HTTP-Endpunkte der Anwendung bereit.
// Er empfängt hochgeladene Dateien und leitet sie an die KI-Services
// weiter. Die Antworten werden als JSON zurückgegeben.

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;
    private readonly IAiService _geminiService;
    private readonly IAiService _claudeService;
    private readonly IFileDecoderService _fileDecoderService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    // Die Abhängigkeiten werden über den Konstruktor injiziert.
    // Dadurch können wir sie in allen Methoden verwenden.
    public PocketController(ILogger<PocketController> logger,
        [FromKeyedServices("gemini")] IAiService geminiService,
        IFileDecoderService fileDecoderService,
        [FromKeyedServices("claude")] IAiService claudeService,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _geminiService = geminiService;
        _fileDecoderService = fileDecoderService;
        _claudeService = claudeService;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet("/test")]
    public async Task<string?> Test()
    {
        // Kleiner Test-Endpunkt, der eine Anfrage an den
        // alternativen KI-Dienst (Claude) stellt.
        // Hier wird einfach ein festes Prompt gesendet
        // und die Antwort als String zurückgegeben.

        // Beispiel 2: Mit spezifischer JSON-Struktur
        var analysisPrompt = @"Gib mir die gefragten Daten vom Land Liechtenstein an.
        Verwende diese JSON-Struktur:
        {
            'Name': 'String',
            'Hauptstadt': 'String',
            'Population': 'number',
            'Flaeche': 'number',
            'Bundeslaender': ['string'],
            'Sehenswuerdigkeiten': ['string'],
        }";

        // Anfrage an den KI-Dienst senden
        string answer = await _claudeService.SendPrompt(analysisPrompt, null, false);
        // Die rohe Antwort wird direkt zurückgegeben
        return answer;
    }

    [HttpGet("/mockdata")]
    public async Task<ActionResult<ContractAnalysisDto>> GetMockData()
    {
        // Gibt zufällige Testdaten zurück, um die Oberfläche
        // ohne echten KI-Aufruf testen zu können.
        _logger.LogDebug("Entering {Method}", nameof(GetMockData));

        // Pfad zum Ordner, in dem Beispielantworten abgelegt sind.
        string responsesPath = Path.Combine(
            _environment.ContentRootPath,
            _configuration["ResponsesPath"] ?? "Responses");

        // Stellt sicher, dass der Ordner existiert.
        Directory.CreateDirectory(responsesPath);

        string mockContent;
        try
        {
            var files = Directory.GetFiles(responsesPath);
            // Wenn keine Dateien vorhanden sind, geben wir einen Fehler zurück.
            if (files.Length == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "Keine Beispieldaten gefunden.");

            // Zufällig eine Datei auswählen und den Inhalt lesen.
            string filePath = files[Random.Shared.Next(files.Length)];
            mockContent = await System.IO.File.ReadAllTextAsync(filePath);
        }
        catch (IOException ex)
        {
            // Fehler beim Lesen der Datei
            _logger.LogError(ex, "Failed to read mock data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldaten.");
        }
        catch (UnauthorizedAccessException ex)
        {
            // Fehlende Berechtigung zum Zugriff auf die Datei
            _logger.LogError(ex, "No permission to access mock data");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Lesen der Beispieldaten.");
        }

        // Versuch, die gelesene JSON-Datei in unser Datenobjekt zu verwandeln
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


    [HttpPost("/validate")]
    public async Task<ActionResult<ContractValidationDto>> ValidateBinaryContract([FromForm] FileUploadDto request)
    {
        // Prüft zunächst, ob die hochgeladene Datei lesbar ist
        // und ob es sich überhaupt um einen Arbeitsvertrag handelt.
        _logger.LogDebug("Entering {Method}", nameof(ValidateBinaryContract));
        return await ProcessContractRequest<ContractValidationDto>(request.MyFile!, BuildValidationPrompt, GeminiService.PreAnalyzerSchema, true, true);
    }

    [HttpPost("/contract")]
    public async Task<ActionResult<ContractAnalysisDto>> PostBinaryContract([FromForm] FileUploadDto request)
    {
        // Führt die eigentliche Analyse des Vertrags durch.
        _logger.LogDebug("Entering {Method}", nameof(PostBinaryContract));
        return await ProcessContractRequest<ContractAnalysisDto>(request.MyFile!, _geminiService.PromptBuilder, GeminiService.AnalyzerSchema, false, false);
    }

    private static string BuildValidationPrompt(string filename, string filecontent)
    {
        // Erstellt das Prompt für die Voranalyse. Die KI soll hier nur
        // feststellen, ob die Datei lesbar ist und ob es sich um einen
        // Arbeitsvertrag handelt. Das Ergebnis wird als JSON erwartet.
        var sb = new StringBuilder();
        sb.Append("Ich sende dir eine Datei. Analysiere, ob der Inhalt der Datei lesbar ist. Schreibe das Ergebnis in die boolean Variable \"isReadable\".");
        sb.Append("Wenn der Text lesbar ist, analysiere ob es sich dabei um einen Arbeitsvertrag handelt. Unabhängig von der Sprache der Datei soll seine Antwort auf deutsch sein!");
        sb.Append("Schreibe das Ergebnis in die bool Variable \"isContract\". ");
        sb.Append("Gib eine Begründung deiner Ergebnisse und falls es sich um einen Arbeitsvertrag handelt, eine überblicksmäßige Zusammenfassung vom Vertrag in die Variable \"result\". ");
        sb.Append("Verwende keine Markdown-Formatierungen! Formatiere die Antwort in HTML! Hier die Datei:\n\n Dateiname: \n");
        sb.Append(filename);
        sb.Append("\nDateiinhalt: ");
        sb.Append(filecontent);
        return sb.ToString();
    }

    // Gemeinsame Logik für die beiden Endpunkte /validate und /contract.
    // Liest die Datei ein, baut das Prompt auf und wertet die Antwort der KI aus.
    private async Task<ActionResult<T>> ProcessContractRequest<T>(IFormFile file, Func<string, string, string> promptBuilder, object schema, bool sendAltKey, bool validationErrorFormat)
    {
        FileContentDto requestContent;
        try
        {
            // Zunächst wird die Datei geprüft und in Text umgewandelt.
            requestContent = await PreValidate(file);
        }
        catch (Exception ex)
        {
            if (validationErrorFormat)
            {
                _logger.LogError(ex, "Pre-validation failed");
                return BadRequest($"{{\"isReadable\":false, \"isContract\":false, \"result\":\"{ex.Message}\"}}");
            }
            return BadRequest(ex.Message);
        }

        // Mit Hilfe der übergebenen Funktion wird das Prompt aufgebaut,
        // das an den KI-Dienst geschickt werden soll.
        string prompt = promptBuilder(requestContent.FileName, requestContent.FileContent);
        string answer;
        try
        {
            // Anfrage an den KI-Dienst senden
            answer = await _geminiService.SendPrompt(prompt, schema, sendAltKey);
        }
        catch (AiServiceException ex)
        {
            _logger.LogError(ex, "AI service failed while processing the prompt");
            return StatusCode(StatusCodes.Status502BadGateway,
                "Fehler bei der Anfrage an den KI-Dienst.");
        }

        // Antwort in unser Zielobjekt umwandeln
        T? dto;
        try
        {
            dto = JsonSerializer.Deserialize<T>(answer);
        }
        catch (JsonException ex)
        {
            // Die Antwort war kein gültiges JSON
            string truncated = answer.Length > 200 ? answer[..200] + "..." : answer;
            _logger.LogError(ex, "Failed to parse AI response: {Snippet}", truncated);
            return BadRequest($"Fehler beim Interpretieren der KI-Antwort: {truncated}");
        }
        if (dto == null)
            // Falls die Deserialisierung null ergibt, war die Antwort ungültig
            return BadRequest("Invalid AI response");

        return Ok(dto);
    }

    private async Task<FileContentDto> PreValidate(IFormFile file)
    {
        // Liest die hochgeladene Datei ein und führt grundlegende
        // Prüfungen durch (Dateityp, Größe, Inhalt extrahieren).
        _logger.LogDebug("Entering {Method}", nameof(PreValidate));

        string filename = file.FileName;
        string filecontent;

        var extension = Path.GetExtension(filename).TrimStart('.');
        // Prüfen, ob die Dateiendung unterstützt wird.
        if (!_fileDecoderService.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Fileextension not allowed: {FileName}", filename);
            throw new BadHttpRequestException($"Fileextension not recognised! Recognised extensions are {_fileDecoderService.AllowedExtensionsString}.");
        }

        // Maximale Dateigröße aus der Konfiguration lesen.
        long maxBytes = _configuration.GetValue<long>("MaxUploadSizeMB") * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            // Datei ist zu groß
            throw new BadHttpRequestException($"File too large. Maximum {_configuration.GetValue<long>("MaxUploadSizeMB")} MB allowed.");
        }

        // Inhalt der Datei in einen Stream lesen.
        await using var stream = file.OpenReadStream();

        try
        {
            // Dateiinhalt in lesbaren Text umwandeln
            filecontent = await _fileDecoderService.GetFileContentAsync(filename, stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding file {FileName}", filename);
            throw new BadHttpRequestException(ex.Message);
        }

        // Ergebnis als DTO zurückgeben
        return new FileContentDto(filename, filecontent);
    }

}
