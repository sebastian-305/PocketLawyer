using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure.KvDetection;

public class KvDetectionService : IKvDetectionService
{
    private readonly IAiService _aiService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<KvDetectionService> _logger;

    public KvDetectionService(
        [FromKeyedServices("claude")] IAiService aiService,
        IWebHostEnvironment environment,
        ILogger<KvDetectionService> logger)
    {
        _aiService = aiService;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> DetectKvAsync(string contractText)
    {
        ArgumentNullException.ThrowIfNull(contractText);

        string kvsPath = Path.Combine(
            _environment.ContentRootPath,
            "Infrastructure",
            "KVS");

        string[] kvsFolders = Directory.GetDirectories(kvsPath)
            .Select(name => Path.GetFileName(name) ?? "")
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();

        _logger.LogDebug("KVs: {KV}", string.Join(", ", kvsFolders));

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Arbeitsvertrag:");
        promptBuilder.AppendLine(contractText);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Kollektivverträge:");
        promptBuilder.AppendLine(string.Join(", ", kvsFolders));
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Finde heraus, ob einer der bereits übermittelten Kollektivverträge zu dem Arbeitsvertrag passt. Schreibe Multiple wenn mehrere Kollektivverträge passen würden.");
        promptBuilder.AppendLine("Falls keiner Kollektivvertrag ausfindig gemacht werden kann oder wenn keiner der Kollegtivveträge in der übermittelten Liste zum Arbeitsvertrag passt.");
        promptBuilder.AppendLine("Antworte im JSON-Format {\"kollektivvertrag\":\"NAME Oder Multiple ODER Keiner\"}.");
        promptBuilder.AppendLine("Der Wortlaut deiner Antwort solle exakt dem Wortlaut der übermittelten Namen der Kollektivverträge entsprechen, weil diese danach in einem Programm verarbeitet werden.");

        try
        {
            return await _aiService.SendPrompt(promptBuilder.ToString(), null, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI service failed while processing the prompt");
            throw new AiServiceException("Kollektivvertrag detection failed", ex);
        }
    }
}
