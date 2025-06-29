using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PocketLawyer.Domain.Services;
using PocketLawyer.Service;
using PocketLawyer;

namespace PocketLawyer.Infrastructure;

public class ContractAnalyzerService : IContractAnalyzerService
{
    private readonly ContractAnalyzer _analyzer;
    private readonly IAiService _aiService;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    public ContractAnalyzerService(
        ContractAnalyzer analyzer,
        [FromKeyedServices("gemini")] IAiService aiService,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService)
    {
        _analyzer = analyzer;
        _aiService = aiService;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
    }

    public Task<T> ProcessContractRequest<T>(
        FileContentDto content,
        Func<string, string, string> promptBuilder,
        object schema,
        bool useAltKey,
        bool detectKv = false,
        string? kvName = null)
    {
        return _analyzer.ProcessAsync<T>(
            content,
            promptBuilder,
            schema,
            _aiService,
            _kvService,
            _kvDetectionService,
            useAltKey,
            detectKv,
            kvName);
    }

}
