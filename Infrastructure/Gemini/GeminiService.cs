
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Chatbot_in.Infrastructure.Gemini;
using Microsoft.Extensions.Primitives;
using PocketLawyer.Service;

namespace Chatbot_in.Domain.Service;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiService> _logger;
    private readonly bool _writeInFile;

    public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = configuration["Gemini:Key"] ?? "";
        _model = "gemini-1.5-flash-001";
        _logger = logger;
        _writeInFile = configuration.GetValue<bool>("WriteGeminiResponseInFile");
    }


    //Die Mindestanzahl der Eingabetokens für das Kontext-Caching beträgt 32.768 -- Für unsren Use-Case overkill
    //Deshalb nicht fertig implementiert, aber so wie sie ist schon funktionsfähig
    public async Task<string> SendContext()
    {
        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/cachedContents?key={_apiKey}";

        var contentAsString = ReadFiles();

        _logger.LogDebug("The length of the normal Context is {0}", contentAsString.Length);

        var contentAs64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(contentAsString));

        _logger.LogDebug("The length of the 64 Context is {0}", contentAs64.Length);

        try
        {


            var context = new
            {
                model = "models/gemini-1.5-flash-001",
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
                    text = "Du bist ein juristischer Experte, der Arbeitsverträge auf Basis des österreichischen Arbeitsrechts analysieren kann. Im Kontext befinden sich die relevanten Gesetzestexte auf die du dich beziehst."
                }
                    }
                },
                ttl = "300s"
                // Fügen Sie hier ggf. weitere optionale Felder mit Standardwerten hinzu, z.B. safetySettings
            };

            var jsonContent = JsonSerializer.Serialize(context);
            //_logger.LogDebug(jsonContent);
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


    public async Task<string> SendPrompt(string prompt, List<KeyValuePair<string, string>> files)
    {
        StringBuilder promptBuilder = new StringBuilder();

        //promptBuilder.Append("You are an expert in analyzing Texts and answering Questions. I have the following question that might be in another language:\n\n");
        promptBuilder.Append(prompt);
        promptBuilder.Append("Bitte verwende keine Markdownformatierung, * or #");
        promptBuilder.Append("Formatiere deine Antwort in HTML");
        promptBuilder.Append("\n\nBitte begründe auch deine Antworten");

        if (files.Count > 0)
        {
            //promptBuilder.Append("Answer it considering" + (files.Count > 1 ? "these files" : "this file"));
            //promptBuilder.Append(" that might be in another language");
            promptBuilder.Append("---------------------------------");

            foreach (var file in files)
            {
                promptBuilder.Append("\n\nFilename: ");
                promptBuilder.Append(file.Key);
                promptBuilder.Append("\n\nVertragsinhalt: \n");
                promptBuilder.Append(file.Value);
                promptBuilder.Append("---------------------------------");
            }
            //promptBuilder.Append("\n\nI gave you now the question and all the files in textform for the answer.");
        }

        //promptBuilder.Append("Please answer the question in the language the question is written.");


        _logger.LogInformation(promptBuilder.ToString());
        return await SendPrompt(promptBuilder.ToString());
    }

    public async Task<string> SendPrompt(string userMessage)
    {
        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";




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
                        text = userMessage
                    }
                },
        //        role = "user"
            },

        },
            generationConfig = new
            {
                response_mime_type = "application/json",
                response_schema = new
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
                                beginn = new { type = "string", format = "date-time" },
                                dauer = new { type = "string" },
                                gehalt_brutto = new { type = "string" },
                                zuschlaege = new { type = "string" },
                                arbeitsstunden = new { type = "string" },
                                ueberstunden = new { type = "string" },
                                probezeit = new { type = "string" }
                            },
                            required = new[]{
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
                }

            }

            //      cachedContent = "cachedContents/fsowom1ynsmg",
        };



        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");


        try
        {
            var response = await _httpClient.PostAsync(requestUri, content);

            var responseString = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<GeminiReponse>(responseString);

            _logger.LogDebug(responseString);

            response.EnsureSuccessStatusCode();

            string resultText = result?.candidates[0]?.content?.parts[0]?.text ?? String.Empty;

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


    public static string ReadFiles()
    {
        var dir = Directory.GetCurrentDirectory() + @"\Infrastructure\Context";

        var files = Directory.GetFiles(dir);

        List<string> contents = new();

        string context = "";


        foreach (var file in files)
        {
            context += "Erster Gesetzestext mit dem Dateinamen " + file + "\n";
            context += File.ReadAllText(file);
            context += "\n\n";
        }


        return context;
    }

    public void WriteResponseInFile(string response)
    {
     
        _logger.LogDebug("Writing Response to file");

        string folder = Directory.GetCurrentDirectory();

        folder += @"\Responses\";

        folder += Directory.GetFiles(folder).Length + ".txt";

        _logger.LogDebug("Filename: {name} Content Length: {length}", folder.Split(@"\").Last(), response.Length);

        using (StreamWriter sr = new StreamWriter(folder))
        {
            sr.Write(response);
        }
    }
}
