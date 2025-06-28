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
    public async Task<ActionResult<TestResultDto>> Test()
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

        string answer;
        try
        {
            answer = await _kvDetectionService.DetectAsync(contractContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI service failed while processing the prompt");
            return StatusCode(StatusCodes.Status502BadGateway, "Fehler bei der Anfrage an den KI-Dienst.");
        }

        var resultDto = new TestResultDto
        {
            Responses = new[] { answer },
            AllEqual = true
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
