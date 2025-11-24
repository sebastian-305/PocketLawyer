# Requirements Document

## Introduction

Das PocketLawyer-System soll eine neue Agentenarchitektur erhalten, die Vertragsanalysen effizienter und wartbarer durchführt. Die neue Architektur basiert auf dem Single-Source-of-Truth-Prinzip, bei dem JSON-Schemas die zentrale Wahrheitsquelle für Datenstrukturen bilden. Das System soll Basic- und Premium-Analysen für verschiedene Vertragstypen (Mietvertrag, Arbeitsvertrag) durchführen, wobei einzelne Analyse-Schritte parallel ablaufen können, um die Gesamtdauer zu reduzieren.

## Glossary

- **Agent**: Eine Komponente, die Vertragsanalysen durchführt und den Analyseprozess orchestriert
- **Basic-Analyse**: Die erste Analysestufe, die grundlegende Vertragsinformationen extrahiert (Allgemeine Daten, Konditionen, Bedingungen)
- **Premium-Analyse**: Die zweite Analysestufe, die eine detaillierte rechtliche Bewertung mit Risikoscores durchführt
- **Single-Source-of-Truth**: Ein Designprinzip, bei dem JSON-Schemas die zentrale Definition für Datenstrukturen bilden
- **Nebius**: Der AI-Service-Provider, der für alle KI-Anfragen verwendet wird
- **WebSocket**: Das Kommunikationsprotokoll zwischen Frontend und Backend für Echtzeit-Datenübertragung
- **Schema-Provider**: Ein Service, der JSON-Schemas für verschiedene Analysetypen bereitstellt
- **Schema-Validator**: Ein Service, der AI-Antworten gegen JSON-Schemas validiert
- **Analyse-Step**: Ein einzelner, unabhängiger Schritt innerhalb einer Basic- oder Premium-Analyse
- **Contract-Type**: Die Art des Vertrags (Mietvertrag, Arbeitsvertrag), wird vom Frontend beim Upload mitgesendet
- **Risk-Score**: Eine Bewertung einer Vertragsklausel (Zulässig, Nachteilhaft, Unzulässig)
- **Risk-Level**: Eine Gesamtbewertung des Vertrags (Grün, Gelb, Rot)
- **Styling-Information**: Metadaten im JSON-Schema, die dem Frontend mitteilen, wie Daten dargestellt werden sollen (z.B. hervorgehoben, Warnung)

## Requirements

### Requirement 1

**User Story:** Als Benutzer möchte ich einen Vertrag hochladen und analysieren lassen, damit ich schnell eine rechtliche Einschätzung erhalte.

#### Acceptance Criteria

1. WHEN ein Benutzer einen Vertrag hochlädt THEN das System SHALL eine WebSocket-Verbindung etablieren und den Vertrag mit Vertragstyp empfangen
2. WHEN das System einen Vertrag empfängt THEN das System SHALL die Dateiinformationen und den Vertragstyp extrahieren
3. WHEN die Dateiinformationen extrahiert sind THEN das System SHALL die Datei validieren und dekodieren
4. WHEN die Validierung fehlschlägt THEN das System SHALL eine Fehlermeldung über das WebSocket senden und die Verbindung schließen
5. WHEN die Validierung erfolgreich ist THEN das System SHALL mit der Analyse fortfahren

### Requirement 2

**User Story:** Als System möchte ich Agenten in drei Phasen betreiben, damit die Verantwortlichkeiten klar getrennt sind.

#### Acceptance Criteria

1. WHEN ein Agent konstruiert wird THEN das System SHALL ausschließlich Abhängigkeiten injizieren ohne Vertragslogik auszuführen
2. WHEN Abhängigkeiten injiziert werden THEN das System SHALL KI-Client, Schema-Provider und Validatoren übergeben
3. WHEN der Agent konstruiert ist THEN das System SHALL den Zustand auf nicht-initialisiert setzen
4. WHEN eine Analyse-Methode aufgerufen wird und der Agent nicht initialisiert ist THEN das System SHALL einen Fehler werfen
5. WHEN der Agent für die Dauer einer Analyse verwendet wird THEN das System SHALL den Zustand in der Instanz speichern

### Requirement 3

**User Story:** Als System möchte ich Agenten initialisieren, damit nur geprüfte Verträge analysiert werden.

#### Acceptance Criteria

1. WHEN ein Agent initialisiert wird THEN das System SHALL den Vertragstext als Parameter entgegennehmen
2. WHEN die Initialisierung startet THEN das System SHALL prüfen ob der Vertrag lesbar ist
3. WHEN der Vertrag nicht lesbar ist THEN das System SHALL einen Fehler werfen mit der Information dass das Dokument nicht verarbeitet werden kann
4. WHEN der Vertrag lesbar ist THEN das System SHALL die Vertragsart erkennen
5. WHEN die erkannte Vertragsart nicht mit der erwarteten Vertragsart des Agenten übereinstimmt THEN das System SHALL einen Fehler werfen
6. WHEN die Initialisierung erfolgreich ist THEN das System SHALL den Vertragstext dauerhaft in der Instanz speichern
7. WHEN die Initialisierung erfolgreich ist THEN das System SHALL den Zustand des Agenten auf ready setzen
8. WHEN die Initialisierung erfolgreich ist THEN das System SHALL eine Bestätigungsmeldung über das WebSocket an das Frontend senden

### Requirement 4

**User Story:** Als System möchte ich Basic-Analysen durchführen, damit Benutzer schnell grundlegende Vertragsinformationen erhalten.

#### Acceptance Criteria

1. WHEN ein Agent eine Basic-Analyse startet THEN das System SHALL prüfen ob der Agent initialisiert ist
2. WHEN der Agent nicht initialisiert ist THEN das System SHALL einen Fehler werfen
3. WHEN der Agent initialisiert ist THEN das System SHALL alle Basic-Analyse-Steps parallel ausführen
4. WHEN ein Basic-Analyse-Step ausgeführt wird THEN das System SHALL den gespeicherten Vertragstext verwenden
5. WHEN ein Basic-Analyse-Step ausgeführt wird THEN das System SHALL das entsprechende JSON-Schema vom Schema-Provider abrufen
6. WHEN das Schema abgerufen wurde THEN das System SHALL einen AI-Prompt mit dem Schema an Nebius senden
7. WHEN Nebius eine Antwort liefert THEN das System SHALL die Antwort gegen das JSON-Schema validieren
8. WHEN die Validierung fehlschlägt THEN das System SHALL einen Fehler werfen mit Details zur Validierung
9. WHEN alle Basic-Analyse-Steps abgeschlossen sind THEN das System SHALL die Ergebnisse zu einem Basic-Analyse-DTO zusammenführen
10. WHEN das Basic-Analyse-DTO erstellt ist THEN das System SHALL es über das WebSocket an das Frontend senden

### Requirement 5

**User Story:** Als System möchte ich Premium-Analysen durchführen, damit Benutzer detaillierte rechtliche Bewertungen erhalten.

#### Acceptance Criteria

1. WHEN ein Agent eine Premium-Analyse startet THEN das System SHALL prüfen ob der Agent initialisiert ist
2. WHEN der Agent nicht initialisiert ist THEN das System SHALL einen Fehler werfen
3. WHEN der Agent initialisiert ist THEN das System SHALL alle Premium-Analyse-Steps parallel ausführen
4. WHEN ein Premium-Analyse-Step ausgeführt wird THEN das System SHALL den gespeicherten Vertragstext verwenden
5. WHEN ein Premium-Analyse-Step ausgeführt wird THEN das System SHALL das entsprechende JSON-Schema vom Schema-Provider abrufen
6. WHEN das Schema abgerufen wurde THEN das System SHALL einen AI-Prompt mit dem Schema an Nebius senden
7. WHEN Nebius eine Antwort liefert THEN das System SHALL die Antwort gegen das JSON-Schema validieren
8. WHEN die Validierung fehlschlägt THEN das System SHALL einen Fehler werfen mit Details zur Validierung
9. WHEN alle Premium-Analyse-Steps abgeschlossen sind THEN das System SHALL die Ergebnisse zu einem Premium-Analyse-DTO zusammenführen
10. WHEN das Premium-Analyse-DTO erstellt ist THEN das System SHALL es über das WebSocket an das Frontend senden
11. WHEN Basic-Analyse und Premium-Analyse ausgeführt werden THEN das System SHALL sicherstellen dass beide komplett unabhängig voneinander sind

### Requirement 6

**User Story:** Als Entwickler möchte ich JSON-Schemas als Single-Source-of-Truth verwenden, damit Datenstrukturen zentral definiert und konsistent sind.

#### Acceptance Criteria

1. WHEN das System ein Schema benötigt THEN der Schema-Provider SHALL das entsprechende JSON-Schema basierend auf Vertragstyp und Analyse-Step zurückgeben
2. WHEN ein JSON-Schema definiert wird THEN das Schema SHALL alle erforderlichen Felder und deren Typen spezifizieren
3. WHEN ein JSON-Schema definiert wird THEN das Schema SHALL Styling-Informationen für das Frontend enthalten
4. WHEN eine AI-Antwort empfangen wird THEN der Schema-Validator SHALL die Antwort gegen das JSON-Schema validieren
5. WHEN die Validierung erfolgreich ist THEN der Schema-Validator SHALL true zurückgeben
6. WHEN die Validierung fehlschlägt THEN der Schema-Validator SHALL false zurückgeben und Details zu den Validierungsfehlern bereitstellen
7. WHEN das Frontend Schema-Daten benötigt THEN das System SHALL einen Endpoint bereitstellen um JSON-Schemas abzurufen

### Requirement 7

**User Story:** Als System möchte ich verschiedene Vertragstypen unterstützen, damit Benutzer verschiedene Verträge analysieren können und das System einfach erweiterbar ist.

#### Acceptance Criteria

1. WHEN der Vertragstyp Mietvertrag angegeben wird THEN das System SHALL einen RentalContractAgent konstruieren
2. WHEN der Vertragstyp Arbeitsvertrag angegeben wird THEN das System SHALL einen EmploymentContractAgent konstruieren
3. WHEN ein Agent konstruiert wird THEN das System SHALL nur Abhängigkeiten injizieren
4. WHEN ein unbekannter Vertragstyp angegeben wird THEN das System SHALL einen Fehler werfen
5. WHEN ein neuer Agententyp hinzugefügt wird THEN das System SHALL die Erweiterung ohne Änderungen an bestehenden Agenten ermöglichen

### Requirement 8

**User Story:** Als System möchte ich Analyse-Ergebnisse strukturiert zurückgeben, damit das Frontend die Daten konsistent verarbeiten kann.

#### Acceptance Criteria

1. WHEN eine Analyse abgeschlossen ist THEN das System SHALL ein AnalysisDto mit Result und Metadata zurückgeben
2. WHEN Metadata erstellt werden THEN das System SHALL ContractId, CountryCode, Language, AnalyzedAt, IsPreview und ModelVersion setzen
3. WHEN ein Basic-Analyse-DTO für Mietverträge erstellt wird THEN das System SHALL AllgemeineVertragsdaten, Mietkonditionen, Vertragsbedingungen und Summary enthalten
4. WHEN ein Premium-Analyse-DTO für Mietverträge erstellt wird THEN das System SHALL Sektionen für Mietzahlung, Mietdauer, Gebrauch, Instandhaltung, Rechte und Datenschutz enthalten
5. WHEN eine Premium-Sektion erstellt wird THEN das System SHALL Zusammenfassung, Bewertung, Rechtsgrundlage, RisikoScore und RiskLevel enthalten

### Requirement 9

**User Story:** Als System möchte ich nur Nebius als AI-Service verwenden, damit die AI-Integration vereinfacht und konsistent ist.

#### Acceptance Criteria

1. WHEN ein Agent eine AI-Anfrage stellt THEN das System SHALL ausschließlich den Nebius-Client verwenden
2. WHEN der Nebius-Client eine Anfrage sendet THEN das System SHALL den Prompt und das JSON-Schema als Parameter übergeben
3. WHEN Nebius antwortet THEN das System SHALL die Antwort als JSON-Objekt zurückgeben
4. WHEN Nebius einen Fehler zurückgibt THEN das System SHALL eine AiServiceException werfen

### Requirement 10

**User Story:** Als System möchte ich Analyse-Steps parallel ausführen, damit die Gesamtanalysezeit reduziert wird.

#### Acceptance Criteria

1. WHEN eine Basic-Analyse gestartet wird THEN das System SHALL alle Basic-Steps gleichzeitig mit Task.WhenAll ausführen
2. WHEN eine Premium-Analyse gestartet wird THEN das System SHALL alle Premium-Steps gleichzeitig mit Task.WhenAll ausführen
3. WHEN ein Step fehlschlägt THEN das System SHALL die anderen Steps trotzdem weiter ausführen
4. WHEN ein Step fehlschlägt THEN das System SHALL statt dem Ergebnis eine kundenfreundliche Fehlermeldung für diesen Step zurückgeben
5. WHEN alle Steps abgeschlossen sind THEN das System SHALL die Ergebnisse und Fehlermeldungen in der richtigen Reihenfolge zusammenführen
6. WHEN ein Analyse-Step startet THEN das System SHALL den Startzeitpunkt loggen
7. WHEN ein Analyse-Step abgeschlossen ist THEN das System SHALL die Dauer auf Info-Level loggen

### Requirement 11

**User Story:** Als System möchte ich den Analyseprozess über WebSocket kommunizieren, damit das Frontend Echtzeit-Updates erhält.

#### Acceptance Criteria

1. WHEN eine WebSocket-Verbindung akzeptiert wird THEN das System SHALL die Datei und den Vertragstyp empfangen
2. WHEN ein Agent konstruiert wird THEN das System SHALL das WebSocket als Abhängigkeit injizieren
3. WHEN die Initialisierung abgeschlossen ist THEN das System SHALL eine Bestätigungsmeldung als JSON über das WebSocket senden
4. WHEN die Basic-Analyse abgeschlossen ist THEN das System SHALL das Basic-Analyse-Ergebnis als JSON über das WebSocket senden
5. WHEN die Premium-Analyse abgeschlossen ist THEN das System SHALL das Premium-Analyse-Ergebnis als JSON über das WebSocket senden
6. WHEN alle Analysen abgeschlossen sind THEN das System SHALL die WebSocket-Verbindung mit Status NormalClosure schließen
7. WHEN ein Fehler auftritt THEN das System SHALL eine Fehlermeldung über das WebSocket senden und die Verbindung mit entsprechendem Status schließen

### Requirement 12

**User Story:** Als Entwickler möchte ich alle Prompts zentral verwalten, damit Prompt-Engineering einfach durchgeführt werden kann.

#### Acceptance Criteria

1. WHEN das System Prompts benötigt THEN das System SHALL alle Prompts aus einer zentralen Prompt-Datei abrufen
2. WHEN die Prompt-Datei organisiert wird THEN das System SHALL Kommentare und Regions verwenden um die Übersichtlichkeit zu gewährleisten
3. WHEN ein neuer Vertragstyp hinzugefügt wird THEN das System SHALL die Prompts in der zentralen Datei oder in Unterklassen organisieren
4. WHEN System-Prompts definiert werden THEN das System SHALL diese ebenfalls in der zentralen Prompt-Verwaltung speichern
5. WHEN ein Prompt abgerufen wird THEN das System SHALL den Prompt basierend auf Vertragstyp und Analyse-Step zurückgeben
