# Projekt-Diagramme

## Layered Component Diagram

```mermaid
graph TD
    subgraph Web
        FE[index.html<br>main.js]
        PC[PocketController]
    end
    subgraph Application
        WSAS[WebSocketAnalysisService]
        FVS[FileValidatorService]
        CA[AnalyzeContractUseCase]
        KAD[KvAiDetectionService]
    end
    subgraph Domain
        DTOs[ContractValidationDto\nContractAnalysisDto]
    end
    subgraph Infrastructure
        FDS[FileDecoderService]
        GEM[GeminiService]
        CLA[ClaudeService]
        KFS[KvFileService]
        RL[ResponseLogger]
        GCM[GeminiContextManager]
    end
    subgraph External_APIs
        GeminiAPI[(Gemini API)]
        ClaudeAPI[(Claude API)]
    end

    FE -- WebSocket --> PC
    PC --> WSAS
    WSAS --> FVS
    WSAS --> KAD
    WSAS --> CA
    FVS --> FDS
    KAD --> CLA
    CA --> GEM
    CA --> KFS
    GEM --> GCM
    GEM --> RL
    GEM --> GeminiAPI
    CLA --> ClaudeAPI
```

## Domänenmodell (DTOs)

```mermaid
classDiagram
    class ContractAnalysisDto {
        Summary summary
        ProblemKlausel[] unzulaessige_klauseln
        ProblemKlausel[] nachteile_klauseln
        WiderspruechlicheKlausel[] widerspruechliche_klauseln
        VorteilKlausel[] vorteilhafte_klauseln
        string fazit
        string? kollektivvertrag
    }
    class Summary {
        string arbeitgeber
        string taetigkeit
        string beginn
        string dauer
        string gehalt_brutto
        string zuschlaege
        string arbeitsstunden
        string ueberstunden
        string probezeit
    }
    class ProblemKlausel {
        string titel
        string zitat
        string problem
        RechtsgrundlageEintrag[] rechtsgrundlage
    }
    class WiderspruechlicheKlausel {
        string titel
        string zitat_klausel_a
        string zitat_klausel_b
        string problem
        RechtsgrundlageEintrag[] rechtsgrundlage
    }
    class VorteilKlausel {
        string titel
        string zitat
        string vorteil
    }
    class RechtsgrundlageEintrag {
        string rechtsgrundlage
        string wortlaut
    }
    class ContractValidationDto {
        bool isReadable
        bool isContract
        string result
        string? kollektivvertrag
    }
    class KvDetectionResult {
        string KvName
        bool AllEqual
    }
    class FileContentDto {
        string FileName
        string FileContent
    }
    class WebSocketFileDto {
        string fileName
        string data
    }

    ContractAnalysisDto --> Summary
    ContractAnalysisDto --> ProblemKlausel
    ContractAnalysisDto --> WiderspruechlicheKlausel
    ContractAnalysisDto --> VorteilKlausel
    ProblemKlausel --> RechtsgrundlageEintrag
    WiderspruechlicheKlausel --> RechtsgrundlageEintrag
```

## PlantUML

```plantuml
@startuml
interface IAiClient {
    +Task<string> SendPrompt(string prompt, object? outputSchema, bool useAltKey)
}

class GeminiService {
    -HttpClient _httpClient
    -GeminiOptions _options
    -ILogger _logger
    -bool _writeInFile
    -IGeminiContextManager _contextManager
    -IResponseLogger _responseLogger
    +Task<string> SendPrompt(...)
}

class ClaudeService {
    -AnthropicClient _client
    -ILogger _logger
    +Task<string> SendPrompt(...)
}

IAiClient <|.. GeminiService
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
    +string[] AllowedExtensions
    +string AllowedExtensionsString
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
    +string[] GetKvsDirectories()
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

interface IContractAnalyzerService {
    +Task<T> ProcessContractRequest(FileContentDto content, Func<string,string,string> promptBuilder, object schema, bool useAltKey, bool detectKv = false, string? kvName = null)
}

class AnalyzeContractUseCase {
    -ILogger _logger
    -IAiClient _aiService
    -IKollektivvertragService _kvService
    -IKvDetectionService _kvDetectionService
    +Task<T> ProcessContractRequest(...)
}
IContractAnalyzerService <|.. AnalyzeContractUseCase
AnalyzeContractUseCase --> IAiClient
AnalyzeContractUseCase --> IKollektivvertragService
AnalyzeContractUseCase --> IKvDetectionService

interface IWebSocketAnalysisService {
    +Task HandleAsync(HttpContext context)
}

class WebSocketAnalysisService {
    -ILogger _logger
    -IFileValidatorService _fileValidatorService
    -IKvDetectionService _kvDetectionService
    -IContractAnalyzerService _analyzeContractUseCase
    +Task HandleAsync(HttpContext context)
}
IWebSocketAnalysisService <|.. WebSocketAnalysisService
WebSocketAnalysisService --> IFileValidatorService
WebSocketAnalysisService --> IKvDetectionService
WebSocketAnalysisService --> IContractAnalyzerService

class PocketController {
    -ILogger _logger
    -IWebHostEnvironment _environment
    -PathOptions _paths
    -IWebSocketAnalysisService _socketService
    +GetMockData()
    +AnalyzeViaWebSocket()
}
PocketController --> IWebSocketAnalysisService

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

class Summary {
    +Arbeitgeber
    +Taetigkeit
    +Beginn
    +Dauer
    +GehaltBrutto
    +Zuschlaege
    +Arbeitsstunden
    +Ueberstunden
    +Probezeit
}

class RechtsgrundlageEintrag {
    +Rechtsgrundlage
    +Wortlaut
}

class ProblemKlausel {
    +Titel
    +Zitat
    +Problem
    +RechtsgrundlageEintrag[] Rechtsgrundlage
}

class WiderspruechlicheKlausel {
    +Titel
    +ZitatKlauselA
    +ZitatKlauselB
    +Problem
    +RechtsgrundlageEintrag[] Rechtsgrundlage
}

class VorteilKlausel {
    +Titel
    +Zitat
    +Vorteil
}

class ContractAnalysisDto {
    +Summary Summary
    +ProblemKlausel[] UnzulaessigeKlauseln
    +ProblemKlausel[] NachteileKlauseln
    +WiderspruechlicheKlausel[] WiderspruechlicheKlauseln
    +VorteilKlausel[] VorteilhafteKlauseln
    +string Fazit
    +string? Kollektivvertrag
}

ContractAnalysisDto --> Summary
ContractAnalysisDto --> ProblemKlausel
ContractAnalysisDto --> WiderspruechlicheKlausel
ContractAnalysisDto --> VorteilKlausel
ProblemKlausel --> RechtsgrundlageEintrag
WiderspruechlicheKlausel --> RechtsgrundlageEintrag
@enduml
```

## Onion-Layer Sequence Diagram

```mermaid
sequenceDiagram
    participant FE as Browser
    participant WEB as Web
    participant APP as Application
    participant INF as Infrastructure

    FE->>WEB: Datei hochladen (WebSocket)
    WEB->>APP: Voranalyse starten
    APP->>INF: KI-Aufruf
    INF-->>APP: ContractValidationDto
    APP-->>WEB: Voranalyse-Ergebnis
    WEB-->>FE: Ergebnis zurückgeben

    alt Vertrag gültig
        WEB->>APP: Hauptanalyse
        APP->>INF: KI-Aufruf
        INF-->>APP: ContractAnalysisDto
        APP-->>WEB: Analyse-Ergebnis
        WEB-->>FE: Analyse senden
    else Vertrag ungültig
        WEB-->>FE: Verbindung schließen
    end
```

## Sequence Diagram

```mermaid
sequenceDiagram
    participant FE as Browser
    participant PC as PocketController
    participant WS as WebSocketAnalysisService
    participant FV as FileValidatorService
    participant FDS as FileDecoderService
    participant KD as KvAiDetectionService
    participant CLA as ClaudeService
    participant CA as AnalyzeContractUseCase
    participant GEM as GeminiService

    FE->>PC: WebSocket /analysis öffnen
    FE->>PC: Datei senden
    PC->>WS: HandleAsync()
    WS->>FV: ValidateAndDecodeAsync(file)
    FV->>FDS: GetFileContentAsync()
    FDS-->>FV: Text
    FV-->>WS: FileContentDto
    WS->>KD: DetectAsync(Text)
    KD->>CLA: SendPrompt()
    CLA-->>KD: KV-Name
    KD-->>WS: KvDetectionResult
    WS->>CA: ProcessContractRequest(PreAnalysis)
    CA->>GEM: SendPrompt()
    GEM-->>CA: ContractValidationDto
    CA-->>WS: ContractValidationDto
    WS-->>PC: PreAnalysis
    PC-->>FE: PreAnalysis

    alt Datei ungültig
        WS-->>PC: Socket schließen
        PC-->>FE: Verbindung geschlossen
    else Datei gültig
        WS->>CA: ProcessContractRequest(Analysis)
        CA->>GEM: SendPrompt()
        GEM-->>CA: ContractAnalysisDto
        CA-->>WS: ContractAnalysisDto
        WS-->>PC: Analyseergebnis
        PC-->>FE: Analyseergebnis
    end
```

## Datenfluss-/Kontextdiagramm

```mermaid
graph TD
    subgraph Browser
        U[File Upload] --> N[network.js]
    end
    N -- WebSocket --> PC[WebSocket /analysis]
    PC --> WSAS[WebSocketAnalysisService]
    WSAS --> FV[FileValidatorService]
    FV --> FDS[FileDecoderService]
    WSAS --> KD[KvAiDetectionService]
    KD --> CLA[ClaudeService]
    WSAS --> CA[AnalyzeContractUseCase]
    CA --> GEM[GeminiService]
    CA --> KFS[KvFileService]
    GEM --> RL[ResponseLogger]
    GEM --> GCM[GeminiContextManager]
    RL --> Files[data/Responses/]
    subgraph External
        GeminiAPI[(Gemini API)]
        ClaudeAPI[(Claude API)]
    end
    GEM --> GeminiAPI
    CLA --> ClaudeAPI
```

## UI-/Flow-Diagramm

```mermaid
flowchart TD
    A[Datei auswählen] --> B{main.js}
    B -->|startAnalysisSocket| C[/WebSocket /analysis/]
    C -->|JSON| D[PreAnalysis-Result]
    D -->|wenn ok| E[/weitere WebSocket-Nachricht/]
    E -->|JSON| F[Analyseergebnis]
    F --> G[render.js zeigt Ergebnis]
    D -->|Fehler| H[Fehleranzeige]
```

## UML-Übersicht der Services

```mermaid
classDiagram
    class IAiService {
        +SendPrompt(prompt, schema, useAltKey)
    }
    class GeminiService
    class ClaudeService
    IAiService <|.. GeminiService
    IAiService <|.. ClaudeService

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
    KvAiDetectionService --> IKollektivvertragService
    KvAiDetectionService --> IAiService

    class IKollektivvertragService {
        +GetKvsDirectories()
        +GetKvFileContentsAsync(kvName)
    }
    class KvFileService
    IKollektivvertragService <|.. KvFileService

    class IContractAnalyzerService {
        +ProcessContractRequest(content, promptBuilder, schema, useAltKey, detectKv, kvName)
    }
    class AnalyzeContractUseCase
    IContractAnalyzerService <|.. AnalyzeContractUseCase
    AnalyzeContractUseCase --> IAiService
    AnalyzeContractUseCase --> IKvDetectionService
    AnalyzeContractUseCase --> IKollektivvertragService

    class IWebSocketAnalysisService {
        +HandleAsync(context)
    }
    class WebSocketAnalysisService
    IWebSocketAnalysisService <|.. WebSocketAnalysisService
    WebSocketAnalysisService --> IFileValidatorService
    WebSocketAnalysisService --> IKvDetectionService
    WebSocketAnalysisService --> IContractAnalyzerService

    class PocketController
    PocketController --> IWebSocketAnalysisService
```
