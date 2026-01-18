namespace PocketLawyer.Domain.Dtos.Shared;

/// <summary>
/// Base interface for all analysis DTOs.
/// </summary>
public interface IAnalysisDto
{
    /// <summary>
    /// The type of contract being analyzed.
    /// </summary>
    string ContractType { get; }

    /// <summary>
    /// The analysis tier (Basic or Premium).
    /// </summary>
    string Tier { get; }
}
