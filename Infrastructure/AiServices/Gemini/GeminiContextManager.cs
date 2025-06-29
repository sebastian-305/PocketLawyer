using PocketLawyer.Infrastructure.AiServices.Gemini.Models;
using PocketLawyer.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace PocketLawyer.Infrastructure.AiServices.Gemini;

public class GeminiContextManager : IGeminiContextManager
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelId;
    private readonly ILogger<GeminiContextManager> _logger;

    public GeminiContextManager(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiContextManager> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:Key"] ?? string.Empty;
        _modelId = configuration["Gemini:ModelId"] ?? "gemini-1.5-flash-002";
        _logger = logger;
    }

    public async Task<string?> GetContextKeyAsync()
    {
        _logger.LogDebug("Entering {Method}", nameof(GetContextKeyAsync));
        var contextUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";
        var response = await _httpClient.GetAsync(contextUri);
        var responseString = await response.Content.ReadAsStringAsync();

        string? result = ExtractContextKey(responseString);

        if (result == null)
        {
            _logger.LogDebug("Couldn't find a Context Key, creating a new context");
            result = ExtractContextKey(await SendContextAsync());
        }

        return result;
    }

    public async Task<string> SendContextAsync()
    {
        _logger.LogDebug("Entering {Method}", nameof(SendContextAsync));
        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";

        var contentAsString = await ReadLawContentAsync();
        var contentAs64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(contentAsString));

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

    private static string? ExtractContextKey(string responseString)
    {
        return JsonSerializer.Deserialize<GeminiContext>(responseString)?
            .CachedContents?
            .FirstOrDefault()?.Name;
    }
}
