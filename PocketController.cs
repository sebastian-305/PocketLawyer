using Chatbot_in.Domain.Service;
using Microsoft.AspNetCore.Mvc;
using PocketLawyer.Service;
using static System.Net.Mime.MediaTypeNames;

namespace PocketLawyer;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;
    private readonly IAiService _geminiService;
    private readonly IFileDecoderService _fileDecoderService;
    public PocketController(ILogger<PocketController> logger, IAiService geminiService, IFileDecoderService fileDecoderService)
    {
        _logger = logger;
        _geminiService = geminiService;
        _fileDecoderService = fileDecoderService;
    }

    [HttpGet("/test")]
    public string Test()
    {
        return GeminiService.ReadLawContent();
    }

    [HttpGet("/createcontext")]
    public async Task<string> CreateContext()
    {
        return await _geminiService.SendContext();
    }

    [HttpGet("/mockdata")]
    public async Task<ActionResult<string>> GetMockData()
    {
        _logger.Log(LogLevel.Debug, "Entered MainController.GetMockData");

        string path = Directory.GetCurrentDirectory() + @"\Responses";

        string filename = await System.IO.File.ReadAllTextAsync(path + @"\" + Random.Shared.Next(Directory.GetFiles(path).Length) + ".txt");

        return filename;
    }

    [HttpPost("/contract")]
    public async Task<ActionResult<string>> PostBinaryContract()
    {
        _logger.LogDebug("Entered MainController.PostBinaryContract");

        string? filename = Request.Headers["filename"];
        string filecontent;

        if (filename == null)
        {
            _logger.LogDebug("Filename missing");
            return BadRequest("Filename missing.");
        }

        if (!_fileDecoderService.AllowedExtensions.Contains(filename.Split(".", StringSplitOptions.RemoveEmptyEntries).Last()))
        {
            _logger.LogDebug("Fileextension not allowed: {ex}", filename);
            return BadRequest($"Fileextension not recognised! Recognised exensions are {_fileDecoderService.AllowedExtensionsString}.");
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
            return BadRequest(ex.Message);
        }

        _logger.LogDebug($"Extracted contend: {filename} {filecontent}");

        string prompt = "Ich sende dir eine Datei. Dabei handelt es sich um einen Arbeitsvertrag. " +
     " Falls es sich um einen Arbeitsvertrag handelt, bitte analysiere diesen und präsentiere mir deine Auswertungen. Bewerte ihn und untersuche ihn nach fragwürdigen Klauseln." + " " +
     "Die Analyse und Bewertung erfolgt aus Sicht des Arbeitnehmers. Gibt zu einer fachlichen Begrüdung auch immer eine Erklärung in einfacher Sprache. Bitte beachte folgende Gesetze: \n";

        prompt += GeminiService.ReadLawContent();

        var answer = await _geminiService.SendPrompt(prompt, filename, filecontent, GeminiService.AnalyzerSchema);

        return Ok(answer);
    }

    [HttpPost("/upload")]
    public async Task<ActionResult<string>> PostFile()
    {
        _logger.Log(LogLevel.Debug, "Entered MainController.PostFile");

        string? filename = Request.Headers["filename"];


        if (filename?.Split('.').Last() != "txt")
            return BadRequest($"Wrong file type! Only txt files are allowed!");

        //byte[] data; //Hier werden die Daten der Datei gespeichert
        //using (var ms = new MemoryStream(2048))
        //{
        //    await Request.Body.CopyToAsync(ms);
        //    data = ms.ToArray();//Daten der Datei werden im Byte-Array gespeichert
        //}

        //int size = data.Length;
        //string sizeString = size.ToString("#,##0").Replace(",", " ");

        //_logger.LogInformation("Size of the uploaded file is {size} bytes", sizeString);

        //if (size > 40_000)
        //    return BadRequest($"File size is too big! Maximum Filesize is 40 000 bytes and your file has {sizeString} bytes");



        if (filename == null)
            return BadRequest("Filename missing!");

        string text;
        using (StreamReader s = new StreamReader(Request.Body))
        {
            text = await s.ReadToEndAsync();
            Console.WriteLine(text);
        }

        string prompt = "Ich sende dir eine Datei. Dabei handelt es sich um einen Arbeitsvertrag. Falls es sich nicht um einen Arbeitsvertrag handelt, schreibe dass es sich  nicht um einen Arbeitsvertrag handelt." +
            " Falls es sich um einen Arbeitsvertrag handelt, bitte analysiere diesen und präsentiere mir deine Auswertungen. Bewerte ihn und untersuche ihn nach fragwürdigen Klauseln." + " " +
            "Die Analyse und Bewertung erfolgt aus Sicht des Arbeitnehmers. Gibt zu einer fachlichen Begrüdung auch immer eine Erklärung in einfacher Sprache.  Bitte beachte folgende Gesetze: \n";

        prompt += GeminiService.ReadLawContent();

        var file = new KeyValuePair<string, string>(filename, text);

        var answer = await _geminiService.SendPrompt(prompt, new List<KeyValuePair<string, string>>() { file });

        return answer;
    }
}
