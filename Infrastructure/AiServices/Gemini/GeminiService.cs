using PocketLawyer.Infrastructure.AiServices.Gemini.Models;
using PocketLawyer.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Text.Json;

namespace PocketLawyer.Infrastructure.AiServices.Gemini;

// Dieser Service kapselt alle Aufrufe an die Google Gemini API.
// Er implementiert IAiService, sodass der Controller ihn verwenden kann.

public class GeminiService : IAiService
{
    // HTTP-Client für die Kommunikation mit der API
    private readonly HttpClient _httpClient;
    // Haupt-API-Schlüssel
    private readonly string _apiKey;
    // Alternativer Schlüssel, falls benötigt
    private readonly string _apiAltKey;
    // Modellbezeichnung der Gemini-API
    private readonly string _model;
    private readonly ILogger<GeminiService> _logger;
    // Soll die Antwort zusätzlich in eine Datei geschrieben werden?
    private readonly bool _writeInFile;
    public static AnalyzerSchema AnalyzerSchema => new AnalyzerSchema();
    public static PreAnalyzerSchema PreAnalyzerSchema => new PreAnalyzerSchema();
    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        // Abhängigkeiten über den Konstruktor setzen
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
        // Lädt Gesetzestexte hoch, damit Gemini sie bei späteren
        // Anfragen als Kontext nutzen kann.
        _logger.LogDebug("Entering {Method}", nameof(SendContext));

        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";

        // Gesetzestexte von der Festplatte laden
        var contentAsString = await ReadLawContentAsync();

        _logger.LogDebug("The length of the normal Context is {Length}", contentAsString.Length);

        // Google erwartet den Text Base64-kodiert
        var contentAs64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(contentAsString));

        _logger.LogDebug("The length of the 64 Context is {Length}", contentAs64.Length);

        try
        {
            // Aufbau des Request-Bodys für die Context-API
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

            // Request zusammenbauen und senden
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(requestUri, content);
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Context API response: {Response}", responseString);
            response.EnsureSuccessStatusCode();
            return responseString ?? "";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Request Error, Status Code: {StatusCode}", ex.StatusCode);

            return "Something went wrong with the API request.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending context");
            return "Something went wrong";
        }
    }
    public string PromptBuilder(string filename, string filecontent)
    {
        // Erstellt das Prompt, das dem KI-Modell geschickt wird
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
        // Sendet ein Prompt an Gemini. useAltKey bestimmt, welcher API-Key verwendet wird.
        _logger.LogDebug("Entering {Method}", nameof(SendPrompt));
        string apiKey = useAltKey ? _apiAltKey : _apiKey;

        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={apiKey}";

        // Aufbau des JSON-Requestkörpers für die GenerateContent-API
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    },
                    role = "user",
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

        _logger.LogInformation("Sending prompt to Gemini API");
        try
        {
            var response = await _httpClient.PostAsync(requestUri, content);
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Prompt response: {Response}", responseString);
            var result = JsonSerializer.Deserialize<GeminiResponse>(responseString);


            response.EnsureSuccessStatusCode();

            string resultText = result?.candidates[0]?.content?.parts[0]?.text ?? string.Empty;

            if (_writeInFile)
                WriteResponseInFile(resultText);
            // Reinen Text an den Aufrufer zurückgeben
            return resultText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending prompt");
            throw new AiServiceException("Gemini API request failed", ex);
        }
    }
    #endregion

    #region private_methods
    private static async Task<string> ReadLawContentAsync()
    {
        // Pfad zu den Gesetzestexten zusammensetzen
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Context");

        var files = Directory.GetFiles(dir);

        StringBuilder contextBuilder = new StringBuilder();

        // Alle Dateien zusammenführen, damit sie später als ein Textblock
        // gesendet werden können.
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
        // Optionale Funktion zum Debuggen: Antwort in Datei speichern
        _logger.LogDebug("Writing response to file");

        string folder = Path.Combine(Directory.GetCurrentDirectory(), "Responses");

        try
        {
            Directory.CreateDirectory(folder);

            string filePath = Path.Combine(
                folder,
                $"{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");

            _logger.LogDebug(
                "Filename: {Name} Content Length: {Length}",
                Path.GetFileName(filePath),
                response.Length);

            using StreamWriter sr = new StreamWriter(filePath);
            sr.Write(response);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to write response to file");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Failed to write response due to permission issues");
        }
    }
    private async Task<string?> GetContextKey()
    {
        // Prüft, ob bereits ein Context-Key existiert, ansonsten wird ein neuer angelegt.
        _logger.LogDebug("Entering {Method}", nameof(GetContextKey));
        var contextUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";
        var response = await _httpClient.GetAsync(contextUri);
        var responseString = await response.Content.ReadAsStringAsync();

        string? result = ExtractContextKey(responseString);

        if (result == null)
        {
            // Falls kein Schlüssel vorhanden ist, legen wir einen neuen an
            _logger.LogDebug("Couldn't find a Context Key, creating a new context");
            result = ExtractContextKey(await SendContext());
        }

        return result;
    }
    private string? ExtractContextKey(string responseString)
    {
        // Kontextschlüssel aus der API-Antwort herausfiltern
        return JsonSerializer.Deserialize<GeminiContext>(responseString)?.cachedContents?.First().name;
    }
    #endregion
}
