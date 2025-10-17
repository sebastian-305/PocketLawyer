using System.Text.Json;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.UseCases;

public class AnalyzeRentUseCase : IContractAnalyzerService
{
    private readonly ILogger<AnalyzeContractUseCase> _logger;
    private readonly IAiClient _aiService;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    public AnalyzeRentUseCase(
        ILogger<AnalyzeContractUseCase> logger,
        [FromKeyedServices("nebius")] IAiClient aiService,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService)
    {
        _logger = logger;
        _aiService = aiService;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
    }

    public async Task<T> ProcessContractRequest<T>(
        FileContentDto content,
        Func<string, string, string> promptBuilder,
        object schema,
        bool detectKv = false,
        string? kvName = null)
    {
        string prompt = promptBuilder(content.FileName, content.FileContent);

        string answer = await _aiService.SendPrompt(prompt, schema);

        T? dto;
        try
        {
            dto = JsonSerializer.Deserialize<T>(answer);
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
}
