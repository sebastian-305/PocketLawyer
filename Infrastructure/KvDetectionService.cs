using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using PocketLawyer.Service;
using System.Text;
using System.Text.Json;
using PocketLawyer;

namespace PocketLawyer.Infrastructure;

public class KvDetectionService : IKvDetectionService
{
    private readonly ILogger<KvDetectionService> _logger;
    private readonly IKollektivvertragService _kvService;
    private readonly IAiService _aiService;

    public KvDetectionService(
        ILogger<KvDetectionService> logger,
        IKollektivvertragService kvService,
        [FromKeyedServices("claude")] IAiService aiService)
    {
        _logger = logger;
        _kvService = kvService;
        _aiService = aiService;
    }

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
