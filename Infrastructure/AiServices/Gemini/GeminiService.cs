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

    #region private_methods

    private async Task<string> SendContext()
    {
        _logger.LogDebug("Entering {Method}", nameof(SendContext));

        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";

        var contentAsString = await ReadLawContentAsync();

        _logger.LogDebug("The length of the normal Context is {Length}", contentAsString.Length);

        var contentAs64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(contentAsString));

        _logger.LogDebug("The length of the 64 Context is {Length}", contentAs64.Length);

        try
        {
            var context = new
            {
                model = $"models/{_modelId}",
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
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Context API response: {Response}", responseString);
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

    private static async Task<string> ReadLawContentAsync()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Context");

        var files = Directory.GetFiles(dir);

        StringBuilder contextBuilder = new StringBuilder();

        int index = 1;
        foreach (var file in files)
        {
            contextBuilder.Append($"Gesetzestext {index} mit dem Dateinamen ");
            contextBuilder.Append(Path.GetFileName(file));
            contextBuilder.AppendLine();
            contextBuilder.Append(await File.ReadAllTextAsync(file));
            contextBuilder.AppendLine();
            contextBuilder.AppendLine();
            index++;
        }

        return contextBuilder.ToString();
    }
    private void WriteResponseToFile(string response)
    {
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
        _logger.LogDebug("Entering {Method}", nameof(GetContextKey));
        var contextUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";
        var response = await _httpClient.GetAsync(contextUri);
        var responseString = await response.Content.ReadAsStringAsync();

        string? result = ExtractContextKey(responseString);

        if (result == null)
        {
            _logger.LogDebug("Couldn't find a Context Key, creating a new context");
            result = ExtractContextKey(await SendContext());
        }

        return result;
    }
    private string? ExtractContextKey(string responseString)
    {
        return JsonSerializer.Deserialize<GeminiContext>(responseString)?
            .CachedContents?
            .FirstOrDefault()?.Name;

    }
    #endregion
}
