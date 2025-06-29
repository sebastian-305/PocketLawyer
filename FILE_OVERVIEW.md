# Repository File Overview: PocketLawyer

This document provides a structured summary of all files and directories in the **PocketLawyer** repository. It lists each file along with a brief description of its purpose.

---

# Inhaltsverzeichnis
1. [Root-Verzeichnis](#root-verzeichnis)
2. [Dtos](#dtos)
3. [Infrastructure](#infrastructure)
4. [Service](#service)
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
| `Program.cs`                              | Einstiegspunkt der Webanwendung, konfiguriert Dependency Injection, Middleware und Endpunkte. |
| `PocketController.cs`                     | ASP.NET-Controller mit WebSocket-Endpunkt `/analysis` und Mock-API `/mockdata`. Orchestriert Validierung und Analyse. |
| `DesignPatterns.md`                       | Englische Dokumentation zu verwendeten Entwurfsmustern und Prinzipien. |
| `DesignPatterns_de.md`                    | Deutsche Version der Entwurfsmuster-Dokumentation. |
| `README.md`                               | Hauptdokumentation des Projekts mit Funktionsübersicht, Installation und Diagrammen. |
| `appsettings.json`                        | Standardkonfiguration (Logging, Pfade, Modell-ID, etc.). |
| `appsettings.Development.json`            | Abweichende Einstellungen für Development (z.\u00a0B. LogLevel "Debug"). |
| `Properties/launchSettings.json`          | Profildefinitionen zum Starten der Anwendung (http/https). |

---

## Dtos

Datenübertragungsobjekte für Requests und Responses.

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
| `ClaudeService.cs` | Bindung an die Claude\u2011API via Anthropic SDK; sendet Prompt und gibt JSON-String zurück. |

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
Enthält vorbereitete Gesetzestexte als reine Textdateien, z.\u00a0B.:

- `ABGB-RIS.txt`
- `ARG_Arbeitsruhegesetz-RIS.txt`
- `ASchG_ArbeitnehmerInnenschutzgesetz-RIS.txt`
- `AVRAG_Arbeitsvertragsrechts-Anpassungsgesetz-RIS.txt`
- `AZG_Arbeitszeitgesetz-RIS.txt`
- `AngG_Angestelltengesetz-RIS.txt`
- `ArbVG_Arbeitsverfassungsgesetz-RIS.txt`
- `A\u00dcG_Arbeitskr\u00e4fte\u00fcberlassungsgesetz-RIS.txt`

### FileDecoding

| Datei/Ordner | Zweck |
|--------------|-------|
| `FileDecoderService.cs` | Verwaltet verf\u00fcgbare Decoder-Strategien, liest Inhalt anhand der Dateiendung. |
| `Strategies/TextFileDecoder.cs` | Liest Textdateien als UTF\u2011\u00a8 ein. |
| `Strategies/PdfFileDecoder.cs` | Verwendet PdfPig zum Extrahieren des Textes aus PDF-Dateien. |
| `Strategies/ImageFileDecoder.cs` | Nutzt Tesseract f\u00fcr OCR auf JPG/JPEG/PNG-Bildern. |

### Weitere Services

| Datei | Zweck |
|-------|-------|
| `ContractAnalyzerService.cs` | Fassade zur Verarbeitung von Vertragsanfragen (Prompt-Erstellung, KV-Erkennung, KI-Aufruf). |
| `FileValidatorService.cs` | Pr\u00fcft Dateigr\u00f6\u00dfe/-typ und ruft den FileDecoderService auf. |
| `KvAiDetectionService.cs` | L\u00e4sst von Claude den passenden Kollektivvertrag erkennen. |
| `KvFileService.cs` | Liefert verf\u00fcgbare KV-Ordner und deren Dateiinhalte. |
| `NoLoggingHttpClientBuilderFilter.cs` | Entfernt Logging-Handler aus HttpClient-Pipelines. |
| `PromptBuilder.cs` | Hilfsmethoden zur Generierung der Prompts (Analyse, Voranalyse, KV-Erkennung). |

### RawContextFiles

Enth\u00e4lt urspr\u00fcngliche Fassungen der Gesetzestexte (RTF), Skripte und Gerichtsurteile.

- RTF-Dateien wie `ABGB-RIS.rtf`, `ARG_Arbeitsruhegesetz-RIS.rtf` usw.
- Unterordner `Context/` \u2013 gleiche Gesetzestexte als `.txt`.
- Unterordner `KV/` \u2013 Python-Skripte `import_kv.py` (l\u00e4dt KV-PDFs von vida.at) und `rename.py` (benennt Ordner um).
- Unterordner `OHG/` \u2013 diverse RTF-Gerichtsurteile plus `mindestlohn.txt`.

### KVS

Gro\u00dfe Sammlung an PDFs zu Kollektivvertr\u00e4gen und Gehaltstabellen. Beispiele:

- `kollektivvertrag_aua/KV-Luftfahrt-AUA-Bordpersonal-ab-2018.pdf`
- `kollektivvertrag_flughafenarbeiter_innen/KV-Luftfahrt-Flughafen-Arbeiter-2025.pdf`
- `lohntabellen_eisenbahn/Lohntabelle-Eisenbahn-\u00d6BB-KV-EU.pdf`
- Insgesamt 123 PDF-Dateien (Gehaltstabellen, Rahmen- und Zusatz-KVs usw.).

### tessdata

Tesseract-Sprachmodelle:
- `deu.traineddata`, `deu_latf.traineddata`, `eng.traineddata`, `script/Latin.traineddata`
- `README.md` enth\u00e4lt Hinweise zur Nutzung der *tessdata_fast*-Modelle.

---

## Service

Schnittstellendefinitionen und Ausnahmeklasse.

| Datei | Zweck |
|-------|-------|
| `AiServiceException.cs` | Eigene Exception f\u00fcr Fehler bei KI-Diensten. |
| `IAiService.cs` | Interface f\u00fcr KI-Dienste (`SendPrompt`). |
| `IContractAnalyzerService.cs` | Interface f\u00fcr die Vertragsanalyse. |
| `IFileDecoder.cs` | Basisschnittstelle f\u00fcr einzelne Decoder-Strategien. |
| `IFileDecoderService.cs` | Abstraktion \u00fcber Dateidekodierung. |
| `IFileValidatorService.cs` | Validiert und dekodiert hochgeladene Dateien. |
| `IGeminiContextManager.cs` | Liefert Context Keys f\u00fcr Gemini. |
| `IKollektivvertragService.cs` | Bietet Zugriff auf KV-Dateien. |
| `IKvDetectionService.cs` | Erkennt mittels KI den passenden Kollektivvertrag. |
| `IResponseLogger.cs` | Logging-Schnittstelle zum Speichern von Antworten. |

---

## Diagrams

| Datei | Beschreibung |
|-------|--------------|
| `Architecture-Compontents.png` | Bild einer Architektur\u00fcbersicht. |
| `Data-Flow.png` | Veranschaulicht den Datenfluss innerhalb der Anwendung. |
| `Domainmodell.png` | UML-Diagramm des Dom\u00e4nenmodells. |
| `Sequence.png` | Sequenzdiagramm des Analyse-Workflows. |
| `UI-FLow.png` | Flussdiagramm der UI. |
| `UML.png` | \u00dcbersicht der Service-Klassen als Klassendiagramm. |
| `diagrams.md` | Markdown-Datei mit allen Diagrammen (Mermaid-Code). |

---

## Samples

Beispieldateien zu Demonstrations- und Testzwecken.

| Datei | Zweck |
|-------|-------|
| `contract.txt` | Vollst\u00e4ndiger Musterarbeitsvertrag (deutsch). |
| `ctr2.txt` | Stark verk\u00fcrzte Vertragspassage. |
| `ctr3.txt` | Datei mit wenigen Zeilen/Nummern. |
| `int.txt` | Internationaler/anderer Vertragstext. |
| `nichtlesbaredatei.pdf` | Defektes/ungeeignetes PDF zur Fehlertestung. |
| `test.pdf` | Kleines Beispiel-PDF. |
| `uml_alt.png` | Veraltetes UML-Diagramm. |

---

## Responses

Drei gespeicherte JSON-Dateien (Beispiele f\u00fcr Analyseergebnisse):

- `0.json`
- `1.json`
- `2.json`

---

## wwwroot

Statische Dateien f\u00fcr die Weboberfl\u00e4che.

### Media
Bilder, die in der Startseite verwendet werden:
- `Frau_am_Laptop.jpg`
- `Frau_mit_Vertr\u00e4gen.jpg`
- `Mann_mit_Notizbuch.jpg`
- `klara.png`
- `klara_full.png`

### Weitere Dateien
| Datei | Zweck |
|-------|-------|
| `index.html` | HTML-Seite der Benutzeroberfl\u00e4che (Formular f\u00fcr Upload, Modal zur Ergebnisanzeige usw.). |
| `main.js` | Event-Handler f\u00fcr Dateiupload und Kommunikation mit dem Backend. |
| `network.js` | Stellt WebSocket-Verbindung her, sendet Datei und verarbeitet Antworten. |
| `render.js` | Rendert Voranalyse/Analyse in DOM, enth\u00e4lt Druckfunktion. |
| `styles.css` | Layout- und Design-Definitionen. |
| `print.css` | Spezielle Styles f\u00fcr die Druckansicht. |

---

## Weitere Dateien

Im Repository befinden sich zus\u00e4tzlich Git-bezogene Dateien wie `.gitignore`, `.gitattributes` und der `.git`-Ordner, die Standardinformationen f\u00fcr Versionskontrolle enthalten.

---

Diese Aufstellung deckt alle Dateien (insgesamt ca. 277) ab. PDFs und Bilder wurden lediglich aufgelistet, da sie nicht im Klartext vorliegen. Damit bietet die Markdown-Datei einen umfassenden \u00dcberblick \u00fcber s\u00e4mtliche Inhalte und ihre Funktionen im Projekt PocketLawyer.

