using PocketLawyer;
namespace PocketLawyer.Service;

public interface IContractAnalyzerService
{
    Task<T> ProcessContractRequest<T>(
        FileContentDto content,
        Func<string, string, string> promptBuilder,
        object schema,
        bool useAltKey,
        bool detectKv = false,
        string? kvName = null);
}
