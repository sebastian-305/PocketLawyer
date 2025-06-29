using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using PocketLawyer.Service;
using System.Text.Json;
using PocketLawyer;

namespace PocketLawyer.Infrastructure;

// Erkennt den passenden Kollektivvertrag ueber eine KI-Abfrage
public class KvAiDetectionService : IKvDetectionService
{
    private readonly ILogger<KvAiDetectionService> _logger;
    private readonly IKollektivvertragService _kvService;
    private readonly IAiService _aiService;

    // KI-Service (hier Claude) und Service zum Lesen der KV-Dateien werden injiziert
    public KvAiDetectionService(
        ILogger<KvAiDetectionService> logger,
        IKollektivvertragService kvService,
        [FromKeyedServices("claude")] IAiService aiService)
    {
        _logger = logger;
        _kvService = kvService;
        _aiService = aiService;
    }

    // Sendet den Vertragstext an die KI und wertet mehrfache Antworten aus
    public async Task<KvDetectionResult> DetectAsync(string contractContent)
    {
        _logger.LogDebug("Entering {Method}", nameof(DetectAsync));

        string[] kvsFolders = _kvService.GetKvsDirectories();

        // Prompt fuer die KV-Erkennung erstellen
        string prompt = PromptBuilder.BuildKvDetectionPrompt(contractContent, kvsFolders);

        // Mehrfach anfragen um ein stabileres Ergebnis zu erhalten
        var tasks = Enumerable
            .Range(0, 3)
            .Select(_ => _aiService.SendPrompt(prompt, null, false))
            .ToArray();

        string[] answers = await Task.WhenAll(tasks);

        string[] kvNames = answers
            .Select(ExtractKvName)
            .ToArray();

        bool allEqual = kvNames.Distinct().Count() == 1;
        _logger.LogDebug("Detection answers all equal: {AllEqual}", allEqual);

        string result = kvNames
            .GroupBy(a => a)
            .OrderByDescending(g => g.Count())
            .First().Key;

        return new KvDetectionResult
        {
            KvName = result,
            AllEqual = allEqual
        };
    }

    // Hilfsfunktion um aus der KI-Antwort den KV-Namen zu extrahieren
    private string ExtractKvName(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<KvDetectionAnswer>(json)?.Kollektivvertrag ?? json;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse KV detection answer: {Json}", json);
            return json;
        }
    }
}
