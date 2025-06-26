using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Service;
using System.Text.Json;
using System.Text;

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

        string answer = await _claudeService.SendPrompt(analysisPrompt, null, false);
        return answer;
    }

    [HttpGet("/mockdata")]
    public async Task<ActionResult<ContractAnalysisDto>> GetMockData()
    {
        _logger.LogDebug("Entering {Method}", nameof(GetMockData));

        string responsesPath = Path.Combine(
            _environment.ContentRootPath,
            _configuration["ResponsesPath"] ?? "Responses");

        string mockContent = await System.IO.File.ReadAllTextAsync(
            Path.Combine(responsesPath,
                $"{Random.Shared.Next(Directory.GetFiles(responsesPath).Length)}.json"));

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
        _logger.LogDebug("Entering {Method}", nameof(ValidateBinaryContract));
        return await ProcessContractRequest<ContractValidationDto>(request.MyFile!, BuildValidationPrompt, GeminiService.PreAnalyzerSchema, true, true);
    }

    [HttpPost("/contract")]
    public async Task<ActionResult<ContractAnalysisDto>> PostBinaryContract([FromForm] FileUploadDto request)
    {
        _logger.LogDebug("Entering {Method}", nameof(PostBinaryContract));
        return await ProcessContractRequest<ContractAnalysisDto>(request.MyFile!, _geminiService.PromptBuilder, GeminiService.AnalyzerSchema, false, false);
    }

    private static string BuildValidationPrompt(string filename, string filecontent)
    {
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

    private async Task<ActionResult<T>> ProcessContractRequest<T>(IFormFile file, Func<string, string, string> promptBuilder, object schema, bool sendAltKey, bool validationErrorFormat)
    {
        FileContentDto requestContent;
        try
        {
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

        string prompt = promptBuilder(requestContent.FileName, requestContent.FileContent);
        string answer;
        try
        {
            answer = await _geminiService.SendPrompt(prompt, schema, sendAltKey);
        }
        catch (AiServiceException ex)
        {
            _logger.LogError(ex, "AI service failed while processing the prompt");
            return StatusCode(StatusCodes.Status502BadGateway,
                "Fehler bei der Anfrage an den KI-Dienst.");
        }

        T? dto;
        try
        {
            dto = JsonSerializer.Deserialize<T>(answer);
        }
        catch (JsonException ex)
        {
            string truncated = answer.Length > 200 ? answer[..200] + "..." : answer;
            _logger.LogError(ex, "Failed to parse AI response: {Snippet}", truncated);
            return BadRequest($"Fehler beim Interpretieren der KI-Antwort: {truncated}");
        }
        if (dto == null)
            return BadRequest("Invalid AI response");

        return Ok(dto);
    }

    private async Task<FileContentDto> PreValidate(IFormFile file)
    {
        _logger.LogDebug("Entering {Method}", nameof(PreValidate));

        string filename = file.FileName;
        string filecontent;

        var extension = Path.GetExtension(filename).TrimStart('.');
        if (!_fileDecoderService.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Fileextension not allowed: {FileName}", filename);
            throw new BadHttpRequestException($"Fileextension not recognised! Recognised extensions are {_fileDecoderService.AllowedExtensionsString}.");
        }

        long maxBytes = _configuration.GetValue<long>("MaxUploadSizeMB") * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new BadHttpRequestException($"File too large. Maximum {_configuration.GetValue<long>("MaxUploadSizeMB")} MB allowed.");
        }

        await using var stream = file.OpenReadStream();

        try
        {
            filecontent = await _fileDecoderService.GetFileContentAsync(filename, stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding file {FileName}", filename);
            throw new BadHttpRequestException(ex.Message);
        }

        return new FileContentDto(filename, filecontent);
    }

}
