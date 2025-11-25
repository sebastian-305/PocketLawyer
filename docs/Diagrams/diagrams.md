# Projekt-Diagramme

## Layered Component Diagram

```mermaid
classDiagram
    subgraph Web
        FE[index.html<br>main.js<br>render.js]
        NET[network.js]
        PC[PocketController]
    end
    subgraph Application
        RCA[RentalContractAgent<br>+ Token Tracking]
        ECA[EmploymentContractAgent<br>+ Token Tracking]
        ICAA[IContractAnalysisAgent]
        AR[AiResponse Record<br>Content, InputTokens,<br>OutputTokens, TotalTokens]
    end
    subgraph Domain
        DTOs[AnalysisDto<br>RentalBasicAnalysisDto<br>RentalPremiumAnalysisDto<br>EmploymentAnalysisDto]
        PP[PromptProvider]
    end
    subgraph Infrastructure
        WSH[WebSocketAnalysisHandler]
        FVS[FileValidatorService]
        FDS[FileDecoderService]
        SP[SchemaProvider<br>+ x-displayName<br>+ x-sectionTitle]
        SV[SchemaValidator]
        NEB[NebiusService<br>+ SendPromptWithMetadata]
        LOG[Serilog<br>Filesystem Logging]
    end
    subgraph External_APIs
        NebiusAPI[(Nebius API)]
    end
    subgraph Filesystem
        LOGS[(logs/<br>YYYY-MM-DD.log)]
    end

    FE --> NET
    NET -- WebSocket --> PC
    PC --> WSH
    WSH --> FVS
    FVS --> FDS
    WSH --> ICAA
    ICAA <|.. RCA
    ICAA <|.. ECA
    RCA --> SP
    RCA --> SV
    RCA --> PP
    RCA --> NEB
    RCA --> AR
    RCA --> LOG
    ECA --> SP
    ECA --> SV
    ECA --> PP
    ECA --> NEB
    ECA --> AR
    ECA --> LOG
    NEB --> NebiusAPI
    NEB --> AR
    LOG --> LOGS
```

## Domänenmodell (DTOs)

Das neue System verwendet eine generische AnalysisDto-Struktur mit Metadata. Alle Analyse-Ergebnisse implementieren `IAnalysisDto` und werden in einem `AnalysisDto<T>` Wrapper mit Metadaten zurückgegeben.

```mermaid
classDiagram
    class IAnalysisDto {
        <<interface>>
        +string ContractType
        +string Tier
    }

    class AnalysisDto~T~ {
        +T Result
        +AnalysisMetadataDto Metadata
    }

    class AnalysisMetadataDto {
        +string ContractId
        +string CountryCode
        +string Language
        +DateTimeOffset AnalyzedAt
        +bool IsPreview
        +string ModelVersion
    }

    class RentalBasicAnalysisDto {
        +RentalBasicGeneralDto AllgemeineVertragsdaten
        +RentalBasicRentDto Mietkonditionen
        +RentalBasicConditionsDto Vertragsbedingungen
        +RentalBasicSummaryDto Summary
        +string ContractType = "Mietvertrag"
        +string Tier = "Basic"
    }

    class RentalBasicGeneralDto {
        +string Vertragstyp
        +string Mieter
        +string Vermieter
        +string MietobjektAdresse
        +DateOnly Vertragsbeginn
        +string Vertragsart
    }

    class RentalBasicRentDto {
        +decimal Kaltmiete
        +decimal Nebenkosten
        +decimal Warmmiete
        +decimal Kaution
        +string Zahlungsweise
        +string Nebenkostenregelung
    }

    class RentalBasicConditionsDto {
        +string Kuendigungsfrist
        +string Tierhaltung
        +string Untervermietung
        +string Besonderheiten
    }

    class RentalBasicSummaryDto {
        +string Kurzfazit
        +string Gesamtfazit
        +string Gesamtbewertung
        +OverallRiskLevel RiskLevel
    }

    class RentalPremiumAnalysisDto {
        +PremiumSectionDto MietzahlungUndNebenkosten
        +PremiumSectionDto MietdauerUndKuendigung
        +PremiumSectionDto GebrauchDerMietsache
        +PremiumSectionDto InstandhaltungUndReparaturen
        +PremiumSectionDto RechteUndPflichten
        +PremiumSectionDto DatenschutzUndKommunikation
        +string Gesamtfazit
        +OverallRiskLevel Gesamtbewertung
        +string ContractType = "Mietvertrag"
        +string Tier = "Premium"
    }

    class PremiumSectionDto {
        +string Zusammenfassung
        +string Bewertung
        +string Rechtsgrundlage
        +ClauseRiskScore RisikoScore
        +OverallRiskLevel RiskLevel
    }

    class ClauseRiskScore {
        <<enumeration>>
        Zulaessig
        Nachteilhaft
        Unzulaessig
    }

    class OverallRiskLevel {
        <<enumeration>>
        Gruen
        Gelb
        Rot
    }

    IAnalysisDto <|.. RentalBasicAnalysisDto
    IAnalysisDto <|.. RentalPremiumAnalysisDto
    AnalysisDto~T~ --> IAnalysisDto
    AnalysisDto~T~ --> AnalysisMetadataDto
    RentalBasicAnalysisDto --> RentalBasicGeneralDto
    RentalBasicAnalysisDto --> RentalBasicRentDto
    RentalBasicAnalysisDto --> RentalBasicConditionsDto
    RentalBasicAnalysisDto --> RentalBasicSummaryDto
    RentalBasicSummaryDto --> OverallRiskLevel
    RentalPremiumAnalysisDto --> PremiumSectionDto
    RentalPremiumAnalysisDto --> OverallRiskLevel
    PremiumSectionDto --> ClauseRiskScore
    PremiumSectionDto --> OverallRiskLevel
```

## PlantUML

```plantuml
@startuml
skinparam classAttributeIconSize 0

interface IAiClient {
    +Task<string> SendPrompt(string prompt, object? schema)
}

class NebiusService {
    -ILogger _logger
    -NebiusOptions _options
    +Task<string> SendPrompt(...)
}

IAiClient <|.. NebiusService

interface IFileDecoder {
    +bool CanDecode(string ext)
    +Task<string> DecodeAsync(Stream input)
}

class TextFileDecoder
class PdfFileDecoder
class ImageFileDecoder
IFileDecoder <|.. TextFileDecoder
IFileDecoder <|.. PdfFileDecoder
IFileDecoder <|.. ImageFileDecoder

interface IFileDecoderService {
    +string[] AllowedExtensions
    +string AllowedExtensionsString
    +Task<string> GetFileContentAsync(string filename, Stream content)
}

class FileDecoderService {
    -ILogger _logger
    -IEnumerable<IFileDecoder> _decoders
    +Task<string> GetFileContentAsync(...)
}
IFileDecoderService <|.. FileDecoderService
FileDecoderService --> IFileDecoder

interface IFileValidatorService {
    +Task<FileContentDto> ValidateAndDecodeAsync(IFormFile file)
}

class FileValidatorService {
    -ILogger _logger
    -IFileDecoderService _decoderService
    -UploadOptions _options
    +Task<FileContentDto> ValidateAndDecodeAsync(...)
}
IFileValidatorService <|.. FileValidatorService
FileValidatorService --> IFileDecoderService

interface ISchemaProvider {
    +BinaryData GetSchema(string contractType, string tier, string step)
    +JsonObject GetSchemaForEndpoint(string contractType, string tier, string step)
}

class SchemaProvider {
    -Dictionary<string, Func<BinaryData>> _schemaFactories
    +BinaryData GetSchema(...)
    +JsonObject GetSchemaForEndpoint(...)
}
ISchemaProvider <|.. SchemaProvider

interface ISchemaValidator {
    +bool Validate(JsonObject schema, JsonObject response)
    +ValidationResult ValidateWithDetails(JsonObject schema, JsonObject response)
}

class SchemaValidator {
    +bool Validate(...)
    +ValidationResult ValidateWithDetails(...)
}
ISchemaValidator <|.. SchemaValidator

class PromptProvider {
    <<static>>
    +string GetPrompt(string contractType, string tier, string step)
}

interface IContractAnalysisAgent {
    +Task<InitializationResult> InitializeAsync(string contractText)
    +Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync()
    +Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync()
    +bool IsInitialized
}

abstract class ContractAnalysisAgentBase {
    #IAiClient _aiClient
    #ISchemaProvider _schemaProvider
    #ISchemaValidator _schemaValidator
    #ILogger _logger
    #string? _contractText
    #bool _isInitialized
    +bool IsInitialized
    #{abstract} string ExpectedContractType
    +{abstract} Task<InitializationResult> InitializeAsync(string contractText)
    +{abstract} Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync()
    +{abstract} Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync()
}

class RentalContractAgent {
    #string ExpectedContractType = "Mietvertrag"
    +Task<InitializationResult> InitializeAsync(...)
    +Task<AnalysisDto<RentalBasicAnalysisDto>> AnalyzeBasicAsync()
    +Task<AnalysisDto<RentalPremiumAnalysisDto>> AnalyzePremiumAsync()
}

class EmploymentContractAgent {
    #string ExpectedContractType = "Arbeitsvertrag"
    +Task<InitializationResult> InitializeAsync(...)
    +Task<AnalysisDto<IAnalysisDto>> AnalyzeBasicAsync()
    +Task<AnalysisDto<IAnalysisDto>> AnalyzePremiumAsync()
}

IContractAnalysisAgent <|.. ContractAnalysisAgentBase
ContractAnalysisAgentBase <|-- RentalContractAgent
ContractAnalysisAgentBase <|-- EmploymentContractAgent
ContractAnalysisAgentBase --> IAiClient
ContractAnalysisAgentBase --> ISchemaProvider
ContractAnalysisAgentBase --> ISchemaValidator
ContractAnalysisAgentBase --> PromptProvider

class WebSocketAnalysisHandler {
    -ILogger _logger
    -IFileValidatorService _fileValidatorService
    -IServiceProvider _serviceProvider
    +Task HandleAsync(HttpContext context)
    -IContractAnalysisAgent CreateAgent(string contractType)
}
WebSocketAnalysisHandler --> IFileValidatorService
WebSocketAnalysisHandler --> IContractAnalysisAgent

class PocketController {
    -ILogger _logger
    +Task RentalAnalysis()
    +Task EmploymentAnalysis()
    +Task GetSchema(string contractType, string tier, string step)
}
PocketController --> WebSocketAnalysisHandler
PocketController --> ISchemaProvider

record InitializationResult {
    +bool Success
    +string? ErrorMessage
}

record AnalysisDto~T~ {
    +T Result
    +AnalysisMetadataDto Metadata
}

record AnalysisMetadataDto {
    +string ContractId
    +string CountryCode
    +string Language
    +DateTimeOffset AnalyzedAt
    +bool IsPreview
    +string ModelVersion
}
@enduml
```

## Agent Lifecycle Sequence Diagram (with Token Tracking)

```mermaid
sequenceDiagram
    participant FE as Browser
    participant WEB as Web (PocketController)
    participant WSH as WebSocketAnalysisHandler
    participant FV as FileValidatorService
    participant AG as ContractAnalysisAgent
    participant SP as SchemaProvider
    participant SV as SchemaValidator
    participant PP as PromptProvider
    participant AI as Nebius AI
    participant LOG as Logger

    FE->>WEB: WebSocket /rental-analysis öffnen
    WEB->>WSH: HandleAsync(HttpContext)
    WSH->>WSH: AcceptWebSocketAsync()
    WSH->>WSH: ReceiveFileAsync()
    Note over WSH: Empfängt Datei + ContractType
    WSH->>FV: ValidateAndDecodeAsync(file)
    FV-->>WSH: FileContentDto

    Note over WSH,AG: Phase 1: Construction
    WSH->>AG: CreateAgent(contractType)
    Note over AG: Nur DI, keine Logik<br>Token-Tracking initialisiert

    Note over WSH,AG: Phase 2: Initialization
    WSH->>AG: InitializeAsync(contractText)
    AG->>AI: Check Readability
    AI-->>AG: IsReadable
    AG->>AI: Detect Contract Type
    AI-->>AG: DetectedType
    AG->>AG: Store contractText
    AG->>AG: Set IsInitialized = true
    AG-->>WSH: InitializationResult(Success)
    WSH->>FE: Confirmation Message

    Note over WSH,AG: Phase 3: Analysis - Basic
    WSH->>AG: AnalyzeBasicAsync()
    par Parallel Basic Steps
        AG->>SP: GetSchema("Rental", "Basic", "General")
        SP-->>AG: Schema (mit x-displayName)
        AG->>PP: GetPrompt("Rental", "Basic", "General")
        PP-->>AG: Prompt
        AG->>AI: SendPromptWithMetadata(prompt, schema)
        AI-->>AG: AiResponse (Content + Tokens)
        AG->>AG: TrackTokens(AiResponse)
        AG->>SV: Validate(schema, response)
        SV-->>AG: Valid
    and
        AG->>SP: GetSchema("Rental", "Basic", "Rent")
        AG->>PP: GetPrompt("Rental", "Basic", "Rent")
        AG->>AI: SendPromptWithMetadata(prompt, schema)
        AI-->>AG: AiResponse (Content + Tokens)
        AG->>AG: TrackTokens(AiResponse)
        AG->>SV: Validate(schema, response)
    and
        AG->>SP: GetSchema("Rental", "Basic", "Conditions")
        AG->>PP: GetPrompt("Rental", "Basic", "Conditions")
        AG->>AI: SendPromptWithMetadata(prompt, schema)
        AI-->>AG: AiResponse (Content + Tokens)
        AG->>AG: TrackTokens(AiResponse)
        AG->>SV: Validate(schema, response)
    and
        AG->>SP: GetSchema("Rental", "Basic", "Summary")
        AG->>PP: GetPrompt("Rental", "Basic", "Summary")
        AG->>AI: SendPromptWithMetadata(prompt, schema)
        AI-->>AG: AiResponse (Content + Tokens)
        AG->>AG: TrackTokens(AiResponse)
        AG->>SV: Validate(schema, response)
    end
    AG->>AG: Aggregate Results
    AG->>LOG: LogTokenSummary() [Info Level]
    Note over LOG: Total Input: X, Output: Y, Total: Z
    AG-->>WSH: AnalysisDto<RentalBasicAnalysisDto>
    WSH->>FE: Basic Result Message

    Note over WSH,AG: Phase 3: Analysis - Premium
    WSH->>AG: AnalyzePremiumAsync()
    par Parallel Premium Steps
        AG->>SP: GetSchema("Rental", "Premium", "Costs")
        Note over SP: Schema mit x-sectionTitle
        AG->>PP: GetPrompt("Rental", "Premium", "Costs")
        AG->>AI: SendPromptWithMetadata(prompt, schema)
        AI-->>AG: AiResponse (Content + Tokens)
        AG->>AG: TrackTokens(AiResponse)
        AG->>SV: Validate(schema, response)
    and
        AG->>SP: GetSchema("Rental", "Premium", "Terms")
        AG->>PP: GetPrompt("Rental", "Premium", "Terms")
        AG->>AI: SendPromptWithMetadata(prompt, schema)
        AI-->>AG: AiResponse (Content + Tokens)
        AG->>AG: TrackTokens(AiResponse)
        AG->>SV: Validate(schema, response)
    and
        Note over AG: ... weitere Premium Steps parallel<br>mit Token-Tracking
    end
    AG->>AG: Aggregate Results
    AG->>LOG: LogTokenSummary() [Info Level]
    Note over LOG: Total Input: X, Output: Y, Total: Z
    AG-->>WSH: AnalysisDto<RentalPremiumAnalysisDto>
    WSH->>FE: Premium Result Message

    WSH->>WSH: CloseAsync(NormalClosure)
```

## WebSocket Message Flow Diagram

```mermaid
sequenceDiagram
    participant FE as Browser
    participant PC as PocketController
    participant WSH as WebSocketAnalysisHandler
    participant AG as ContractAnalysisAgent
    participant NEB as Nebius AI

    FE->>PC: WebSocket Connection<br>/rental-analysis
    PC->>WSH: HandleAsync(HttpContext)
    
    Note over FE,WSH: Message 1: File Upload
    FE->>WSH: { fileName, fileContent, contractType }
    WSH->>WSH: Validate & Decode File
    WSH->>AG: CreateAgent(contractType)
    
    Note over WSH,AG: Initialization Phase
    WSH->>AG: InitializeAsync(contractText)
    AG->>NEB: Check Readability
    AG->>NEB: Detect Contract Type
    AG->>AG: Store Text & Set Ready
    AG-->>WSH: InitializationResult(Success)
    
    Note over FE,WSH: Message 2: Confirmation
    WSH->>FE: { type: "confirmation",<br>message: "Vertrag akzeptiert" }
    
    Note over WSH,AG: Basic Analysis Phase
    WSH->>AG: AnalyzeBasicAsync()
    par Parallel Steps
        AG->>NEB: Analyze General Data
        AG->>NEB: Analyze Rent Conditions
        AG->>NEB: Analyze Contract Conditions
        AG->>NEB: Analyze Summary
    end
    AG-->>WSH: AnalysisDto<BasicAnalysisDto>
    
    Note over FE,WSH: Message 3: Basic Result
    WSH->>FE: { type: "result",<br>data: { result, metadata } }
    
    Note over WSH,AG: Premium Analysis Phase
    WSH->>AG: AnalyzePremiumAsync()
    par Parallel Steps
        AG->>NEB: Analyze Costs
        AG->>NEB: Analyze Terms
        AG->>NEB: Analyze Use
        AG->>NEB: Analyze Maintenance
        AG->>NEB: Analyze Rights
        AG->>NEB: Analyze Privacy
        AG->>NEB: Analyze Summary
    end
    AG-->>WSH: AnalysisDto<PremiumAnalysisDto>
    
    Note over FE,WSH: Message 4: Premium Result
    WSH->>FE: { type: "result",<br>data: { result, metadata } }
    
    Note over FE,WSH: Connection Close
    WSH->>FE: CloseAsync(NormalClosure)
    
    alt Error Scenario
        WSH->>FE: { type: "error",<br>message: "Fehlermeldung" }
        WSH->>FE: CloseAsync(ErrorStatus)
    end
```

## Datenfluss-/Kontextdiagramm

```mermaid
graph TD
    subgraph Browser
        Form[Upload-Formular<br>contractType Auswahl] --> NET[network.js]
        NET --> REN[render.js<br>Generic Renderer]
        NET --> SCH[Schema Fetch]
    end
    NET -- WebSocket --> PCE[/employment-analysis/]
    NET -- WebSocket --> PCR[/rental-analysis/]
    SCH -- HTTP GET --> SCHE[/api/schemas/]
    PCE & PCR --> PC[PocketController]
    SCHE --> PC
    PC --> WSH[WebSocketAnalysisHandler]
    PC --> SP[SchemaProvider]
    WSH --> FV[FileValidatorService]
    FV --> FDS[FileDecoderService]
    WSH --> RCA[RentalContractAgent]
    WSH --> ECA[EmploymentContractAgent]
    RCA --> SP
    RCA --> SV[SchemaValidator]
    RCA --> PP[PromptProvider]
    RCA --> NEB[NebiusService]
    ECA --> SP
    ECA --> SV
    ECA --> PP
    ECA --> NEB
    subgraph External
        NebiusAPI[(Nebius API)]
    end
    NEB --> NebiusAPI
```

## UI-/Flow-Diagramm

```mermaid
flowchart TD
    A[Datei auswählen<br>Vertragstyp setzen] --> B{main.js}
    B -->|rental| CR[/WebSocket /rental-analysis/]
    B -->|employment| CE[/WebSocket /employment-analysis/]
    CR & CE -->|Message 1| D[File Upload]
    D --> E[/Message 2: Confirmation/]
    E --> F[Loading anzeigen]
    F --> G[/Message 3: Basic Result/]
    G --> H[Schema abrufen]
    H --> I[render.js<br>Generic Renderer]
    I --> J[Basic Accordion anzeigen]
    J --> K[/Message 4: Premium Result/]
    K --> L[Schema abrufen]
    L --> M[render.js<br>Generic Renderer]
    M --> N[Premium Accordion anzeigen]
    N --> O[WebSocket schließt]
    
    E -->|Fehler| P[Fehleranzeige]
    G -->|Fehler| P
    K -->|Fehler| P
```

## 3-Phasen Agent Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Construction: new Agent(dependencies)
    
    Construction --> Initialized: InitializeAsync(contractText)
    Construction --> [*]: Error (nicht initialisiert)
    
    state Construction {
        [*] --> DI: Inject Dependencies
        DI --> Ready: Set _isInitialized = false
        Ready --> [*]
    }
    
    state Initialized {
        [*] --> CheckReadability: AI Call
        CheckReadability --> DetectType: AI Call
        DetectType --> StoreText: Store contractText
        StoreText --> SetReady: Set _isInitialized = true
        SetReady --> [*]
    }
    
    Initialized --> Analysis: AnalyzeBasicAsync() / AnalyzePremiumAsync()
    
    state Analysis {
        [*] --> CheckInit: Check IsInitialized
        CheckInit --> ParallelSteps: If true
        CheckInit --> Error: If false (throw)
        
        state ParallelSteps {
            [*] --> Step1
            [*] --> Step2
            [*] --> Step3
            [*] --> StepN
            
            Step1 --> [*]
            Step2 --> [*]
            Step3 --> [*]
            StepN --> [*]
        }
        
        ParallelSteps --> Aggregate: Task.WhenAll
        Aggregate --> [*]: Return AnalysisDto
    }
    
    Analysis --> [*]: Complete
```

## Logging Architecture Diagram

```mermaid
graph TD
    subgraph Application
        AG[Contract Analysis Agents]
        NEB[NebiusService]
        WSH[WebSocketAnalysisHandler]
        FV[FileValidatorService]
    end
    
    subgraph Logging_Infrastructure
        SERI[Serilog]
        CONS[Console Sink]
        FILE[File Sink<br>Rolling Daily]
    end
    
    subgraph Configuration
        APP[appsettings.json<br>Logging:File]
        FLO[FileLoggingOptions<br>Enabled, MinimumLevel, Path]
    end
    
    subgraph Filesystem
        LOGS[(logs/<br>2024-01-15.log<br>2024-01-16.log<br>...)]
    end
    
    AG -->|Info: Token Summary| SERI
    NEB -->|Debug: Token Details| SERI
    WSH -->|Info: Analysis Events| SERI
    FV -->|Warning: Validation| SERI
    
    APP --> FLO
    FLO --> SERI
    
    SERI --> CONS
    SERI --> FILE
    FILE -->|Daily Rollover| LOGS
    
    style LOGS fill:#e1f5ff
    style SERI fill:#fff4e1
    style FLO fill:#ffe1f5
```

## Token Tracking Flow Diagram

```mermaid
sequenceDiagram
    participant AG as ContractAnalysisAgent
    participant NEB as NebiusService
    participant API as Nebius API
    participant LOG as Logger
    
    Note over AG: Initialize Token Counters<br>TotalInputTokens = 0<br>TotalOutputTokens = 0
    
    loop For Each Analysis Step
        AG->>NEB: SendPromptWithMetadata(prompt, schema)
        NEB->>API: POST /chat/completions
        API-->>NEB: Response + Usage Metadata
        Note over NEB: Extract Token Counts<br>InputTokens, OutputTokens
        NEB->>LOG: Log Token Details [Debug Level]
        NEB-->>AG: AiResponse(Content, InputTokens, OutputTokens, TotalTokens)
        AG->>AG: TrackTokens(AiResponse)<br>TotalInputTokens += InputTokens<br>TotalOutputTokens += OutputTokens
    end
    
    Note over AG: All Steps Complete
    AG->>LOG: LogTokenSummary() [Info Level]
    Note over LOG: Total Input Tokens: X<br>Total Output Tokens: Y<br>Total Tokens: X+Y
```

## Schema Provider with x-displayName

```mermaid
graph TD
    subgraph Schema_Provider
        SP[SchemaProvider]
        RGS[RentalGeneralSchemaProvider]
        RRS[RentalRentSchemaProvider]
        RCS[RentalConditionsSchemaProvider]
        CAS[ClauseAnalysisSchemaProvider]
    end
    
    subgraph Schema_Structure
        SCH[JSON Schema]
        PROPS[Properties]
        PROP1[Property: vermieter<br>type: string<br>x-displayName: Vermieter]
        PROP2[Property: mieter<br>type: string<br>x-displayName: Mieter]
        PROP3[Property: kaltmiete<br>type: number<br>x-displayName: Kaltmiete]
        SECT[x-sectionTitle:<br>Allgemeine Vertragsdaten]
    end
    
    subgraph Frontend
        FE[render.js]
        ACC[Accordion Renderer]
        LABEL[Label Generator]
    end
    
    SP --> RGS
    SP --> RRS
    SP --> RCS
    SP --> CAS
    
    RGS --> SCH
    SCH --> SECT
    SCH --> PROPS
    PROPS --> PROP1
    PROPS --> PROP2
    PROPS --> PROP3
    
    FE -->|Fetch Schema| SP
    SP -->|Return Schema| FE
    FE --> ACC
    ACC -->|Extract x-sectionTitle| LABEL
    ACC -->|Extract x-displayName| LABEL
    LABEL -->|Render German Labels| ACC
    
    style PROP1 fill:#e1ffe1
    style PROP2 fill:#e1ffe1
    style PROP3 fill:#e1ffe1
    style SECT fill:#ffe1e1
```

## UML-Übersicht der Services

```mermaid
classDiagram
    class IAiClient {
        +SendPrompt(prompt, schema)
        +SendPromptWithMetadata(prompt, schema)
    }
    class NebiusService {
        +SendPrompt(prompt, schema)
        +SendPromptWithMetadata(prompt, schema)
    }
    IAiClient <|.. NebiusService

    class AiResponse {
        <<record>>
        +string Content
        +int InputTokens
        +int OutputTokens
        +int TotalTokens
    }
    NebiusService --> AiResponse

    class IContractAnalysisAgent {
        +InitializeAsync(contractText)
        +AnalyzeBasicAsync()
        +AnalyzePremiumAsync()
        +IsInitialized
    }
    class ContractAnalysisAgentBase {
        <<abstract>>
        #IAiClient _aiClient
        #ISchemaProvider _schemaProvider
        #ISchemaValidator _schemaValidator
        #string? _contractText
        #bool _isInitialized
        #int TotalInputTokens
        #int TotalOutputTokens
        #int TotalTokens
        +IsInitialized
        #TrackTokens(AiResponse)
        #LogTokenSummary()
    }
    class RentalContractAgent
    class EmploymentContractAgent
    IContractAnalysisAgent <|.. ContractAnalysisAgentBase
    ContractAnalysisAgentBase <|-- RentalContractAgent
    ContractAnalysisAgentBase <|-- EmploymentContractAgent
    ContractAnalysisAgentBase --> IAiClient
    ContractAnalysisAgentBase --> ISchemaProvider
    ContractAnalysisAgentBase --> ISchemaValidator
    ContractAnalysisAgentBase --> PromptProvider
    ContractAnalysisAgentBase --> AiResponse

    class ISchemaProvider {
        +GetSchema(contractType, tier, step)
        +GetSchemaForEndpoint(contractType, tier, step)
    }
    class SchemaProvider {
        +GetSchema(contractType, tier, step)
        +GetSchemaForEndpoint(contractType, tier, step)
        -Dictionary~string, Func~BinaryData~~ _schemaFactories
    }
    ISchemaProvider <|.. SchemaProvider
    
    note for SchemaProvider "Schemas enthalten:\n- x-displayName für Properties\n- x-sectionTitle für Sections"

    class ISchemaValidator {
        +Validate(schema, response)
        +ValidateWithDetails(schema, response)
    }
    class SchemaValidator
    ISchemaValidator <|.. SchemaValidator

    class PromptProvider {
        <<static>>
        +GetPrompt(contractType, tier, step)
    }
    
    class FileLoggingOptions {
        +bool Enabled
        +string MinimumLevel
        +string Path
    }
    
    note for FileLoggingOptions "Konfiguriert Filesystem-Logging:\n- Täglicher Rollover\n- Format: YYYY-MM-DD.log"

    class WebSocketAnalysisHandler {
        +HandleAsync(context)
        -CreateAgent(contractType)
    }
    WebSocketAnalysisHandler --> IFileValidatorService
    WebSocketAnalysisHandler --> IContractAnalysisAgent

    class IFileDecoder {
        +CanDecode(ext)
        +DecodeAsync(stream)
    }
    class TextFileDecoder
    class PdfFileDecoder
    class ImageFileDecoder
    IFileDecoder <|.. TextFileDecoder
    IFileDecoder <|.. PdfFileDecoder
    IFileDecoder <|.. ImageFileDecoder

    class IFileDecoderService {
        +AllowedExtensions
        +AllowedExtensionsString
        +GetFileContentAsync(filename, stream)
    }
    class FileDecoderService
    IFileDecoderService <|.. FileDecoderService
    FileDecoderService --> IFileDecoder

    class IFileValidatorService {
        +ValidateAndDecodeAsync(file)
    }
    class FileValidatorService
    IFileValidatorService <|.. FileValidatorService
    FileValidatorService --> IFileDecoderService

    class PocketController
    PocketController --> WebSocketAnalysisHandler
    PocketController --> ISchemaProvider
    
    class PromptBuilder {
        <<deprecated>>
    }
    class PromptBuilder_Deprecated {
        <<deprecated>>
    }
    class ClaudeService {
        <<deprecated>>
    }
    class GeminiService {
        <<deprecated>>
    }
    
    note for PromptBuilder "Legacy: Ersetzt durch PromptProvider"
    note for ClaudeService "Legacy: Verwende NebiusService"
    note for GeminiService "Legacy: Verwende NebiusService"
```
