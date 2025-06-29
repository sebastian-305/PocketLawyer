using Microsoft.Extensions.Logging;

// Führt eine Kollektivvertragserkennung mittels KI durch.
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using PocketLawyer.Application.Interfaces;
using System.Text.Json;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Infrastructure;

// Nutzt mehrfache KI-Anfragen, um den wahrscheinlichsten KV zu bestimmen.
public class KvAiDetectionService : IKvDetectionService
{
    private readonly ILogger<KvAiDetectionService> _logger;
    private readonly IKollektivvertragService _kvService;
    private readonly IAiClient _aiService;

    public KvAiDetectionService(
        ILogger<KvAiDetectionService> logger,
        IKollektivvertragService kvService,
        [FromKeyedServices("claude")] IAiClient aiService)
    {
        _logger = logger;
        _kvService = kvService;
        _aiService = aiService;
    }

    public async Task<KvDetectionResult> DetectAsync(string contractContent)
    {
        _logger.LogDebug("Entering {Method}", nameof(DetectAsync));

        // 1. Laden verfügbarer KV-Verzeichnisse
        string[] kvsFolders = _kvService.GetKvsDirectories();

        // 2. Erstellen des Prompt-Strings
        string prompt = PromptBuilder.BuildKvDetectionPrompt(contractContent, kvsFolders);

        // 3. Parallele KI-Anfragen (z.B. drei Stück)
        var tasks = Enumerable
            .Range(0, 3)
            .Select(_ => _aiService.SendPrompt(prompt, null, false))
            .ToArray();

        string[] answers = await Task.WhenAll(tasks);

        // 4. Auswertung der Antworten und Majority Vote
        string[] kvNames = answers
            .Select(ExtractKvName)
            .ToArray();

        bool allEqual = kvNames.Distinct().Count() == 1;
        _logger.LogDebug("Detection answers all equal: {AllEqual}", allEqual);

        string result = kvNames
            .GroupBy(a => a)
            .OrderByDescending(g => g.Count())
            .First().Key;

        // 5. Aufbau des KvDetectionResult
        return new KvDetectionResult
        {
            KvName = result,
            AllEqual = allEqual
        };
    }

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
