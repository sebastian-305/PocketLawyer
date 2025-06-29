using PocketLawyer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure;

// Zentrale Logik fuer die Kommunikation mit dem KI-Dienst.
// Erstellt aus einem Vertragstext einen Prompt und wertet die Antwort aus.
public class ContractAnalyzerService : IContractAnalyzerService
{
    private readonly ILogger<ContractAnalyzerService> _logger;
    private readonly IAiService _aiService;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    // Alle benoetigten Services werden injiziert. Vor allem der KI-Service
    // (hier Gemini) kann so spaeter leicht ausgetauscht werden.
    public ContractAnalyzerService(
        ILogger<ContractAnalyzerService> logger,
        [FromKeyedServices("gemini")] IAiService aiService,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService)
    {
        _logger = logger;
        _aiService = aiService;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
    }

    // Baut den Prompt zusammen, ruft den KI-Dienst auf und
    // wandelt die Antwort in ein DTO des Typs T um.
    public async Task<T> ProcessContractRequest<T>(
        FileContentDto content,
        Func<string, string, string> promptBuilder,
        object schema,
        bool useAltKey,
        bool detectKv = false,
        string? kvName = null)
    {
        // Grund-Prompt fuer die KI erstellen
        string prompt = promptBuilder(content.FileName, content.FileContent);

        // Optional kann hier ein passender Kollektivvertrag erkannt
        // und an den Prompt angehaengt werden.
        if (!string.IsNullOrWhiteSpace(kvName) || detectKv)
        {
            try
            {
                if (detectKv && string.IsNullOrWhiteSpace(kvName))
                {
                    // KI nach dem passenden KV fragen
                    KvDetectionResult kvResult = await _kvDetectionService.DetectAsync(content.FileContent);
                    _logger.LogInformation("KV detection result: {Result}", kvResult.KvName);

                    if (kvResult.AllEqual && kvResult.KvName != "Keiner" && kvResult.KvName != "Multiple")
                    {
                        kvName = kvResult.KvName;
                    }
                }

                if (!string.IsNullOrWhiteSpace(kvName))
                {
                    // Alle Dateien des erkannten Kollektivvertrags lesen
                    string[] kvFiles = await _kvService.GetKvFileContentsAsync(kvName);
                    var sbPrompt = new StringBuilder(prompt);
                    sbPrompt.AppendLine();
                    sbPrompt.AppendLine($"Der folgende Text enthält den dazugehörigen Kollektivvertrag \"{kvName}\" und mögliche Anhänge. " +
                        $"Diese Informationen sind laut Gesetz zwingend Grundlage der Vertragsanalyse. Wenn der Vertrag in einem Punkt so etwas schreibt wie \"laut Kollektivvertrag\" soll die entsprechende Regelung" +
                        $"aus dem Kollektivvetrag angegeben werden mit dem Zusatz \"(lt. Kollektivvertrag)\"");
                    sbPrompt.AppendLine(string.Join("\n\n", kvFiles));
                    prompt = sbPrompt.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kollektivvertrag detection failed");
            }
        }

        // Anfrage an den KI-Dienst schicken
        string answer = await _aiService.SendPrompt(prompt, schema, useAltKey);

        // Antwort in unser Datenmodell ueberfuehren
        T? dto;
        try
        {
            dto = JsonSerializer.Deserialize<T>(answer);
        }
        catch (JsonException ex)
        {
            string truncated = answer.Length > 200 ? answer[..200] + "..." : answer;
            _logger.LogError(ex, "Failed to parse AI response: {Snippet}", truncated);
            throw new InvalidOperationException($"Fehler beim Interpretieren der KI-Antwort: {truncated}", ex);
        }
        if (dto == null)
            throw new InvalidOperationException("Invalid AI response");

        // Erkenntnis ueber den KV ins Ergebnis schreiben
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

        return dto;
    }

}
