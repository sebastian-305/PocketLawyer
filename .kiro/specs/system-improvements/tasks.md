# Implementation Plan

- [x] 1. Property-Displaynamen im Schema implementieren





  - Hilfsmethode in Base Schema Provider erstellen für `x-displayName`
  - Alle Rental Basic Schema Provider mit Displaynamen erweitern
  - Alle Rental Premium Schema Provider mit Displaynamen erweitern
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [x] 1.1 Hilfsmethode für x-displayName erstellen


  - Neue statische Hilfsmethode `AddDisplayName` in einer Utility-Klasse erstellen
  - Methode nimmt JsonObject und displayName-String als Parameter
  - Methode fügt `x-displayName` Property zum JsonObject hinzu
  - _Requirements: 1.1_

- [x] 1.2 Rental Basic Schema Provider aktualisieren


  - RentalGeneralSchemaProvider mit Displaynamen erweitern
  - RentalRentConditionsSchemaProvider mit Displaynamen erweitern
  - RentalContractConditionsSchemaProvider mit Displaynamen erweitern
  - RentalBasicSummarySchemaProvider mit Displaynamen erweitern
  - _Requirements: 1.2, 1.3_

- [x] 1.3 Rental Premium Schema Provider aktualisieren


  - ClauseAnalysisSchemaProvider mit Displaynamen erweitern
  - RentalPremiumSummarySchemaProvider mit Displaynamen erweitern
  - _Requirements: 1.2, 1.3_

- [x] 2. Legacy Code isolieren




  - PromptBuilder und PromptBuilder_Deprecated mit Obsolete-Attribut markieren
  - AgentType-Abhängigkeit in PromptBuilder entfernen
  - AgentType-Abhängigkeit in PromptBuilder_Deprecated entfernen
  - ClaudeService und GeminiService mit Obsolete-Attribut markieren
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

- [x] 2.1 PromptBuilder isolieren


  - Obsolete-Attribut zu PromptBuilder-Klasse hinzufügen
  - AgentType-Parameter durch String-Parameter ersetzen
  - Alle Methoden auf String-basierte Vertragstypen umstellen
  - _Requirements: 2.1, 2.2, 2.6_

- [x] 2.2 PromptBuilder_Deprecated isolieren


  - Obsolete-Attribut zu PromptBuilder_Deprecated-Klasse hinzufügen
  - AgentType-Parameter durch String-Parameter ersetzen
  - Alle Methoden auf String-basierte Vertragstypen umstellen
  - _Requirements: 2.1, 2.2, 2.6_

- [x] 2.3 ClaudeService als Legacy markieren


  - Obsolete-Attribut zu ClaudeService-Klasse hinzufügen
  - XML-Dokumentation mit Legacy-Hinweis erweitern
  - _Requirements: 2.3, 2.6_

- [x] 2.4 GeminiService als Legacy markieren


  - Obsolete-Attribut zu GeminiService-Klasse hinzufügen
  - XML-Dokumentation mit Legacy-Hinweis erweitern
  - _Requirements: 2.3, 2.6_

- [x] 3. Token-Tracking für Agenten implementieren





  - AiResponse Record erstellen
  - IAiClient Interface erweitern
  - NebiusService mit Token-Tracking implementieren
  - ContractAnalysisAgentBase mit Token-Aggregierung erweitern
  - Agents auf neue Methode umstellen
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7_

- [x] 3.1 AiResponse Record erstellen


  - Neues Record `AiResponse` in `Application/Models` erstellen
  - Properties: Content (string), InputTokens (int), OutputTokens (int), TotalTokens (int)
  - _Requirements: 3.1_

- [x] 3.2 IAiClient Interface erweitern



  - Neue Methode `SendPromptWithMetadata` zu IAiClient hinzufügen
  - Rückgabetyp: Task<AiResponse>
  - Bestehende SendPrompt-Methode beibehalten für Backward Compatibility
  - _Requirements: 3.1_

- [x] 3.3 NebiusService Token-Tracking implementieren


  - SendPromptWithMetadata-Methode in NebiusService implementieren
  - Token-Werte aus completion.Usage extrahieren
  - AiResponse-Objekt mit Token-Informationen zurückgeben
  - Token-Logging auf Debug-Level ändern
  - _Requirements: 3.1, 3.6_

- [x] 3.4 ContractAnalysisAgentBase erweitern


  - Properties für TotalInputTokens, TotalOutputTokens, TotalTokens hinzufügen
  - Methode TrackTokens(AiResponse) implementieren
  - Methode LogTokenSummary() implementieren
  - _Requirements: 3.2, 3.3, 3.4, 3.5, 3.7_

- [x] 3.5 RentalContractAgent auf Token-Tracking umstellen


  - Alle AI-Anfragen auf SendPromptWithMetadata umstellen
  - TrackTokens nach jeder AI-Anfrage aufrufen
  - LogTokenSummary am Ende von AnalyzeBasicAsync aufrufen
  - LogTokenSummary am Ende von AnalyzePremiumAsync aufrufen
  - _Requirements: 3.2, 3.3, 3.4, 3.5, 3.7_

- [x] 3.6 EmploymentContractAgent auf Token-Tracking umstellen


  - Alle AI-Anfragen auf SendPromptWithMetadata umstellen
  - TrackTokens nach jeder AI-Anfrage aufrufen
  - LogTokenSummary am Ende der Analyse-Methoden aufrufen
  - _Requirements: 3.2, 3.3, 3.4, 3.5, 3.7_

- [x] 4. Filesystem-Logging implementieren





  - Serilog NuGet Packages installieren
  - FileLoggingOptions Klasse erstellen
  - Program.cs mit Serilog konfigurieren
  - appsettings.json mit Logging-Konfiguration erweitern
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_

- [x] 4.1 Serilog NuGet Packages installieren


  - Serilog.AspNetCore Package hinzufügen
  - Serilog.Sinks.File Package hinzufügen
  - _Requirements: 4.1, 4.2, 4.3_

- [x] 4.2 FileLoggingOptions erstellen


  - Neue Klasse FileLoggingOptions in Options-Ordner erstellen
  - Properties: Enabled (bool), MinimumLevel (string), Path (string)
  - Standardwerte: Enabled=true, MinimumLevel="Information", Path="logs"
  - _Requirements: 4.1, 4.2_

- [x] 4.3 Program.cs mit Serilog konfigurieren


  - UseSerilog in Program.cs hinzufügen
  - File Sink mit Rolling Interval Day konfigurieren
  - Dateiformat: {Path}/YYYY-MM-DD.log
  - MinimumLevel aus Configuration lesen
  - Enabled-Flag aus Configuration berücksichtigen
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_

- [x] 4.4 appsettings.json erweitern


  - Logging:File Sektion hinzufügen
  - Enabled, MinimumLevel und Path konfigurieren
  - _Requirements: 4.1, 4.2_

- [x] 5. Integration Tests schreiben







  - End-to-End Test für Token-Tracking
  - Test für Filesystem-Logging über mehrere Tage
  - Test für Schema mit x-displayName
  - _Requirements: Alle_

- [x] 5.1 Token-Tracking Integration Test


  - Test erstellt vollständige Analyse mit RentalContractAgent
  - Test verifiziert, dass Token-Summen korrekt aggregiert werden
  - Test verifiziert, dass Token-Logging mit Level Info erfolgt
  - _Requirements: 3.2, 3.3, 3.4, 3.5, 3.7_

- [x] 5.2 Filesystem-Logging Integration Test


  - Test verifiziert, dass Log-Dateien mit korrektem Namen erstellt werden
  - Test verifiziert, dass nur Logs >= MinimumLevel geschrieben werden
  - Test verifiziert, dass Enabled-Flag funktioniert
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_

- [x] 5.3 Schema x-displayName Test


  - Test verifiziert, dass x-displayName in Schemas vorhanden ist
  - Test verifiziert, dass Frontend x-displayName extrahieren kann
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [x] 6. Dokumentation und Diagramme aktualisieren





  - Alle Markdown-Dokumentationsdateien durchsuchen und aktualisieren
  - Mermaid-Diagramme auf Aktualität prüfen und anpassen
  - Neue Features in Dokumentation aufnehmen
  - _Requirements: Alle_

- [x] 6.1 README und Hauptdokumentation aktualisieren


  - README.md auf neue Features prüfen und aktualisieren
  - Token-Tracking Feature dokumentieren
  - Filesystem-Logging Feature dokumentieren
  - x-displayName Feature für Frontend dokumentieren
  - Legacy-Komponenten als deprecated kennzeichnen
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 2.1, 2.2, 2.3, 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_

- [x] 6.2 Architektur-Diagramme aktualisieren


  - Alle Mermaid-Diagramme in der Dokumentation identifizieren
  - Diagramme auf Aktualität mit neuer Implementierung prüfen
  - Token-Tracking Flow in relevante Diagramme einarbeiten
  - Logging-Architektur in Diagramme aufnehmen
  - Schema-Provider Struktur mit x-displayName visualisieren
  - _Requirements: 1.1, 1.2, 1.3, 3.1, 3.2, 3.3, 3.4, 4.1, 4.2, 4.3_

- [x] 6.3 API-Dokumentation aktualisieren


  - XML-Dokumentationskommentare in geänderten Klassen prüfen
  - Neue Methoden und Properties dokumentieren
  - Obsolete-Markierungen in Dokumentation erklären
  - Code-Beispiele für neue Features hinzufügen
  - _Requirements: 1.1, 2.1, 2.2, 2.3, 3.1, 3.2, 3.3, 3.4, 3.5, 4.1, 4.2_

- [x] 6.4 Konfigurationsdokumentation erweitern


  - appsettings.json Struktur dokumentieren
  - FileLoggingOptions Konfiguration erklären
  - Beispiele für verschiedene Logging-Szenarien hinzufügen
  - Best Practices für Produktionsumgebung dokumentieren
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7_
