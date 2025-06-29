using Microsoft.AspNetCore.Http;
namespace PocketLawyer.Service;

public interface IContractAnalyzerService
{
    Task<T> ProcessContractRequest<T>(
        IFormFile file,
        Func<string, string, string> promptBuilder,
        object schema,
        bool useAltKey,
        bool detectKv = false,
        string? kvName = null);
}
