using PocketLawyer.Domain.Dtos;
using PocketLawyer.Web;

namespace PocketLawyer.Application.Interfaces;

public interface IContractAnalyzerAgent
{
    Task<T> ProcessContractRequest<T>(FileContentDto content, AgentAction action, object? data = null);
}

public enum AgentAction
{
    Validate,
    Analyze
}



