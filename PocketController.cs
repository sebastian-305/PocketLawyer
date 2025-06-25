using Chatbot_in.Domain.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PocketLawyer.Infrastructure.Claude;
using PocketLawyer.Service;

namespace PocketLawyer;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;
    private readonly IAiService _geminiService;
    private readonly IAiService _claudeService;
    private readonly IFileDecoderService _fileDecoderService;

    public PocketController(ILogger<PocketController> logger, [FromKeyedServices("gemini")] IAiService geminiService, IFileDecoderService fileDecoderService, [FromKeyedServices("claude")] IAiService claudeService)
    {
        _logger = logger;
        _geminiService = geminiService;
        _fileDecoderService = fileDecoderService;
        _claudeService = claudeService;
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
        _logger.Log(LogLevel.Debug, "Entered MainController.GetMockData");

        string path = Directory.GetCurrentDirectory() + @"\Responses";

        string filename = await System.IO.File.ReadAllTextAsync(path + @"\" + Random.Shared.Next(Directory.GetFiles(path).Length) + ".txt");

        return filename;
    }

    [HttpPost("/validate")]
    public async Task<ActionResult<string>> ValidateBinaryContract()
    {
        _logger.LogDebug("Entered MainController.ValidateBinaryContract");
        string[] requestContent;
        try
        {
            requestContent = await PreValidate(Request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return BadRequest($"{{\"isReadable\":false, \"isContract\":false, \"result\":\"{ex.Message}\"}}");
        }

        string filename = requestContent[0];
        string filecontent = requestContent[1];

        string prompt = "Ich sende dir eine Datei. Analysiere, ob der Inhalt der Datei lesbar ist. Schreibe das Ergebnis in die boolan Variable \"isReadable\". " +
            "Wenn der Text lesbar ist, analysiere ob es sich dabei um einen Arbeitsvertrag handelt. Unabhängig von der Sprache der Datei soll seine Antwort auf deutsch sein!" +
            "Schreibe das Ergebnis in die bool Variable \"isContract\". " +
            "Gib eine Begründung deiner Ergebnisse und falls es sich um einen Arbeitsvertrag handelt, eine überblicksmäßige Zusammenfassung vom Vertrag in die Variable \"result\". " +
            "Verwende keine Markdown-Formatierungen! Formatiere die Antwort in HTML! Hier die Datei:\n\n Dateiname: \n" +
            filename +
            "\nDateiinhalt: " +
            filecontent;

        string answer = await _geminiService.SendPrompt(prompt, GeminiService.PreAnalyzerSchema, true);

        return answer;
    }

    [HttpPost("/contract")]
    public async Task<ActionResult<string>> PostBinaryContract()
    {
        _logger.LogDebug("Entered MainController.PostBinaryContract");
        string[] requestContent;
        try
        {
            requestContent = await PreValidate(Request);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        string filename = requestContent[0];
        string filecontent = requestContent[1];


        _logger.LogDebug("Extracted contend: {fn} {fc}", filename, filecontent);

        string prompt = "Ich sende dir eine Datei. Dabei handelt es sich um einen Arbeitsvertrag. " +
     " Falls es sich um einen Arbeitsvertrag handelt, bitte analysiere diesen und präsentiere mir deine Auswertungen. Bewerte ihn und untersuche ihn nach fragwürdigen Klauseln." + " " +
     "Die Analyse und Bewertung erfolgt aus Sicht des Arbeitnehmers. Gibt zu einer fachlichen Begrüdung auch immer eine Erklärung in einfacher Sprache. Bitte die Gesetze im Cached Kontext \n";

        //prompt += GeminiService.ReadLawContent();

        string fullprompt = _geminiService.PromptBuilder(prompt, filename, filecontent);
        string answer = await _geminiService.SendPrompt(fullprompt, GeminiService.AnalyzerSchema, false);

        return Ok(answer);
    }

    private async Task<string[]> PreValidate(HttpRequest request)
    {
        string? filename = request.Headers["filename"];
        string filecontent;

        if (filename == null)
        {
            _logger.LogDebug("Filename missing");
            throw new BadHttpRequestException("Filename missing.");
        }

        if (!_fileDecoderService.AllowedExtensions.Contains(filename.Split(".", StringSplitOptions.RemoveEmptyEntries).Last()))
        {
            _logger.LogDebug("Fileextension not allowed: {ex}", filename);
            throw new BadHttpRequestException($"Fileextension not recognised! Recognised exensions are {_fileDecoderService.AllowedExtensionsString}.");
        }

        byte[] data; //Hier werden die Daten der Datei gespeichert
        using (var ms = new MemoryStream(2048))
        {
            await Request.Body.CopyToAsync(ms);
            data = ms.ToArray();//Daten der Datei werden im Byte-Array gespeichert
        }

        try
        {
            filecontent = await _fileDecoderService.GetFileContentAsync(filename, data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in decoding file: {err}", ex.Message);
            throw new BadHttpRequestException(ex.Message);
        }

        return [filename, filecontent];
    }

}
