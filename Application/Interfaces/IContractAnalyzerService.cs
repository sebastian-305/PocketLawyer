using PocketLawyer.Domain.Dtos;
using PocketLawyer.Web;

namespace PocketLawyer.Application.Interfaces;

public interface IContractAnalyzerService
{
    Task<T> ProcessContractRequest<T> (
        FileContentDto content,
        Func<AnalysisType, string, string, string> promptBuilder,
        object schema,
        bool detectKv = false,
        string? kvName = null);
}
