# Design Document: Employment Contract Agent

## Overview

Der Employment Contract Agent implementiert die vollständige Analyse von Arbeitsverträgen basierend auf der bestehenden 3-Phasen-Architektur (Construction, Initialization, Analysis). Das Design orientiert sich eng am vollständig implementierten RentalContractAgent und passt die Struktur an die spezifischen Anforderungen von Arbeitsverträgen an.

### Key Design Decisions

1. **Schema-Anpassung**: Die Schemas orientieren sich an der bestehenden `EmploymentContractAnalysisDto` Struktur, werden aber sinnvoll auf die neue Agentenlogik angepasst
2. **KV-Erkennung in Initialization**: Der Kollektivvertrag wird bereits in der Initialisierungsphase erkannt und gespeichert, nicht erst in der Basic-Analyse
3. **Parallele Ausführung**: Alle Analyse-Steps werden parallel mit `Task.WhenAll` ausgeführt
4. **Fehlerbehandlung**: Jeder Step hat eigene Try-Catch-Blöcke und gibt bei Fehlern kundenfreundliche Fehler-DTOs zurück
5. **Context Management**: Automatisches Laden von Kontext-Dateien pro Step via `ContextStepAttribute`

## Architecture

### Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                  EmploymentContractAgent                     │
├─────────────────────────────────────────────────────────────┤
│  Phase 1: Construction                                       │
│  - Dependency Injection (AI_Client, Schema_Provider, etc.)  │
│  - Agent_Id Generation                                       │
├─────────────────────────────────────────────────────────────┤
│  Phase 2: Initialization                                     │
│  - Readability Check (AI)                                    │
│  - Contract Type Detection (AI)                              │
│  - KV Detection (KV_Detection_Service)                       │
│  - Store Contract Text + KV Text                             │
├─────────────────────────────────────────────────────────────┤
│  Phase 3: Analysis                                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ Basic Analysis (Parallel Execution)                   │  │
│  │ - General Employment Data                             │  │
│  │ - Compensation                                        │  │
│  │ - Working Conditions                                  │  │
│  │ - Basic Summary (mentions KV)                         │  │
│  └───────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ Premium Analysis (Parallel Execution)                 │  │
│  │ - Compensation & Benefits Clauses (with KV context)   │  │
│  │ - Working Time & Vacation (with KV context)           │  │
│  │ - Termination & Notice Period (with KV context)       │  │
│  │ - Non-Compete & Confidentiality (with KV context)     │  │
│  │ - Rights & Duties (with KV context)                   │  │
│  │ - Premium Summary                                     │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
         │                │                │                │
         ▼                ▼                ▼                ▼
    AI_Client    Schema_Provider  Context_Provider  KV_Detection
```

### Inheritance Hierarchy

```
ContractAnalysisAgentBase (abstract)
    │
    ├── RentalContractAgent (fully implemented - reference)
    │
    └── EmploymentContractAgent (to be implemented)
```

## Components and Interfaces

### EmploymentContractAgent

**Responsibilities:**
- Implement 3-phase lifecycle for employment contracts
- Orchestrate parallel execution of analysis steps
- Handle KV detection and storage during initialization
- Provide error handling with customer-friendly messages
- Track token usage for all AI requests

**Key Fields:**
```csharp
private readonly IContextProvider _contextProvider;
private readonly IKvDetectionService _kvDetectionService;
private string? _contractText;
private string? _detectedKvName;
private string? _detectedKvText;
private bool _isInitialized;
```

**Key Methods:**
- `InitializeAsync(string contractText)`: Phase 2 - Validation and KV detection
- `AnalyzeBasicAsync()`: Phase 3 - Basic analysis orchestration
- `AnalyzePremiumAsync()`: Phase 3 - Premium analysis orchestration
- Private analysis step methods with `[ContextStep]` attributes

### Dependencies

**IAiClient (Nebius Service):**
- `SendPromptWithMetadata(string prompt, BinaryData schema)`: Sends AI requests with JSON schema
- Returns `AiResponse` with content and token metadata

**ISchemaProvider:**
- `GetSchema(string contractType, string tier, string step)`: Returns schema as BinaryData for AI
- `GetSchemaForEndpoint(string contractType, string tier, string step)`: Returns JsonObject for validation

**ISchemaValidator:**
- `Validate(JsonObject schema, JsonObject response)`: Validates AI response against schema
- `ValidateWithDetails(JsonObject schema, JsonObject response)`: Returns detailed validation errors

**IContextProvider:**
- `GetContextForCurrentStepAsync()`: Automatically loads context based on `[ContextStep]` attribute

**IKvDetectionService:**
- `DetectAsync(string contractContent)`: Detects matching Kollektivvertrag
- Returns `KvDetectionResult` with KV name and text

## Data Models

### Schema Structure Adaptation

Die neue Schema-Struktur orientiert sich an der alten `EmploymentContractAnalysisDto`, passt sie aber an die neue Agentenlogik an:

**Alte Struktur (EmploymentContractAnalysisDto):**
```csharp
- Keypoints (SummaryClass)
  - Arbeitgeber, Taetigkeit, Beginn, Dauer
  - GehaltBrutto, Zuschlaege
  - Arbeitsstunden, Ueberstunden, Probezeit
- UnzulaessigeKlauseln (ProblemKlausel[])
- NachteileKlauseln (ProblemKlausel[])
- WiderspruechlicheKlauseln (WiderspruechlicheKlausel[])
- VorteilhafteKlauseln (VorteilKlausel[])
- Fazit (string)
- Kollektivvertrag (string?)
```

**Neue Struktur (Angepasst an Agentenlogik):**

#### Basic Analysis DTOs

**Location:** `Domain/Dtos/EmploymentBasicAnalysisDto.cs`

**EmploymentBasicGeneralDto:**
```csharp
public record EmploymentBasicGeneralDto(
    [property: JsonPropertyName("arbeitgeber")] string Arbeitgeber,
    [property: JsonPropertyName("arbeitnehmer")] string Arbeitnehmer,
    [property: JsonPropertyName("position")] string Position,
    [property: JsonPropertyName("startdatum")] string Startdatum,
    [property: JsonPropertyName("vertragsart")] string Vertragsart,  // befristet/unbefristet
    [property: JsonPropertyName("probezeit")] string Probezeit,
    [property: JsonPropertyName("vertragsende")] string? Vertragsende = null
);
```

**EmploymentBasicCompensationDto:**
```csharp
public record EmploymentBasicCompensationDto(
    [property: JsonPropertyName("gehaltBrutto")] string GehaltBrutto,
    [property: JsonPropertyName("gehaltNettoCa")] string GehaltNettoCa,
    [property: JsonPropertyName("zahlungsweise")] string Zahlungsweise,  // monatlich, etc.
    [property: JsonPropertyName("sonderzahlungen")] string Sonderzahlungen,  // 13./14. Gehalt
    [property: JsonPropertyName("zuschlaege")] string Zuschlaege,
    [property: JsonPropertyName("zusatzleistungen")] string[] Zusatzleistungen  // Firmenwagen, etc.
);
```

**EmploymentBasicWorkingConditionsDto:**
```csharp
public record EmploymentBasicWorkingConditionsDto(
    [property: JsonPropertyName("wochenarbeitszeit")] string Wochenarbeitszeit,
    [property: JsonPropertyName("arbeitszeitmodell")] string Arbeitszeitmodell,  // Vollzeit, Teilzeit, etc.
    [property: JsonPropertyName("ueberstundenregelung")] string Ueberstundenregelung,
    [property: JsonPropertyName("urlaubstage")] string Urlaubstage,
    [property: JsonPropertyName("arbeitsort")] string Arbeitsort,
    [property: JsonPropertyName("remoteArbeit")] string RemoteArbeit
);
```

**EmploymentBasicSummaryDto:**
```csharp
public record EmploymentBasicSummaryDto(
    [property: JsonPropertyName("kurzfazit")] string Kurzfazit,
    [property: JsonPropertyName("kollektivvertrag")] string Kollektivvertrag,  // Erwähnung des erkannten KV
    [property: JsonPropertyName("gesamtbewertung")] string Gesamtbewertung,
    [property: JsonPropertyName("riskLevel")] string RiskLevel  // Grün/Gelb/Rot
);
```

**EmploymentBasicAnalysisDto (Combined):**
```csharp
public record EmploymentBasicAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("General")]
    public required EmploymentBasicGeneralDto General { get; init; }
    
    [JsonPropertyName("Compensation")]
    public required EmploymentBasicCompensationDto Compensation { get; init; }
    
    [JsonPropertyName("WorkingConditions")]
    public required EmploymentBasicWorkingConditionsDto WorkingConditions { get; init; }
    
    [JsonPropertyName("Summary")]
    public required EmploymentBasicSummaryDto Summary { get; init; }
    
    [JsonIgnore]
    public string ContractType => "Arbeitsvertrag";
    
    [JsonIgnore]
    public string Tier => "Basic";
}
```

#### Premium Analysis DTOs

**Location:** `Domain/Dtos/EmploymentPremiumAnalysisDto.cs`

**PremiumSectionDto (Reused from Rental):**
```csharp
// Already exists in Domain/Dtos/RentalPremiumAnalysisDto.cs
public record PremiumSectionDto(
    [property: JsonPropertyName("zusammenfassung")] string Zusammenfassung,
    [property: JsonPropertyName("bewertung")] string Bewertung,
    [property: JsonPropertyName("rechtsgrundlage")] string Rechtsgrundlage,
    [property: JsonPropertyName("risikoScore")] string RisikoScore,  // Vorteilhaft/Neutral/Nachteilhaft
    [property: JsonPropertyName("riskLevel")] string RiskLevel  // Grün/Gelb/Rot
);
```

**EmploymentPremiumSummaryDto:**
```csharp
public record EmploymentPremiumSummaryDto(
    [property: JsonPropertyName("gesamtfazit")] string Gesamtfazit,
    [property: JsonPropertyName("gesamtbewertung")] string Gesamtbewertung,
    [property: JsonPropertyName("handlungsempfehlungen")] string[] Handlungsempfehlungen
);
```

**EmploymentPremiumAnalysisDto (Combined):**
```csharp
public record EmploymentPremiumAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("CompensationBenefits")]
    public required PremiumSectionDto CompensationBenefits { get; init; }
    
    [JsonPropertyName("WorkingTimeVacation")]
    public required PremiumSectionDto WorkingTimeVacation { get; init; }
    
    [JsonPropertyName("TerminationNotice")]
    public required PremiumSectionDto TerminationNotice { get; init; }
    
    [JsonPropertyName("NonCompeteConfidentiality")]
    public required PremiumSectionDto NonCompeteConfidentiality { get; init; }
    
    [JsonPropertyName("RightsDuties")]
    public required PremiumSectionDto RightsDuties { get; init; }
    
    [JsonPropertyName("Summary")]
    public required EmploymentPremiumSummaryDto Summary { get; init; }
    
    [JsonIgnore]
    public string ContractType => "Arbeitsvertrag";
    
    [JsonIgnore]
    public string Tier => "Premium";
}
```

### Analysis Steps Mapping

**Basic Analysis Steps:**
1. `AnalyzeGeneralEmploymentDataAsync()` → `[ContextStep("general-employment-data")]`
2. `AnalyzeCompensationAsync()` → `[ContextStep("compensation")]`
3. `AnalyzeWorkingConditionsAsync()` → `[ContextStep("working-conditions")]`
4. `AnalyzeBasicSummaryAsync()` → `[ContextStep("basic-summary")]`

**Premium Analysis Steps:**
1. `AnalyzeCompensationBenefitsClausesAsync()` → `[ContextStep("compensation-benefits-clauses")]`
2. `AnalyzeWorkingTimeVacationAsync()` → `[ContextStep("working-time-vacation")]`
3. `AnalyzeTerminationNoticeAsync()` → `[ContextStep("termination-notice")]`
4. `AnalyzeNonCompeteConfidentialityAsync()` → `[ContextStep("non-compete-confidentiality")]`
5. `AnalyzeRightsDutiesAsync()` → `[ContextStep("rights-duties")]`
6. `AnalyzePremiumSummaryAsync()` → `[ContextStep("premium-summary")]`

### KV Storage Location

**Empfohlene Struktur:**
```
data/
  KVS/
    Employment/
      <kv-name>.txt
```

**Rationale:**
- Trennung nach Vertragstyp (Employment vs. Rental)
- Klare Namenskonvention
- Einfache Erweiterbarkeit für weitere Vertragstypen

## Error Handling

### Error Handling Strategy

**Principle:** Fail gracefully - never throw exceptions that break the entire analysis.

**Implementation Pattern (from RentalContractAgent):**
```csharp
private async Task<EmploymentBasicGeneralDto> AnalyzeGeneralEmploymentDataAsync()
{
    var stepName = "AnalyzeGeneralEmploymentDataAsync";
    var sw = Stopwatch.StartNew();
    
    try
    {
        _logger.LogInformation("[Agent {AgentId}] {StepName} started", AgentId, stepName);
        
        // Get schema and context
        var schemaBinary = _schemaProvider.GetSchema("Employment", "Basic", "General");
        var schemaObject = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "General");
        var prompt = PromptProvider.GetPrompt("Employment", "Basic", "General");
        
        // Load context
        var context = await _contextProvider.GetContextForCurrentStepAsync();
        if (!string.IsNullOrWhiteSpace(context))
        {
            prompt += $"\n\n## Reference Context\n\n{context}";
        }
        
        // Call AI
        var aiResponse = await _aiClient.SendPromptWithMetadata(
            prompt + "\n\nVertragstext:\n" + _contractText, 
            schemaBinary);
        TrackTokens(aiResponse);
        
        // Validate and deserialize
        var jsonDoc = JsonDocument.Parse(aiResponse.Content);
        var responseObject = JsonObject.Create(jsonDoc.RootElement);
        
        if (!_schemaValidator.Validate(schemaObject, responseObject))
        {
            var validationResult = _schemaValidator.ValidateWithDetails(schemaObject, responseObject);
            throw new SchemaValidationException(
                "AI response failed schema validation",
                validationResult.Errors);
        }
        
        var result = JsonSerializer.Deserialize<EmploymentBasicGeneralDto>(aiResponse.Content);
        if (result == null)
        {
            throw new AnalysisException("Failed to deserialize AI response");
        }
        
        _logger.LogInformation("[Agent {AgentId}] {StepName} completed in {Duration}ms", 
            AgentId, stepName, sw.ElapsedMilliseconds);
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "[Agent {AgentId}] {StepName} failed", AgentId, stepName);
        
        // Return error DTO with customer-friendly message
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
        _logger.LogInformation("[Agent {AgentId}] {StepName} duration: {Duration}ms", 
            AgentId, stepName, sw.ElapsedMilliseconds);
    }
}
```

**Error DTO Pattern:**
- All string fields contain "Fehler" for consistent error detection
- First field contains detailed error message
- Numeric fields set to 0 or default values
- Analysis continues even if individual steps fail

### Exception Types

**ContractValidationException:**
- Thrown during initialization for validation failures
- Caught and converted to `InitializationResult` with error message

**AnalysisException:**
- Thrown during analysis for general failures
- Caught in step methods and converted to error DTOs

**SchemaValidationException:**
- Thrown when AI response doesn't match schema
- Caught in step methods and converted to error DTOs

## Testing Strategy

### Dual Testing Approach

**Unit Tests:**
- Test specific examples and edge cases
- Test error conditions and exception handling
- Test DTO serialization/deserialization
- Test initialization logic
- Test KV detection integration

**Property-Based Tests:**
- Test universal properties across all inputs
- Minimum 100 iterations per property test
- Each test references design document property
- Tag format: `Feature: employment-contract-agent, Property {number}: {property_text}`

### Testing Configuration

**Property-Based Testing Library:** Use existing PBT library from project (likely xUnit with FsCheck or similar)

**Test Organization:**
```
PocketLawyer.Tests/
  Application/
    Agents/
      EmploymentContractAgentTests.cs  // Unit tests
      EmploymentContractAgentPropertyTests.cs  // Property tests
```

**Test Doubles:**
- Mock `IAiClient` for predictable AI responses
- Mock `ISchemaProvider` for test schemas
- Mock `ISchemaValidator` for validation control
- Mock `IContextProvider` for context injection
- Mock `IKvDetectionService` for KV detection control



## Correctness Properties

A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.

### Property 1: Unique Agent IDs

*For any* sequence of agent constructions, each agent should have a unique Agent_Id that differs from all previously created agents.

**Validates: Requirements 1.2**

### Property 2: Null Dependency Rejection

*For any* combination of constructor dependencies where at least one is null, the constructor should throw an ArgumentNullException.

**Validates: Requirements 1.4**

### Property 3: Token Tracking Completeness

*For any* AI request made during analysis, the system should track both input tokens and output tokens, and the total tokens should equal the sum of all individual request tokens.

**Validates: Requirements 10.1, 10.3**

### Property 4: Error DTO Resilience

*For any* analysis step that encounters an exception, the system should return an error DTO with customer-friendly messages rather than propagating the exception, ensuring the analysis continues.

**Validates: Requirements 11.2**

