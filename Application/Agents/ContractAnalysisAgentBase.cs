using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.Agents;

/// <summary>
/// Abstract base class for contract analysis agents implementing the 3-phase lifecycle:
/// 1. Construction: Lightweight creation with dependency injection only
/// 2. Initialization: Contract validation, type checking, and text storage
/// 3. Analysis: Execution of basic and premium analysis steps
/// </summary>
public abstract class ContractAnalysisAgentBase : IContractAnalysisAgent
{
    protected readonly IAiClient _aiClient;
    protected readonly ISchemaProvider _schemaProvider;
    protected readonly ISchemaValidator _schemaValidator;
    protected readonly ILogger _logger;

    protected string? _contractText;
    protected bool _isInitialized;

    // Token tracking properties
    protected int TotalInputTokens { get; private set; }
    protected int TotalOutputTokens { get; private set; }
    protected int TotalTokens => TotalInputTokens + TotalOutputTokens;

    /// <summary>
    /// Unique identifier for this agent instance, used for logging and tracking.
    /// </summary>
    public string AgentId { get; }

    /// <summary>
    /// Indicates whether the agent has been successfully initialized.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// The expected contract type for this agent (e.g., "Mietvertrag", "Arbeitsvertrag").
    /// Must be overridden by concrete implementations.
    /// </summary>
    protected abstract string ExpectedContractType { get; }

    /// <summary>
    /// Constructor for dependency injection.
    /// Phase 1: Construction - only injects dependencies, no contract logic.
    /// </summary>
    protected ContractAnalysisAgentBase(
        IAiClient aiClient,
        ISchemaProvider schemaProvider,
        ISchemaValidator schemaValidator,
        ILogger logger)
    {
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
        _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
        _schemaValidator = schemaValidator ?? throw new ArgumentNullException(nameof(schemaValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _isInitialized = false;
        
        // Generate unique agent ID for logging and tracking
        AgentId = Guid.NewGuid().ToString("N")[..8]; // Use first 8 characters for readability
        
        _logger.LogInformation("Agent created with ID: {AgentId}, Type: {AgentType}", 
            AgentId, GetType().Name);
    }

    /// <summary>
    /// Phase 2: Initialization - validates and stores contract text.
    /// Must be implemented by concrete agents to perform contract-specific validation.
    /// </summary>
    public abstract Task<InitializationResult> InitializeAsync(string contractText);

    /// <summary>
    /// Phase 3: Analysis - executes basic analysis steps in parallel.
    /// Must be implemented by concrete agents to define analysis steps.
    /// </summary>
    public abstract Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync();

    /// <summary>
    /// Phase 3: Analysis - executes premium analysis steps in parallel.
    /// Must be implemented by concrete agents to define analysis steps.
    /// </summary>
    public abstract Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync();

    /// <summary>
    /// Validates that the agent has been initialized before analysis.
    /// Throws InvalidOperationException if not initialized.
    /// </summary>
    protected void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException(
                "Agent must be initialized before performing analysis. Call InitializeAsync first.");
        }
    }

    /// <summary>
    /// Tracks token usage from an AI response.
    /// Aggregates input and output tokens for the entire analysis.
    /// </summary>
    /// <param name="response">The AI response containing token information</param>
    protected void TrackTokens(AiResponse response)
    {
        TotalInputTokens += response.InputTokens;
        TotalOutputTokens += response.OutputTokens;
    }

    /// <summary>
    /// Logs a summary of token usage for the analysis.
    /// Should be called at the end of AnalyzeBasicAsync or AnalyzePremiumAsync.
    /// </summary>
    protected void LogTokenSummary()
    {
        _logger.LogInformation(
            "[Agent {AgentId}] Analysis completed. Total Input Tokens: {InputTokens}, Total Output Tokens: {OutputTokens}, Total Tokens: {TotalTokens}",
            AgentId, TotalInputTokens, TotalOutputTokens, TotalTokens);
    }
}
