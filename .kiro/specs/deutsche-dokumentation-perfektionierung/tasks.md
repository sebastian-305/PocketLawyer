# Implementierungsplan: Deutsche Dokumentation & Perfektionierung

## Übersicht

Dieser Plan beschreibt die schrittweise Implementierung der Verbesserungen am Dokumentationsgenerationssystem. Der Fokus liegt auf der Integration deutscher Strings, verbesserter DTO-Extraktion und Behebung aller 92 Verifikationsfehler.

## Tasks

- [x] 1. Erweitere DocumentationLanguage.cs mit allen benötigten deutschen Strings
  - Füge fehlende Strings für alle 6 Dokumenttypen hinzu
  - Stelle sicher, dass alle Überschriften, Labels und Beschreibungen abgedeckt sind
  - Füge Strings für Tabellen-Header hinzu (Name, Typ, Beschreibung, Erforderlich, Optional)
  - Füge Strings für Enum-Dokumentation hinzu (Werte, Wert)
  - _Requirements: 1.1, 1.4_

- [x] 2. Implementiere AnchorGenerator für konsistente Link-Generierung
  - [x] 2.1 Erstelle AnchorGenerator.cs mit Generate-Methode
    - Konvertiere zu Kleinbuchstaben
    - Ersetze Leerzeichen durch Bindestriche
    - Ersetze deutsche Umlaute (ä→ae, ö→oe, ü→ue, ß→ss)
    - Entferne Sonderzeichen (außer Bindestriche)
    - Reduziere mehrfache Bindestriche zu einem
    - Trimme führende/nachfolgende Bindestriche
    - _Requirements: 8.4_
  
  - [x] 2.2 Schreibe Property-Test für Anchor-Generierung
    - **Property 9: Konsistente Anchor-Generierung (Round-Trip)**
    - **Validates: Requirements 8.4**
    - Teste Idempotenz: Generate(text) == Generate(text)
    - Teste Leerzeichen-Ersetzung: Kein Leerzeichen im Ergebnis
    - Teste Sonderzeichen-Entfernung
    - _Requirements: 8.4_

- [x] 3. Implementiere ImprovedDtoExtractor für vollständige DTO-Extraktion
  - [x] 3.1 Erstelle ImprovedDtoExtractor.cs mit ExtractAllDtos-Methode
    - Verwende Roslyn für Quellcode-Analyse
    - Finde alle .cs Dateien im Projekt rekursiv
    - Identifiziere DTO-Klassen (Attribut oder Namespace-Konvention)
    - Extrahiere Name, Namespace, XML-Dokumentation
    - Extrahiere alle Properties (inkl. geerbte von Basisklassen)
    - Identifiziere Enums und extrahiere Enum-Werte
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 7.1, 7.2_
  
  - [x] 3.2 Implementiere ExtractProperties-Methode für Property-Extraktion
    - Extrahiere direkt deklarierte Properties
    - Rekursiv geerbte Properties von Basisklassen extrahieren
    - Markiere geerbte Properties mit IsInherited = true
    - Extrahiere Property-Typ, Name, XML-Dokumentation
    - Bestimme IsRequired basierend auf Nullable-Kontext
    - _Requirements: 2.2, 2.5_
  
  - [x] 3.3 Implementiere ExtractXmlDocumentation-Methode
    - Parse XML-Dokumentationskommentare aus Quellcode
    - Extrahiere <summary> für Beschreibungen
    - Fallback auf leeren String wenn keine Dokumentation vorhanden
    - _Requirements: 2.2_
  
  - [x] 3.4 Schreibe Property-Test für DTO-Extraktion
    - **Property 2: Vollständige DTO-Extraktion mit Metadaten**
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4, 2.5**
    - Erstelle Test-DTOs mit verschiedenen Property-Typen
    - Teste Extraktion aller Properties
    - Teste Extraktion geerbter Properties
    - Teste Namespace-Erfassung
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_
  
  - [x] 3.5 Schreibe Property-Test für Enum-Extraktion
    - **Property 8: Vollständige Enum-Extraktion und -Dokumentation**
    - **Validates: Requirements 7.1, 7.2**
    - Erstelle Test-Enums mit verschiedenen Werten
    - Teste Extraktion aller Enum-Werte
    - Teste numerische Werte
    - _Requirements: 7.1, 7.2_

- [x] 4. Checkpoint - Stelle sicher, dass alle Tests bestehen
  - Stelle sicher, dass alle Tests bestehen, frage den Benutzer bei Fragen.

- [x] 5. Implementiere MermaidValidator für robuste Diagramm-Generierung
  - [x] 5.1 Erstelle MermaidValidator.cs mit Validate-Methode
    - Prüfe auf ungültige Zeichen in Knotennamen
    - Prüfe auf fehlende Anführungszeichen bei Sonderzeichen
    - Prüfe auf korrekte Syntax (graph, flowchart, etc.)
    - Prüfe auf geschlossene Klammern
    - Gebe ValidationResult mit IsValid und Errors zurück
    - _Requirements: 6.1, 6.2_
  
  - [x] 5.2 Implementiere SanitizeNodeName-Methode
    - Entferne oder escape Sonderzeichen in Knotennamen
    - Entferne: " ' [ ] { } ( )
    - Ersetze Leerzeichen durch Unterstriche
    - _Requirements: 6.2_
  
  - [x] 5.3 Schreibe Property-Test für Mermaid-Validierung
    - **Property 7: Valide Mermaid-Diagramme mit deutschen Beschriftungen**
    - **Validates: Requirements 6.1, 6.2, 6.4**
    - Generiere zufällige Knotennamen mit Sonderzeichen
    - Teste SanitizeNodeName
    - Teste Validierung von generierten Diagrammen
    - _Requirements: 6.1, 6.2_

- [x] 6. Aktualisiere DataModelsDocGenerator für deutsche Dokumentation
  - [x] 6.1 Integriere DocumentationLanguage.cs in DataModelsDocGenerator
    - Ersetze alle Hardcoded-Strings durch DocumentationLanguage-Konstanten
    - Verwende deutsche Überschriften (Datenmodelle, Eigenschaften, etc.)
    - Verwende deutsche Tabellen-Header (Name, Typ, Beschreibung, Erforderlich)
    - _Requirements: 1.1, 1.3_
  
  - [x] 6.2 Implementiere GeneratePropertiesSection für DTO-Properties
    - Erstelle Markdown-Tabelle mit allen Properties
    - Zeige Name, Typ, Beschreibung, Erforderlich/Optional
    - Markiere geerbte Properties (optional)
    - _Requirements: 3.1_
  
  - [x] 6.3 Implementiere GenerateEnumSection für Enums
    - Erstelle Markdown-Tabelle mit allen Enum-Werten
    - Zeige Name, numerischer Wert, Beschreibung
    - _Requirements: 7.2_
  
  - [x] 6.4 Stelle sicher, dass jedes DTO genau einmal dokumentiert wird
    - Sortiere DTOs alphabetisch
    - Erstelle für jedes DTO einen Abschnitt mit ## Überschrift
    - _Requirements: 3.5_
  
  - [x] 6.5 Schreibe Property-Test für DTO-Dokumentation
    - **Property 3: Eindeutige und vollständige DTO-Dokumentation**
    - **Validates: Requirements 3.1, 3.5**
    - Generiere zufällige DTOs
    - Teste, dass jedes DTO genau einmal dokumentiert wird
    - Teste, dass alle erforderlichen Felder vorhanden sind
    - _Requirements: 3.1, 3.5_

- [x] 7. Aktualisiere ApiReferenceDocGenerator für deutsche Dokumentation und valide Links
  - [x] 7.1 Integriere DocumentationLanguage.cs in ApiReferenceDocGenerator
    - Ersetze alle Hardcoded-Strings durch DocumentationLanguage-Konstanten
    - Verwende deutsche Überschriften (API-Referenz, Endpunkte, etc.)
    - Verwende deutsche Labels (Request-Body, Response-Body, etc.)
    - _Requirements: 1.1, 1.3_
  
  - [x] 7.2 Verwende AnchorGenerator für DTO-Links
    - Generiere Anchors mit AnchorGenerator.Generate(dtoName)
    - Erstelle Links im Format [DtoName](DATA_MODELS.md#anchor)
    - _Requirements: 3.2, 8.1_
  
  - [x] 7.3 Schreibe Property-Test für DTO-Referenzen
    - **Property 4: Valide DTO-Referenzen in API-Dokumentation**
    - **Validates: Requirements 3.2**
    - Generiere zufällige Endpoints mit DTOs
    - Teste, dass Links korrekt formatiert sind
    - Teste, dass Anchors mit AnchorGenerator übereinstimmen
    - _Requirements: 3.2_
  
  - [x] 7.4 Schreibe Property-Test für Endpoint-Dokumentation
    - **Property 5: Vollständige Endpoint-Extraktion und -Dokumentation**
    - **Validates: Requirements 4.1, 4.2, 4.4**
    - Generiere zufällige Endpoints
    - Teste, dass alle Felder dokumentiert werden
    - Teste, dass DTOs korrekt referenziert werden
    - _Requirements: 4.1, 4.2, 4.4_

- [x] 8. Aktualisiere ReadmeDocGenerator für deutsche Dokumentation
  - Integriere DocumentationLanguage.cs
  - Ersetze alle Hardcoded-Strings durch deutsche Konstanten
  - Verwende deutsche Überschriften (Übersicht, Installation, Verwendung, etc.)
  - _Requirements: 1.1, 1.3_

- [x] 9. Aktualisiere ArchitectureDocGenerator für deutsche Dokumentation
  - [x] 9.1 Integriere DocumentationLanguage.cs in ArchitectureDocGenerator
    - Ersetze alle Hardcoded-Strings durch deutsche Konstanten
    - Verwende deutsche Überschriften (Architektur, Komponenten, etc.)
    - _Requirements: 1.1, 1.3_
  
  - [x] 9.2 Integriere MermaidValidator in Diagramm-Generierung
    - Verwende MermaidValidator.SanitizeNodeName für alle Knotennamen
    - Validiere generierte Diagramme mit MermaidValidator.Validate
    - Werfe InvalidDiagramException bei Validierungsfehlern
    - _Requirements: 6.1, 6.2_
  
  - [x] 9.3 Verwende deutsche Beschriftungen in Diagrammen
    - Verwende DocumentationLanguage für Diagramm-Labels
    - _Requirements: 6.4_

- [x] 10. Aktualisiere ConfigurationDocGenerator für deutsche Dokumentation
  - [x] 10.1 Integriere DocumentationLanguage.cs in ConfigurationDocGenerator
    - Ersetze alle Hardcoded-Strings durch deutsche Konstanten
    - Verwende deutsche Überschriften (Konfiguration, Umgebungsvariablen, etc.)
    - _Requirements: 1.1, 1.3_
  
  - [x] 10.2 Schreibe Property-Test für Konfigurationsdokumentation
    - **Property 6: Vollständige Konfigurationsdokumentation**
    - **Validates: Requirements 5.1, 5.2**
    - Erstelle Test-Konfigurationen
    - Teste, dass alle Einträge dokumentiert werden
    - Teste, dass alle Felder vorhanden sind
    - _Requirements: 5.1, 5.2_

- [x] 11. Aktualisiere DeploymentDocGenerator für deutsche Dokumentation
  - Integriere DocumentationLanguage.cs
  - Ersetze alle Hardcoded-Strings durch deutsche Konstanten
  - Verwende deutsche Überschriften (Deployment, Voraussetzungen, Schritte, etc.)
  - _Requirements: 1.1, 1.3_

- [x] 12. Checkpoint - Stelle sicher, dass alle Tests bestehen
  - Stelle sicher, dass alle Tests bestehen, frage den Benutzer bei Fragen.

- [x] 13. Aktualisiere LinkVerifier für robuste Link-Validierung
  - [x] 13.1 Verwende AnchorGenerator für Anchor-Vergleiche
    - Generiere erwartete Anchors mit AnchorGenerator.Generate
    - Vergleiche Links mit generierten Anchors
    - _Requirements: 8.1, 8.4_
  
  - [x] 13.2 Verbessere Fehlerberichterstattung
    - Gebe Quelldatei, Zeilennummer und Ziel-Anchor in Fehlermeldungen an
    - Sammle alle Fehler statt beim ersten Fehler abzubrechen
    - _Requirements: 8.3_
  
  - [x] 13.3 Schreibe Property-Test für Link-Validierung
    - **Property 10: Valide Markdown-Links**
    - **Validates: Requirements 8.1, 8.2, 8.3**
    - Generiere zufällige Links
    - Teste, dass Anchor-Links auf existierende Anchors zeigen
    - Teste, dass Datei-Links auf existierende Dateien zeigen
    - _Requirements: 8.1, 8.2, 8.3_

- [x] 14. Integriere ImprovedDtoExtractor in DocumentationOrchestrator
  - Ersetze bestehende DTO-Extraktion durch ImprovedDtoExtractor
  - Stelle sicher, dass alle 6 Generatoren die extrahierten DTOs verwenden
  - Übergebe vollständige DTO-Liste an DataModelsDocGenerator
  - Übergebe DTO-Liste an ApiReferenceDocGenerator für Link-Generierung
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 10.3_

- [x] 15. Schreibe Integration-Test für vollständige Dokumentationsgenerierung
  - [x] 15.1 Erstelle Integration-Test mit realem Projekt
    - Verwende PocketLawyer-Projekt als Testbasis
    - Führe vollständige Dokumentationsgenerierung durch
    - Führe Verifikation durch
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_
  
  - [x] 15.2 Teste Null-Fehler-Verifikation
    - **Property 11: Null-Fehler-Verifikation**
    - **Validates: Requirements 3.3, 3.4, 4.3, 5.3, 6.3, 7.3, 8.3, 9.1-9.7**
    - Prüfe, dass Verification Engine 0 Completeness-Fehler meldet
    - Prüfe, dass Verification Engine 0 DTO-Verification-Fehler meldet
    - Prüfe, dass Verification Engine 0 Endpoint-Fehler meldet
    - Prüfe, dass Verification Engine 0 Konfigurationsfehler meldet
    - Prüfe, dass Verification Engine 0 Diagramm-Parsing-Fehler meldet
    - Prüfe, dass Verification Engine 0 Enum-Fehler meldet
    - Prüfe, dass Verification Engine 0 Broken-Anchor-Fehler meldet
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_

- [x] 16. Aktualisiere bestehende Tests für deutsche Strings
  - Passe alle bestehenden Unit-Tests an, um deutsche Strings zu erwarten
  - Aktualisiere Assertions für deutsche Überschriften und Labels
  - Stelle sicher, dass alle Tests bestehen
  - _Requirements: 10.4_

- [x] 17. Final Checkpoint - Vollständige Verifikation
  - Führe vollständige Dokumentationsgenerierung durch
  - Führe Verifikation durch und prüfe auf 0 Fehler
  - Führe alle Tests durch (Unit + Property + Integration)
  - Stelle sicher, dass alle 6 Dokumentationsdateien auf Deutsch sind
  - Frage den Benutzer bei Fragen oder Problemen

## Hinweise

- Jeder Task referenziert spezifische Requirements für Nachvollziehbarkeit
- Checkpoints stellen sicher, dass Fehler früh erkannt werden
- Property-Tests validieren universelle Korrektheitseigenschaften mit 100+ zufälligen Inputs
- Property-Tests sind erforderlich, um die 92 Verifikationsfehler zu beheben und zu verhindern
- Unit-Tests validieren spezifische Beispiele und Edge Cases
- Der Integration-Test am Ende stellt sicher, dass alle Komponenten zusammenarbeiten und 0 Fehler gemeldet werden
