# Klausel Klara

**KI-gestützte Vertragsanalyse für österreichische Arbeits- und Mietverträge**


Klausel Klara ist eine produktionsreife Webanwendung, die österreichische Arbeits- und Mietverträge mithilfe von KI analysiert. Entwickelt mit ASP.NET Core und unterstützt durch KI-Dienste, bietet sie umfassende Vertragsanalysen mit Echtzeit-Ergebnissen über WebSocket-Verbindungen.

## Funktionen

- **Vertragsanalyse** - Umfassende Analyse von Arbeits- und Mietverträgen
- **Multi-Format-Unterstützung** - Unterstützt PDF, Bilder (via OCR) und Textdateien
- **Echtzeit-Ergebnisse** - WebSocket-basiertes Streaming der Analyseergebnisse
- **Gestaffelte Analyse** - Basis- und Premium-Analyseebenen für unterschiedliche Detailstufen
- **Schema-gesteuertes UI** - Dynamisches Frontend-Rendering basierend auf JSON-Schemas
- **Österreichischer Rechtskontext** - Spezialisiert auf österreichisches Arbeits- und Mietrecht
- **Produktionsbereit** - Umfassende Tests, Logging und Fehlerbehandlung

## Voraussetzungen

Bevor Sie beginnen, stellen Sie sicher, dass Folgendes installiert ist:

- **.NET SDK** - Version 8.0 oder höher (extrahiert aus `PocketLawyer.csproj`)
- **Git** - Zum Klonen des Repositorys
- **Nebius API-Schlüssel** - API-Schlüssel für den Nebius KI-Dienst

### .NET SDK herunterladen

Falls Sie .NET 8.0 SDK nicht installiert haben, laden Sie es hier herunter:

- [.NET Downloads](https://dotnet.microsoft.com/download)

Überprüfen Sie Ihre Installation:

```bash
dotnet --version
```

## Schnellstart

Folgen Sie diesen Schritten, um Klausel Klara auf Ihrem lokalen Rechner zu starten:

### 1. Repository klonen

```bash
git clone <repository-url>
cd PocketLawyer
```

### 2. Abhängigkeiten wiederherstellen

Alle NuGet-Paketabhängigkeiten wiederherstellen:

```bash
dotnet restore
```

Dies lädt alle Pakete herunter, die in `PocketLawyer.csproj` aufgelistet sind.

### 3. KI-Dienste konfigurieren

Richten Sie Ihren Nebius API-Schlüssel mit User Secrets ein (empfohlen für die Entwicklung):

```bash
dotnet user-secrets init
dotnet user-secrets set "Nebius:Key" "your-nebius-api-key"
```

> **Note:** Committen Sie niemals API-Schlüssel in die Versionskontrolle. Verwenden Sie User Secrets für die Entwicklung und Umgebungsvariablen für die Produktion.

### 4. Kontextdateien einrichten

Stellen Sie sicher, dass das Kontextdatei-Verzeichnis unter `data/Context/` existiert. Diese Dateien enthalten domänenspezifisches Wissen für die KI-Modelle.

✓ Kontextverzeichnis gefunden unter `data/Context/`

### 5. Anwendung starten

Starten Sie die Anwendung:

```bash
dotnet run
```

Die Anwendung startet standardmäßig mit dem HTTP-Profil.

### 6. Browser öffnen

Öffnen Sie Ihren Webbrowser und navigieren Sie zu:

- **HTTP:** http://localhost:5112

> **Note:** Für HTTPS-Unterstützung verwenden Sie: dotnet run --launch-profile https (Port 7218). Dies erfordert ein gültiges Entwicklerzertifikat.

Sie sollten die Klausel Klara Startseite mit Optionen zur Analyse von Arbeits- oder Mietverträgen sehen.

## Verwendung

### Einen Vertrag analysieren

1. Navigieren Sie zur Anwendung in Ihrem Webbrowser
2. Wählen Sie den Vertragstyp (Arbeitsvertrag oder Mietvertrag)
3. Laden Sie Ihre Vertragsdatei hoch (PDF, Bild oder Text)
4. Warten Sie, bis die Analyse abgeschlossen ist
5. Überprüfen Sie die Basis- und Premium-Analyseergebnisse

### Unterstützte Dateiformate

- **PDF** - Textextraktion mit UglyToad.PdfPig
- **Bilder** - JPG, PNG (OCR mit Tesseract)
- **Text** - Einfache Textdateien (.txt)

### Analyseebenen

Klausel Klara bietet zwei Analyseebenen:

#### Basis-Analyse

- Allgemeine Vertragsinformationen
- Wichtige Bedingungen
- Erste Risikobewertung
- Vertragszusammenfassung

#### Premium-Analyse

- Detaillierte Klausel-für-Klausel-Analyse
- Überprüfung der rechtlichen Konformität
- Umfassende Risikobewertung
- Spezifische Empfehlungen und Warnungen

### WebSocket API

Für programmatischen Zugriff verbinden Sie sich mit den WebSocket-Endpunkten:

- **Arbeitsverträge:** `ws://localhost:5112/employment-analysis`
- **Mietverträge:** `ws://localhost:5112/rental-analysis`

> **Note:** Die Ports können in Properties/launchSettings.json konfiguriert werden.

Für detaillierte API-Dokumentation siehe [API_REFERENCE.md](docs/API_REFERENCE.md).

## Dokumentation

Ausführliche Dokumentation finden Sie in den folgenden Dateien:

- [Architektur](docs/ARCHITECTURE.md) - Systemarchitektur und Design-Prinzipien
- [API-Referenz](docs/API_REFERENCE.md) - API-Endpunkte und WebSocket-Protokoll
- [Datenmodelle](docs/DATA_MODELS.md) - DTO-Strukturen und Datenmodelle
- [Konfiguration](docs/CONFIGURATION.md) - Konfigurationsoptionen und Einstellungen
- [Deployment](docs/DEPLOYMENT.md) - Deployment-Anleitung und Systemanforderungen

## Tests

Das Projekt enthält umfassende Tests:

### Tests ausführen

```bash
# Alle Tests ausführen
dotnet test
```

### Test-Coverage

Einen Code-Coverage-Bericht generieren:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test-Kategorien

- **Unit-Tests** - Testen einzelne Komponenten und Methoden
- **Property-Based Tests** - Überprüfen universelle Eigenschaften mit FsCheck
- **Integrationstests** - Testen End-to-End-Workflows

## Technologie-Stack

- **Backend:** ASP.NET Core 8.0
- **KI-Dienst:** Nebius
- **PDF-Verarbeitung:** UglyToad.PdfPig
- **OCR:** Tesseract
- **Schema-Validierung:** NJsonSchema
- **Logging:** Serilog
- **Testing:** xUnit, FsCheck, FluentAssertions
- **Frontend:** Vanilla JavaScript, Bootstrap 5

Für eine vollständige Liste der Abhängigkeiten siehe `PocketLawyer.csproj`.

## Architekturprinzipien

Das Projekt folgt bewährten Softwareentwicklungsprinzipien:

- **Onion-Architektur:** Abhängigkeiten fließen nach innen zur Domäne
- **Single Source of Truth:** JSON-Schemas definieren alle Datenstrukturen
- **Separation of Concerns:** Jede Schicht hat eine spezifische Verantwortung
- **Testbarkeit:** Alle Komponenten sind für einfaches Testen konzipiert

Für detaillierte Architekturinformationen siehe [ARCHITECTURE.md](docs/ARCHITECTURE.md).

---

**Erstellt mit ❤️ für österreichische Vertragsanalyse**
