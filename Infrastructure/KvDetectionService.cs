using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using PocketLawyer.Service;
using System.Text;
using System.Text.Json;
using PocketLawyer;

namespace PocketLawyer.Infrastructure;

// Nutzt einen KI-Dienst, um zu ermitteln, welchem Kollektivvertrag ein Vertrag
// höchstwahrscheinlich zuzuordnen ist.

public class KvDetectionService : IKvDetectionService
{
    // Logger für Fehlersuche
    private readonly ILogger<KvDetectionService> _logger;
    // Zugriff auf die verfügbaren Kollektivverträge
    private readonly IKollektivvertragService _kvService;
    // KI-Dienst (hier Claude), der die Erkennung durchführt
    private readonly IAiService _aiService;

    // Konstruktor, dem die Abhängigkeiten vom DI-Container übergeben werden
    public KvDetectionService(
        ILogger<KvDetectionService> logger,
        IKollektivvertragService kvService,
        [FromKeyedServices("claude")] IAiService aiService)
    {
        _logger = logger;
        _kvService = kvService;
        _aiService = aiService;
    }

    // Sendet den Vertragstext mehrfach an den KI-Dienst und fasst die Antworten
    // zusammen, um den wahrscheinlichsten KV-Namen zu ermitteln.
    public async Task<KvDetectionResult> DetectAsync(string contractContent)
    {
        _logger.LogDebug("Entering {Method}", nameof(DetectAsync));

        string[] kvsFolders = await _kvService.GetKvsDirectoriesAsync();

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Arbeitsvertrag:");
        promptBuilder.AppendLine(contractContent);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Kollektivverträge:");
        promptBuilder.AppendLine(string.Join(", ", kvsFolders));
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Finde heraus, ob einer der bereits übermittelten Kollektivverträge zu dem Arbeitsvertrag passt. Schreibe Multiple wenn mehrere Kollektivverträge passen würden.");
        promptBuilder.AppendLine("Falls keiner Kollektivvertrag ausfindig gemacht werden kann oder wenn keiner der Kollegtivveträge in der übermittelten Liste zum Arbeitsvertrag passt.");
        promptBuilder.AppendLine("Antworte im JSON-Format {\"kollektivvertrag\":\"NAME Oder Multiple ODER Keiner\"}.");
        promptBuilder.AppendLine("Der Wortlaut deiner Antwort solle exakt dem Wortlaut der übermittelten Namen der Kollektivverträge entsprechen, weil diese danach in einem Programm verarbeitet werden.");

        var tasks = Enumerable
            .Range(0, 3)
            .Select(_ => _aiService.SendPrompt(promptBuilder.ToString(), null, false))
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

    // Versucht, das von der KI gelieferte JSON zu parsen und die Property
    // "kollektivvertrag" auszulesen. Schlägt das fehl, wird der rohe String
    // zurückgegeben, damit der Aufrufer damit umgehen kann.
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
