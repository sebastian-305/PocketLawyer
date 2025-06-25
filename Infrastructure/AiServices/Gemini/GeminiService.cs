using PocketLawyer.Infrastructure.AiServices.Gemini.Models;
using PocketLawyer.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Text.Json;

namespace PocketLawyer.Infrastructure.AiServices.Gemini;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiAltKey;
    private readonly string _model;
    private readonly ILogger<GeminiService> _logger;
    private readonly bool _writeInFile;
    public static object AnalyzerSchema => new
    {
        type = "object",
        properties = new
        {
            summary = new
            {
                type = "object",
                properties = new
                {
                    arbeitgeber = new { type = "string" },
                    taetigkeit = new { type = "string" },
                    beginn = new { type = "string" },
                    dauer = new { type = "string" },
                    gehalt_brutto = new { type = "string" },
                    zuschlaege = new { type = "string" },
                    arbeitsstunden = new { type = "string" },
                    ueberstunden = new { type = "string" },
                    probezeit = new { type = "string" }
                },
                required = new[] {
                        "arbeitgeber", "taetigkeit", "beginn", "dauer",
                        "gehalt_brutto", "zuschlaege", "arbeitsstunden",
                        "ueberstunden", "probezeit"
                    }
            },
            unzulaessige_klauseln = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        titel = new { type = "string" },
                        zitat = new { type = "string" },
                        problem = new { type = "string" },
                        rechtsgrundlage = new { type = "string" }
                    },
                    required = new[] { "titel", "zitat", "problem", "rechtsgrundlage" }
                }
            },
            nachteile_klauseln = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        titel = new { type = "string" },
                        zitat = new { type = "string" },
                        problem = new { type = "string" },
                        rechtsgrundlage = new { type = "string" }
                    },
                    required = new[] { "titel", "zitat", "problem", "rechtsgrundlage" }
                }
            },
            widerspruechliche_klauseln = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        titel = new { type = "string" },
                        zitat_klausel_a = new { type = "string" },
                        zitat_klausel_b = new { type = "string" },
                        problem = new { type = "string" },
                        rechtsgrundlage = new { type = "string" }
                    },
                    required = new[] { "titel", "zitat_klausel_a", "zitat_klausel_b", "problem", "rechtsgrundlage" }
                }
            },
            vorteilhafte_klauseln = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        titel = new { type = "string" },
                        zitat = new { type = "string" },
                        vorteil = new { type = "string" }
                    },
                    required = new[] { "titel", "zitat", "vorteil" }
                }
            },
            fazit = new { type = "string" }
        },
        required = new[] {
                "summary",
                "unzulaessige_klauseln",
                "nachteile_klauseln",
                "widerspruechliche_klauseln",
                "vorteilhafte_klauseln",
                "fazit"
            }


    };
    public static object PreAnalyzerSchema => new
    {
        type = "object",
        properties = new
        {
            isReadable = new { type = "boolean" },
            isContract = new { type = "boolean" },
            result = new { type = "string" },
        },
        required = new[] {
                "isReadable",
                "result",
            }
    };
    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:Key"] ?? string.Empty;
        _apiAltKey = configuration["Gemini:Key:Alternative"] ?? string.Empty;
        _model = "gemini-1.5-flash-002";
        _logger = logger;
        _writeInFile = configuration.GetValue<bool>("WriteGeminiResponseInFile");
    }

    #region public_methods
    public async Task<string> SendContext()
    {
        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";

        var contentAsString = await ReadLawContentAsync();

        _logger.LogDebug("The length of the normal Context is {0}", contentAsString.Length);

        var contentAs64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(contentAsString));

        _logger.LogDebug("The length of the 64 Context is {0}", contentAs64.Length);

        try
        {
            var context = new
            {
                model = $"models/{_model}",
                contents = new[]
                {
            new
            {
                parts = new object[]
                {
                    new
                    {
                        inline_data = new
                        {
                            mime_type = "text/plain",
                            data = contentAs64
                        }
                    }
                },
                role = "user"
            }
        },
                systemInstruction = new
                {
                    parts = new object[]
                {
                    new
                    {
                        text = "Du bist ein juristischer Experte, der Arbeitsverträge auf Basis des österreichischen Arbeitsrechts analysieren kann. " +
                        "Im Kontext befinden sich die relevanten Gesetzestexte auf die du dich beziehst. " +
                        "\n\"Bitte verwende keine Markdownformatierung, * oder #\" " +
                        "\n\"Formatiere deine Antwort in HTML\""
                    }
                }
                },
                ttl = "600s"
            };

            var jsonContent = JsonSerializer.Serialize(context);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(requestUri, content);
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug(responseString);
            response.EnsureSuccessStatusCode();
            return responseString ?? "";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"HTTP Request Error: {ex.Message}, Status Code: {ex.StatusCode}");

            return "Something went wrong with the API request.";
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred: {ex.Message}");
            return "Something went wrong";
        }
    }
    public string PromptBuilder(string filename, string filecontent)
    {
        StringBuilder promptBuilder = new StringBuilder();

        promptBuilder.Append("Ich sende dir eine Datei. Dabei handelt es sich um einen Arbeitsvertrag. ");
        promptBuilder.Append("Falls es sich um einen Arbeitsvertrag handelt, bitte analysiere diesen und präsentiere mir deine Auswertungen. ");
        promptBuilder.Append("Bewerte ihn und untersuche ihn nach fragwürdigen Klauseln.");
        promptBuilder.Append("Die Analyse und Bewertung erfolgt aus Sicht des Arbeitnehmers. ");
        promptBuilder.Append("Gibt zu einer fachlichen Begründung auch immer eine Erklärung in einfacher Sprache.");
        promptBuilder.Append("Bitte gründe deine Aussagen auf die Gesetze im Cached Kontext. \n");
        promptBuilder.Append("Bitte verwende keine Markdownformatierung, * oder #");
        promptBuilder.Append("Formatiere deine Antwort in HTML.");
        promptBuilder.Append("Gib neben dem Paragraphen der Rechtsgrundlage auch den Gesetztestext an.\n");
        promptBuilder.Append("---------------------------------");
        promptBuilder.Append("\n\nDateiname des Vertrags: ");
        promptBuilder.Append(filename);
        promptBuilder.Append("\n\nVertragsinhalt: \n");
        promptBuilder.Append(filecontent);
        promptBuilder.Append("---------------------------------");

        _logger.LogInformation("Built prompt with the length of {length} symbols", promptBuilder.Length);
        return promptBuilder.ToString();
    }
    public async Task<string> SendPrompt(string prompt, object? outputSchema, bool useAltKey = false)
    {
        _logger.LogDebug("Endered SendPrompt");
        string apiKey = useAltKey ? _apiAltKey : _apiKey;

        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={apiKey}";

        var requestBody = new
        {
            contents = new[]
        {
                new
                {
                    parts = new[]
                {
                        new
                        {
                            text = prompt
                        }
                    },
                    //        role = "user"
                role= "user",
                },
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = outputSchema,
                temperature = 0.2,
            },
            cachedContent = useAltKey ? null : await GetContextKey(),
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending Prompt...");
        try
        {
            var response = await _httpClient.PostAsync(requestUri, content);
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug(responseString);
            var result = JsonSerializer.Deserialize<GeminiReponse>(responseString);


            response.EnsureSuccessStatusCode();

            string resultText = result?.candidates[0]?.content?.parts[0]?.text ?? string.Empty;

            if (_writeInFile)
                WriteResponseInFile(resultText);
            return resultText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return "Error";
        }
    }
    #endregion

    #region private_methods
    private static async Task<string> ReadLawContentAsync()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Context");

        var files = Directory.GetFiles(dir);

        StringBuilder contextBuilder = new StringBuilder();

        foreach (var file in files)
        {
            contextBuilder.Append("Erster Gesetzestext mit dem Dateinamen ");
            contextBuilder.Append(file);
            contextBuilder.AppendLine();
            contextBuilder.Append(await File.ReadAllTextAsync(file));
            contextBuilder.AppendLine();
            contextBuilder.AppendLine();
        }

        return contextBuilder.ToString();
    }
    private void WriteResponseInFile(string response)
    {

        _logger.LogDebug("Writing Response to file");

        string folder = Path.Combine(Directory.GetCurrentDirectory(), "Responses");
        string filePath = Path.Combine(folder, Directory.GetFiles(folder).Length + ".txt");

        _logger.LogDebug("Filename: {name} Content Length: {length}", Path.GetFileName(filePath), response.Length);
        using (StreamWriter sr = new StreamWriter(filePath))
        {
            sr.Write(response);
        }
    }
    private async Task<string?> GetContextKey()
    {
        _logger.LogDebug("GeminiService.GetContextKey()");
        var contextUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";
        var response = await _httpClient.GetAsync(contextUri);
        var responseString = await response.Content.ReadAsStringAsync();

        string? result = ExtractContextKey(responseString);

        if (result == null)
        {
            _logger.LogDebug("Couldn't find a Context Key, Creating a new Context....");
            result = ExtractContextKey(await SendContext());
        }

        return result;
    }
    private string? ExtractContextKey(string responseString)
    {
        return JsonSerializer.Deserialize<GeminiContext>(responseString)?.cachedContents?.First().name;
    }
    #endregion
}
