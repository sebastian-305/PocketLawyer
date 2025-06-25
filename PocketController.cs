using Microsoft.AspNetCore.Mvc;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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

        return await _claudeService.SendPrompt(analysisPrompt, null, false);
    }

    [HttpGet("/mockdata")]
    public async Task<ActionResult<string>> GetMockData()
    {
        _logger.LogDebug("Entering {Method}", nameof(GetMockData));

        string responsesPath = Path.Combine(
            _environment.ContentRootPath,
            _configuration["ResponsesPath"] ?? "Responses");

        string filename = await System.IO.File.ReadAllTextAsync(
            Path.Combine(responsesPath,
                $"{Random.Shared.Next(Directory.GetFiles(responsesPath).Length)}.txt"));

        return filename;
    }

    [HttpPost("/validate")]
    public async Task<ActionResult<string>> ValidateBinaryContract()
    {
        _logger.LogDebug("Entering {Method}", nameof(ValidateBinaryContract));
        FileContentDto requestContent;
        try
        {
            requestContent = await PreValidate(Request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pre-validation failed");
            return BadRequest($"{{\"isReadable\":false, \"isContract\":false, \"result\":\"{ex.Message}\"}}");
        }

        string filename = requestContent.FileName;
        string filecontent = requestContent.FileContent;

        string prompt = "Ich sende dir eine Datei. Analysiere, ob der Inhalt der Datei lesbar ist. Schreibe das Ergebnis in die boolan Variable \"isReadable\". " +
            "Wenn der Text lesbar ist, analysiere ob es sich dabei um einen Arbeitsvertrag handelt. Unabhängig von der Sprache der Datei soll seine Antwort auf deutsch sein!" +
            "Schreibe das Ergebnis in die bool Variable \"isContract\". " +
            "Gib eine Begründung deiner Ergebnisse und falls es sich um einen Arbeitsvertrag handelt, eine überblicksmäßige Zusammenfassung vom Vertrag in die Variable \"result\". " +
            "Verwende keine Markdown-Formatierungen! Formatiere die Antwort in HTML! Hier die Datei:\n\n Dateiname: \n" +
            filename +
            "\nDateiinhalt: " +
            filecontent;

        return await _geminiService.SendPrompt(prompt, GeminiService.PreAnalyzerSchema, true);
    }

    [HttpPost("/contract")]
    public async Task<ActionResult<string>> PostBinaryContract()
    {
        _logger.LogDebug("Entering {Method}", nameof(PostBinaryContract));
        FileContentDto requestContent;
        try
        {
            requestContent = await PreValidate(Request);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        string filename = requestContent.FileName;
        string filecontent = requestContent.FileContent;

        _logger.LogDebug("Extracted content from {FileName} with length {Length}",
            filename, filecontent.Length);


        //prompt += GeminiService.ReadLawContent();

        string fullprompt = _geminiService.PromptBuilder(filename, filecontent);
        string answer = await _geminiService.SendPrompt(fullprompt, GeminiService.AnalyzerSchema, false);

        return Ok(answer);
    }

    private async Task<FileContentDto> PreValidate(HttpRequest request)
    {
        _logger.LogDebug("Entering {Method}", nameof(PreValidate));

        string? filename = request.Headers["filename"];
        string filecontent;

        if (filename == null)
        {
            _logger.LogWarning("Filename missing");
            throw new BadHttpRequestException("Filename missing.");
        }

        var extension = Path.GetExtension(filename).TrimStart('.');
        if (!_fileDecoderService.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Fileextension not allowed: {FileName}", filename);
            throw new BadHttpRequestException($"Fileextension not recognised! Recognised exensions are {_fileDecoderService.AllowedExtensionsString}.");
        }

        long maxBytes = _configuration.GetValue<long>("MaxUploadSizeMB") * 1024 * 1024;
        if (request.ContentLength != null && request.ContentLength > maxBytes)
        {
            throw new BadHttpRequestException($"File too large. Maximum {_configuration.GetValue<long>("MaxUploadSizeMB")} MB allowed.");
        }

        byte[] data; //Hier werden die Daten der Datei gespeichert
        using (var ms = new MemoryStream())
        {
            await Request.Body.CopyToAsync(ms);
            if (ms.Length > maxBytes)
            {
                throw new BadHttpRequestException($"File too large. Maximum {_configuration.GetValue<long>("MaxUploadSizeMB")} MB allowed.");
            }
            data = ms.ToArray();//Daten der Datei werden im Byte-Array gespeichert
        }

        try
        {
            filecontent = await _fileDecoderService.GetFileContentAsync(filename, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding file {FileName}", filename);
            throw new BadHttpRequestException(ex.Message);
        }

        return new FileContentDto(filename, filecontent);
    }

}
