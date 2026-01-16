using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.Context;
using PocketLawyer.Options;
using System.Diagnostics;

namespace PocketLawyer.Application.Agents;

/// <summary>
/// Contract analysis agent for employment contracts (Arbeitsverträge).
/// Implements the 3-phase lifecycle for analyzing employment contracts.
/// This is a skeleton implementation to be completed later.
/// </summary>
public class EmploymentContractAgent : ContractAnalysisAgentBase
{
    private readonly IContextProvider _contextProvider;
    private readonly IKvDetectionService _kvDetectionService;
    private readonly IKollektivvertragService _kvService;
    private readonly ContextOptions _contextOptions;
    private string? _detectedKvName;
    private string? _detectedKvText;
    private string? _kvDetectionMessage;

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
    /// <param name="kvDetectionService">Service for detecting Kollektivvertrag</param>
    /// <param name="kvService">Service for loading Kollektivvertrag content</param>
    /// <param name="contextOptions">Configuration options for context handling</param>
    /// <param name="logger">Logger for diagnostic information</param>
    public EmploymentContractAgent(
        [FromKeyedServices("nebius")] IAiClient aiClient,
        ISchemaProvider schemaProvider,
        ISchemaValidator schemaValidator,
        IContextProvider contextProvider,
        IKvDetectionService kvDetectionService,
        IKollektivvertragService kvService,
        IOptions<ContextOptions> contextOptions,
        ILogger<EmploymentContractAgent> logger)
        : base(aiClient, schemaProvider, schemaValidator, logger)
    {
        _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        _kvDetectionService = kvDetectionService ?? throw new ArgumentNullException(nameof(kvDetectionService));
        _kvService = kvService ?? throw new ArgumentNullException(nameof(kvService));
        _contextOptions = contextOptions?.Value ?? throw new ArgumentNullException(nameof(contextOptions));
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

            // Step 4: Detect Kollektivvertrag
            _logger.LogDebug("[Agent {AgentId}] Detecting Kollektivvertrag", AgentId);
            try
            {
                var kvDetectionResult = await _kvDetectionService.DetectAsync(contractText);
                
                // Store detection message
                _kvDetectionMessage = kvDetectionResult.Message;
                
                // Log detection details
                _logger.LogInformation("[Agent {AgentId}] KV Detection Result: {Message}", AgentId, kvDetectionResult.Message);
                _logger.LogInformation("[Agent {AgentId}] Detected KVs from attempts: [{KvList}]", 
                    AgentId, string.Join(", ", kvDetectionResult.DetectedKvs));
                
                if (!string.IsNullOrWhiteSpace(kvDetectionResult.KvName))
                {
                    _detectedKvName = kvDetectionResult.KvName;
                    _logger.LogInformation("[Agent {AgentId}] Kollektivvertrag detected: {KvName} (AllEqual: {AllEqual})", 
                        AgentId, _detectedKvName, kvDetectionResult.AllEqual);
                    
                    // Load KV text content
                    var kvContents = await _kvService.GetKvFileContentsAsync(_detectedKvName);
                    if (kvContents != null && kvContents.Length > 0)
                    {
                        _detectedKvText = string.Join("\n\n", kvContents);
                        _logger.LogInformation("[Agent {AgentId}] KV text loaded: {Length} characters from {FileCount} files", 
                            AgentId, _detectedKvText.Length, kvContents.Length);
                    }
                    else
                    {
                        _logger.LogWarning("[Agent {AgentId}] KV detected but no content found for: {KvName}", AgentId, _detectedKvName);
                    }
                }
                else
                {
                    _logger.LogInformation("[Agent {AgentId}] No Kollektivvertrag detected: {Message}", 
                        AgentId, kvDetectionResult.Message);
                }
            }
            catch (Exception kvEx)
            {
                // KV detection failure should not prevent initialization
                _logger.LogWarning(kvEx, "[Agent {AgentId}] KV detection failed, continuing without KV", AgentId);
                _kvDetectionMessage = "Fehler bei der Kollektivvertragserkennung";
            }

            // Step 5: Store contract text on success
            _contractText = contractText;
            
            // Step 6: Set IsInitialized to true
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
    /// Orchestrates all basic analysis steps and combines results.
    /// </summary>
    public override async Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync()
    {
        // Ensure agent is initialized
        EnsureInitialized();

        _logger.LogInformation("[Agent {AgentId}] Starting Basic Analysis for Employment Contract", AgentId);

        // Execute all basic analysis steps in parallel
        var generalTask = AnalyzeGeneralEmploymentDataAsync();
        var compensationTask = AnalyzeCompensationAsync();
        var workingConditionsTask = AnalyzeWorkingConditionsAsync();
        var summaryTask = AnalyzeBasicSummaryAsync();

        // Wait for all tasks to complete
        await Task.WhenAll(generalTask, compensationTask, workingConditionsTask, summaryTask);

        // Get results (errors are already handled in individual methods)
        var generalData = await generalTask;
        var compensationData = await compensationTask;
        var workingConditionsData = await workingConditionsTask;
        var summaryData = await summaryTask;

        // Create the combined result
        var result = new EmploymentBasicAnalysisDto
        {
            General = generalData,
            Compensation = compensationData,
            WorkingConditions = workingConditionsData,
            Summary = summaryData
        };

        // Create metadata
        var metadata = new AnalysisMetadataDto(
            ContractId: Guid.NewGuid().ToString(),
            CountryCode: "AT",
            Language: "de",
            AnalyzedAt: DateTimeOffset.UtcNow,
            IsPreview: false,
            ModelVersion: "1.0"
        );

        _logger.LogInformation("[Agent {AgentId}] Basic Analysis completed successfully", AgentId);
        
        // Log token summary
        LogTokenSummary();

        return new AnalysisDto<IAnalysisDto>(result, metadata);
    }

    /// <summary>
    /// Phase 3: Analysis - executes premium analysis steps in parallel.
    /// Orchestrates all premium analysis steps and combines results.
    /// </summary>
    public override async Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync()
    {
        // Ensure agent is initialized
        EnsureInitialized();

        _logger.LogInformation("[Agent {AgentId}] Starting Premium Analysis for Employment Contract", AgentId);

        // Execute all premium analysis steps in parallel
        var compensationBenefitsTask = AnalyzeCompensationBenefitsClausesAsync();
        var workingTimeVacationTask = AnalyzeWorkingTimeVacationAsync();
        var terminationNoticeTask = AnalyzeTerminationNoticeAsync();
        var nonCompeteConfidentialityTask = AnalyzeNonCompeteConfidentialityAsync();
        var rightsDutiesTask = AnalyzeRightsDutiesAsync();
        var summaryTask = AnalyzePremiumSummaryAsync();

        // Wait for all tasks to complete
        await Task.WhenAll(
            compensationBenefitsTask, 
            workingTimeVacationTask, 
            terminationNoticeTask, 
            nonCompeteConfidentialityTask, 
            rightsDutiesTask, 
            summaryTask);

        // Get results (errors are already handled in individual methods)
        var compensationBenefitsData = await compensationBenefitsTask;
        var workingTimeVacationData = await workingTimeVacationTask;
        var terminationNoticeData = await terminationNoticeTask;
        var nonCompeteConfidentialityData = await nonCompeteConfidentialityTask;
        var rightsDutiesData = await rightsDutiesTask;
        var (gesamtfazit, gesamtbewertung) = await summaryTask;

        // Create the combined result
        var result = new EmploymentPremiumAnalysisDto
        {
            CompensationBenefits = compensationBenefitsData,
            WorkingTimeVacation = workingTimeVacationData,
            TerminationNotice = terminationNoticeData,
            NonCompeteConfidentiality = nonCompeteConfidentialityData,
            RightsDuties = rightsDutiesData,
            Summary = new EmploymentPremiumSummaryDto(
                Gesamtfazit: gesamtfazit,
                Gesamtbewertung: gesamtbewertung,
                Handlungsempfehlungen: Array.Empty<string>()  // TODO: Implement recommendations
            )
        };

        // Create metadata
        var metadata = new AnalysisMetadataDto(
            ContractId: Guid.NewGuid().ToString(),
            CountryCode: "AT",
            Language: "de",
            AnalyzedAt: DateTimeOffset.UtcNow,
            IsPreview: false,
            ModelVersion: "1.0"
        );

        _logger.LogInformation("[Agent {AgentId}] Premium Analysis completed successfully", AgentId);
        
        // Log token summary
        LogTokenSummary();

        return new AnalysisDto<IAnalysisDto>(result, metadata);
    }

    // ============================================================================
    // STUB METHODS WITH CONTEXTSTEP ATTRIBUTES
    // These methods are placeholders for future implementation
    // ============================================================================

    /// <summary>
    /// Analyzes general employment data (employer, employee, position, start date).
    /// </summary>
    [ContextStep("general-employment-data")]
    private async Task<EmploymentBasicGeneralDto> AnalyzeGeneralEmploymentDataAsync()
    {
        var stepName = "AnalyzeGeneralEmploymentDataAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Basic", "General");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "General");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Basic", "General");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for general-employment-data: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Basic_General",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<EmploymentBasicGeneralDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            // All string fields contain "Fehler" for consistent error detection
            return new EmploymentBasicGeneralDto(
                Arbeitgeber: "Fehler bei der Analyse der allgemeinen Arbeitsdaten",
                Arbeitnehmer: "Fehler - Die allgemeinen Arbeitsdaten konnten nicht analysiert werden.",
                Position: "Fehler",
                Startdatum: "Fehler",
                Vertragsart: "Fehler",
                Probezeit: "Fehler"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes compensation (salary, bonuses, benefits).
    /// </summary>
    [ContextStep("compensation")]
    private async Task<EmploymentBasicCompensationDto> AnalyzeCompensationAsync()
    {
        var stepName = "AnalyzeCompensationAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Basic", "Compensation");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "Compensation");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Basic", "Compensation");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for compensation: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Basic_Compensation",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<EmploymentBasicCompensationDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            return new EmploymentBasicCompensationDto(
                GehaltBrutto: "Fehler bei der Analyse der Vergütung",
                GehaltNettoCa: "Fehler - Die Vergütungsdaten konnten nicht analysiert werden.",
                Zahlungsweise: "Fehler",
                Sonderzahlungen: "Fehler",
                Zuschlaege: "Fehler",
                Zusatzleistungen: new[] { "Fehler" }
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes working conditions (hours, location, remote work).
    /// </summary>
    [ContextStep("working-conditions")]
    private async Task<EmploymentBasicWorkingConditionsDto> AnalyzeWorkingConditionsAsync()
    {
        var stepName = "AnalyzeWorkingConditionsAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Basic", "WorkingConditions");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "WorkingConditions");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Basic", "WorkingConditions");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for working-conditions: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Basic_WorkingConditions",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<EmploymentBasicWorkingConditionsDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            return new EmploymentBasicWorkingConditionsDto(
                Wochenarbeitszeit: "Fehler bei der Analyse der Arbeitsbedingungen",
                Arbeitszeitmodell: "Fehler - Die Arbeitsbedingungen konnten nicht analysiert werden.",
                Ueberstundenregelung: "Fehler",
                Urlaubstage: "Fehler",
                Arbeitsort: "Fehler",
                RemoteArbeit: "Fehler"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes basic summary including detected KV name.
    /// </summary>
    [ContextStep("basic-summary")]
    private async Task<EmploymentBasicSummaryDto> AnalyzeBasicSummaryAsync()
    {
        var stepName = "AnalyzeBasicSummaryAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Basic", "Summary");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "Summary");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Basic", "Summary");

            // Add KV information to prompt if available
            if (!string.IsNullOrWhiteSpace(_detectedKvName))
            {
                prompt += $"\n\n## Erkannter Kollektivvertrag\n\n{_detectedKvName}";
                prompt += $"\n\n**WICHTIG für das Feld 'kollektivvertrag'**: Verwende EXAKT den folgenden Text: \"{_detectedKvName}\"";
                _logger.LogDebug("[Agent {AgentId}] Including detected KV in summary: {KvName}", AgentId, _detectedKvName);
            }
            else
            {
                var kvMessage = _kvDetectionMessage ?? "Kein Kollektivvertrag erkannt";
                prompt += $"\n\n## Kollektivvertrag\n\n{kvMessage}";
                prompt += $"\n\n**WICHTIG für das Feld 'kollektivvertrag'**: Verwende EXAKT den folgenden Text: \"{kvMessage}\"";
                _logger.LogDebug("[Agent {AgentId}] No KV detected for summary: {Message}", AgentId, kvMessage);
            }

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for basic-summary: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Basic_Summary",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<EmploymentBasicSummaryDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            // Include KV information even in error case
            var kvInfo = !string.IsNullOrWhiteSpace(_detectedKvName) 
                ? _detectedKvName 
                : (_kvDetectionMessage ?? "Kein Kollektivvertrag erkannt");
            
            return new EmploymentBasicSummaryDto(
                Kurzfazit: "Fehler bei der Analyse der Zusammenfassung",
                Kollektivvertrag: kvInfo,
                Gesamtbewertung: "Fehler - Die Zusammenfassung konnte nicht erstellt werden.",
                RiskLevel: "Gelb"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes compensation and benefits clauses with KV context.
    /// </summary>
    [ContextStep("compensation-benefits-clauses", Tier = "Premium")]
    private async Task<PremiumSectionDto> AnalyzeCompensationBenefitsClausesAsync()
    {
        var stepName = "AnalyzeCompensationBenefitsClausesAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Premium", "CompensationBenefits");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "CompensationBenefits");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Premium", "CompensationBenefits");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for compensation-benefits-clauses: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Add KV text as additional context if available and enabled
            prompt = AddKvContextIfEnabled(prompt);

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Premium_CompensationBenefits",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<PremiumSectionDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            return new PremiumSectionDto(
                Zusammenfassung: "Fehler bei der Analyse der Vergütungs- und Zusatzleistungsklauseln",
                Bewertung: "Die Analyse der Vergütungs- und Zusatzleistungsregelungen konnte nicht durchgeführt werden.",
                Rechtsgrundlage: "Fehler",
                RisikoScore: "Nachteilhaft",
                RiskLevel: "Gelb"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes working time and vacation clauses with KV context.
    /// </summary>
    [ContextStep("working-time-vacation", Tier = "Premium")]
    private async Task<PremiumSectionDto> AnalyzeWorkingTimeVacationAsync()
    {
        var stepName = "AnalyzeWorkingTimeVacationAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Premium", "WorkingTimeVacation");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "WorkingTimeVacation");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Premium", "WorkingTimeVacation");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for working-time-vacation: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Add KV text as additional context if available and enabled
            prompt = AddKvContextIfEnabled(prompt);

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Premium_WorkingTimeVacation",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<PremiumSectionDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            return new PremiumSectionDto(
                Zusammenfassung: "Fehler bei der Analyse der Arbeitszeit- und Urlaubsklauseln",
                Bewertung: "Die Analyse der Arbeitszeit- und Urlaubsregelungen konnte nicht durchgeführt werden.",
                Rechtsgrundlage: "Fehler",
                RisikoScore: "Nachteilhaft",
                RiskLevel: "Gelb"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes termination and notice period clauses with KV context.
    /// </summary>
    [ContextStep("termination-notice", Tier = "Premium")]
    private async Task<PremiumSectionDto> AnalyzeTerminationNoticeAsync()
    {
        var stepName = "AnalyzeTerminationNoticeAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Premium", "TerminationNotice");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "TerminationNotice");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Premium", "TerminationNotice");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for termination-notice: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Add KV text as additional context if available and enabled
            prompt = AddKvContextIfEnabled(prompt);

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Premium_TerminationNotice",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<PremiumSectionDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            return new PremiumSectionDto(
                Zusammenfassung: "Fehler bei der Analyse der Kündigungs- und Kündigungsfristklauseln",
                Bewertung: "Die Analyse der Kündigungs- und Kündigungsfristregelungen konnte nicht durchgeführt werden.",
                Rechtsgrundlage: "Fehler",
                RisikoScore: "Nachteilhaft",
                RiskLevel: "Gelb"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes non-compete and confidentiality clauses with KV context.
    /// </summary>
    [ContextStep("non-compete-confidentiality", Tier = "Premium")]
    private async Task<PremiumSectionDto> AnalyzeNonCompeteConfidentialityAsync()
    {
        var stepName = "AnalyzeNonCompeteConfidentialityAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Premium", "NonCompeteConfidentiality");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "NonCompeteConfidentiality");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Premium", "NonCompeteConfidentiality");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for non-compete-confidentiality: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Add KV text as additional context if available and enabled
            prompt = AddKvContextIfEnabled(prompt);

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Premium_NonCompeteConfidentiality",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<PremiumSectionDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            return new PremiumSectionDto(
                Zusammenfassung: "Fehler bei der Analyse der Wettbewerbs- und Vertraulichkeitsklauseln",
                Bewertung: "Die Analyse der Wettbewerbs- und Vertraulichkeitsregelungen konnte nicht durchgeführt werden.",
                Rechtsgrundlage: "Fehler",
                RisikoScore: "Nachteilhaft",
                RiskLevel: "Gelb"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes rights and duties clauses with KV context.
    /// </summary>
    [ContextStep("rights-duties", Tier = "Premium")]
    private async Task<PremiumSectionDto> AnalyzeRightsDutiesAsync()
    {
        var stepName = "AnalyzeRightsDutiesAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Premium", "RightsDuties");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "RightsDuties");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Premium", "RightsDuties");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for rights-duties: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Add KV text as additional context if available and enabled
            prompt = AddKvContextIfEnabled(prompt);

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Premium_RightsDuties",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<PremiumSectionDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            return new PremiumSectionDto(
                Zusammenfassung: "Fehler bei der Analyse der Rechte- und Pflichtenklauseln",
                Bewertung: "Die Analyse der Rechte- und Pflichtenregelungen konnte nicht durchgeführt werden.",
                Rechtsgrundlage: "Fehler",
                RisikoScore: "Nachteilhaft",
                RiskLevel: "Gelb"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes premium summary.
    /// Returns a tuple of (Gesamtfazit, Gesamtbewertung) strings.
    /// </summary>
    [ContextStep("premium-summary", Tier = "Premium")]
    private async Task<(string Gesamtfazit, string Gesamtbewertung)> AnalyzePremiumSummaryAsync()
    {
        var stepName = "AnalyzePremiumSummaryAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Employment", "Premium", "Summary");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "Summary");
            var prompt = Domain.PromptProvider.GetPrompt("Employment", "Premium", "Summary");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
                _logger.LogDebug("[Agent {AgentId}] Context loaded for premium-summary: {ContextLength} chars", 
                    AgentId, context.Length);
            }

            // Add KV text as additional context if available and enabled
            prompt = AddKvContextIfEnabled(prompt);

            // Call AI service
            var aiResponse = await _aiClient.SendPromptWithMetadata(prompt + "\n\nVertragstext:\n" + _contractText, schemaBinary);
            TrackTokens(aiResponse);

            // Parse and validate response
            var jsonDoc = System.Text.Json.JsonDocument.Parse(aiResponse.Content);
            var responseObject = System.Text.Json.Nodes.JsonObject.Create(jsonDoc.RootElement);

            if (responseObject == null)
            {
                throw new Application.Exceptions.AnalysisException("AI response could not be parsed as JSON object");
            }

            // Validate against schema
            if (!_schemaValidator.Validate(schemaObject, responseObject))
            {
                var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
                throw new Application.Exceptions.SchemaValidationException(
                    "AI response failed schema validation for Employment_Premium_Summary",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<EmploymentPremiumSummaryDto>(aiResponse.Content);
            if (result == null)
            {
                throw new Application.Exceptions.AnalysisException("Failed to deserialize AI response to DTO");
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            
            // Return tuple of (Gesamtfazit, Gesamtbewertung)
            return (result.Gesamtfazit, result.Gesamtbewertung);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error tuple with customer-friendly messages
            return (
                Gesamtfazit: "Fehler bei der Analyse der Premium-Zusammenfassung",
                Gesamtbewertung: "Die Premium-Zusammenfassung konnte nicht erstellt werden."
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }
    
    /// <summary>
    /// Adds KV context to prompt if available and enabled in configuration.
    /// </summary>
    private string AddKvContextIfEnabled(string prompt)
    {
        if (!_contextOptions.IncludeKvContentInAnalysis)
        {
            _logger.LogDebug("[Agent {AgentId}] KV content inclusion is disabled in configuration", AgentId);
            return prompt;
        }
        
        if (!string.IsNullOrWhiteSpace(_detectedKvText))
        {
            prompt += $"\n\n## Kollektivvertrag Context\n\n{_detectedKvText}";
            _logger.LogDebug("[Agent {AgentId}] KV context added: {KvLength} chars", AgentId, _detectedKvText.Length);
        }
        
        return prompt;
    }
}
