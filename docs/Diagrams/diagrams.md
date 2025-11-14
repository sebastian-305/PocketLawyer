# Projekt-Diagramme

## Layered Component Diagram

```mermaid
graph TD
    subgraph Web
        FE[index.html<br>main.js<br>render-*.js]
        NET[network.js]
        PC[PocketController]
    end
    subgraph Application
        AE[AnalyzeEmploymentAgent]
        AR[AnalyzeRentAgent]
    end
    subgraph Domain
        DTOs[ContractValidationDto\nEmploymentContractAnalysisDto\nRentContractAnalysisDto]
    end
    subgraph Infrastructure
        WSAS[WebSocketAgentAnalysisService]
        FVS[FileValidatorService]
        FDS[FileDecoderService]
        KFS[KvFileService]
        KAD[KvAiDetectionService]
        NEB[NebiusService]
        CLA[ClaudeService]
    end
    subgraph External_APIs
        NebiusAPI[(Nebius API)]
        ClaudeAPI[(Claude API)]
    end

    FE --> NET
    NET -- WebSocket --> PC
    PC --> WSAS
    WSAS --> FVS
    FVS --> FDS
    WSAS --> AE
    WSAS --> AR
    AE --> KFS
    AE --> KAD
    AE --> NEB
    AR --> NEB
    KAD --> CLA
    NEB --> NebiusAPI
    CLA --> ClaudeAPI
```

## Domänenmodell (DTOs)

Die Pre-Analyse liefert weiterhin ein `ContractValidationDto`, die eigentliche Auswertung unterscheidet jetzt aber zwischen Arbeits-
und Mietverträgen. Beide Ergebnis-DTOs implementieren `IAnalysisDto`. Zur besseren Lesbarkeit sind die verschachtelten Klassen der
Employment-Analyse im Diagramm mit dem Präfix `Employment*` versehen.

```mermaid
classDiagram
    class IValidationDto
    class IAnalysisDto

    class ContractValidationDto {
        IsReadable: bool
        IsContract: bool
        Result: string
        Kollektivvertrag: string
    }
    IValidationDto <|.. ContractValidationDto

    class EmploymentContractAnalysisDto {
        Keypoints: EmploymentSummary
        UnzulaessigeKlauseln: EmploymentProblemKlausel[]
        NachteileKlauseln: EmploymentProblemKlausel[]
        WiderspruechlicheKlauseln: EmploymentWiderspruechlicheKlausel[]
        VorteilhafteKlauseln: EmploymentVorteilKlausel[]
        Fazit: string
        Kollektivvertrag: string
    }

    class EmploymentSummary {
        <<nested>>
        Arbeitgeber: string
        Taetigkeit: string
        Beginn: string
        Dauer: string
        GehaltBrutto: string
        Zuschlaege: string
        Arbeitsstunden: string
        Ueberstunden: string
        Probezeit: string
    }

    class EmploymentProblemKlausel {
        <<nested>>
        Titel: string
        Zitat: string
        Problem: string
        Rechtsgrundlage: EmploymentRechtsgrundlage[]
    }

    class EmploymentWiderspruechlicheKlausel {
        <<nested>>
        Titel: string
        ZitatKlauselA: string
        ZitatKlauselB: string
        Problem: string
        Rechtsgrundlage: EmploymentRechtsgrundlage[]
    }

    class EmploymentVorteilKlausel {
        <<nested>>
        Titel: string
        Zitat: string
        Vorteil: string
    }

    class EmploymentRechtsgrundlage {
        <<nested>>
        Rechtsgrundlage: string
        Wortlaut: string
    }

    class RentContractAnalysisDto {
        Keypoints: RentSummary
        UnzulaessigeKlauseln: RentProblemKlausel[]
        NachteileKlauseln: RentProblemKlausel[]
        WiderspruechlicheKlauseln: RentWiderspruechlicheKlausel[]
        VorteilhafteKlauseln: RentVorteilKlausel[]
        Fazit: string
        Kollektivvertrag: string
    }

    class RentSummary {
        Vermieter: string
        Mieter: string
        Beginn: string
        Dauer: string
        Mindestdauer: string
        Kuendigungsfrist: string
        MieteKalt: string
        Betriebskosten: string
        Wohnflaeche: string
        Zimmer: int
        ImMietgegenstandEnthalten: string
    }

    class RentProblemKlausel {
        Titel: string
        Zitat: string
        Problem: string
        Rechtsgrundlage: RentRechtsgrundlage[]
    }

    class RentWiderspruechlicheKlausel {
        Titel: string
        ZitatKlauselA: string
        ZitatKlauselB: string
        Problem: string
        Rechtsgrundlage: RentRechtsgrundlage[]
    }

    class RentVorteilKlausel {
        Titel: string
        Zitat: string
        Vorteil: string
    }

    class RentRechtsgrundlage {
        Rechtsgrundlage: string
        Wortlaut: string
    }

    IAnalysisDto <|.. EmploymentContractAnalysisDto
    IAnalysisDto <|.. RentContractAnalysisDto

    EmploymentContractAnalysisDto --> EmploymentSummary
    EmploymentContractAnalysisDto --> EmploymentProblemKlausel
    EmploymentContractAnalysisDto --> EmploymentWiderspruechlicheKlausel
    EmploymentContractAnalysisDto --> EmploymentVorteilKlausel
    EmploymentProblemKlausel --> EmploymentRechtsgrundlage
    EmploymentWiderspruechlicheKlausel --> EmploymentRechtsgrundlage

    RentContractAnalysisDto --> RentSummary
    RentContractAnalysisDto --> RentProblemKlausel
    RentContractAnalysisDto --> RentWiderspruechlicheKlausel
    RentContractAnalysisDto --> RentVorteilKlausel
    RentProblemKlausel --> RentRechtsgrundlage
    RentWiderspruechlicheKlausel --> RentRechtsgrundlage
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

class ClaudeService {
    -ILogger _logger
    +Task<string> SendPrompt(...)
}

IAiClient <|.. NebiusService
IAiClient <|.. ClaudeService

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

interface IKollektivvertragService {
    +string[] GetKvsDirectories()
    +Task<string[]> GetKvFileContentsAsync(string kvName)
}

class KvFileService {
    -ILogger _logger
    -PathOptions _paths
    -IWebHostEnvironment _environment
    -IFileDecoderService _fileDecoderService
    +Task<string[]> GetKvFileContentsAsync(string kvName)
}
IKollektivvertragService <|.. KvFileService
KvFileService --> IFileDecoderService

interface IKvDetectionService {
    +Task<KvDetectionResult> DetectAsync(string contractContent)
}

class KvAiDetectionService {
    -ILogger _logger
    -IKollektivvertragService _kvService
    -IAiClient _aiService
    +Task<KvDetectionResult> DetectAsync(string contractContent)
}
IKvDetectionService <|.. KvAiDetectionService
KvAiDetectionService --> IKollektivvertragService
KvAiDetectionService --> IAiClient

interface IContractAnalyzerAgent {
    +Task<T> ProcessContractRequest(FileContentDto content, AgentAction action, object? data)
}

class AnalyzeEmploymentAgent {
    -ILogger _logger
    -IAiClient _aiService
    -IKollektivvertragService _kvService
    -IKvDetectionService _kvDetectionService
    +Task<T> ProcessContractRequest(...)
}

class AnalyzeRentAgent {
    -ILogger _logger
    -IAiClient _aiService
    +Task<T> ProcessContractRequest(...)
}

IContractAnalyzerAgent <|.. AnalyzeEmploymentAgent
IContractAnalyzerAgent <|.. AnalyzeRentAgent
AnalyzeEmploymentAgent --> IAiClient
AnalyzeEmploymentAgent --> IKollektivvertragService
AnalyzeEmploymentAgent --> IKvDetectionService
AnalyzeRentAgent --> IAiClient

interface IWebSocketAnalysisService {
    +Task HandleAsync(HttpContext context, IContractAnalyzerAgent agent)
}

class WebSocketAgentAnalysisService {
    -ILogger _logger
    -IFileValidatorService _fileValidatorService
    +Task HandleAsync(HttpContext context, IContractAnalyzerAgent agent)
}
IWebSocketAnalysisService <|.. WebSocketAgentAnalysisService
WebSocketAgentAnalysisService --> IFileValidatorService
WebSocketAgentAnalysisService --> IContractAnalyzerAgent

class PocketController {
    -ILogger _logger
    -IWebSocketAnalysisService _socketService
    -IContractAnalyzerAgent _employmentAgent
    -IContractAnalyzerAgent _rentAgent
    +AnalyzeEmploymentViaWebSocket()
    +AnalyzeRentViaWebSocket()
}
PocketController --> IWebSocketAnalysisService
PocketController --> IContractAnalyzerAgent

class FileContentDto {
    +string FileName
    +string FileContent
}

class ContractValidationDto {
    +bool IsReadable
    +bool IsContract
    +string Result
    +string? Kollektivvertrag
}
@enduml
```

## Onion-Layer Sequence Diagram

```mermaid
sequenceDiagram
    participant FE as Browser
    participant WEB as Web (PocketController)
    participant INF as Infrastructure (WebSocketAgentAnalysisService)
    participant APP as Application (Analyze*-Agent)
    participant AI as IAiClient

    FE->>WEB: WebSocket /employment-analysis<br>oder /rent-analysis öffnen
    WEB->>INF: IWebSocketAnalysisService.HandleAsync(agent)
    INF->>INF: ValidateAndDecodeAsync()
    INF-->>APP: FileContentDto
    APP->>AI: SendPrompt(Validation)
    AI-->>APP: ContractValidationDto
    APP-->>INF: ContractValidationDto
    INF-->>WEB: Voranalyse-Ergebnis
    WEB-->>FE: JSON senden

    alt Vertrag gültig
        INF->>APP: ProcessContractRequest(Analyze)
        APP->>AI: SendPrompt(Analyse)
        AI-->>APP: EmploymentContractAnalysisDto<br>oder RentContractAnalysisDto
        APP-->>INF: IAnalysisDto
        INF-->>WEB: Analyse-Ergebnis
        WEB-->>FE: Ergebnis darstellen
    else Vertrag ungültig
        INF-->>WEB: Socket schließen
        WEB-->>FE: Verbindung schließen
    end
```

## Sequence Diagram

```mermaid
sequenceDiagram
    participant FE as Browser
    participant PC as PocketController
    participant WS as WebSocketAgentAnalysisService
    participant FV as FileValidatorService
    participant FDS as FileDecoderService
    participant AG as AnalyzeEmploymentAgent<br>/ AnalyzeRentAgent
    participant KD as KvAiDetectionService
    participant CLA as ClaudeService
    participant NEB as NebiusService

    FE->>PC: WebSocket /employment-analysis<br>oder /rent-analysis öffnen
    FE->>PC: Datei senden
    PC->>WS: HandleAsync(agent)
    WS->>FV: ValidateAndDecodeAsync(file)
    FV->>FDS: GetFileContentAsync()
    FDS-->>FV: Text
    FV-->>WS: FileContentDto
    WS->>AG: ProcessContractRequest<ContractValidationDto>(Validate)
    AG->>NEB: SendPrompt()
    NEB-->>AG: ContractValidationDto
    AG-->>WS: ContractValidationDto
    WS-->>PC: PreAnalysis
    PC-->>FE: PreAnalysis

    alt Vertrag ungültig
        WS-->>PC: Socket schließen
        PC-->>FE: Verbindung geschlossen
    else Vertrag gültig
        opt Arbeitsvertrag
            AG->>KD: DetectAsync(Text)
            KD->>CLA: SendPrompt()
            CLA-->>KD: KvDetectionResult
            KD-->>AG: KvDetectionResult
        end
        WS->>AG: ProcessContractRequest<IAnalysisDto>(Analyze)
        AG->>NEB: SendPrompt()
        NEB-->>AG: EmploymentContractAnalysisDto<br>oder RentContractAnalysisDto
        AG-->>WS: Analyse-DTO
        WS-->>PC: Analyseergebnis
        PC-->>FE: Analyseergebnis
    end
```

## Datenfluss-/Kontextdiagramm

```mermaid
graph TD
    subgraph Browser
        Form[Upload-Formular<br>contractType Auswahl] --> NET[network.js]
        NET --> RE[render-employment.js]
        NET --> RR[render-rent.js]
    end
    NET -- WebSocket --> PCE[/employment-analysis/]
    NET -- WebSocket --> PCR[/rent-analysis/]
    PCE & PCR --> PC[PocketController]
    PC --> WSAS[WebSocketAgentAnalysisService]
    WSAS --> FV[FileValidatorService]
    FV --> FDS[FileDecoderService]
    WSAS --> AGE[AnalyzeEmploymentAgent]
    WSAS --> AGR[AnalyzeRentAgent]
    AGE --> KFS[KvFileService]
    AGE --> KD[KvAiDetectionService]
    KD --> CLA[ClaudeService]
    AGE --> NEB[NebiusService]
    AGR --> NEB
    subgraph External
        NebiusAPI[(Nebius API)]
        ClaudeAPI[(Claude API)]
    end
    NEB --> NebiusAPI
    CLA --> ClaudeAPI
```

## UI-/Flow-Diagramm

```mermaid
flowchart TD
    A[Datei auswählen<br>Vertragstyp setzen] --> B{main.js}
    B -->|rent-analysis| CR[/WebSocket /rent-analysis/]
    B -->|employment-analysis| CE[/WebSocket /employment-analysis/]
    CR & CE -->|JSON| D[PreAnalysis-Result]
    D -->|wenn ok| E[/weitere WebSocket-Nachricht/]
    E -->|rent| FR[render-rent.js]
    E -->|employment| FE[render-employment.js]
    FR --> G[Anzeige aktualisieren]
    FE --> G
    D -->|Fehler| H[Fehleranzeige]
```

## UML-Übersicht der Services

```mermaid
classDiagram
    class IAiClient {
        +SendPrompt(prompt, schema)
    }
    class NebiusService
    class ClaudeService
    IAiClient <|.. NebiusService
    IAiClient <|.. ClaudeService

    class IContractAnalyzerAgent {
        +ProcessContractRequest(content, action, data)
    }
    class AnalyzeEmploymentAgent
    class AnalyzeRentAgent
    IContractAnalyzerAgent <|.. AnalyzeEmploymentAgent
    IContractAnalyzerAgent <|.. AnalyzeRentAgent
    AnalyzeEmploymentAgent --> IAiClient
    AnalyzeEmploymentAgent --> IKvDetectionService
    AnalyzeEmploymentAgent --> IKollektivvertragService
    AnalyzeRentAgent --> IAiClient

    class IWebSocketAnalysisService {
        +HandleAsync(context, agent)
    }
    class WebSocketAgentAnalysisService
    IWebSocketAnalysisService <|.. WebSocketAgentAnalysisService
    WebSocketAgentAnalysisService --> IFileValidatorService
    WebSocketAgentAnalysisService --> IContractAnalyzerAgent

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

    class IKvDetectionService {
        +DetectAsync(contractContent)
    }
    class KvAiDetectionService
    IKvDetectionService <|.. KvAiDetectionService
    KvAiDetectionService --> IAiClient

    class IKollektivvertragService {
        +GetKvsDirectories()
        +GetKvFileContentsAsync(kvName)
    }
    class KvFileService
    IKollektivvertragService <|.. KvFileService
    KvFileService --> IFileDecoderService

    class PocketController
    PocketController --> IWebSocketAnalysisService
    PocketController --> IContractAnalyzerAgent
```
