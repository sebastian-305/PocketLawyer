using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.Context;

namespace PocketLawyer.Application.Agents;

/// <summary>
/// Contract analysis agent for employment contracts (Arbeitsverträge).
/// Implements the 3-phase lifecycle for analyzing employment contracts.
/// This is a skeleton implementation to be completed later.
/// </summary>
public class EmploymentContractAgent : ContractAnalysisAgentBase
{
    private readonly IContextProvider _contextProvider;

    /// <summary>
    /// The expected contract type for this agent.
    /// </summary>
    protected override string ExpectedContractType => "Arbeitsvertrag";

    /// <summary>
    /// Constructor for dependency injection.
    /// Phase 1: Construction - only injects dependencies, no contract logic.
    /// </summary>
    /// <param name="aiClient">AI client for making AI requests (Nebius service)</param>
    /// <param name="schemaProvider">Provider for JSON schemas</param>
    /// <param name="schemaValidator">Validator for AI responses against schemas</param>
    /// <param name="contextProvider">Provider for context files</param>
    /// <param name="logger">Logger for diagnostic information</param>
    public EmploymentContractAgent(
        [FromKeyedServices("nebius")] IAiClient aiClient,
        ISchemaProvider schemaProvider,
        ISchemaValidator schemaValidator,
        IContextProvider contextProvider,
        ILogger<EmploymentContractAgent> logger)
        : base(aiClient, schemaProvider, schemaValidator, logger)
    {
        _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        // Phase 1: Construction complete - only dependency injection, no contract logic
    }

    /// <summary>
    /// Phase 2: Initialization - validates and stores contract text.
    /// Checks contract readability, detects contract type, and stores text on success.
    /// </summary>
    /// <param name="contractText">The contract text to initialize with</param>
    /// <returns>InitializationResult indicating success or failure with error message</returns>
    public override async Task<InitializationResult> InitializeAsync(string contractText)
    {
        try
        {
            _logger.LogInformation("[Agent {AgentId}] Starting initialization for EmploymentContractAgent", AgentId);

            // Validate input
            if (string.IsNullOrWhiteSpace(contractText))
            {
                _logger.LogWarning("[Agent {AgentId}] Initialization failed: Contract text is null or empty", AgentId);
                return new InitializationResult(false, "Vertragstext ist leer oder ungültig");
            }

            // Step 1: Check contract readability using AI
            _logger.LogDebug("[Agent {AgentId}] Checking contract readability", AgentId);
            bool isReadable = await IsReadableAsync(contractText);
            
            if (!isReadable)
            {
                _logger.LogWarning("[Agent {AgentId}] Initialization failed: Contract is not readable", AgentId);
                return new InitializationResult(false, "Das Dokument ist nicht lesbar oder kann nicht verarbeitet werden");
            }

            // Step 2: Detect contract type using AI
            _logger.LogDebug("[Agent {AgentId}] Detecting contract type", AgentId);
            string detectedType = await DetectContractTypeAsync(contractText);
            
            // Step 3: Compare detected type with expected type
            if (!string.Equals(detectedType, ExpectedContractType, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[Agent {AgentId}] Initialization failed: Contract type mismatch. Expected: {Expected}, Detected: {Detected}", AgentId, ExpectedContractType, detectedType);
                return new InitializationResult(
                    false, 
                    $"Falscher Vertragstyp erkannt. Erwartet: {ExpectedContractType}, Erkannt: {detectedType}");
            }

            // Step 4: Store contract text on success
            _contractText = contractText;
            
            // Step 5: Set IsInitialized to true
            _isInitialized = true;

            _logger.LogInformation("[Agent {AgentId}] Initialization successful for EmploymentContractAgent", AgentId);
            
            // Step 6: Return success result
            return new InitializationResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] Initialization failed with exception", AgentId);
            return new InitializationResult(false, "Initialisierung fehlgeschlagen: " + ex.Message);
        }
    }

    /// <summary>
    /// Checks if the contract text is readable using AI.
    /// </summary>
    /// <param name="contractText">The contract text to check</param>
    /// <returns>True if readable, false otherwise</returns>
    private async Task<bool> IsReadableAsync(string contractText)
    {
        try
        {
            var prompt = Domain.PromptProvider.GetReadabilityCheckPrompt();
            
            // Create schema as BinaryData for Nebius
            var schemaJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "object",
                properties = new
                {
                    isReadable = new { type = "boolean" }
                },
                required = new[] { "isReadable" },
                additionalProperties = false
            });
            var schema = BinaryData.FromString(schemaJson);

            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nText:\n" + contractText, schema);
            TrackTokens(aiResponse);
            
            // Parse the JSON response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            if (jsonDoc.RootElement.TryGetProperty("isReadable", out var isReadableElement))
            {
                return isReadableElement.GetBoolean();
            }

            _logger.LogWarning("[Agent {AgentId}] AI response did not contain 'isReadable' field", AgentId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] Error checking contract readability", AgentId);
            throw new Application.Exceptions.ContractValidationException(
                "Fehler bei der Prüfung der Lesbarkeit des Vertrags", 
                "readability_check", 
                ex);
        }
    }

    /// <summary>
    /// Detects the contract type using AI.
    /// </summary>
    /// <param name="contractText">The contract text to analyze</param>
    /// <returns>The detected contract type</returns>
    private async Task<string> DetectContractTypeAsync(string contractText)
    {
        try
        {
            var prompt = Domain.PromptProvider.GetContractTypeDetectionPrompt();
            
            // Create schema as BinaryData for Nebius
            var schemaJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "object",
                properties = new
                {
                    contractType = new { type = "string" }
                },
                required = new[] { "contractType" },
                additionalProperties = false
            });
            var schema = BinaryData.FromString(schemaJson);

            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + contractText, schema);
            TrackTokens(aiResponse);
            
            // Parse the JSON response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            if (jsonDoc.RootElement.TryGetProperty("contractType", out var contractTypeElement))
            {
                return contractTypeElement.GetString() ?? "Unbekannt";
            }

            _logger.LogWarning("[Agent {AgentId}] AI response did not contain 'contractType' field", AgentId);
            return "Unbekannt";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] Error detecting contract type", AgentId);
            throw new Application.Exceptions.ContractValidationException(
                "Fehler bei der Erkennung des Vertragstyps", 
                "type_detection", 
                ex);
        }
    }

    /// <summary>
    /// Phase 3: Analysis - executes basic analysis steps in parallel.
    /// STUB: To be implemented later with employment-specific analysis steps.
    /// </summary>
    public override async Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync()
    {
        // Ensure agent is initialized
        EnsureInitialized();

        _logger.LogInformation("[Agent {AgentId}] Starting Basic Analysis for Employment Contract (STUB)", AgentId);

        // TODO: Implement employment-specific basic analysis steps
        // This will include steps like:
        // - Analyze general employment data (employer, employee, position, start date)
        // - Analyze compensation (salary, bonuses, benefits)
        // - Analyze working conditions (hours, location, remote work)
        // - Analyze summary

        await Task.CompletedTask;

        throw new NotImplementedException(
            "AnalyzeBasicAsync for EmploymentContractAgent is not yet implemented. " +
            "This is a skeleton implementation to be completed later.");
    }

    /// <summary>
    /// Phase 3: Analysis - executes premium analysis steps in parallel.
    /// STUB: To be implemented later with employment-specific premium analysis steps.
    /// </summary>
    public override async Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync()
    {
        // Ensure agent is initialized
        EnsureInitialized();

        _logger.LogInformation("[Agent {AgentId}] Starting Premium Analysis for Employment Contract (STUB)", AgentId);

        // TODO: Implement employment-specific premium analysis steps
        // This will include steps like:
        // - Analyze compensation and benefits clauses
        // - Analyze working time and vacation clauses
        // - Analyze termination and notice period clauses
        // - Analyze non-compete and confidentiality clauses
        // - Analyze rights and duties clauses
        // - Analyze premium summary

        await Task.CompletedTask;

        throw new NotImplementedException(
            "AnalyzePremiumAsync for EmploymentContractAgent is not yet implemented. " +
            "This is a skeleton implementation to be completed later.");
    }

    // ============================================================================
    // STUB METHODS WITH CONTEXTSTEP ATTRIBUTES
    // These methods are placeholders for future implementation
    // ============================================================================

    /// <summary>
    /// STUB: Analyzes general employment data (employer, employee, position, start date).
    /// </summary>
    [ContextStep("general-employment-data")]
    private async Task<object> AnalyzeGeneralEmploymentDataAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeGeneralEmploymentDataAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for general-employment-data: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeGeneralEmploymentDataAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes compensation (salary, bonuses, benefits).
    /// </summary>
    [ContextStep("compensation")]
    private async Task<object> AnalyzeCompensationAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeCompensationAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for compensation: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeCompensationAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes working conditions (hours, location, remote work).
    /// </summary>
    [ContextStep("working-conditions")]
    private async Task<object> AnalyzeWorkingConditionsAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeWorkingConditionsAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for working-conditions: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeWorkingConditionsAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes basic summary.
    /// </summary>
    [ContextStep("basic-summary")]
    private async Task<object> AnalyzeBasicSummaryAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeBasicSummaryAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for basic-summary: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeBasicSummaryAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes compensation and benefits clauses.
    /// </summary>
    [ContextStep("compensation-benefits-clauses")]
    private async Task<object> AnalyzeCompensationBenefitsClausesAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeCompensationBenefitsClausesAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for compensation-benefits-clauses: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeCompensationBenefitsClausesAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes working time and vacation clauses.
    /// </summary>
    [ContextStep("working-time-vacation")]
    private async Task<object> AnalyzeWorkingTimeVacationAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeWorkingTimeVacationAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for working-time-vacation: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeWorkingTimeVacationAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes termination and notice period clauses.
    /// </summary>
    [ContextStep("termination-notice")]
    private async Task<object> AnalyzeTerminationNoticeAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeTerminationNoticeAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for termination-notice: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeTerminationNoticeAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes non-compete and confidentiality clauses.
    /// </summary>
    [ContextStep("non-compete-confidentiality")]
    private async Task<object> AnalyzeNonCompeteConfidentialityAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeNonCompeteConfidentialityAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for non-compete-confidentiality: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeNonCompeteConfidentialityAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes rights and duties clauses.
    /// </summary>
    [ContextStep("rights-duties")]
    private async Task<object> AnalyzeRightsDutiesAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzeRightsDutiesAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for rights-duties: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzeRightsDutiesAsync is not yet implemented");
    }

    /// <summary>
    /// STUB: Analyzes premium summary.
    /// </summary>
    [ContextStep("premium-summary")]
    private async Task<object> AnalyzePremiumSummaryAsync()
    {
        _logger.LogInformation("[Agent {AgentId}] AnalyzePremiumSummaryAsync (STUB)", AgentId);
        
        // Load context for this step
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            _logger.LogDebug("[Agent {AgentId}] Context loaded for premium-summary: {ContextLength} chars", 
                AgentId, context.Length);
        }

        throw new NotImplementedException("AnalyzePremiumSummaryAsync is not yet implemented");
    }
}
