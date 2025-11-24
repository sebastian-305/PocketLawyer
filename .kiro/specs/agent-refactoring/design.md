# Design Document: Agent Refactoring

## Overview

Das Agent-Refactoring-System implementiert eine neue Architektur für Vertragsanalysen basierend auf dem Single-Source-of-Truth-Prinzip. Das System verwendet JSON-Schemas als zentrale Wahrheitsquelle für Datenstrukturen und ermöglicht parallele Ausführung von Analyse-Steps für optimale Performance.

Die Architektur folgt einem 3-Phasen-Modell:
1. **Konstruktion**: Leichtgewichtige Erstellung mit Dependency Injection
2. **Initialisierung**: Pflichtschritt zur Vertragsvalidierung und -speicherung
3. **Analyse**: Unabhängige Basic- und Premium-Analysen

## Architecture

### High-Level Architecture

```
┌─────────────┐
│  Frontend   │
└──────┬──────┘
       │ WebSocket
       │ (Upload + ContractType)
       ▼
┌─────────────────────────────────────┐
│     WebSocket Handler               │
│  - Receive File + ContractType      │
│  - Validate & Decode File           │
│  - Create Agent (Factory)           │
└──────┬──────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────┐
│     Contract Analysis Agent         │
│  Phase 1: Construction (DI only)    │
│  Phase 2: Initialize(contractText)  │
│  Phase 3: AnalyzeBasic/Premium()    │
└──────┬──────────────────────────────┘
       │
       ├──────────────┬──────────────┐
       ▼              ▼              ▼
┌──────────┐   ┌──────────┐   ┌──────────┐
│ Schema   │   │ Nebius   │   │ Prompt   │
│ Provider │   │ AI Client│   │ Provider │
└──────────┘   └──────────┘   └──────────┘
```

### Agent Lifecycle

```
┌──────────────┐
│ Construction │  ← Inject: IAiClient, ISchemaProvider, ISchemaValidator, WebSocket
└──────┬───────┘
       │
       ▼
┌──────────────┐
│Initialize()  │  ← contractText
│  - Check     │
│  - Validate  │
│  - Store     │
│  - Set Ready │
└──────┬───────┘
       │
       ├─────────────┬─────────────┐
       ▼             ▼             ▼
┌──────────┐  ┌──────────┐  ┌──────────┐
│ Analyze  │  │ Analyze  │  │  ...     │
│ Basic()  │  │Premium() │  │          │
└──────────┘  └──────────┘  └──────────┘
```

## Components and Interfaces

### Core Interfaces

```csharp
// Agent Interface
public interface IContractAnalysisAgent
{
    Task<InitializationResult> InitializeAsync(string contractText);
    Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync();
    Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync();
    bool IsInitialized { get; }
}

// Initialization Result
public record InitializationResult(
    bool Success,
    string? ErrorMessage = null
);

// Base Agent
public abstract class ContractAnalysisAgentBase : IContractAnalysisAgent
{
    protected readonly IAiClient _aiClient;
    protected readonly ISchemaProvider _schemaProvider;
    protected readonly ISchemaValidator _schemaValidator;
    protected readonly ILogger _logger;
    
    protected string? _contractText;
    protected bool _isInitialized;
    
    public bool IsInitialized => _isInitialized;
    
    protected ContractAnalysisAgentBase(
        IAiClient aiClient,
        ISchemaProvider schemaProvider,
        ISchemaValidator schemaValidator,
        ILogger logger)
    {
        _aiClient = aiClient;
        _schemaProvider = schemaProvider;
        _schemaValidator = schemaValidator;
        _logger = logger;
        _isInitialized = false;
    }
    
    public abstract Task<InitializationResult> InitializeAsync(string contractText);
    public abstract Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync();
    public abstract Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync();
    
    protected abstract string ExpectedContractType { get; }
}

// Concrete Agent Example
public class RentalContractAgent : ContractAnalysisAgentBase
{
    protected override string ExpectedContractType => "Mietvertrag";
    
    public override async Task<InitializationResult> InitializeAsync(string contractText)
    {
        try
        {
            // Validate readability
            if (!await IsReadableAsync(contractText))
                return new InitializationResult(false, "Dokument nicht lesbar");
                
            // Detect contract type
            string detectedType = await DetectContractTypeAsync(contractText);
            if (detectedType != ExpectedContractType)
                return new InitializationResult(false, $"Erwarteter Typ: {ExpectedContractType}, Erkannt: {detectedType}");
                
            // Store and mark ready
            _contractText = contractText;
            _isInitialized = true;
            
            return new InitializationResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initialization failed");
            return new InitializationResult(false, "Initialisierung fehlgeschlagen");
        }
    }
    
    public override async Task<AnalysisDto<BasicRentalAnalysisDto>> AnalyzeBasicAsync()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Agent not initialized");
            
        // Execute steps in parallel
        var tasks = new[]
        {
            AnalyzeGeneralDataAsync(),
            AnalyzeRentConditionsAsync(),
            AnalyzeContractConditionsAsync()
        };
        
        var results = await Task.WhenAll(tasks);
        
        // Combine results
        return CreateBasicAnalysisDto(results);
    }
    
    private async Task<RentalGeneralDataDto> AnalyzeGeneralDataAsync()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var schema = _schemaProvider.GetSchema("Rental", "Basic", "General");
            var prompt = PromptProvider.GetPrompt("Rental", "Basic", "General");
            var response = await _aiClient.CallAsync(prompt, _contractText, schema);
            
            if (!_schemaValidator.Validate(schema, response))
                return CreateErrorResult<RentalGeneralDataDto>("Validierung fehlgeschlagen");
                
            return JsonSerializer.Deserialize<RentalGeneralDataDto>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AnalyzeGeneralDataAsync");
            return CreateErrorResult<RentalGeneralDataDto>("Fehler bei der Analyse der allgemeinen Daten");
        }
        finally
        {
            _logger.LogInformation("AnalyzeGeneralDataAsync completed in {Duration}ms", sw.ElapsedMilliseconds);
        }
    }
}
```

### Schema Provider

```csharp
// Individual Schema Providers (one per analysis step)
public sealed class RentalGeneralSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["vertragstyp"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Typ des Mietvertrags, z. B. 'I', 'II', 'III', 'WG'.",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-bold",
                    ["importance"] = "high"
                }
            },
            ["mieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Mieter:in.",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary"
                }
            },
            // ... weitere Felder
        },
        ["required"] = new JsonArray("vertragstyp", "mieter", "vermieter", "mietobjektAdresse", "vertragsbeginn", "vertragsart")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
    public BinaryData GetSchemaBinaryData() => BinaryData.FromString(_schema.ToJsonString());
}

// Central Schema Provider (Factory)
public interface ISchemaProvider
{
    BinaryData GetSchema(string contractType, string tier, string step);
    JsonObject GetSchemaForEndpoint(string contractType, string tier, string step);
}

public class SchemaProvider : ISchemaProvider
{
    private readonly Dictionary<string, Func<BinaryData>> _schemaFactories;
    
    public SchemaProvider()
    {
        // Register schema providers
        _schemaFactories = new Dictionary<string, Func<BinaryData>>
        {
            ["Rental_Basic_General"] = () => new RentalGeneralSchemaProvider().GetSchemaBinaryData(),
            ["Rental_Basic_Rent"] = () => new RentalRentConditionsSchemaProvider().GetSchemaBinaryData(),
            ["Rental_Basic_Conditions"] = () => new RentalContractConditionsSchemaProvider().GetSchemaBinaryData(),
            ["Rental_Premium_Costs"] = () => new ClauseAnalysisSchemaProvider().GetSchemaBinaryData(),
            // ... weitere Schemas
        };
    }
    
    public BinaryData GetSchema(string contractType, string tier, string step)
    {
        string key = $"{contractType}_{tier}_{step}";
        if (!_schemaFactories.ContainsKey(key))
            throw new SchemaNotFoundException(key);
            
        return _schemaFactories[key]();
    }
    
    public JsonObject GetSchemaForEndpoint(string contractType, string tier, string step)
    {
        string key = $"{contractType}_{tier}_{step}";
        if (!_schemaFactories.ContainsKey(key))
            throw new SchemaNotFoundException(key);
            
        // Get the provider and return JsonObject for frontend
        return key switch
        {
            "Rental_Basic_General" => new RentalGeneralSchemaProvider().GetSchema(),
            "Rental_Basic_Rent" => new RentalRentConditionsSchemaProvider().GetSchema(),
            // ... weitere Mappings
            _ => throw new SchemaNotFoundException(key)
        };
    }
}
```

### Schema Validator

```csharp
public interface ISchemaValidator
{
    bool Validate(JsonObject schema, JsonObject response);
    ValidationResult ValidateWithDetails(JsonObject schema, JsonObject response);
}

public class SchemaValidator : ISchemaValidator
{
    public bool Validate(JsonObject schema, JsonObject response)
    {
        // Use JSON Schema validation library (e.g., NJsonSchema)
        var validator = JsonSchema.FromJsonAsync(schema.ToString()).Result;
        var errors = validator.Validate(response.ToString());
        return errors.Count == 0;
    }
    
    public ValidationResult ValidateWithDetails(JsonObject schema, JsonObject response)
    {
        var validator = JsonSchema.FromJsonAsync(schema.ToString()).Result;
        var errors = validator.Validate(response.ToString());
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors.Select(e => e.ToString()).ToList()
        };
    }
}
```

### Prompt Provider

```csharp
public static class PromptProvider
{
    #region System Prompts
    
    private const string SystemPrompt = @"
Du bist ein Experte für Vertragsanalyse. 
Analysiere den Vertrag präzise und strukturiert.
Antworte ausschließlich im angegebenen JSON-Format.
";
    
    #endregion
    
    #region Rental Contract Prompts
    
    #region Basic Analysis
    
    private const string RentalBasicGeneralPrompt = @"
Analysiere die allgemeinen Vertragsdaten des Mietvertrags.
Extrahiere: Vertragstyp, Mieter, Vermieter, Adresse, Beginn, Befristung.
";
    
    private const string RentalBasicRentPrompt = @"
Analysiere die Mietkonditionen des Mietvertrags.
Extrahiere: Kaltmiete, Nebenkosten, Warmmiete, Kaution, Zahlungsweise.
";
    
    private const string RentalBasicConditionsPrompt = @"
Analysiere die Vertragsbedingungen des Mietvertrags.
Extrahiere: Kündigungsfrist, Tierhaltung, Untervermietung, Besonderheiten.
";
    
    #endregion
    
    #region Premium Analysis
    
    private const string RentalPremiumCostsPrompt = @"
Analysiere die Mietzahlungs- und Nebenkostenklauseln rechtlich.
Bewerte: Zulässigkeit, Risiken, Rechtsgrundlagen.
";
    
    // ... weitere Premium-Prompts
    
    #endregion
    
    #endregion
    
    #region Employment Contract Prompts
    
    // ... Employment-Prompts
    
    #endregion
    
    public static string GetPrompt(string contractType, string tier, string step)
    {
        string key = $"{contractType}{tier}{step}";
        return key switch
        {
            "RentalBasicGeneral" => SystemPrompt + RentalBasicGeneralPrompt,
            "RentalBasicRent" => SystemPrompt + RentalBasicRentPrompt,
            "RentalBasicConditions" => SystemPrompt + RentalBasicConditionsPrompt,
            "RentalPremiumCosts" => SystemPrompt + RentalPremiumCostsPrompt,
            // ... weitere Mappings
            _ => throw new PromptNotFoundException(key)
        };
    }
}
```

### WebSocket Handler

```csharp
public class WebSocketAnalysisHandler
{
    private readonly ILogger<WebSocketAnalysisHandler> _logger;
    private readonly IFileValidatorService _fileValidator;
    private readonly IServiceProvider _serviceProvider;
    
    public async Task HandleAsync(HttpContext context)
    {
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        
        try
        {
            // Receive file + contract type
            var (fileBytes, fileName, contractType) = await ReceiveFileAsync(socket);
            
            // Validate and decode
            var fileContent = await _fileValidator.ValidateAndDecodeAsync(fileBytes, fileName);
            
            // Create agent based on contract type
            var agent = CreateAgent(contractType);
            
            // Initialize agent
            var initResult = await agent.InitializeAsync(fileContent);
            
            if (!initResult.Success)
            {
                await SendErrorAsync(socket, initResult.ErrorMessage);
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, initResult.ErrorMessage, CancellationToken.None);
                return;
            }
            
            // Send confirmation
            await SendConfirmationAsync(socket, "Vertrag akzeptiert, Analyse gestartet");
            
            // Execute analyses
            var basicResult = await agent.AnalyzeBasicAsync();
            await SendResultAsync(socket, basicResult);
            
            var premiumResult = await agent.AnalyzePremiumAsync();
            await SendResultAsync(socket, premiumResult);
            
            // Close connection
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Analysis complete", CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during analysis");
            await SendErrorAsync(socket, ex.Message);
            await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
        }
    }
    
    private IContractAnalysisAgent CreateAgent(string contractType)
    {
        return contractType switch
        {
            "Rental" => _serviceProvider.GetRequiredService<RentalContractAgent>(),
            "Employment" => _serviceProvider.GetRequiredService<EmploymentContractAgent>(),
            _ => throw new UnsupportedContractTypeException(contractType)
        };
    }
    
    private async Task SendConfirmationAsync(WebSocket socket, string message)
    {
        var confirmation = new { type = "confirmation", message };
        await SendJsonAsync(socket, confirmation);
    }
    
    private async Task SendResultAsync<T>(WebSocket socket, T result)
    {
        var message = new { type = "result", data = result };
        await SendJsonAsync(socket, message);
    }
    
    private async Task SendErrorAsync(WebSocket socket, string error)
    {
        var message = new { type = "error", message = error };
        await SendJsonAsync(socket, message);
    }
    
    private async Task SendJsonAsync<T>(WebSocket socket, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
```

## Data Models

### Analysis DTOs

```csharp
// Base
public interface IAnalysisDto
{
    string ContractType { get; }
    string Tier { get; }
}

public record AnalysisDto<TResult>(
    TResult Result,
    AnalysisMetadataDto Metadata
) where TResult : IAnalysisDto;

public record AnalysisMetadataDto(
    string ContractId,
    string CountryCode,
    string Language,
    DateTimeOffset AnalyzedAt,
    bool IsPreview,
    string ModelVersion
);

// Rental Basic
public record RentalBasicGeneralDto(
    string Vertragstyp,
    string Mieter,
    string Vermieter,
    string MietobjektAdresse,
    DateOnly Vertragsbeginn,
    string Vertragsart
);

public record RentalBasicRentDto(
    decimal Kaltmiete,
    decimal Nebenkosten,
    decimal Warmmiete,
    decimal Kaution,
    string Zahlungsweise,
    string Nebenkostenregelung
);

public record RentalBasicConditionsDto(
    string Kuendigungsfrist,
    string Tierhaltung,
    string Untervermietung,
    string Besonderheiten
);

public record RentalBasicSummaryDto(
    string Kurzfazit,
    string Gesamtfazit,
    string Gesamtbewertung,
    OverallRiskLevel RiskLevel
);

public record RentalBasicAnalysisDto(
    RentalBasicGeneralDto AllgemeineVertragsdaten,
    RentalBasicRentDto Mietkonditionen,
    RentalBasicConditionsDto Vertragsbedingungen,
    RentalBasicSummaryDto Summary
) : IAnalysisDto
{
    public string ContractType => "Mietvertrag";
    public string Tier => "Basic";
}

// Rental Premium
public enum ClauseRiskScore
{
    Zulaessig,
    Nachteilhaft,
    Unzulaessig
}

public enum OverallRiskLevel
{
    Gruen,
    Gelb,
    Rot
}

public record PremiumSectionDto(
    string Zusammenfassung,
    string Bewertung,
    string Rechtsgrundlage,
    ClauseRiskScore RisikoScore,
    OverallRiskLevel RiskLevel
);

public record RentalPremiumAnalysisDto(
    PremiumSectionDto MietzahlungUndNebenkosten,
    PremiumSectionDto MietdauerUndKuendigung,
    PremiumSectionDto GebrauchDerMietsache,
    PremiumSectionDto InstandhaltungUndReparaturen,
    PremiumSectionDto RechteUndPflichten,
    PremiumSectionDto DatenschutzUndKommunikation,
    string Gesamtfazit,
    OverallRiskLevel Gesamtbewertung
) : IAnalysisDto
{
    public string ContractType => "Mietvertrag";
    public string Tier => "Premium";
}
```

### JSON Schema Structure

Schemas werden als C#-Klassen mit `JsonObject` definiert (wie im PoC):

```csharp
public sealed class RentalGeneralSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["vertragstyp"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Typ des Mietvertrags, z. B. 'I', 'II', 'III', 'WG'.",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-bold",
                    ["importance"] = "high",
                    ["icon"] = "document"
                }
            },
            ["mieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Mieter:in.",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "user"
                }
            }
        },
        ["required"] = new JsonArray("vertragstyp", "mieter")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
    public BinaryData GetSchemaBinaryData() => BinaryData.FromString(_schema.ToJsonString());
}
```

**Vorteile dieses Ansatzes:**
- Schemas sind in C# definiert (typsicher, IntelliSense)
- Einfach zu lesen und zu warten
- `x-styling` Metadaten für Frontend-Rendering
- `DeepClone()` für Immutability
- `BinaryData` für Nebius API


## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system-essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: WebSocket receives contract with type
*For any* valid WebSocket connection and file upload, when a contract is sent with a contract type, the system should successfully receive both the file data and the contract type.
**Validates: Requirements 1.1, 1.2**

### Property 2: File validation and decoding
*For any* valid file format (PDF, text, image), when the file is validated and decoded, the system should extract readable text content.
**Validates: Requirements 1.3**

### Property 3: Invalid files produce errors
*For any* invalid or corrupted file, when validation is attempted, the system should send an error message over WebSocket and close the connection.
**Validates: Requirements 1.4**

### Property 4: Agent construction is lightweight
*For any* agent type, when the agent is constructed, the constructor should complete quickly without executing any AI calls or contract logic.
**Validates: Requirements 2.1**

### Property 5: Uninitialized agents reject analysis
*For any* agent that has not been initialized, when an analysis method is called, the system should throw an InvalidOperationException.
**Validates: Requirements 2.4**

### Property 6: Agent state persistence
*For any* agent instance, when contract text is stored during initialization, subsequent analysis calls should use the same stored text.
**Validates: Requirements 2.5**

### Property 7: Unreadable contracts are rejected
*For any* contract text that is not readable (empty, corrupted, or non-text), when initialization is attempted, the system should throw a ContractValidationException.
**Validates: Requirements 3.2, 3.3**

### Property 8: Contract type mismatch detection
*For any* agent with an expected contract type, when a contract of a different type is provided during initialization, the system should throw a ContractValidationException.
**Validates: Requirements 3.4, 3.5**

### Property 9: Successful initialization stores text
*For any* valid contract text, when initialization succeeds, the contract text should be stored in the agent instance and the IsInitialized property should be true.
**Validates: Requirements 3.6, 3.7**

### Property 10: Initialization sends confirmation
*For any* successful initialization, when the agent is marked as ready, a confirmation message should be sent over the WebSocket to the frontend.
**Validates: Requirements 3.8**

### Property 11: Basic analysis uses stored text
*For any* initialized agent, when a basic analysis step is executed, the step should use the contract text that was stored during initialization.
**Validates: Requirements 4.4**

### Property 12: Schema retrieval for analysis steps
*For any* analysis step (contract type, tier, step name), when the step is executed, the system should retrieve the corresponding JSON schema from the schema provider.
**Validates: Requirements 4.5, 5.5**

### Property 13: AI responses are validated
*For any* AI response received from Nebius, when the response is processed, the system should validate it against the corresponding JSON schema.
**Validates: Requirements 4.7, 5.7**

### Property 14: Invalid AI responses produce errors
*For any* AI response that fails schema validation, when validation is attempted, the system should return a customer-friendly error message for that step instead of the result.
**Validates: Requirements 4.8**

### Property 15: Basic analysis result aggregation
*For any* set of completed basic analysis steps (including errors), when all steps finish, the system should aggregate the results into a single BasicAnalysisDto.
**Validates: Requirements 4.9**

### Property 16: Basic results sent over WebSocket
*For any* completed basic analysis, when the BasicAnalysisDto is created, the system should serialize and send it over the WebSocket to the frontend.
**Validates: Requirements 4.10**

### Property 17: Premium analysis independence
*For any* agent, when both basic and premium analyses are executed, the premium analysis should not depend on or use results from the basic analysis.
**Validates: Requirements 5.11**

### Property 18: Schema contains required fields
*For any* JSON schema, when the schema is retrieved, it should contain all required fields with their types specified.
**Validates: Requirements 6.2**

### Property 19: Schema contains styling information
*For any* JSON schema, when the schema is retrieved, it should contain x-styling metadata for frontend rendering.
**Validates: Requirements 6.3**

### Property 20: Schema validation returns details
*For any* failed schema validation, when validation is performed, the validator should return false and provide detailed error information.
**Validates: Requirements 6.6**

### Property 21: Schema endpoint availability
*For any* contract type and analysis step, when the frontend requests a schema, the system should provide an HTTP endpoint that returns the JSON schema.
**Validates: Requirements 6.7**

### Property 22: Correct agent creation by type
*For any* supported contract type (Rental, Employment), when an agent is requested, the system should create an instance of the corresponding agent class.
**Validates: Requirements 7.1, 7.2**

### Property 23: Unknown contract types rejected
*For any* unsupported contract type, when agent creation is attempted, the system should throw an UnsupportedContractTypeException.
**Validates: Requirements 7.4**

### Property 24: Analysis DTOs contain metadata
*For any* completed analysis, when the AnalysisDto is created, it should contain both the result and metadata (ContractId, CountryCode, Language, AnalyzedAt, IsPreview, ModelVersion).
**Validates: Requirements 8.1, 8.2**

### Property 25: Rental basic DTO structure
*For any* completed rental basic analysis, when the DTO is created, it should contain AllgemeineVertragsdaten, Mietkonditionen, Vertragsbedingungen, and Summary sections.
**Validates: Requirements 8.3**

### Property 26: Rental premium DTO structure
*For any* completed rental premium analysis, when the DTO is created, it should contain all six premium sections (Mietzahlung, Mietdauer, Gebrauch, Instandhaltung, Rechte, Datenschutz) plus Gesamtfazit and Gesamtbewertung.
**Validates: Requirements 8.4**

### Property 27: Premium section structure
*For any* premium section, when the section is created, it should contain Zusammenfassung, Bewertung, Rechtsgrundlage, RisikoScore, and RiskLevel.
**Validates: Requirements 8.5**

### Property 28: Exclusive Nebius usage
*For any* AI request made by an agent, when the request is sent, it should be sent exclusively through the Nebius client.
**Validates: Requirements 9.1**

### Property 29: Nebius receives prompt and schema
*For any* Nebius AI request, when the request is made, it should include both the prompt text and the JSON schema.
**Validates: Requirements 9.2**

### Property 30: Nebius errors throw exceptions
*For any* Nebius request that fails, when the error is received, the system should throw an AiServiceException.
**Validates: Requirements 9.4**

### Property 31: Failed steps don't stop others
*For any* set of parallel analysis steps where one fails, when the failure occurs, the other steps should continue executing to completion.
**Validates: Requirements 10.3**

### Property 32: Failed steps return error messages
*For any* analysis step that fails, when the step completes with an error, the system should return a customer-friendly error message instead of throwing an exception.
**Validates: Requirements 10.4**

### Property 33: Step duration logging
*For any* analysis step, when the step completes (success or failure), the system should log the duration on Info level.
**Validates: Requirements 10.6, 10.7**

### Property 34: WebSocket receives contract type
*For any* WebSocket connection, when a file is uploaded, the message should include both the file data and the contract type.
**Validates: Requirements 11.1**

### Property 35: Confirmation message after initialization
*For any* successful agent initialization, when the agent is ready, a confirmation message should be sent as JSON over the WebSocket.
**Validates: Requirements 11.3**

### Property 36: Results sent over WebSocket
*For any* completed analysis (basic or premium), when the AnalysisDto is created, it should be serialized to JSON and sent over the WebSocket.
**Validates: Requirements 11.4, 11.5**

### Property 37: WebSocket closes after completion
*For any* successful analysis workflow, when both basic and premium analyses complete, the WebSocket should close with status NormalClosure.
**Validates: Requirements 11.6**

### Property 38: Errors sent over WebSocket
*For any* error during the analysis workflow, when the error occurs, an error message should be sent over the WebSocket before closing the connection.
**Validates: Requirements 11.7**

### Property 39: Central prompt retrieval
*For any* analysis step (contract type, tier, step name), when a prompt is needed, the system should retrieve it from the central PromptProvider.
**Validates: Requirements 12.1, 12.5**

### Property 40: Prompt organization with regions
*For any* prompt file, when prompts are defined, they should be organized using comments and/or regions for clarity.
**Validates: Requirements 12.2**

## Error Handling

### Exception Hierarchy

```csharp
public class ContractAnalysisException : Exception
{
    public ContractAnalysisException(string message) : base(message) { }
    public ContractAnalysisException(string message, Exception inner) : base(message, inner) { }
}

public class ContractValidationException : ContractAnalysisException
{
    public ContractValidationException(string message) : base(message) { }
}

public class UnsupportedContractTypeException : ContractAnalysisException
{
    public string ContractType { get; }
    
    public UnsupportedContractTypeException(string contractType) 
        : base($"Contract type '{contractType}' is not supported")
    {
        ContractType = contractType;
    }
}

public class SchemaNotFoundException : ContractAnalysisException
{
    public string SchemaKey { get; }
    
    public SchemaNotFoundException(string schemaKey) 
        : base($"Schema '{schemaKey}' not found")
    {
        SchemaKey = schemaKey;
    }
}

public class PromptNotFoundException : ContractAnalysisException
{
    public string PromptKey { get; }
    
    public PromptNotFoundException(string promptKey) 
        : base($"Prompt '{promptKey}' not found")
    {
        PromptKey = promptKey;
    }
}

public class AiServiceException : ContractAnalysisException
{
    public AiServiceException(string message) : base(message) { }
    public AiServiceException(string message, Exception inner) : base(message, inner) { }
}
```

### Error Handling Strategy

1. **Initialization Errors**: Throw exceptions immediately, send error over WebSocket, close connection
2. **Analysis Step Errors**: Catch exceptions, log error, return customer-friendly error message, continue other steps
3. **Schema/Prompt Errors**: Throw exceptions (configuration errors should fail fast)
4. **AI Service Errors**: Catch, log, return error message for that step
5. **WebSocket Errors**: Log, attempt to send error message, close connection with appropriate status

### Customer-Friendly Error Messages

```csharp
public static class ErrorMessages
{
    public const string ContractUnreadable = "Das Dokument konnte nicht gelesen werden. Bitte stellen Sie sicher, dass es sich um eine gültige PDF-, Text- oder Bilddatei handelt.";
    public const string WrongContractType = "Der hochgeladene Vertrag entspricht nicht dem erwarteten Vertragstyp.";
    public const string AnalysisStepFailed = "Ein Teil der Analyse konnte nicht durchgeführt werden. Die übrigen Ergebnisse sind verfügbar.";
    public const string AiServiceUnavailable = "Der KI-Service ist vorübergehend nicht verfügbar. Bitte versuchen Sie es später erneut.";
    public const string ValidationFailed = "Die Analyse konnte nicht validiert werden. Bitte versuchen Sie es erneut.";
}
```

## Testing Strategy

### Unit Testing

Unit tests will verify specific behaviors and edge cases:

1. **Agent Construction Tests**
   - Verify constructor only injects dependencies
   - Verify initial state is not-initialized
   - Verify dependencies are properly stored

2. **Initialization Tests**
   - Test successful initialization with valid contract
   - Test rejection of unreadable contracts
   - Test rejection of wrong contract types
   - Test state changes after initialization

3. **Schema Provider Tests**
   - Test schema retrieval for all contract types and steps
   - Test exception for unknown schema keys
   - Test schema structure validation

4. **Schema Validator Tests**
   - Test validation of valid responses
   - Test rejection of invalid responses
   - Test detailed error reporting

5. **Prompt Provider Tests**
   - Test prompt retrieval for all contract types and steps
   - Test exception for unknown prompt keys
   - Test prompt composition (system + specific)

6. **Error Handling Tests**
   - Test exception types and messages
   - Test error message generation
   - Test WebSocket error communication

### Property-Based Testing

Property-based tests will verify universal properties across many inputs using **FsCheck** for C#:

1. **Property 5: Uninitialized agents reject analysis**
   - Generate: Random agent instances (not initialized)
   - Test: Calling AnalyzeBasicAsync() or AnalyzePremiumAsync() throws InvalidOperationException
   - **Validates: Requirements 2.4**

2. **Property 6: Agent state persistence**
   - Generate: Random contract text, initialized agent
   - Test: Multiple analysis calls use the same stored text
   - **Validates: Requirements 2.5**

3. **Property 7: Unreadable contracts are rejected**
   - Generate: Random unreadable text (empty, whitespace, corrupted)
   - Test: InitializeAsync() throws ContractValidationException
   - **Validates: Requirements 3.2, 3.3**

4. **Property 8: Contract type mismatch detection**
   - Generate: Random agent type, contract of different type
   - Test: InitializeAsync() throws ContractValidationException
   - **Validates: Requirements 3.4, 3.5**

5. **Property 9: Successful initialization stores text**
   - Generate: Random valid contract text
   - Test: After InitializeAsync(), stored text equals input and IsInitialized is true
   - **Validates: Requirements 3.6, 3.7**

6. **Property 12: Schema retrieval for analysis steps**
   - Generate: Random valid (contractType, tier, step) combinations
   - Test: GetSchema() returns a non-null JsonObject
   - **Validates: Requirements 4.5, 5.5**

7. **Property 13: AI responses are validated**
   - Generate: Random AI responses (valid and invalid)
   - Test: Validate() is called for every response
   - **Validates: Requirements 4.7, 5.7**

8. **Property 14: Invalid AI responses produce errors**
   - Generate: Random invalid AI responses
   - Test: Step returns error message instead of throwing
   - **Validates: Requirements 4.8**

9. **Property 18: Schema contains required fields**
   - Generate: All schema keys
   - Test: Each schema has "type" and "properties" fields
   - **Validates: Requirements 6.2**

10. **Property 19: Schema contains styling information**
    - Generate: All schema keys
    - Test: Each schema property has "x-styling" metadata
    - **Validates: Requirements 6.3**
    - Note: Schemas also include "x-title" metadata for German field labels (added in Task 15)

11. **Property 22: Correct agent creation by type**
    - Generate: All supported contract types
    - Test: CreateAgent() returns correct agent class
    - **Validates: Requirements 7.1, 7.2**

12. **Property 23: Unknown contract types rejected**
    - Generate: Random unsupported contract type strings
    - Test: CreateAgent() throws UnsupportedContractTypeException
    - **Validates: Requirements 7.4**

13. **Property 24: Analysis DTOs contain metadata**
    - Generate: Random analysis results
    - Test: AnalysisDto has non-null Metadata with all required fields
    - **Validates: Requirements 8.1, 8.2**

14. **Property 31: Failed steps don't stop others**
    - Generate: Random set of steps where some fail
    - Test: All steps complete (success or error)
    - **Validates: Requirements 10.3**

15. **Property 32: Failed steps return error messages**
    - Generate: Random failing steps
    - Test: Result contains error message string, not exception
    - **Validates: Requirements 10.4**

16. **Property 39: Central prompt retrieval**
    - Generate: Random valid (contractType, tier, step) combinations
    - Test: GetPrompt() returns non-empty string
    - **Validates: Requirements 12.1, 12.5**

### Integration Testing

Integration tests will verify component interactions:

1. **WebSocket End-to-End Tests**
   - Test complete workflow from upload to results
   - Test error scenarios and WebSocket closure
   - Test message format and serialization

2. **Agent Lifecycle Tests**
   - Test construction → initialization → analysis flow
   - Test parallel step execution
   - Test error recovery

3. **Nebius Integration Tests**
   - Test actual AI calls with mock responses
   - Test schema validation with real responses
   - Test error handling for AI failures

### Test Configuration

```csharp
// FsCheck configuration for property-based tests
public class PropertyTestConfig
{
    public static Configuration QuickConfig => Configuration.QuickThrowOnFailure;
    public static Configuration StandardConfig => Configuration.Default.WithMaxTest(100);
}

// Example property test
[Property]
public Property UninitializedAgentRejectsAnalysis()
{
    return Prop.ForAll<string>(contractType =>
    {
        var agent = CreateUninitializedAgent(contractType);
        Assert.Throws<InvalidOperationException>(() => 
            agent.AnalyzeBasicAsync().Wait());
    });
}
```

### Test Results

All property-based tests have been implemented and are passing:
- Schema structure and styling tests verify x-title and x-styling metadata
- Agent lifecycle tests verify 3-phase construction, initialization, and analysis
- WebSocket handler tests verify message flow and error handling
- DTO structure tests verify metadata and result structure
- Prompt provider tests verify central prompt retrieval

Integration tests verify end-to-end workflows:
- Complete rental contract analysis flow
- Schema endpoint retrieval
- WebSocket message flow (confirmation → basic → premium)
- Error scenarios and recovery

## Performance Considerations

1. **Parallel Execution**: All analysis steps within a tier (Basic/Premium) run in parallel using Task.WhenAll
2. **Lightweight Construction**: Agent construction is fast (< 1ms) with only DI
3. **Schema Caching**: Schemas are loaded once at startup and cached in memory
4. **Prompt Caching**: Prompts are static strings, no runtime generation
5. **WebSocket Streaming**: Results are sent immediately when available, no buffering
6. **Logging**: Step duration logging helps identify bottlenecks

## Security Considerations

1. **Input Validation**: All file uploads are validated before processing/
3. **Error Messages**: Customer-facing errors don't expose internal details
4. **WebSocket Security**: Use WSS (WebSocket Secure) in production
5. **Rate Limiting**: Consider rate limiting for AI service calls
6. **Contract Data**: Ensure contract text is not logged or persisted unnecessarily

## Deployment Considerations

1. **Schema Files**: Deploy schemas as embedded resources or configuration files
2. **Prompt Updates**: Prompts can be updated without code changes if externalized
3. **AI Service Configuration**: Nebius API keys and endpoints in configuration
4. **Monitoring**: Log step durations and error rates for monitoring
5. **Scalability**: Agents are stateless (per-request), can scale horizontally
6. **Health Checks**: Implement health checks for Nebius connectivity

## Future Enhancements

1. **Additional Contract Types**: Easy to add new agent types following the same pattern
2. **Caching**: Cache analysis results for identical contracts
3. **Streaming Results**: Stream partial results as steps complete
4. **Multi-Language Support**: Extend schemas and prompts for multiple languages
5. **Advanced Validation**: Add business rule validation beyond schema validation
6. **Analytics**: Track analysis patterns and common issues
