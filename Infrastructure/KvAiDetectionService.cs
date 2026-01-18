using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using System.Text.Json;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain;

namespace PocketLawyer.Infrastructure;

public class KvAiDetectionService : IKvDetectionService
{
    private readonly ILogger<KvAiDetectionService> _logger;
    private readonly IKollektivvertragService _kvService;
    private readonly IAiClient _aiService;

    public KvAiDetectionService(
        ILogger<KvAiDetectionService> logger,
        IKollektivvertragService kvService,
        [FromKeyedServices("nebius")] IAiClient aiService)
    {
        _logger = logger;
        _kvService = kvService;
        _aiService = aiService;
    }

    public async Task<KvDetectionResult> DetectAsync(string contractContent)
    {
        _logger.LogDebug("Entering {Method}", nameof(DetectAsync));

        string[] kvsFolders = _kvService.GetKvsDirectories();
        _logger.LogInformation("Available KV directories: {Count}", kvsFolders.Length);
        _logger.LogDebug("KV directories: {KvList}", string.Join(", ", kvsFolders));

        string prompt = Application.PromptProvider.GetKvDetectionPrompt(contractContent, kvsFolders);

        var kvDetectionSchema = GetKvDetectionSchema();

        var tasks = Enumerable
            .Range(0, 2)
            .Select(_ => _aiService.SendPromptWithMetadata(prompt, kvDetectionSchema))
            .ToArray();

        AiResponse[] responses = await Task.WhenAll(tasks);
        string[] answers = responses.Select(r => r.Content).ToArray();

        var parsedAnswers = answers
            .Select(ExtractKvAnswer)
            .ToArray();

        string[] kvNames = parsedAnswers.Select(a => a.Kollektivvertrag).ToArray();
        string[] begruendungen = parsedAnswers.Select(a => a.Begruendung).ToArray();

        // Validate that detected KVs are in the available list
        for (int i = 0; i < kvNames.Length; i++)
        {
            var kvName = kvNames[i];
            if (!string.Equals(kvName, "Multiple", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kvName, "Keiner", StringComparison.OrdinalIgnoreCase) &&
                !kvsFolders.Contains(kvName, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogError("AI returned invalid KV name: {KvName} - This name is NOT in the available list!", kvName);
                _logger.LogError("Available KVs: {KvList}", string.Join(", ", kvsFolders));
                _logger.LogError("AI reasoning: {Begruendung}", begruendungen[i]);
                
                // Replace invalid name with "Keiner"
                kvNames[i] = "Keiner";
                begruendungen[i] = $"FEHLER: AI hat ungültigen KV-Namen '{kvName}' zurückgegeben, der nicht in der Liste ist. Original-Begründung: {begruendungen[i]}";
            }
        }

        // Log what each AI call detected with reasoning
        for (int i = 0; i < kvNames.Length; i++)
        {
            _logger.LogInformation("KV Detection attempt {Attempt}: {KvName}", i + 1, kvNames[i]);
            _logger.LogInformation("  Begründung: {Begruendung}", begruendungen[i]);
        }

        bool allEqual = kvNames.Distinct().Count() == 1;
        _logger.LogInformation("Detection answers all equal: {AllEqual}", allEqual);

        string result = kvNames
            .GroupBy(a => a)
            .OrderByDescending(g => g.Count())
            .First().Key;

        // Check if result is "Multiple" or "Keiner"
        if (string.Equals(result, "Multiple", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Multiple KVs detected, cannot determine unique KV");
            _logger.LogWarning("Detailed reasoning from AI:");
            for (int i = 0; i < begruendungen.Length; i++)
            {
                _logger.LogWarning("  Attempt {Attempt}: {Begruendung}", i + 1, begruendungen[i]);
            }
            
            return new KvDetectionResult
            {
                KvName = string.Empty,
                AllEqual = allEqual,
                DetectedKvs = kvNames,
                Begruendungen = begruendungen,
                Message = "Kollektivvertrag konnte nicht eindeutig identifiziert werden (mehrere mögliche Treffer)"
            };
        }

        if (string.Equals(result, "Keiner", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("No matching KV detected");
            _logger.LogInformation("Reasoning from AI:");
            for (int i = 0; i < begruendungen.Length; i++)
            {
                _logger.LogInformation("  Attempt {Attempt}: {Begruendung}", i + 1, begruendungen[i]);
            }
            
            return new KvDetectionResult
            {
                KvName = string.Empty,
                AllEqual = allEqual,
                DetectedKvs = kvNames,
                Begruendungen = begruendungen,
                Message = "Kein passender Kollektivvertrag gefunden"
            };
        }

        // Valid KV detected
        _logger.LogInformation("KV detected: {KvName}", result);
        _logger.LogInformation("Reasoning from AI:");
        for (int i = 0; i < begruendungen.Length; i++)
        {
            _logger.LogInformation("  Attempt {Attempt}: {Begruendung}", i + 1, begruendungen[i]);
        }
        
        return new KvDetectionResult
        {
            KvName = result,
            AllEqual = allEqual,
            DetectedKvs = kvNames,
            Begruendungen = begruendungen,
            Message = allEqual ? $"Kollektivvertrag eindeutig erkannt: {result}" : $"Kollektivvertrag erkannt (nicht eindeutig): {result}"
        };
    }

    private KvDetectionAnswer ExtractKvAnswer(string json)
    {
        try
        {
            var answer = JsonSerializer.Deserialize<KvDetectionAnswer>(json);
            return answer ?? new KvDetectionAnswer { Kollektivvertrag = json, Begruendung = "Keine Begründung verfügbar" };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse KV detection answer: {Json}", json);
            return new KvDetectionAnswer { Kollektivvertrag = json, Begruendung = "Fehler beim Parsen der Antwort" };
        }
    }

    private static System.BinaryData GetKvDetectionSchema()
    {
        return BinaryData.FromBytes("""
            {
              "type": "object",
              "properties": {
                "kollektivvertrag": {
                  "type": "string",
                  "description": "Der Name des erkannten Kollektivvertrags oder 'Keiner' oder 'Multiple'"
                },
                "begruendung": {
                  "type": "string",
                  "description": "Begründung für die Auswahl des Kollektivvertrags"
                }
              },
              "required": [
                "kollektivvertrag",
                "begruendung"
              ],
              "additionalProperties": false
            }
            """u8.ToArray());
    }
}

