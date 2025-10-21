using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure;
using PocketLawyer.Infrastructure.AiServices.Nebius;
using PocketLawyer.Web;
using System.Text;
using System.Text.Json;

namespace PocketLawyer.Application.Agents;

public class AnalyzeRentAgent : IContractAnalyzerAgent
{
    private readonly ILogger<AnalyzeRentAgent> _logger;
    private readonly IAiClient _aiService;
    private readonly IWebHostEnvironment _environment;


    public AnalyzeRentAgent(
        ILogger<AnalyzeRentAgent> logger,
        [FromKeyedServices("nebius")] IAiClient aiService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _aiService = aiService;
        _environment = environment;
    }

    public async Task<T> ProcessContractRequest<T>(FileContentDto content, AgentAction action, object? data = null)
    {
        Func<AgentType, string, string, string> promptBuilder;
        object schema;

        _logger.LogDebug("Entering {Method}", nameof(ProcessContractRequest));

        switch (action)
        {
            case AgentAction.Validate:
                promptBuilder = PromptBuilder.BuildValidationPrompt;
                schema = NebiusService.PreAnalyzerSchemaDefinition;
                break;
            case AgentAction.Analyze:
                promptBuilder = PromptBuilder.BuildRentAnalysisPrompt;
                schema = NebiusService.AnalyzerSchemaDefinition;
                break;
            default:
                throw new Exception("Action unknown not valid");
        }

        string prompt = promptBuilder(AgentType.Rent, content.FileName, content.FileContent);

        if (action == AgentAction.Analyze)
        {
            prompt += "Beachte folgende Gesetzte: \n";
            prompt += await ReadLawContentAsync();
            _logger.LogDebug("Prompt length: {f} chars", prompt.Length);
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
        var dir = Path.Combine(_environment.ContentRootPath, DefaultPaths.RentContext);
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
