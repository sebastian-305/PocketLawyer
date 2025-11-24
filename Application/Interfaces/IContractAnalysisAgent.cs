using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Interface for contract analysis agents that follow a 3-phase lifecycle:
/// 1. Construction (dependency injection only)
/// 2. Initialization (contract validation and storage)
/// 3. Analysis (basic and premium analysis execution)
/// </summary>
public interface IContractAnalysisAgent
{
    /// <summary>
    /// Initializes the agent with contract text.
    /// Validates readability and contract type before storing the text.
    /// </summary>
    /// <param name="contractText">The contract text to analyze</param>
    /// <returns>Result indicating success or failure with error message</returns>
    Task<InitializationResult> InitializeAsync(string contractText);

    /// <summary>
    /// Executes basic analysis on the initialized contract.
    /// Runs multiple analysis steps in parallel.
    /// </summary>
    /// <returns>Analysis DTO containing basic analysis results and metadata</returns>
    Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync();

    /// <summary>
    /// Executes premium analysis on the initialized contract.
    /// Runs multiple analysis steps in parallel, independent of basic analysis.
    /// </summary>
    /// <returns>Analysis DTO containing premium analysis results and metadata</returns>
    Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync();

    /// <summary>
    /// Indicates whether the agent has been successfully initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Unique identifier for this agent instance, used for logging and tracking.
    /// </summary>
    string AgentId { get; }
}
