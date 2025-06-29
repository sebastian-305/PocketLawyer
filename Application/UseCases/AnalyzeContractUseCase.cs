using System.Text;

// Zentrale Logik um einen Arbeitsvertrag mit KI auszuwerten.
using System.Text.Json;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure;

namespace PocketLawyer.Application.UseCases;

// Baut Prompts auf, ruft die KI auf und interpretiert deren Antworten.
public class AnalyzeContractUseCase : IContractAnalyzerService
{
    private readonly ILogger<AnalyzeContractUseCase> _logger;
    private readonly IAiClient _aiService;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    public AnalyzeContractUseCase(
        ILogger<AnalyzeContractUseCase> logger,
        [FromKeyedServices("gemini")] IAiClient aiService,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService)
    {
        _logger = logger;
        _aiService = aiService;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
    }

    // Führt je nach Parameter eine Voranalyse oder Vollanalyse durch.
    public async Task<T> ProcessContractRequest<T>(
        FileContentDto content,
        Func<string, string, string> promptBuilder,
        object schema,
        bool useAltKey,
        bool detectKv = false,
        string? kvName = null)
    {
        // Prompt-Erstellung: erzeugt aus Dateiname und Inhalt einen KI-tauglichen Text
        string prompt = promptBuilder(content.FileName, content.FileContent);

        if (!string.IsNullOrWhiteSpace(kvName) || detectKv)
        {
            // Optionale KV-Erkennung und Einbindung in das Prompt
            try
            {
                if (detectKv && string.IsNullOrWhiteSpace(kvName))
                {
                    KvDetectionResult kvResult = await _kvDetectionService.DetectAsync(content.FileContent);
                    _logger.LogInformation("KV detection result: {Result}", kvResult.KvName);

                    if (kvResult.AllEqual && kvResult.KvName != "Keiner" && kvResult.KvName != "Multiple")
                    {
                        kvName = kvResult.KvName;
                    }
                }

                if (!string.IsNullOrWhiteSpace(kvName))
                {
                    string[] kvFiles = await _kvService.GetKvFileContentsAsync(kvName);
                    prompt = PromptBuilder.AddKvInfo(prompt, kvName, kvFiles);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kollektivvertrag detection failed");
            }
        }

        // Aufruf des KI-Dienstes mit Haupt- oder Voranalyse
        string answer = await _aiService.SendPrompt(prompt, schema, useAltKey);

        T? dto;
        try
        {
            // JSON-Deserialisierung der KI-Antwort
            dto = JsonSerializer.Deserialize<T>(answer);
        }
        catch (JsonException ex)
        {
            // Fehlerbehandlung bei ungültigem JSON
            string truncated = answer.Length > 200 ? answer[..200] + "..." : answer;
            _logger.LogError(ex, "Failed to parse AI response: {Snippet}", truncated);
            throw new AnalysisException($"Fehler beim Interpretieren der KI-Antwort: {truncated}", "deserialization", ex);
        }
        if (dto == null)
            throw new AnalysisException("Invalid AI response", "deserialization");

        // Einfügen des KV-Namens ins Ergebnis
        if (!string.IsNullOrWhiteSpace(kvName))
        {
            switch (dto)
            {
                case ContractValidationDto v:
                    v.Kollektivvertrag = kvName;
                    break;
                case ContractAnalysisDto a:
                    a.Kollektivvertrag = kvName;
                    break;
            }
        }

        return dto!;
    }
}
