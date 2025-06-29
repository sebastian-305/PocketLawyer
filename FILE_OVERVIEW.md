# Repository File Overview: PocketLawyer

This document provides a structured summary of all files and directories in the **PocketLawyer** repository. It lists each file along with a brief description of its purpose.

---

# Inhaltsverzeichnis
1. [Root-Verzeichnis](#root-verzeichnis)
2. [Domain](#domain)
3. [Application](#application)
4. [Infrastructure](#infrastructure)
5. [Diagrams](#diagrams)
6. [Samples](#samples)
7. [Responses](#responses)
8. [wwwroot](#wwwroot)
9. [Weitere Dateien](#weitere-dateien)

---

## Root-Verzeichnis

| Pfad/Datei                                | Beschreibung |
|-------------------------------------------|--------------|
| `PocketLawyer.sln`                        | Visual-Studio-Lösung, verweist auf das Hauptprojekt. |
| `PocketLawyer.csproj`                     | Projektdatei für .NET 8 mit Abhängigkeiten (Anthropic.SDK, Google_GenerativeAI, PdfPig, Tesseract, Swashbuckle). |
| `Web/Program.cs`                          | Einstiegspunkt der Webanwendung, konfiguriert Dependency Injection, Middleware und Endpunkte. |
| `Web/PocketController.cs`                 | ASP.NET-Controller mit WebSocket-Endpunkt `/analysis` und Mock-API `/mockdata`. Orchestriert Validierung und Analyse. |
| `docs/DesignPatterns_de.md`               | Deutsche Version der Entwurfsmuster-Dokumentation. |
| `README.md`                               | Hauptdokumentation des Projekts mit Funktionsübersicht, Installation und Diagrammen. |
| `FILE_OVERVIEW.md`                        | Übersicht aller Dateien im Repository. |
| `Options/*.cs`                            | Konfigurationsobjekte für Pfade, Upload und API-Schlüssel. |
| `appsettings.json`                        | Standardkonfiguration (Logging, Pfade, Modell-ID, etc.). |
| `appsettings.Development.json`            | Abweichende Einstellungen für Development (z. B. LogLevel "Debug"). |
| `Properties/launchSettings.json`          | Profildefinitionen zum Starten der Anwendung (http/https). |

---

## Domain

Enthält die grundlegenden Datenobjekte (DTOs) und einfache Entitäten.

| Datei | Zweck |
|-------|-------|
| `ContractAnalysisDto.cs` | Struktur für detaillierte Analyseergebnisse inkl. Zusammenfassung und Klauseln. |
| `ContractValidationDto.cs` | Ergebnis der Voranalyse: Lesbarkeit, Vertragsstatus und Zusammenfassung. |
| `FileContentDto.cs` | Record mit Dateiname und Inhalt. |
| `KvDetectionAnswer.cs` | Antwortformat der KI für Kollektivvertragserkennung (ein String). |
| `KvDetectionResult.cs` | Ergebnis mehrerer KI-Aufrufe; enthält KV-Namen und Flag, ob alle Antworten gleich waren. |
| `WebSocketFileDto.cs` | Format einer hochgeladenen Datei über WebSocket (Name + base64-Daten). |

---

## Infrastructure

Implementierungen von Diensten und Hilfsklassen.

### AiServices/Claude
| Datei | Zweck |
|-------|-------|
| `ClaudeService.cs` | Bindung an die Claude‑API via Anthropic SDK; sendet Prompt und gibt JSON-String zurück. |

### AiServices/Gemini
| Datei | Zweck |
|-------|-------|
| `GeminiService.cs` | Kommunikation mit Googles Gemini-API, inkl. Schema-Definitionen und optionalem Zwischenspeichern der Antworten. |
| `GeminiContextManager.cs` | Erstellt bzw. holt einen Context Key (Cached Content) für Gemini. |
| `RedactingLoggingHandler.cs` | Entfernt sensible Teile aus Log-Ausgaben von HTTP-Anfragen. |
| `ResponseLogger.cs` | Speichert KI-Antworten als Datei. |
| `Models/AnalyzerSchema.cs` | JSON-Schema-Klassen für die Hauptanalyse (Summary, Klauseln usw.). |
| `Models/GeminiContext.cs` | Datenmodell zur Verarbeitung der Gemini-Context-API. |
| `Models/GeminiResponse.cs` | Strukturen zum Deserialisieren der Gemini-Antwort. |

### Context
Dateien befinden sich in `data/Context/`.
Enthält vorbereitete Gesetzestexte als reine Textdateien, z. B.:

- `ABGB-RIS.txt`
- `ARG_Arbeitsruhegesetz-RIS.txt`
- `ASchG_ArbeitnehmerInnenschutzgesetz-RIS.txt`
- `AVRAG_Arbeitsvertragsrechts-Anpassungsgesetz-RIS.txt`
- `AZG_Arbeitszeitgesetz-RIS.txt`
- `AngG_Angestelltengesetz-RIS.txt`
- `ArbVG_Arbeitsverfassungsgesetz-RIS.txt`
- `AÜG_Arbeitskräfteüberlassungsgesetz-RIS.txt`

### FileDecoding

| Datei/Ordner | Zweck |
|--------------|-------|
| `FileDecoderService.cs` | Verwaltet verfügbare Decoder-Strategien, liest Inhalt anhand der Dateiendung. |
| `Strategies/TextFileDecoder.cs` | Liest Textdateien als UTF‑¨ ein. |
| `Strategies/PdfFileDecoder.cs` | Verwendet PdfPig zum Extrahieren des Textes aus PDF-Dateien. |
| `Strategies/ImageFileDecoder.cs` | Nutzt Tesseract für OCR auf JPG/JPEG/PNG-Bildern. |

### Weitere Services

| Datei | Zweck |
|-------|-------|
| `AnalyzeContractUseCase.cs` | Fassade zur Verarbeitung von Vertragsanfragen (Prompt-Erstellung, KV-Erkennung, KI-Aufruf). |
| `DefaultPaths.cs` | Definiert Standardverzeichnisse. |
| `ExceptionTranslatorMiddleware.cs` | Übersetzt Domänenfehler in HTTP-Antworten. |
| `FileValidatorService.cs` | Prüft Dateigröße/-typ und ruft den FileDecoderService auf. |
| `KvAiDetectionService.cs` | Lässt von Claude den passenden Kollektivvertrag erkennen. |
| `KvFileService.cs` | Liefert verfügbare KV-Ordner und deren Dateiinhalte. |
| `NoLoggingHttpClientBuilderFilter.cs` | Deaktiviert ausführliches HttpClient-Logging. |
| `PromptBuilder.cs` | Hilfsmethoden zur Generierung der Prompts (Analyse, Voranalyse, KV-Erkennung). |
| `WebSocketAnalysisService.cs` | Kümmert sich um den kompletten WebSocket-Workflow. |
| `WebSocketHelpers.cs` | Helfermethoden für WebSocket-Antworten. |

### RawContextFiles
Dateien befinden sich in `data/RawContextFiles/`.

Enthält ursprüngliche Fassungen der Gesetzestexte (RTF), Skripte und Gerichtsurteile.

- RTF-Dateien wie `ABGB-RIS.rtf`, `ARG_Arbeitsruhegesetz-RIS.rtf` usw.
- Unterordner `Context/` – gleiche Gesetzestexte als `.txt`.
- Unterordner `KV/` – Python-Skripte `import_kv.py` (lädt KV-PDFs von vida.at) und `rename.py` (benennt Ordner um).
- Unterordner `OHG/` – diverse RTF-Gerichtsurteile plus `mindestlohn.txt`.

### KVS
Dateien befinden sich in `data/KVS/`.

Große Sammlung an PDFs zu Kollektivverträgen und Gehaltstabellen. Beispiele:

- `kollektivvertrag_aua/KV-Luftfahrt-AUA-Bordpersonal-ab-2018.pdf`
- `kollektivvertrag_flughafenarbeiter_innen/KV-Luftfahrt-Flughafen-Arbeiter-2025.pdf`
- `lohntabellen_eisenbahn/Lohntabelle-Eisenbahn-ÖBB-KV-EU.pdf`
- Insgesamt 123 PDF-Dateien (Gehaltstabellen, Rahmen- und Zusatz-KVs usw.).

### tessdata
Dateien befinden sich in `data/tessdata/`.

Tesseract-Sprachmodelle:
- `deu.traineddata`, `deu_latf.traineddata`, `eng.traineddata`, `script/Latin.traineddata`
- `README.md` enthält Hinweise zur Nutzung der *tessdata_fast*-Modelle.

---

## Application

Beinhaltet die Use Cases und Schnittstellen zu externen Diensten.

| Datei | Zweck |
|-------|-------|
| `AiServiceException.cs` | Eigene Exception für Fehler bei KI-Diensten. |
| `IAiClient.cs` | Interface für KI-Dienste (`SendPrompt`). |
| `IContractAnalyzerService.cs` | Interface für die Vertragsanalyse. |
| `IFileDecoder.cs` | Basisschnittstelle für einzelne Decoder-Strategien. |
| `IFileDecoderService.cs` | Abstraktion über Dateidekodierung. |
| `IFileValidatorService.cs` | Validiert und dekodiert hochgeladene Dateien. |
| `IGeminiContextManager.cs` | Liefert Context Keys für Gemini. |
| `IKollektivvertragService.cs` | Bietet Zugriff auf KV-Dateien. |
| `IKvDetectionService.cs` | Erkennt mittels KI den passenden Kollektivvertrag. |
| `IResponseLogger.cs` | Logging-Schnittstelle zum Speichern von Antworten. |
| `UseCases/AnalyzeContractUseCase.cs` | Kern-Use-Case für die Vertragsanalyse. |
| `UseCases/DetectKvUseCase.cs` | Delegiert die KV-Erkennung an einen externen Dienst. |

---

## Diagrams
Die Bilder liegen im Ordner `docs/Diagrams/`.

| Datei | Beschreibung |
|-------|--------------|
| `docs/Diagrams/Architecture-Components.png` | Bild einer Architekturübersicht. |
| `docs/Diagrams/Data-Flow.png` | Veranschaulicht den Datenfluss innerhalb der Anwendung. |
| `docs/Diagrams/Domainmodell.png` | UML-Diagramm des Domänenmodells. |
| `docs/Diagrams/SequenceOnionLayer.png` | Sequenzdiagramm der Onion-Architektur. |
| `docs/Diagrams/SequenceFull.png` | Vollständiges Sequenzdiagramm. |
| `docs/Diagrams/UI-FLow.png` | Flussdiagramm der UI. |
| `docs/Diagrams/Plant_UML.png` | Beispiel für ein PlantUML-Diagramm. |
| `docs/Diagrams/UML.png` | Übersicht der Service-Klassen als Klassendiagramm. |
| `docs/Diagrams/diagrams.md` | Markdown-Datei mit allen Diagrammen (Mermaid-Code). |
| `docs/Diagrams/diagrams_images.md` | Variante mit gerenderten Bildern. |

---

## Samples
Die Dateien befinden sich in `data/Samples/`.

Beispieldateien zu Demonstrations- und Testzwecken.

| Datei | Zweck |
|-------|-------|
| `data/Samples/contract.txt` | Vollständiger Musterarbeitsvertrag (deutsch). |
| `data/Samples/ctr2.txt` | Stark verkürzte Vertragspassage. |
| `data/Samples/ctr3.txt` | Datei mit wenigen Zeilen/Nummern. |
| `data/Samples/int.txt` | Internationaler/anderer Vertragstext. |
| `data/Samples/nichtlesbaredatei.pdf` | Defektes/ungeeignetes PDF zur Fehlertestung. |
| `data/Samples/test.pdf` | Kleines Beispiel-PDF. |
| `data/Samples/uml_alt.png` | Veraltetes UML-Diagramm. |

---

## Responses
Diese Dateien liegen in `data/Responses/`.

Drei gespeicherte JSON-Dateien (Beispiele für Analyseergebnisse):

- `data/Responses/0.json`
- `data/Responses/1.json`
- `data/Responses/2.json`

---

## wwwroot

Statische Dateien für die Weboberfläche.

### Media
Bilder, die in der Startseite verwendet werden:
- `Frau_am_Laptop.jpg`
- `Frau_mit_Verträgen.jpg`
- `Mann_mit_Notizbuch.jpg`
- `klara.png`
- `klara_full.png`

### Weitere Dateien
| Datei | Zweck |
|-------|-------|
| `index.html` | HTML-Seite der Benutzeroberfläche (Formular für Upload, Modal zur Ergebnisanzeige usw.). |
| `main.js` | Event-Handler für Dateiupload und Kommunikation mit dem Backend. |
| `network.js` | Stellt WebSocket-Verbindung her, sendet Datei und verarbeitet Antworten. |
| `render.js` | Rendert Voranalyse/Analyse in DOM, enthält Druckfunktion. |
| `styles.css` | Layout- und Design-Definitionen. |
| `print.css` | Spezielle Styles für die Druckansicht. |

---

## Weitere Dateien

Im Repository befinden sich zusätzlich Git-bezogene Dateien wie `.gitignore`, `.gitattributes` und der `.git`-Ordner, die Standardinformationen für Versionskontrolle enthalten.

---

Diese Aufstellung deckt alle Dateien (insgesamt ca. 261) ab. PDFs und Bilder wurden lediglich aufgelistet, da sie nicht im Klartext vorliegen. Damit bietet die Markdown-Datei einen umfassenden Überblick über sämtliche Inhalte und ihre Funktionen im Projekt PocketLawyer.

