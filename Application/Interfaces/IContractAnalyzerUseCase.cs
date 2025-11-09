using PocketLawyer.Domain.Dtos;
using PocketLawyer.Web;

namespace PocketLawyer.Application.Interfaces;

public interface IContractAnalyzerUseCase
{
    Task<T> ProcessContractRequest<T> (
        FileContentDto content,
        Func<AgentType, string, string, string> promptBuilder,
        object schema,
        bool detectKv = false,
        string? kvName = null);
}
