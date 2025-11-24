# PocketLawyer

PocketLawyer ist eine Webanwendung auf Basis von **ASP.NET 8**, die Verträge (Mietverträge, Arbeitsverträge) analysiert. PDF-Dateien werden dabei mit **UglyToad.PdfPig** ausgelesen, während Bilddateien per **Tesseract OCR** verarbeitet werden. Die extrahierten Texte werden an den **Nebius KI-Dienst** gesendet und die Ergebnisse übersichtlich dargestellt.

## Neue Architektur (Agent Refactoring)

Das System wurde auf eine moderne, wartbare Architektur umgestellt, die auf dem **Single-Source-of-Truth-Prinzip** basiert. JSON-Schemas dienen als zentrale Wahrheitsquelle für alle Datenstrukturen.

### 3-Phasen-Agenten-Lifecycle

Alle Vertragsanalyse-Agenten folgen einem einheitlichen Lifecycle:

1. **Konstruktion**: Leichtgewichtige Erstellung mit Dependency Injection
2. **Initialisierung**: Vertragsvalidierung, Typerkennung und Textspeicherung
3. **Analyse**: Parallele Ausführung von Basic- und Premium-Analyse-Steps

### Kernkomponenten

- **Contract Analysis Agents**: Orchestrieren den Analyseworkflow
  - `RentalContractAgent`: Mietvertragsanalyse
  - `EmploymentContractAgent`: Arbeitsvertragsanalyse (Skeleton)
- **Schema Provider**: Stellt JSON-Schemas als Single Source of Truth bereit
- **Schema Validator**: Validiert KI-Antworten gegen Schemas
- **Prompt Provider**: Zentrale Verwaltung aller KI-Prompts
- **WebSocket Handler**: Verwaltet Echtzeit-Kommunikation mit Clients

### Vorteile der neuen Architektur

- **Single Source of Truth**: JSON-Schemas definieren Datenstrukturen zentral
- **Parallele Ausführung**: Alle Analyse-Steps laufen parallel (Task.WhenAll)
- **Fehlertoleranz**: Fehlgeschlagene Steps blockieren andere nicht
- **Erweiterbarkeit**: Neue Vertragstypen einfach hinzufügbar
- **Testbarkeit**: Umfassende Unit- und Property-Based-Tests
- **Wartbarkeit**: Klare Trennung von Verantwortlichkeiten

## Funktionsweise

* **Nebius API**: Hauptdienst für die KI-gestützte Vertragsanalyse
* **UglyToad.PdfPig**: Liest Text direkt aus PDF-Dokumenten
* **Tesseract OCR**: Liest Bilddateien (JPG, PNG) per OCR aus und wandelt sie in Text um
* **NJsonSchema**: Validiert KI-Antworten gegen JSON-Schemas

Eine Datei wird zunächst validiert und der Vertragstyp erkannt. Anschließend werden Basic- und Premium-Analysen parallel durchgeführt. Die Analysen bewerten Klauseln, geben Zusammenfassungen und stellen Verstöße gegen Gesetz und Kollektivvertrag heraus.

## Ordnerstruktur

```text
Domain/              Enthält DTOs, Entitäten und PromptProvider
Application/         Use Cases, Interfaces und Agenten
  Agents/            Contract Analysis Agents (Rental, Employment)
  Interfaces/        Interfaces für Services (ISchemaProvider, ISchemaValidator, etc.)
  Models/            Modelle wie InitializationResult
  Exceptions/        Exception-Hierarchie für Fehlerbehandlung
Infrastructure/      Implementierung der Services
  SchemaProviders/   JSON-Schema-Provider für alle Analyse-Steps
  AiServices/        KI-Service-Implementierungen (Nebius, etc.)
  FileDecoding/      Dateidecoder für PDF, Text, Bilder
Web/                 ASP.NET-Host mit Program.cs und Controllern
wwwroot/             Statische Dateien der Weboberfläche (HTML, JS, CSS, Medien)
PocketLawyer.Tests/  Unit-, Property-Based- und Integrationstests
data/                Daten und Ressourcen
  KVS/               Kollektivverträge für die Analyse
  Context/           Vorverarbeitete Kontextdaten für Prompts
  tessdata/          Sprachdaten für Tesseract OCR
  Samples/           Beispielverträge für Tests
docs/                Dokumentation
  Diagrams/          Architektur- und Ablaufdiagramme
  API_DOCUMENTATION.md  Vollständige API-Dokumentation
  DesignPatterns_de.md  Verwendete Entwurfsmuster
.kiro/specs/         Spezifikationen für Features
  agent-refactoring/ Requirements, Design und Tasks für Agent Refactoring
```

## Onion-Architektur

Die Lösung ist ringförmig aufgebaut, wobei jede äußere Ebene nur auf die innenliegende zugreifen darf. Dadurch bleibt die Kernlogik isoliert von technischen Details.

1. **Domain** – Basis aller Schichten. Unter `Domain/` liegen die DTOs, Entitäten und der PromptProvider, komplett ohne Referenzen auf andere Ebenen.
2. **Application** – Enthält Use Cases, Interfaces und Agenten. Die Agenten orchestrieren den Analyseworkflow und nutzen ausschließlich Typen aus der Domain.
3. **Infrastructure** – Implementiert die Interfaces aus der Application und bindet KI-Dienste, Schema-Provider und Dateisysteme an.
4. **Web** – ASP.NET-Host mit Controllern und statischer Oberfläche. Diese äußere Schicht darf alle inneren verwenden, jedoch nicht umgekehrt.

Weitere Diagramme aller Art befinden sich im Ordner [docs/Diagrams](docs/Diagrams/). Dort gibt es sowohl den Markdown-Quellcode der Diagramme in [diagrams.md](docs/Diagrams/diagrams.md) als auch eine Variante mit den gerenderten Bildern in [diagrams_images.md](docs/Diagrams/diagrams_images.md).

## Endpunkte

### WebSocket-Endpunkte

* `GET /rental-analysis` – WebSocket-Endpunkt für Mietverträge
  - Empfängt Datei + Vertragstyp
  - Sendet Bestätigung nach Initialisierung
  - Sendet Basic-Analyse-Ergebnis
  - Sendet Premium-Analyse-Ergebnis
  - Schließt Verbindung mit NormalClosure

* `GET /employment-analysis` – WebSocket-Endpunkt für Arbeitsverträge
  - Identischer Ablauf wie Mietverträge
  - Verwendet EmploymentContractAgent (Skeleton)

### HTTP-Endpunkte

* `GET /api/schemas/{contractType}/{tier}/{step}` – Liefert JSON-Schema für einen Analyse-Step
  - Beispiel: `/api/schemas/Rental/Basic/General`
  - Rückgabe: JsonObject mit Schema-Definition und x-styling Metadaten

* `GET /employment-mockdata` – Liefert zufällige Beispielergebnisse (JSON) aus dem `data/Responses`-Ordner

## Analyse-Workflow

### 1. Datei-Upload und Validierung

1. `main.js` lädt die gewählte Datei je nach Vertragstyp per WebSocket auf `/rental-analysis` oder `/employment-analysis` hoch
2. `WebSocketAnalysisHandler` empfängt Datei und Vertragstyp über WebSocket
3. `FileValidatorService` prüft Größe (max 20MB) und Dateityp (.pdf, .txt, .jpg, .png)
4. `FileDecoderService` dekodiert den Inhalt (PDF mit PdfPig, Bilder mit Tesseract OCR, Text direkt)

### 2. Agent-Erstellung und Initialisierung (3-Phasen-Lifecycle)

**Phase 1: Konstruktion**
1. `WebSocketAnalysisHandler` erstellt den passenden Agenten basierend auf Vertragstyp
   - `RentalContractAgent` für Mietverträge ("Rental")
   - `EmploymentContractAgent` für Arbeitsverträge ("Employment")
2. Konstruktor injiziert nur Dependencies (IAiClient, ISchemaProvider, ISchemaValidator, ILogger)
3. Keine Geschäftslogik in dieser Phase (< 1ms)

**Phase 2: Initialisierung**
1. Agent wird mit `InitializeAsync(contractText)` initialisiert
2. KI-gestützte Prüfung der Lesbarkeit (Nebius API)
3. KI-gestützte Erkennung des Vertragstyps (Nebius API)
4. Vergleich: Erkannter Typ muss mit erwartetem Typ übereinstimmen
5. Speicherung des Vertragstexts in Agent-Instanz
6. Setzen von `IsInitialized = true`
7. Bei Erfolg: Bestätigungsmeldung über WebSocket (`{ type: "confirmation", message: "..." }`)
8. Bei Fehler: Fehlermeldung über WebSocket, Verbindung wird geschlossen

### 3. Basic-Analyse (Parallel)

**Phase 3: Analyse - Basic Tier**
1. Agent ruft `AnalyzeBasicAsync()` auf
2. Prüfung: `IsInitialized` muss `true` sein (sonst InvalidOperationException)
3. Alle Basic-Steps laufen parallel mit `Task.WhenAll`:
   - **Allgemeine Vertragsdaten** (Vertragstyp, Parteien, Adressen, Beginn)
   - **Konditionen** (Miete/Gehalt, Nebenkosten, Kaution, Zahlungsweise)
   - **Vertragsbedingungen** (Kündigungsfrist, Tierhaltung, Untervermietung)
   - **Zusammenfassung** (Kurzfazit, Gesamtfazit, Risikobewertung)
4. Jeder Step (unabhängig):
   - Holt JSON-Schema vom SchemaProvider (`GetSchema(contractType, "Basic", stepName)`)
   - Holt Prompt vom PromptProvider (`GetPrompt(contractType, "Basic", stepName)`)
   - Sendet Anfrage an Nebius KI mit Prompt + Schema
   - Validiert Antwort gegen Schema (NJsonSchema)
   - Bei Fehler: Kundenfreundliche Fehlermeldung statt Exception (andere Steps laufen weiter)
   - Loggt Dauer auf Info-Level
5. Ergebnisse werden zu `AnalysisDto<RentalBasicAnalysisDto>` zusammengeführt
6. DTO wird als JSON über WebSocket gesendet (`{ type: "result", data: { result, metadata } }`)

### 4. Premium-Analyse (Parallel)

**Phase 3: Analyse - Premium Tier**
1. Agent ruft `AnalyzePremiumAsync()` auf (unabhängig von Basic-Analyse)
2. Prüfung: `IsInitialized` muss `true` sein
3. Alle Premium-Steps laufen parallel mit `Task.WhenAll`:
   - **Mietzahlung und Nebenkosten** (Rechtliche Bewertung, Risikoscore)
   - **Mietdauer und Kündigung** (Klauselanalyse, Rechtsgrundlagen)
   - **Gebrauch der Mietsache** (Nutzungsrechte, Einschränkungen)
   - **Instandhaltung und Reparaturen** (Pflichten, Kostenverteilung)
   - **Rechte und Pflichten** (Vertragspartner-Verpflichtungen)
   - **Datenschutz und Kommunikation** (DSGVO-Konformität)
   - **Zusammenfassung** (Gesamtfazit, Gesamtrisikobewertung)
4. Jeder Step folgt dem gleichen Muster wie Basic-Analyse:
   - Schema abrufen (`GetSchema(contractType, "Premium", stepName)`)
   - Prompt abrufen (`GetPrompt(contractType, "Premium", stepName)`)
   - KI-Anfrage mit Validierung
   - Fehlertoleranz (andere Steps laufen weiter)
5. Ergebnisse werden zu `AnalysisDto<RentalPremiumAnalysisDto>` zusammengeführt
6. DTO wird als JSON über WebSocket gesendet (`{ type: "result", data: { result, metadata } }`)

### 5. Abschluss

1. Nach erfolgreicher Übertragung beider Analyse-Ergebnisse
2. WebSocket-Verbindung wird mit Status `NormalClosure` geschlossen
3. Bei Fehlern: 
   - Fehlermeldung über WebSocket (`{ type: "error", message: "..." }`)
   - Verbindung mit entsprechendem Status geschlossen (z.B. `InvalidPayloadData`, `InternalServerError`)

### Frontend-Integration

1. `main.js` empfängt WebSocket-Nachrichten
2. Bei Confirmation: Loading-Anzeige
3. Bei Basic-Result: 
   - Schema abrufen über `/api/schemas/Rental/Basic/{step}`
   - Generischer Renderer (`render.js`) erstellt Accordion mit x-title Labels
4. Bei Premium-Result:
   - Schema abrufen über `/api/schemas/Rental/Premium/{step}`
   - Generischer Renderer erstellt Accordion
5. Schemas enthalten `x-title` für deutsche Feldnamen und `x-styling` für Darstellungshinweise

## Installation und Ausführung

1. Abhängigkeiten wiederherstellen:

   ```bash
   dotnet restore
   ```

2. Anwendung starten:

   ```bash
   dotnet run
   ```

3. Die Weboberfläche ist danach unter `http://localhost:5112` erreichbar (vgl. `launchSettings.json`)

## Tests ausführen

```bash
# Alle Tests
dotnet test

# Mit Code Coverage
dotnet test --collect:"XPlat Code Coverage"

# Nur Unit Tests
dotnet test --filter "FullyQualifiedName~PocketLawyer.Tests.Domain"

# Nur Property-Based Tests
dotnet test --filter "FullyQualifiedName~PropertyTests"

# Nur Integration Tests
dotnet test --filter "FullyQualifiedName~Integration"
```

## Konfiguration

Benötigte Einstellungen befinden sich in `appsettings.json` bzw. den User-Secrets:

```json
{
  "Nebius": {
    "Key": "your-api-key",
    "ModelId": "model-identifier",
    "BaseUrl": "https://api.nebius.ai"
  },
  "Upload": {
    "MaxSizeMB": 20,
    "AllowedExtensions": [".pdf", ".txt", ".jpg", ".png"]
  },
  "Paths": {
    "TesseractData": "data/tessdata",
    "Responses": "data/Responses",
    "KVS": "data/KVS"
  }
}
```

API-Schlüssel sollten über Umgebungsvariablen oder User-Secrets gesetzt werden und **nicht** im Quellcode liegen.

### User Secrets einrichten

```bash
# User Secrets initialisieren
dotnet user-secrets init

# Nebius API Key setzen
dotnet user-secrets set "Nebius:Key" "your-api-key"
```

## Dokumentation

- **[API Documentation](docs/API_DOCUMENTATION.md)**: Vollständige API-Dokumentation mit Beispielen
- **[Design Patterns](docs/DesignPatterns_de.md)**: Verwendete Entwurfsmuster
- **[Diagrams](docs/Diagrams/)**: Architektur- und Ablaufdiagramme
- **[Spec: Agent Refactoring](.kiro/specs/agent-refactoring/)**: Requirements, Design und Tasks

## Architektur-Highlights

### Single Source of Truth

JSON-Schemas definieren alle Datenstrukturen zentral:

```csharp
// Schema Provider
var schema = _schemaProvider.GetSchema("Rental", "Basic", "General");

// Schema enthält:
// - Feldtypen und Beschreibungen
// - x-styling Metadaten für Frontend
// - Validierungsregeln
```

### Parallele Ausführung

Alle Analyse-Steps innerhalb eines Tiers laufen parallel:

```csharp
var tasks = new[]
{
    AnalyzeGeneralDataAsync(),
    AnalyzeRentConditionsAsync(),
    AnalyzeContractConditionsAsync()
};

var results = await Task.WhenAll(tasks);
```

### Fehlertoleranz

Fehlgeschlagene Steps blockieren andere nicht:

```csharp
try
{
    return await AnalyzeStepAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Step failed");
    return CreateErrorResult("Kundenfreundliche Fehlermeldung");
}
```

### Erweiterbarkeit

Neue Vertragstypen einfach hinzufügen:

```csharp
// 1. Neuen Agent erstellen
public class PurchaseContractAgent : ContractAnalysisAgentBase { }

// 2. Schemas registrieren
_schemaFactories["Purchase_Basic_General"] = () => new PurchaseGeneralSchemaProvider().GetSchema();

// 3. Prompts hinzufügen
"PurchaseBasicGeneral" => PurchaseBasicGeneralPrompt,

// 4. In DI registrieren
services.AddScoped<PurchaseContractAgent>();
```

## Performance

- **Agent-Konstruktion**: < 1ms (nur Dependency Injection)
- **Initialisierung**: ~1-2 Sekunden (KI-Aufrufe für Lesbarkeit und Typerkennung)
- **Basic-Analyse**: ~5-10 Sekunden (4 parallele Steps)
- **Premium-Analyse**: ~10-20 Sekunden (7 parallele Steps)

## Sicherheit

- **Input-Validierung**: Dateigröße und -typ werden geprüft
- **Datenschutz**: Vertragstexte werden nicht geloggt oder persistiert
- **WebSocket-Sicherheit**: WSS (WebSocket Secure) in Produktion verwenden
- **Rate Limiting**: Sollte für Produktionsumgebungen implementiert werden

## Zukünftige Erweiterungen

1. **Weitere Vertragstypen**: Kaufverträge, Dienstverträge, etc.
2. **Caching**: Analyse-Ergebnisse für identische Verträge cachen
3. **Streaming**: Teilergebnisse streamen während Steps abgeschlossen werden
4. **Mehrsprachigkeit**: Unterstützung für Verträge in mehreren Sprachen
5. **Erweiterte Validierung**: Business-Rule-Validierung zusätzlich zu Schema-Validierung
6. **Analytics**: Analyse-Muster und häufige Probleme tracken

## Lizenz

[Lizenzinformationen hier einfügen]

## Kontakt

[Kontaktinformationen hier einfügen]
