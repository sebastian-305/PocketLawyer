using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.AiServices.Nebius;
using PocketLawyer.Web;
using System.Text.Json;

namespace PocketLawyer.Application.Agents;

public class AnalyzeEmploymentAgent : IContractAnalyzerAgent
{
    private readonly ILogger<AnalyzeEmploymentAgent> _logger;
    private readonly IAiClient _aiService;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    public AnalyzeEmploymentAgent(
        ILogger<AnalyzeEmploymentAgent> logger,
        [FromKeyedServices("nebius")] IAiClient aiService,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService)
    {
        _logger = logger;
        _aiService = aiService;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
    }

    public async Task<T> ProcessContractRequest<T>(FileContentDto content, AgentAction action, object? data = null)
    {
        Func<AgentType, string, string, string> promptBuilder;
        object schema;
        bool detectKv = false;
        string? kvName = ((ContractValidationDto?)data)?.Kollektivvertrag;

        _logger.LogDebug("Entering {Method}", nameof(ProcessContractRequest));

        switch (action)
        {
            case AgentAction.Validate:
                promptBuilder = PromptBuilder.BuildValidationPrompt;
                schema = NebiusService.PreAnalyzerSchemaDefinition;
                break;
            case AgentAction.Analyze:
                promptBuilder = PromptBuilder.BuildEmploymentAnalysisPrompt;
                schema = NebiusService.EmploymentAnalyzerSchemaDefinition;
                break;
            default:
                throw new Exception("Action unknown not valid");
        }

        string prompt = promptBuilder(AgentType.Employment, content.FileName, content.FileContent);

        if (!string.IsNullOrWhiteSpace(kvName) || detectKv)
        {
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

        string answer = await _aiService.SendPrompt(prompt, schema);

      object? dto;
        try
        {
            switch (action)
            {
                case AgentAction.Validate:
                    dto = JsonSerializer.Deserialize<T>(answer);
                    break;
                case AgentAction.Analyze:
                    dto = JsonSerializer.Deserialize<EmploymentContractAnalysisDto>(answer);
                    break;
                default:
                    throw new Exception("Action unknown not valid");
            }


        }
        catch (JsonException ex)
        {
            string truncated = answer.Length > 200 ? answer[..200] + "..." : answer;
            _logger.LogError(ex, "Failed to parse AI response: {Snippet}", truncated);
            throw new AnalysisException($"Fehler beim Interpretieren der KI-Antwort: {truncated}", "deserialization", ex);
        }
        if (dto == null)
            throw new AnalysisException("Invalid AI response", "deserialization");

        if (!string.IsNullOrWhiteSpace(kvName))
        {
            switch (dto)
            {
                case ContractValidationDto v:
                    v.Kollektivvertrag = kvName;
                    break;
                case EmploymentContractAnalysisDto a:
                    a.Kollektivvertrag = kvName;
                    break;
            }
        }

        return (T)dto!;
    }
}
