using Chatbot_in.Domain.Service;
using Microsoft.AspNetCore.Mvc;
using PocketLawyer.Service;

namespace PocketLawyer;

[ApiController]
public class PocketController : ControllerBase
{
    private readonly ILogger<PocketController> _logger;
    private readonly IAiService _geminiService;

    public PocketController(ILogger<PocketController> logger, IAiService geminiService)
    {
        _logger = logger;
        _geminiService = geminiService;
    }

    [HttpGet("/test")]
    public string Test()
    {
        return GeminiService.ReadFiles();  
    }

    [HttpGet("/createcontext")]
    public async Task<string> CreateContext()
    {
        return await _geminiService.SendContext();
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

        string prompt = "Ich sende dir eine Datei. Dabei handelt es sich um einen Arbeitsvertrag. Falls es sich nicht um einen Arbeitsvertrag handelt, schreibe dass es sich  nicht um einen Arbeitsvertrag handelt. Falls es sich um einen Arbeitsvertrag handelt, bitte analysiere diesen und präsentiere mir deine Auswertungen. Bewerte ihn und untersuche ihn nach fragwürdigen Klauseln. Die Analyse und Bewertung erfolgt aus Sicht des Arbeitnehmers. Gibt zu einer fachlichen Begrüdung auch immer eine Erklärung in einfacher Sprache.  Bitte beachte folgende Gesetze: \n";

        prompt += GeminiService.ReadFiles();

        var file = new KeyValuePair<string, string>(filename, text);

        var answer = await _geminiService.SendPrompt(prompt, new List<KeyValuePair<string, string>>() { file });

        return answer;
    }
}
