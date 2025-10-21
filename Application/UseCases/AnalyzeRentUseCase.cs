using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure;
using PocketLawyer.Web;
using System.Text;
using System.Text.Json;

namespace PocketLawyer.Application.UseCases;

public class AnalyzeRentUseCase : IContractAnalyzerAgent
{
    private readonly ILogger<AnalyzeRentUseCase> _logger;
    private readonly IAiClient _aiService;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;
    private readonly IWebHostEnvironment _environment;


    public AnalyzeRentUseCase(
        ILogger<AnalyzeRentUseCase> logger,
        [FromKeyedServices("nebius")] IAiClient aiService,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _aiService = aiService;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
        _environment = environment;
    }

    public async Task<T> ProcessContractRequest<T>(
        FileContentDto content,
        Func<AgentType, string, string, string> promptBuilder,
        object schema,
        bool detectKv = false,
        string? kvName = null)
    {
        _logger.LogDebug("Entering {Method}", nameof(ProcessContractRequest));

        string prompt = promptBuilder(AgentType.Rent, content.FileName, content.FileContent);



        _logger.LogDebug(typeof(T).ToString());

        if (typeof(IAnalysisDto).IsAssignableFrom(typeof(T)))
        {
            prompt += "Beachte folgende Gesetzte: \n";
            prompt += await ReadLawContentAsync();
            _logger.LogDebug("Prompt length: {f}", prompt.Length);
        }
        string answer = await _aiService.SendPrompt(prompt, schema);

        if (answer.Equals("Error"))
            throw new Exception("There was a Problem with the Ai Requet");

        T? dto;
        try
        {
            dto = JsonSerializer.Deserialize<T>(answer);
            _logger.LogDebug(dto?.ToString() ?? "Empty");

        }
        catch (JsonException ex)
        {
            string truncated = answer.Length > 200 ? answer[..200] + "..." : answer;
            _logger.LogError(ex, "Failed to parse AI response: {Snippet}", truncated);
            throw new AnalysisException($"Fehler beim Interpretieren der KI-Antwort: {truncated}", "deserialization", ex);
        }
        if (dto == null)
            throw new AnalysisException("Invalid AI response", "deserialization");


        return dto!;
    }

    private async Task<string> ReadLawContentAsync()
    {
        _logger.LogDebug("Entering {Method}", nameof(ReadLawContentAsync));
        var dir = Path.Combine(_environment.ContentRootPath, DefaultPaths.Context);
        _logger.LogDebug("Directory: {dir}", dir);
        if (!Directory.Exists(dir))
        {
            _logger.LogWarning("Context directory {Dir} does not exist.", dir);
            return string.Empty;
        }
        var files = Directory.GetFiles(dir);
        StringBuilder contextBuilder = new StringBuilder();
        int index = 1;
        foreach (var file in files)
        {
            contextBuilder.Append($"Gesetzestext {index++} mit dem Dateinamen ");
            contextBuilder.Append(Path.GetFileName(file));
            contextBuilder.AppendLine();
            contextBuilder.Append(await File.ReadAllTextAsync(file));
            contextBuilder.AppendLine();
            contextBuilder.AppendLine();
        }
        return contextBuilder.ToString();
    }

}
