# Requirements Document

## Introduction

Dieses Dokument beschreibt die Anforderungen für die vollständige Implementierung des Employment Contract Agent (Arbeitsvertrags-Agent). Der Agent analysiert Arbeitsverträge in drei Phasen: Construction (Dependency Injection), Initialization (Validierung und Speicherung), und Analysis (Basic- und Premium-Analyse). Das System nutzt KI-Services zur Vertragsanalyse, validiert Antworten gegen JSON-Schemas und lädt automatisch Kontext-Dateien für jeden Analyse-Step.

## Glossary

- **Employment_Contract_Agent**: Der Agent zur Analyse von Arbeitsverträgen
- **Contract_Analysis_Agent_Base**: Die abstrakte Basisklasse für alle Vertragsanalyse-Agenten
- **AI_Client**: Der Service für KI-Anfragen (Nebius Service)
- **Schema_Provider**: Service zum Bereitstellen von JSON-Schemas
- **Schema_Validator**: Service zur Validierung von AI-Antworten gegen Schemas
- **Context_Provider**: Service zum automatischen Laden von Kontext-Dateien pro Analyse-Step
- **KV_Detection_Service**: Service zur Erkennung des passenden Kollektivvertrags
- **Basic_Analysis**: Grundlegende Vertragsanalyse mit allgemeinen Informationen
- **Premium_Analysis**: Detaillierte rechtliche Bewertung mit KV-Kontext

## Requirements

### Requirement 1: Agent Construction Phase

**User Story:** Als System möchte ich einen Employment Contract Agent durch Dependency Injection erstellen, so dass alle benötigten Services verfügbar sind.

#### Acceptance Criteria

1. WHEN der Employment_Contract_Agent konstruiert wird, THEN THE System SHALL alle erforderlichen Dependencies injizieren
2. WHEN der Employment_Contract_Agent konstruiert wird, THEN THE System SHALL eine eindeutige Agent_Id generieren
3. WHEN der Employment_Contract_Agent konstruiert wird, THEN THE System SHALL den Expected_Contract_Type auf "Arbeitsvertrag" setzen
4. WHEN eine erforderliche Dependency null ist, THEN THE System SHALL eine ArgumentNullException werfen

### Requirement 2: Agent Initialization Phase

**User Story:** Als System möchte ich einen Arbeitsvertrag validieren und initialisieren, so dass nur gültige Arbeitsverträge analysiert werden.

#### Acceptance Criteria

1. WHEN InitializeAsync mit leerem Vertragstext aufgerufen wird, THEN THE System SHALL ein Initialization_Result mit Fehler zurückgeben
2. WHEN der Vertragstext nicht lesbar ist, THEN THE System SHALL ein Initialization_Result mit kundenfreundlicher Fehlermeldung zurückgeben
3. WHEN der erkannte Vertragstyp nicht "Arbeitsvertrag" ist, THEN THE System SHALL ein Initialization_Result mit Typfehler zurückgeben
4. WHEN alle Validierungen erfolgreich sind, THEN THE System SHALL den passenden Kollektivvertrag ermitteln
5. WHEN ein Kollektivvertrag erkannt wird, THEN THE System SHALL den KV-Text für spätere Verwendung speichern
6. WHEN alle Validierungen erfolgreich sind, THEN THE System SHALL den Vertragstext speichern und IsInitialized auf true setzen
7. WHEN eine Exception während der Initialisierung auftritt, THEN THE System SHALL ein Initialization_Result mit Fehlermeldung zurückgeben

### Requirement 3: Basic Analysis Orchestration

**User Story:** Als System möchte ich eine Basic-Analyse eines Arbeitsvertrags durchführen, so dass grundlegende Vertragsinformationen extrahiert werden.

#### Acceptance Criteria

1. WHEN AnalyzeBasicAsync aufgerufen wird und der Agent nicht initialisiert ist, THEN THE System SHALL eine InvalidOperationException werfen
2. WHEN AnalyzeBasicAsync aufgerufen wird, THEN THE System SHALL alle Basic-Analyse-Steps parallel ausführen
3. WHEN alle Basic-Analyse-Steps abgeschlossen sind, THEN THE System SHALL die Ergebnisse in einem Analysis_Dto kombinieren
4. WHEN ein Basic-Analyse-Step fehlschlägt, THEN THE System SHALL ein Fehler-DTO für diesen Step zurückgeben

### Requirement 4: Schema Design and Adaptation

**User Story:** Als Entwickler möchte ich JSON-Schemas für die Arbeitsvertragsanalyse erstellen, so dass die Schemas sich an den alten Schemas orientieren aber sinnvoll an die neue Agentenlogik angepasst sind.

#### Acceptance Criteria

1. WHEN Schemas erstellt werden, THEN THE System SHALL sich an der Struktur der alten Employment_Contract_Analysis_Dto orientieren
2. WHEN Schemas erstellt werden, THEN THE System SHALL die Struktur sinnvoll an die neue Agentenlogik anpassen
3. WHEN die Trennung von Analysen definiert wird, THEN THE System SHALL eine sinnvolle Aufteilung wählen (nicht zwingend identisch mit Voranalyse/Hauptanalyse)
4. WHEN Schemas definiert werden, THEN THE System SHALL alle relevanten Felder aus dem alten Schema berücksichtigen
5. WHEN Schemas definiert werden, THEN THE System SHALL die Felder logisch auf Basic- und Premium-Analyse-Steps verteilen

### Requirement 5: Basic Analysis Steps

**User Story:** Als Benutzer möchte ich grundlegende Arbeitsdaten extrahieren, so dass ich wichtige Vertragsinformationen kenne.

#### Acceptance Criteria

1. WHEN ein Basic-Analyse-Step ausgeführt wird, THEN THE System SHALL das entsprechende JSON-Schema vom Schema_Provider laden
2. WHEN ein Basic-Analyse-Step ausgeführt wird, THEN THE System SHALL den Kontext vom Context_Provider laden
3. WHEN ein Basic-Analyse-Step ausgeführt wird, THEN THE System SHALL den AI_Client mit Prompt und Vertragstext aufrufen
4. WHEN die AI-Antwort empfangen wird, THEN THE System SHALL die Antwort gegen das Schema validieren
5. WHEN die Validierung erfolgreich ist, THEN THE System SHALL die Antwort in ein DTO deserialisieren
6. WHEN ein Fehler auftritt, THEN THE System SHALL ein Fehler-DTO mit kundenfreundlicher Nachricht zurückgeben

### Requirement 6: KV Storage Location

**User Story:** Als Entwickler möchte ich Kollektivverträge an einem für die neue Agentenlogik passenden Ort speichern, so dass sie effizient geladen werden können.

#### Acceptance Criteria

1. WHEN Kollektivverträge gespeichert werden, THEN THE System SHALL sie in einem für die neue Agentenlogik optimierten Verzeichnis ablegen
2. WHEN Kollektivverträge geladen werden, THEN THE System SHALL den KV_Detection_Service verwenden
3. WHEN ein Kollektivvertrag benötigt wird, THEN THE System SHALL ihn aus dem konfigurierten Speicherort laden
4. WHEN Kollektivverträge organisiert werden, THEN THE System SHALL eine klare Namenskonvention verwenden

### Requirement 7: KV Usage in Analysis

**User Story:** Als Benutzer möchte ich den erkannten Kollektivvertrag in der Analyse verwenden, so dass rechtliche Bewertungen KV-konform sind.

#### Acceptance Criteria

1. WHEN die Basic-Analyse ausgeführt wird, THEN THE System SHALL den in der Initialisierung erkannten KV-Namen in der Zusammenfassung erwähnen
2. WHEN kein KV erkannt wurde, THEN THE System SHALL dies in der Zusammenfassung vermerken
3. WHEN die Premium-Analyse ausgeführt wird, THEN THE System SHALL den gespeicherten KV-Text als zusätzlichen Kontext verwenden

### Requirement 8: Premium Analysis Orchestration

**User Story:** Als System möchte ich eine Premium-Analyse eines Arbeitsvertrags durchführen, so dass detaillierte rechtliche Bewertungen erstellt werden.

#### Acceptance Criteria

1. WHEN AnalyzePremiumAsync aufgerufen wird und der Agent nicht initialisiert ist, THEN THE System SHALL eine InvalidOperationException werfen
2. WHEN AnalyzePremiumAsync aufgerufen wird, THEN THE System SHALL alle Premium-Analyse-Steps parallel ausführen
3. WHEN alle Premium-Analyse-Steps abgeschlossen sind, THEN THE System SHALL die Ergebnisse in einem Analysis_Dto kombinieren
4. WHEN ein Premium-Analyse-Step fehlschlägt, THEN THE System SHALL ein Fehler-DTO für diesen Step zurückgeben

### Requirement 8: Premium Analysis Steps with KV Context

**User Story:** Als Benutzer möchte ich Vertragsklauseln rechtlich bewerten, so dass ich potenzielle Probleme erkenne.

#### Acceptance Criteria

1. WHEN ein Premium-Analyse-Step ausgeführt wird, THEN THE System SHALL das entsprechende JSON-Schema vom Schema_Provider laden
2. WHEN ein Premium-Analyse-Step ausgeführt wird, THEN THE System SHALL den Kontext vom Context_Provider laden
3. WHEN ein KV erkannt wurde, THEN THE System SHALL den KV-Text als zusätzlichen Kontext hinzufügen
4. WHEN die AI-Antwort empfangen wird, THEN THE System SHALL die Antwort gegen das Schema validieren
5. WHEN ein Fehler auftritt, THEN THE System SHALL ein Fehler-DTO mit kundenfreundlicher Nachricht zurückgeben

### Requirement 9: Token Tracking

**User Story:** Als System möchte ich alle AI-Token-Verwendungen tracken, so dass ich die Kosten überwachen kann.

#### Acceptance Criteria

1. WHEN eine AI-Anfrage abgeschlossen wird, THEN THE System SHALL die Input-Tokens und Output-Tokens tracken
2. WHEN eine Analyse abgeschlossen ist, THEN THE System SHALL eine Token-Zusammenfassung loggen
3. WHEN Token getrackt werden, THEN THE System SHALL die Gesamtsumme korrekt berechnen

### Requirement 10: Error Handling

**User Story:** Als System möchte ich Fehler kundenfreundlich behandeln, so dass Benutzer verständliche Fehlermeldungen erhalten.

#### Acceptance Criteria

1. WHEN ein Analyse-Step fehlschlägt, THEN THE System SHALL den Fehler loggen
2. WHEN ein Analyse-Step fehlschlägt, THEN THE System SHALL ein Fehler-DTO mit kundenfreundlicher Nachricht zurückgeben
3. WHEN ein Analyse-Step fehlschlägt, THEN THE System SHALL die Analyse nicht abbrechen
4. WHEN alle Steps fehlschlagen, THEN THE System SHALL trotzdem ein vollständiges Analysis_Dto zurückgeben

### Requirement 11: Logging

**User Story:** Als Entwickler möchte ich detaillierte Logs, so dass ich Probleme debuggen kann.

#### Acceptance Criteria

1. WHEN ein Analyse-Step startet, THEN THE System SHALL den Start mit Agent_Id loggen
2. WHEN ein Analyse-Step abgeschlossen ist, THEN THE System SHALL die Ausführungsdauer loggen
3. WHEN ein Fehler auftritt, THEN THE System SHALL den Fehler mit Stack-Trace loggen
4. WHEN Kontext geladen wird, THEN THE System SHALL die Kontext-Länge loggen
