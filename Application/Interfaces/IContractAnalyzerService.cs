using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.Interfaces;

public interface IContractAnalyzerService
{
    Task<T> ProcessContractRequest<T>(
        FileContentDto content,
        Func<string, string, string> promptBuilder,
        object? schema,
        bool detectKv = false,
        string? kvName = null);
}
