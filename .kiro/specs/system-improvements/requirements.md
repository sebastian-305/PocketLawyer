# Requirements Document

## Introduction

Dieses Dokument beschreibt vier geplante Systemverbesserungen für die PocketLawyer-Anwendung. Die Verbesserungen umfassen Schema-Erweiterungen für Property-Titel, die Isolierung des veralteten PromptBuilders, Token-Tracking für Agenten und die Implementierung eines dateibasierten Logging-Systems.

## Glossary

- **System**: Die PocketLawyer-Anwendung zur Vertragsanalyse
- **Schema**: JSON-Schema-Definition für AI-Responses
- **Property**: Ein Feld im JSON-Schema
- **x-styling**: Erweiterungsfeld im JSON-Schema für Frontend-spezifische Metadaten
- **PromptBuilder**: Veraltete Klasse zur Prompt-Erstellung in `Domain/PromptBuilder.cs`, ersetzt durch PromptProvider
- **PromptBuilder_Deprecated**: Veraltete Klasse zur Prompt-Erstellung in `Infrastructure/PromptBuilder_deprecated1.cs`
- **PromptProvider**: Aktuelle Klasse für Prompt-Management in `Domain/PromptProvider.cs`
- **Legacy Services**: Veraltete AI-Services (ClaudeService, GeminiService), die zukünftig entfernt werden
- **Agent**: Vertragsanalyse-Agent (EmploymentContractAgent, RentalContractAgent)
- **NebiusService**: Der AI-Service, der Token-Informationen von der Nebius-API erhält
- **Token**: Input- und Output-Token-Zähler von AI-Anfragen
- **Filesystem-Logger**: Logger, der Log-Einträge in Dateien speichert
- **Log-Level**: Schweregrad eines Log-Eintrags (Debug, Info, Warning, Error)
- **Configuration**: Anwendungskonfiguration in appsettings.json

## Requirements

### Requirement 1: Schema-basierte Architektur (Single Source of Truth)

**User Story:** Als Entwickler möchte ich, dass Schemas die einzige Quelle für Display-Informationen sind, damit das Frontend vollständig datengetrieben und unabhängig vom Backend arbeitet.

#### Acceptance Criteria

1. WHEN das System ein JSON-Schema definiert, SHALL das System für jede Property einen Displaynamen im Feld "x-displayName" bereitstellen
2. WHEN das System ein JSON-Schema definiert, SHALL das System für jede Section einen Titel im Feld "x-sectionTitle" bereitstellen
3. WHEN das Frontend ein Schema abruft, SHALL das System die Display-Metadaten (x-displayName, x-sectionTitle) im Schema-Response inkludieren
4. WHEN das Frontend eine Property rendert, SHALL das Frontend den Displaynamen aus dem Schema-Feld "x-displayName" verwenden
5. WHEN das Frontend eine Section rendert, SHALL das Frontend den Titel aus dem Schema-Feld "x-sectionTitle" verwenden
6. WHEN das Frontend Daten rendert, SHALL das Frontend nur Properties anzeigen, die im Schema definiert sind
7. WHEN die AI das Schema verarbeitet, SHALL die AI die Frontend-spezifischen Felder (x-displayName, x-sectionTitle, x-styling) ignorieren
8. WHEN das Frontend Daten empfängt, SHALL das Frontend keine hardcodierten Mappings oder Übersetzungen verwenden
9. WHEN ein Schema-Provider ein Schema für mehrere Steps bereitstellt, SHALL jeder Step ein eigenes Schema mit eigenem x-sectionTitle haben

### Requirement 2: Premium-Section-Titel im Schema

**User Story:** Als Entwickler möchte ich, dass jede Premium-Section ihren eigenen Titel im Schema hat, damit das Frontend die korrekten deutschen Titel anzeigt.

#### Acceptance Criteria

1. WHEN das System ein Schema für eine Premium-Section bereitstellt, SHALL das System ein x-sectionTitle-Feld mit dem deutschen Titel enthalten
2. WHEN das System Schemas für Premium-Sections (Costs, Terms, Use, Maintenance, Duties, Privacy) bereitstellt, SHALL jedes Schema einen eindeutigen x-sectionTitle haben
3. WHEN das Frontend eine Premium-Section rendert, SHALL das Frontend den Titel aus dem x-sectionTitle-Feld des Schemas verwenden
4. WHEN ein Schema-Provider für mehrere Steps wiederverwendet wird, SHALL das System für jeden Step ein separates Schema mit eigenem x-sectionTitle erstellen
5. WHEN das Frontend Premium-Sections rendert, SHALL das Frontend keine englischen Step-Namen als Fallback verwenden

### Requirement 3: Legacy Code isolieren

**User Story:** Als Entwickler möchte ich, dass veralteter Code (PromptBuilder, PromptBuilder_Deprecated, ClaudeService, GeminiService) vom Rest des Codes entkoppelt wird, damit diese Klassen keine Abhängigkeiten mehr zu aktiven Codeteilen haben.

#### Acceptance Criteria

1. WHEN das System den PromptBuilder oder PromptBuilder_Deprecated referenziert, SHALL das System keine Abhängigkeiten zu anderen aktiven Klassen haben
2. WHEN das System das AgentType-Enum verwendet, SHALL das System den Enum-Typ durch String-Werte in PromptBuilder und PromptBuilder_Deprecated ersetzen
3. WHEN das System den ClaudeService oder GeminiService referenziert, SHALL das System diese Services als Legacy markieren und isolieren
4. WHEN das System kompiliert wird, SHALL das System keine Compiler-Fehler durch die Isolierung der Legacy Services erzeugen
5. WHEN ein Entwickler einen Legacy Service entfernen möchte, SHALL das System keine Breaking Changes in anderen Codeteilen verursachen
6. WHEN das System Legacy Code verwendet, SHALL das System diese Klassen mit dem Obsolete-Attribut kennzeichnen

### Requirement 4: Token-Tracking für Agenten

**User Story:** Als Entwickler möchte ich, dass Agenten die Gesamtzahl der Input- und Output-Token einer Analyse tracken und loggen, damit ich den Token-Verbrauch pro Analyse nachvollziehen kann.

#### Acceptance Criteria

1. WHEN der NebiusService eine AI-Anfrage sendet, SHALL der NebiusService die Input- und Output-Token-Zahlen zurückgeben
2. WHEN ein Agent eine Analyse durchführt, SHALL der Agent die Token-Zahlen aller AI-Anfragen aggregieren
3. WHEN ein Agent eine Analyse abschließt, SHALL der Agent die gesamten Input-Token mit Log-Level Info loggen
4. WHEN ein Agent eine Analyse abschließt, SHALL der Agent die gesamten Output-Token mit Log-Level Info loggen
5. WHEN ein Agent eine Analyse abschließt, SHALL der Agent die totalen Token (Input + Output) mit Log-Level Info loggen
6. WHEN der NebiusService Token-Informationen loggt, SHALL der NebiusService diese mit Log-Level Debug statt Info loggen
7. WHEN ein Agent mehrere parallele AI-Anfragen durchführt, SHALL der Agent die Token-Zahlen aller Anfragen korrekt summieren

### Requirement 5: Filesystem-Logging

**User Story:** Als Entwickler möchte ich, dass Logs mit konfigurierbarem Level im Filesystem gespeichert werden, damit ich Log-Einträge persistent nachvollziehen kann.

#### Acceptance Criteria

1. WHEN das System konfiguriert wird, SHALL das System ein Minimum-Log-Level für Filesystem-Logging in den Properties definieren können
2. WHEN das System konfiguriert wird, SHALL das System Filesystem-Logging ein- und ausschalten können
3. WHEN Filesystem-Logging aktiviert ist, SHALL das System Log-Einträge mit dem konfigurierten Level oder höher in eine Datei schreiben
4. WHEN das System Log-Dateien erstellt, SHALL das System für jeden Tag eine separate Log-Datei anlegen
5. WHEN das System Log-Dateien speichert, SHALL das System die Dateien im Format "YYYY-MM-DD.log" benennen
6. WHEN das System einen neuen Tag beginnt, SHALL das System automatisch eine neue Log-Datei erstellen
7. WHEN Filesystem-Logging deaktiviert ist, SHALL das System keine Log-Einträge in Dateien schreiben
