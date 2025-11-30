using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.Context;
using System.Diagnostics;

namespace PocketLawyer.Application.Agents;

/// <summary>
/// Contract analysis agent for rental contracts (Mietverträge).
/// Implements the 3-phase lifecycle for analyzing rental contracts.
/// </summary>
public class RentalContractAgent : ContractAnalysisAgentBase
{
    private readonly IContextProvider _contextProvider;

    /// <summary>
    /// The expected contract type for this agent.
    /// </summary>
    protected override string ExpectedContractType => "Mietvertrag";

    /// <summary>
    /// Constructor for dependency injection.
    /// Phase 1: Construction - only injects dependencies, no contract logic.
    /// </summary>
    /// <param name="aiClient">AI client for making AI requests (Nebius service)</param>
    /// <param name="schemaProvider">Provider for JSON schemas</param>
    /// <param name="schemaValidator">Validator for AI responses against schemas</param>
    /// <param name="contextProvider">Provider for context files</param>
    /// <param name="logger">Logger for diagnostic information</param>
    public RentalContractAgent(
        [FromKeyedServices("nebius")] IAiClient aiClient,
        ISchemaProvider schemaProvider,
        ISchemaValidator schemaValidator,
        IContextProvider contextProvider,
        ILogger<RentalContractAgent> logger)
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
            _logger.LogInformation("[Agent {AgentId}] Starting initialization for RentalContractAgent", AgentId);

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
                _logger.LogWarning(
                    "[Agent {AgentId}] Initialization failed: Contract type mismatch. Expected: {Expected}, Detected: {Detected}",
                    AgentId, ExpectedContractType, detectedType);
                return new InitializationResult(
                    false, 
                    $"Falscher Vertragstyp erkannt. Erwartet: {ExpectedContractType}, Erkannt: {detectedType}");
            }

            // Step 4: Store contract text on success
            _contractText = contractText;
            
            // Step 5: Set IsInitialized to true
            _isInitialized = true;

            _logger.LogInformation("[Agent {AgentId}] Initialization successful for RentalContractAgent", AgentId);
            
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
            // Don't throw - return false to indicate the contract is not readable
            return false;
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
            // Don't throw - return "Unbekannt" to indicate the type could not be detected
            return "Unbekannt";
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

        _logger.LogInformation("[Agent {AgentId}] Starting Basic Analysis for Rental Contract", AgentId);

        // Execute all basic analysis steps in parallel
        var generalTask = AnalyzeGeneralDataAsync();
        var rentTask = AnalyzeRentConditionsAsync();
        var conditionsTask = AnalyzeContractConditionsAsync();
        var summaryTask = AnalyzeSummaryAsync();

        // Wait for all tasks to complete
        await Task.WhenAll(generalTask, rentTask, conditionsTask, summaryTask);

        // Get results (errors are already handled in individual methods)
        var generalData = await generalTask;
        var rentData = await rentTask;
        var conditionsData = await conditionsTask;
        var summaryData = await summaryTask;

        // Create the combined result
        var result = new RentalBasicAnalysisDto
        {
            General = generalData,
            Rent = rentData,
            Conditions = conditionsData,
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
    /// Analyzes general contract data (parties, dates, type).
    /// </summary>
    [ContextStep("general-data")]
    private async Task<RentalBasicGeneralDto> AnalyzeGeneralDataAsync()
    {
        var stepName = "AnalyzeGeneralDataAsync";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Basic", "General");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "General");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Basic", "General");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Basic_General",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<RentalBasicGeneralDto>(aiResponse.Content);
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
            return new RentalBasicGeneralDto(
                Vertragstyp: "Fehler bei der Analyse der allgemeinen Vertragsdaten",
                Mieter: "Fehler - Die allgemeinen Vertragsdaten konnten nicht analysiert werden.",
                Vermieter: "Fehler",
                MietobjektAdresse: "Fehler",
                Vertragsbeginn: "Fehler",
                Vertragsart: "Fehler"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes rent conditions (amounts, payment terms).
    /// </summary>
    [ContextStep("rent-conditions")]
    private async Task<RentalBasicRentDto> AnalyzeRentConditionsAsync()
    {
        var stepName = "AnalyzeRentConditionsAsync";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Basic", "Rent");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Rent");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Basic", "Rent");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Basic_Rent",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<RentalBasicRentDto>(aiResponse.Content);
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
            // String fields contain "Fehler" for consistent error detection
            return new RentalBasicRentDto(
                Kaltmiete: 0,
                Nebenkosten: 0,
                Warmmiete: 0,
                Kaution: 0,
                Zahlungsweise: "Fehler bei der Analyse der Mietkonditionen",
                Nebenkostenregelung: "Fehler - Die Mietkonditionen konnten nicht analysiert werden."
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes contract conditions (termination, pets, subletting).
    /// </summary>
    [ContextStep("contract-conditions")]
    private async Task<RentalBasicConditionsDto> AnalyzeContractConditionsAsync()
    {
        var stepName = "AnalyzeContractConditionsAsync";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Basic", "Conditions");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Conditions");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Basic", "Conditions");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Basic_Conditions",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<RentalBasicConditionsDto>(aiResponse.Content);
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
            return new RentalBasicConditionsDto(
                Kuendigungsfrist: "Fehler bei der Analyse der Vertragsbedingungen",
                Tierhaltung: "Fehler",
                Untervermietung: "Fehler",
                Besonderheiten: "Fehler - Die Vertragsbedingungen konnten nicht analysiert werden."
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Analyzes and creates a summary of the basic analysis.
    /// </summary>
    [ContextStep("summary")]
    private async Task<RentalBasicSummaryDto> AnalyzeSummaryAsync()
    {
        var stepName = "AnalyzeSummaryAsync";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Basic", "Summary");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Summary");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Basic", "Summary");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Basic_Summary",
                    validationResult.Errors);
            }

            // Deserialize to DTO
            var result = System.Text.Json.JsonSerializer.Deserialize<RentalBasicSummaryDto>(aiResponse.Content);
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
            return new RentalBasicSummaryDto(
                Kurzfazit: "Fehler bei der Analyse der Zusammenfassung",
                Gesamtfazit: "Fehler - Die Zusammenfassung konnte nicht erstellt werden.",
                Gesamtbewertung: "Fehler",
                RiskLevel: "Gelb"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Phase 3: Analysis - executes premium analysis steps in parallel.
    /// Orchestrates all premium analysis steps and combines results.
    /// </summary>
    public override async Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync()
    {
        // Ensure agent is initialized
        EnsureInitialized();

        _logger.LogInformation("[Agent {AgentId}] Starting Premium Analysis for Rental Contract", AgentId);

        // Execute all premium analysis steps in parallel
        var costsTask = AnalyzeCostsAsync();
        var termsTask = AnalyzeTermsAsync();
        var useTask = AnalyzeUseAsync();
        var maintenanceTask = AnalyzeMaintenanceAsync();
        var dutiesTask = AnalyzeDutiesAsync();
        var privacyTask = AnalyzePrivacyAsync();
        var summaryTask = AnalyzePremiumSummaryAsync();

        // Wait for all tasks to complete
        await Task.WhenAll(costsTask, termsTask, useTask, maintenanceTask, dutiesTask, privacyTask, summaryTask);

        // Get results (errors are already handled in individual methods)
        var costsData = await costsTask;
        var termsData = await termsTask;
        var useData = await useTask;
        var maintenanceData = await maintenanceTask;
        var dutiesData = await dutiesTask;
        var privacyData = await privacyTask;
        var (gesamtfazit, gesamtbewertung) = await summaryTask;

        // Create the combined result
        var result = new RentalPremiumAnalysisDto
        {
            Costs = costsData,
            Terms = termsData,
            Use = useData,
            Maintenance = maintenanceData,
            Duties = dutiesData,
            Privacy = privacyData,
            Summary = new RentalPremiumSummaryDto(
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

    /// <summary>
    /// Analyzes costs and payment clauses (rent, utilities, deposit).
    /// </summary>
    [ContextStep("costs")]
    private async Task<PremiumSectionDto> AnalyzeCostsAsync()
    {
        var stepName = "AnalyzeCostsAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Premium", "Costs");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Costs");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Premium", "Costs");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Premium_Costs",
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
                Zusammenfassung: "Fehler bei der Analyse der Mietzahlungs- und Nebenkostenklauseln",
                Bewertung: "Die Analyse der Kostenregelungen konnte nicht durchgeführt werden.",
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
    /// Analyzes rental duration and termination clauses.
    /// </summary>
    [ContextStep("terms")]
    private async Task<PremiumSectionDto> AnalyzeTermsAsync()
    {
        var stepName = "AnalyzeTermsAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Premium", "Terms");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Terms");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Premium", "Terms");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Premium_Terms",
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
                Zusammenfassung: "Fehler bei der Analyse der Mietdauer- und Kündigungsklauseln",
                Bewertung: "Die Analyse der Laufzeit- und Kündigungsregelungen konnte nicht durchgeführt werden.",
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
    /// Analyzes usage rights and restrictions clauses.
    /// </summary>
    [ContextStep("use")]
    private async Task<PremiumSectionDto> AnalyzeUseAsync()
    {
        var stepName = "AnalyzeUseAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Premium", "Use");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Use");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Premium", "Use");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Premium_Use",
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
                Zusammenfassung: "Fehler bei der Analyse der Gebrauchsklauseln",
                Bewertung: "Die Analyse der Nutzungsrechte und -pflichten konnte nicht durchgeführt werden.",
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
    /// Analyzes maintenance and repair clauses.
    /// </summary>
    [ContextStep("maintenance")]
    private async Task<PremiumSectionDto> AnalyzeMaintenanceAsync()
    {
        var stepName = "AnalyzeMaintenanceAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Premium", "Maintenance");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Maintenance");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Premium", "Maintenance");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Premium_Maintenance",
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
                Zusammenfassung: "Fehler bei der Analyse der Instandhaltungs- und Reparaturklauseln",
                Bewertung: "Die Analyse der Instandhaltungspflichten konnte nicht durchgeführt werden.",
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
    /// Analyzes rights and duties clauses.
    /// </summary>
    [ContextStep("duties")]
    private async Task<PremiumSectionDto> AnalyzeDutiesAsync()
    {
        var stepName = "AnalyzeDutiesAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Premium", "Duties");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Duties");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Premium", "Duties");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Premium_Duties",
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
                Bewertung: "Die Analyse der Rechte und Pflichten konnte nicht durchgeführt werden.",
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
    /// Analyzes privacy and communication clauses.
    /// </summary>
    [ContextStep("privacy")]
    private async Task<PremiumSectionDto> AnalyzePrivacyAsync()
    {
        var stepName = "AnalyzePrivacyAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Premium", "Privacy");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Privacy");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Premium", "Privacy");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Premium_Privacy",
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
                Zusammenfassung: "Fehler bei der Analyse der Datenschutz- und Kommunikationsklauseln",
                Bewertung: "Die Analyse der Datenschutzbestimmungen konnte nicht durchgeführt werden.",
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
    /// Analyzes and creates a summary of the premium analysis.
    /// </summary>
    [ContextStep("premium-summary")]
    private async Task<(string Gesamtfazit, string Gesamtbewertung)> AnalyzePremiumSummaryAsync()
    {
        var stepName = "AnalyzePremiumSummaryAsync";
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);

            // Get schema (as BinaryData for AI) and JsonObject for validation
            var schemaBinary = _schemaProvider.GetSchema("Rental", "Premium", "Summary");
            var schemaObject = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Summary");
            var prompt = Domain.PromptProvider.GetPrompt("Rental", "Premium", "Summary");

            // Load context for this step
            var context = await _contextProvider.GetContextForCurrentStepAsync();
            if (!string.IsNullOrWhiteSpace(context))
            {
                prompt += $"\n\n## Reference Context\n\n{context}";
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
                    "AI response failed schema validation for Rental_Premium_Summary",
                    validationResult.Errors);
            }

            // Extract gesamtfazit and gesamtbewertung from response
            string gesamtfazit = "Fehler";
            string gesamtbewertung = "Gelb";

            if (jsonDoc.RootElement.TryGetProperty("gesamtfazit", out var fazitElement))
            {
                gesamtfazit = fazitElement.GetString() ?? "Fehler bei der Erstellung des Gesamtfazits";
            }

            if (jsonDoc.RootElement.TryGetProperty("gesamtbewertung", out var bewertungElement))
            {
                gesamtbewertung = bewertungElement.GetString() ?? "Gelb";
            }

            _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
            return (gesamtfazit, gesamtbewertung);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
            
            // Return error result with customer-friendly message
            return (
                "Fehler bei der Erstellung der Gesamtzusammenfassung. Die Gesamtbewertung konnte nicht durchgeführt werden.",
                "Gelb"
            );
        }
        finally
        {
            _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", AgentId, stepName, sw.ElapsedMilliseconds);
        }
    }
}
