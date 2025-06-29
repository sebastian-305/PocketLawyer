using PocketLawyer.Infrastructure.AiServices.Gemini.Models;
using PocketLawyer.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;


namespace PocketLawyer.Infrastructure.AiServices.Gemini;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiAltKey;
    private readonly string _modelId;
    private readonly ILogger<GeminiService> _logger;
    private readonly bool _writeInFile;
    private readonly IGeminiContextManager _contextManager;
    private readonly IResponseLogger _responseLogger;
    public static AnalyzerSchema AnalyzerSchemaDefinition => new AnalyzerSchema();
    public static PreAnalyzerSchema PreAnalyzerSchemaDefinition => new PreAnalyzerSchema();
    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiService> logger,
        IGeminiContextManager contextManager,
        IResponseLogger responseLogger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:Key"] ?? string.Empty;
        _apiAltKey = configuration["Gemini:Key:Alternative"] ?? string.Empty;
        _modelId = configuration["Gemini:ModelId"] ?? "gemini-1.5-flash-002";
        _logger = logger;
        _writeInFile = configuration.GetValue<bool>("WriteGeminiResponseInFile");
        _contextManager = contextManager;
        _responseLogger = responseLogger;
    }

    #region public_methods
    public Task<string> SendContext()
    {
        return _contextManager.SendContextAsync();
    }
    public string PromptBuilder(string filename, string fileContent)
    {
        StringBuilder promptBuilder = new StringBuilder();

        promptBuilder.AppendLine("Ich sende dir eine Datei. Dabei handelt es sich um einen Arbeitsvertrag. Eventuell schicke ich dir auch noch den Kollektivvertrag mit. Der Kollektivvetrag is die wichtigste Grundlage der Analyse.");
        promptBuilder.AppendLine("Als Rechtsgrundlage der einzelenen Analysepunkte soll neben dem Paragraphen und dem Namen des jeweiligen Gesetztes auch der Wortlaut des Gesetztes angegeben werden.\n");
        promptBuilder.AppendLine("Die Property 'rechtsgrundlage' ist ein Array von Objekten mit den Feldern 'rechtsgrundlage' und 'wortlaut'. Beide Felder sind erforderlich.\n");
        promptBuilder.AppendLine("Falls es sich um einen Arbeitsvertrag handelt, bitte analysiere diesen und präsentiere mir deine Auswertungen. ");
        promptBuilder.AppendLine("Bewerte ihn und untersuche ihn nach fragwürdigen Klauseln.");
        promptBuilder.AppendLine("Finde alle vorteilhafte Klauseln, falls es welche gibt.");
        promptBuilder.AppendLine("Falls ein Kollektivvertrag mitgeschickt wird, hebe auch alle Klauseln heraus, die für den Arbeitnehmer eine Verbesserung gegenüber dem Kollektivvertrag darstellen.");        
        promptBuilder.AppendLine("Die Analyse und Bewertung erfolgt aus Sicht des Arbeitnehmers. ");
        promptBuilder.AppendLine("Gibt zu einer fachlichen Begründung auch immer eine Erklärung in einfacher Sprache.");
        promptBuilder.AppendLine("Bitte gründe deine Aussagen auf die Gesetze im Cached Kontext und auf den eventuell mitgeschickten Kollektivvertrag. \n");
        promptBuilder.AppendLine("Gib ein sehr ausführliches Fazit an. ");
        promptBuilder.AppendLine("Bitte verwende keine Markdownformatierung, * oder #");
        promptBuilder.AppendLine("Formatiere deine Antwort in HTML.");
        promptBuilder.AppendLine("Gib neben dem Paragraphen der Rechtsgrundlage auch den Wortlaut des Gesetztestextes an.\n");
        promptBuilder.AppendLine("Für Regelungen die im Arbeitsvertrag mit \"laut Gesetzt\" oder \"laut Kollektivvertrag\" angegeben werden, ziehe die entsprechende Regelung aus diesen Quellen (falls vorhanden) und die Informtation aus den Quellen an.\n");
        promptBuilder.AppendLine("---------------------------------");
        promptBuilder.AppendLine("\n\nDateiname des Vertrags: ");
        promptBuilder.AppendLine(filename);
        promptBuilder.AppendLine("\n\nVertragsinhalt: \n");
        promptBuilder.AppendLine(fileContent);
        promptBuilder.AppendLine("---------------------------------");

        _logger.LogInformation("Built prompt with the length of {length} symbols", promptBuilder.Length);
        return promptBuilder.ToString();
    }
    public async Task<string> SendPrompt(string prompt, object? outputSchema, bool useAltKey = false)
    {
        _logger.LogDebug("Entering {Method}", nameof(SendPrompt));
        string apiKey = useAltKey ? _apiAltKey : _apiKey;

        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelId}:generateContent?key={apiKey}";

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
            cachedContent = useAltKey ? null : await _contextManager.GetContextKeyAsync(),
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending prompt to Gemini API");
        try
        {
            var response = await _httpClient.PostAsync(requestUri, content);                      
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Prompt response: {Response}", responseString);
            response.EnsureSuccessStatusCode();

            var result = JsonSerializer.Deserialize<GeminiResponse>(responseString);

            string resultText = result?.Candidates[0]?.Content?.Parts[0]?.Text ?? string.Empty;

            if (_writeInFile)
                await _responseLogger.LogResponseAsync(resultText);
            return resultText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending prompt");
            throw new AiServiceException("Gemini API request failed", ex);
        }
    }
    #endregion
}
