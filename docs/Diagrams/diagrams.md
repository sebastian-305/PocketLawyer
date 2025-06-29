# Projekt-Diagramme

## Architektur-/Komponentenübersicht

```mermaid
graph TD
    subgraph Browser
        FE[index.html<br>main.js<br>network.js]
    end
    subgraph Backend
        PC[PocketController]
        FVS[FileValidatorService]
        FDS[FileDecoderService]
        CA[ContractAnalyzerService]
        KFS[KvFileService]
        KAD[KvAiDetectionService]
        GEM[GeminiService]
        GCM[GeminiContextManager]
        RL[ResponseLogger]
        CLA[ClaudeService]
    end
    subgraph External_APIs
        GeminiAPI[(Gemini API)]
        ClaudeAPI[(Claude API)]
    end

    FE-->|WebSocket /analysis|PC
    PC-->FVS
    FVS-->FDS
    PC-->KAD
    KAD-->CLA
    PC-->CA
    CA-->GEM
    GEM-->GCM
    GEM-->RL
    CA-->KFS
    GEM-->GeminiAPI
    CLA-->ClaudeAPI
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

## Sequenzdiagramm der Analyse

```mermaid
sequenceDiagram
    participant FE as Browser
    participant PC as PocketController
    participant FV as FileValidatorService
    participant FDS as FileDecoderService
    participant KD as KvAiDetectionService
    participant CLA as ClaudeService
    participant CA as ContractAnalyzerService
    participant GEM as GeminiService

    FE->>PC: WebSocket /analysis öffnen
    FE->>PC: Datei senden
    PC->>FV: ValidateAndDecodeAsync()
    FV->>FDS: GetFileContentAsync()
    FDS-->>FV: Text
    FV-->>PC: FileContentDto
    PC->>KD: DetectAsync(Text)
    KD->>CLA: SendPrompt()
    CLA-->>KD: KV-Name
    KD-->>PC: KvDetectionResult
    PC->>CA: ProcessContractRequest(PreAnalysis)
    CA->>GEM: SendPrompt()
    GEM-->>CA: ContractValidationDto
    CA-->>PC: ContractValidationDto
    PC-->>FE: PreAnalysis
    alt gültig
        PC->>CA: ProcessContractRequest(Analysis)
        CA->>GEM: SendPrompt()
        GEM-->>CA: ContractAnalysisDto
        CA-->>PC: ContractAnalysisDto
        PC-->>FE: Analyseergebnis
    end
    FE-->>PC: Verbindung schließen
```

## Datenfluss-/Kontextdiagramm

```mermaid
graph TD
    subgraph Browser
        U[File Upload] --> N[network.js]
    end
    N -- WebSocket --> PC[WebSocket /analysis]
    PC --> FV[FileValidatorService]
    FV --> FDS[FileDecoderService]
    PC --> KD[KvAiDetectionService]
    KD --> CLA[ClaudeService]
    PC --> CA[ContractAnalyzerService]
    CA --> GEM[GeminiService]
    CA --> KFS[KvFileService]
    GEM --> RL[ResponseLogger]
    GEM --> GCM[GeminiContextManager]
    RL --> Files[Responses/]
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
    class ContractAnalyzerService
    IContractAnalyzerService <|.. ContractAnalyzerService
    ContractAnalyzerService --> IAiService
    ContractAnalyzerService --> IKvDetectionService
    ContractAnalyzerService --> IKollektivvertragService

    class PocketController
    PocketController --> IFileValidatorService
    PocketController --> IKvDetectionService
    PocketController --> IContractAnalyzerService
```
