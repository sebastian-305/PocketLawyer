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
}
