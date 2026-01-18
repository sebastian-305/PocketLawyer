# Dokumentationsplan - Klausel Klara

## Übersicht

Dieser Plan beschreibt die neue, verknüpfte Dokumentationsstruktur für das Klausel Klara Projekt. Die Dokumentation wird von Grund auf neu erstellt, orientiert sich aber an den Themen der Legacy-Dokumentation und erweitert diese um moderne Best Practices.

## Ziele

- **Verknüpfte Struktur**: Alle Dokumente sind miteinander verlinkt und bilden ein zusammenhängendes Wissensnetz
- **Mermaid-Diagramme**: Visuelle Darstellung von Architektur, Datenflüssen und Prozessen
- **Mehrsprachigkeit**: Hauptdokumentation auf Deutsch, technische Details auch auf Englisch
- **Wartbarkeit**: Klare Struktur, die einfach zu aktualisieren ist
- **Vollständigkeit**: Alle Aspekte des Systems dokumentiert

## Dokumentenstruktur

### 1. Hauptdokumentation (Root)

#### 1.1 README.md (bereits vorhanden)
- **Status**: ✅ Bereits erstellt
- **Inhalt**: Projekt-Übersicht, Quick Start, Links zu detaillierter Dokumentation
- **Verknüpfungen**: Links zu allen Hauptdokumenten in `docs/`

### 2. Architektur-Dokumentation (`docs/`)

#### 2.1 ARCHITECTURE.md
- **Zweck**: Gesamtarchitektur des Systems
- **Inhalt**:
  - Onion Architecture Übersicht
  - Layer-Beschreibungen (Web, Application, Domain, Infrastructure)
  - Dependency Flow
  - Design-Prinzipien (SOLID, DRY, YAGNI)
- **Mermaid-Diagramme**:
  - Layered Component Diagram
  - Dependency Graph
  - Package Structure
- **Verknüpfungen**:
  - → API_REFERENCE.md (API-Details)
  - → DESIGN_PATTERNS.md (verwendete Patterns)
  - → DATA_FLOW.md (Datenflüsse)

#### 2.2 DESIGN_PATTERNS.md
- **Zweck**: Verwendete Design Patterns und Architekturmuster
- **Inhalt**:
  - Dependency Injection
  - Strategy Pattern (File Decoders)
  - Facade Pattern (Use Cases)
  - Adapter Pattern (AI Services)
  - SOLID-Prinzipien im Projekt
  - GRASP-Prinzipien
  - Domain-Driven Design Aspekte
- **Mermaid-Diagramme**:
  - Strategy Pattern Implementierung
  - Dependency Injection Container
  - Facade Pattern Struktur
- **Verknüpfungen**:
  - → ARCHITECTURE.md (Gesamtarchitektur)
  - → CODE_EXAMPLES.md (Implementierungsbeispiele)

#### 2.3 DATA_FLOW.md
- **Zweck**: Datenflüsse durch das System
- **Inhalt**:
  - WebSocket-Kommunikation
  - Request/Response-Zyklen
  - Agent Lifecycle (3 Phasen)
  - Parallele Verarbeitung
  - Token Tracking Flow
- **Mermaid-Diagramme**:
  - WebSocket Message Flow
  - Agent Lifecycle Sequence
  - Data Flow Context Diagram
  - Token Tracking Flow
- **Verknüpfungen**:
  - → API_REFERENCE.md (API-Endpoints)
  - → AGENT_SYSTEM.md (Agent-Details)

### 3. API-Dokumentation (`docs/`)

#### 3.1 API_REFERENCE.md
- **Zweck**: Vollständige API-Referenz
- **Inhalt**:
  - WebSocket-Endpoints
    - `/rental-analysis`
    - `/employment-analysis`
  - HTTP-Endpoints
    - `/api/schemas/{contractType}/{tier}/{step}`
  - Message-Formate
  - Error-Handling
  - Status-Codes
- **Mermaid-Diagramme**:
  - API Endpoint Overview
  - Message Sequence Diagrams
- **Verknüpfungen**:
  - → DATA_MODELS.md (DTO-Strukturen)
  - → ERROR_HANDLING.md (Fehlerbehandlung)
  - → INTEGRATION_GUIDE.md (Client-Integration)

#### 3.2 DATA_MODELS.md
- **Zweck**: Datenmodelle und DTOs
- **Inhalt**:
  - DTO-Hierarchie
  - Rental Contract DTOs
  - Employment Contract DTOs
  - Shared DTOs
  - Metadata-Strukturen
  - Enumerations (RiskLevel, ClauseRiskScore)
- **Mermaid-Diagramme**:
  - DTO Class Diagram
  - Inheritance Hierarchy
  - Namespace Structure
- **Verknüpfungen**:
  - → API_REFERENCE.md (API-Verwendung)
  - → SCHEMA_SYSTEM.md (JSON Schemas)

#### 3.3 SCHEMA_SYSTEM.md
- **Zweck**: JSON Schema System
- **Inhalt**:
  - Schema Provider Architektur
  - Schema-Generierung
  - Custom Attributes (`x-displayName`, `x-sectionTitle`, `x-styling`)
  - Schema-Validierung
  - Frontend-Integration
- **Mermaid-Diagramme**:
  - Schema Provider Architecture
  - Schema Generation Flow
  - Frontend Schema Usage
- **Verknüpfungen**:
  - → DATA_MODELS.md (DTO-Strukturen)
  - → FRONTEND_GUIDE.md (Frontend-Nutzung)

### 4. System-Komponenten (`docs/`)

#### 4.1 AGENT_SYSTEM.md
- **Zweck**: Contract Analysis Agent System
- **Inhalt**:
  - Agent Lifecycle (Construction, Initialization, Analysis)
  - RentalContractAgent
  - EmploymentContractAgent
  - Parallele Verarbeitung
  - Token Tracking
  - Context Management Integration
- **Mermaid-Diagramme**:
  - Agent Lifecycle State Diagram
  - Agent Class Hierarchy
  - Parallel Execution Flow
- **Verknüpfungen**:
  - → DATA_FLOW.md (Datenflüsse)
  - → AI_INTEGRATION.md (AI-Services)
  - → CONTEXT_MANAGEMENT.md (Context System)

#### 4.2 AI_INTEGRATION.md
- **Zweck**: KI-Service-Integration
- **Inhalt**:
  - Nebius API Integration
  - Token Tracking
  - Prompt Management
  - Response Handling
  - Error Handling
  - Deprecated Services (Gemini, Claude)
- **Mermaid-Diagramme**:
  - AI Service Architecture
  - Token Tracking Flow
  - Prompt Provider Structure
- **Verknüpfungen**:
  - → AGENT_SYSTEM.md (Agent-Nutzung)
  - → CONFIGURATION.md (API-Keys)
  - → MONITORING.md (Token-Logging)

#### 4.3 CONTEXT_MANAGEMENT.md
- **Zweck**: Context Management System
- **Inhalt**:
  - Directory Structure
  - Context Loading
  - ContextStep Attribute
  - Common Context
  - Caching Strategy
  - Performance Considerations
- **Mermaid-Diagramme**:
  - Context Directory Structure
  - Context Loading Flow
  - Caching Architecture
- **Verknüpfungen**:
  - → AGENT_SYSTEM.md (Agent-Integration)
  - → CONFIGURATION.md (Context-Konfiguration)
  - → DEPLOYMENT.md (Context-Dateien)

#### 4.4 FILE_PROCESSING.md
- **Zweck**: Dateiverarbeitung und -validierung
- **Inhalt**:
  - File Decoder System
  - Strategy Pattern Implementierung
  - Unterstützte Dateitypen (PDF, Text, Images)
  - OCR mit Tesseract
  - File Validation
  - Upload Limits
- **Mermaid-Diagramme**:
  - File Decoder Strategy Pattern
  - File Processing Pipeline
  - Validation Flow
- **Verknüpfungen**:
  - → DESIGN_PATTERNS.md (Strategy Pattern)
  - → CONFIGURATION.md (Upload-Limits)

### 5. Konfiguration und Betrieb (`docs/`)

#### 5.1 CONFIGURATION.md
- **Zweck**: Vollständige Konfigurationsreferenz
- **Inhalt**:
  - appsettings.json Struktur
  - Logging Configuration
  - Method Tracing
  - AI Service Configuration
  - File Upload Configuration
  - Path Configuration
  - Context Management Configuration
  - Environment-Specific Settings
  - User Secrets
  - Environment Variables
- **Mermaid-Diagramme**:
  - Configuration Hierarchy
  - Logging Architecture
  - Environment Override Flow
- **Verknüpfungen**:
  - → DEPLOYMENT.md (Production Setup)
  - → MONITORING.md (Logging)
  - → SECURITY.md (Secrets Management)

#### 5.2 DEPLOYMENT.md
- **Zweck**: Deployment-Anleitung
- **Inhalt**:
  - Systemanforderungen
  - Installation
  - Docker Deployment
  - Azure Deployment
  - Environment Setup
  - Database Migration (falls relevant)
  - Context Files Deployment
  - Health Checks
- **Mermaid-Diagramme**:
  - Deployment Architecture
  - Container Structure
  - Network Topology
- **Verknüpfungen**:
  - → CONFIGURATION.md (Konfiguration)
  - → MONITORING.md (Überwachung)
  - → SECURITY.md (Sicherheit)

#### 5.3 MONITORING.md
- **Zweck**: Monitoring und Logging
- **Inhalt**:
  - Filesystem Logging
  - Log Levels
  - Token Tracking Logs
  - Method Tracing
  - Performance Metrics
  - Log Rotation
  - Log Analysis
  - Alerting
- **Mermaid-Diagramme**:
  - Logging Architecture
  - Log Flow Diagram
  - Metrics Collection
- **Verknüpfungen**:
  - → CONFIGURATION.md (Logging-Config)
  - → TROUBLESHOOTING.md (Fehlersuche)
  - → PERFORMANCE.md (Performance-Metriken)

#### 5.4 SECURITY.md
- **Zweck**: Sicherheitsaspekte
- **Inhalt**:
  - Authentication & Authorization (TODO)
  - API Key Management
  - User Secrets
  - Azure Key Vault Integration
  - Input Validation
  - Rate Limiting (TODO)
  - CORS Configuration (TODO)
  - Data Protection
  - Sensitive Parameter Redaction
- **Mermaid-Diagramme**:
  - Security Architecture
  - Secrets Management Flow
  - Authentication Flow (TODO)
- **Verknüpfungen**:
  - → CONFIGURATION.md (Secrets-Config)
  - → DEPLOYMENT.md (Production Security)
  - → API_REFERENCE.md (API-Sicherheit)

### 6. Entwicklung (`docs/`)

#### 6.1 DEVELOPMENT_GUIDE.md
- **Zweck**: Entwickler-Leitfaden
- **Inhalt**:
  - Development Setup
  - Project Structure
  - Coding Standards
  - Git Workflow
  - Branch Strategy
  - Code Review Process
  - Testing Strategy
- **Mermaid-Diagramme**:
  - Development Workflow
  - Git Branch Strategy
  - CI/CD Pipeline
- **Verknüpfungen**:
  - → TESTING.md (Test-Details)
  - → CODE_EXAMPLES.md (Beispiele)
  - → CONTRIBUTING.md (Contribution Guidelines)

#### 6.2 TESTING.md
- **Zweck**: Testing-Strategie und -Praxis
- **Inhalt**:
  - Unit Tests
  - Integration Tests
  - Property-Based Tests (FsCheck)
  - Test Organization
  - Mocking Strategy
  - Test Coverage
  - Performance Tests
- **Mermaid-Diagramme**:
  - Test Pyramid
  - Test Execution Flow
  - Coverage Analysis
- **Verknüpfungen**:
  - → DEVELOPMENT_GUIDE.md (Dev-Setup)
  - → CODE_EXAMPLES.md (Test-Beispiele)

#### 6.3 CODE_EXAMPLES.md
- **Zweck**: Code-Beispiele und Best Practices
- **Inhalt**:
  - Agent Implementation
  - Custom File Decoder
  - Schema Provider
  - Context Step Implementation
  - WebSocket Client
  - Error Handling
  - Logging Best Practices
- **Verknüpfungen**:
  - → DESIGN_PATTERNS.md (Pattern-Verwendung)
  - → DEVELOPMENT_GUIDE.md (Dev-Guidelines)
  - → API_REFERENCE.md (API-Nutzung)

### 7. Frontend (`docs/`)

#### 7.1 FRONTEND_GUIDE.md
- **Zweck**: Frontend-Architektur und -Entwicklung
- **Inhalt**:
  - Page Structure (index.html, employment.html, rental.html)
  - JavaScript Modules
  - WebSocket Communication
  - Schema-Driven Rendering
  - Generic Renderer
  - Bootstrap 5 Integration
  - Styling System
- **Mermaid-Diagramme**:
  - Frontend Architecture
  - Module Dependencies
  - Rendering Pipeline
- **Verknüpfungen**:
  - → SCHEMA_SYSTEM.md (Schema-Nutzung)
  - → API_REFERENCE.md (WebSocket-API)
  - → UI_COMPONENTS.md (UI-Details)

#### 7.2 UI_COMPONENTS.md
- **Zweck**: UI-Komponenten und Styling
- **Inhalt**:
  - Accordion Components
  - Form Components
  - Loading States
  - Error Display
  - Result Rendering
  - CSS Architecture
  - Responsive Design
- **Mermaid-Diagramme**:
  - Component Hierarchy
  - Styling System
  - Responsive Breakpoints
- **Verknüpfungen**:
  - → FRONTEND_GUIDE.md (Frontend-Architektur)
  - → SCHEMA_SYSTEM.md (x-styling Attributes)

### 8. Integration und Migration (`docs/`)

#### 8.1 INTEGRATION_GUIDE.md
- **Zweck**: Client-Integration
- **Inhalt**:
  - JavaScript Client
  - C# Client
  - Python Client
  - Authentication (TODO)
  - Error Handling
  - Best Practices
- **Code-Beispiele**: Vollständige Client-Implementierungen
- **Verknüpfungen**:
  - → API_REFERENCE.md (API-Details)
  - → ERROR_HANDLING.md (Fehlerbehandlung)

#### 8.2 MIGRATION_GUIDE.md
- **Zweck**: Migration von Legacy-Komponenten
- **Inhalt**:
  - PromptBuilder → PromptProvider
  - ClaudeService/GeminiService → NebiusService
  - Manual Context Loading → Context Management System
  - Legacy Schema System → New Schema System
  - Breaking Changes
  - Deprecation Timeline
- **Verknüpfungen**:
  - → AI_INTEGRATION.md (Neue AI-Services)
  - → CONTEXT_MANAGEMENT.md (Neues Context System)
  - → SCHEMA_SYSTEM.md (Neues Schema System)

### 9. Referenz und Troubleshooting (`docs/`)

#### 9.1 ERROR_HANDLING.md
- **Zweck**: Fehlerbehandlung im System
- **Inhalt**:
  - Exception Hierarchy
  - Error Types
  - Error Messages (German)
  - Error Response Format
  - Client Error Handling
  - Logging Errors
- **Mermaid-Diagramme**:
  - Exception Hierarchy
  - Error Flow
  - Error Translation
- **Verknüpfungen**:
  - → API_REFERENCE.md (Error Responses)
  - → TROUBLESHOOTING.md (Fehlersuche)
  - → MONITORING.md (Error Logging)

#### 9.2 TROUBLESHOOTING.md
- **Zweck**: Problemlösung und Debugging
- **Inhalt**:
  - Häufige Probleme
  - WebSocket Connection Issues
  - File Upload Problems
  - AI Service Errors
  - Context Loading Issues
  - Performance Problems
  - Debugging-Strategien
  - Log-Analyse
- **Verknüpfungen**:
  - → MONITORING.md (Logging)
  - → ERROR_HANDLING.md (Fehlertypen)
  - → CONFIGURATION.md (Config-Probleme)

#### 9.3 PERFORMANCE.md
- **Zweck**: Performance-Optimierung
- **Inhalt**:
  - Performance Characteristics
  - Parallel Execution
  - Caching Strategies
  - Token Optimization
  - Context Size Optimization
  - Database Optimization (falls relevant)
  - Profiling
  - Benchmarks
- **Mermaid-Diagramme**:
  - Performance Bottlenecks
  - Optimization Flow
  - Caching Architecture
- **Verknüpfungen**:
  - → MONITORING.md (Performance-Metriken)
  - → CONTEXT_MANAGEMENT.md (Context-Performance)
  - → AGENT_SYSTEM.md (Parallel Processing)

#### 9.4 FAQ.md
- **Zweck**: Häufig gestellte Fragen
- **Inhalt**:
  - Allgemeine Fragen
  - Technische Fragen
  - Deployment-Fragen
  - Entwicklungs-Fragen
  - Lizenz-Fragen
- **Verknüpfungen**: Links zu relevanten Detaildokumenten

### 10. Zusätzliche Dokumentation (`docs/`)

#### 10.1 GLOSSARY.md
- **Zweck**: Begriffsdefinitionen
- **Inhalt**:
  - Fachbegriffe
  - Abkürzungen
  - Domain-Begriffe (Mietvertrag, Arbeitsvertrag, etc.)
  - Technische Begriffe
- **Verknüpfungen**: Verweise auf Dokumente, die Begriffe verwenden

#### 10.2 CHANGELOG.md
- **Zweck**: Versionshistorie
- **Inhalt**:
  - Version History
  - Breaking Changes
  - New Features
  - Bug Fixes
  - Deprecations
- **Format**: Keep a Changelog Format

#### 10.3 CONTRIBUTING.md
- **Zweck**: Contribution Guidelines
- **Inhalt**:
  - How to Contribute
  - Code of Conduct
  - Pull Request Process
  - Issue Templates
  - Development Setup
- **Verknüpfungen**:
  - → DEVELOPMENT_GUIDE.md (Dev-Setup)
  - → TESTING.md (Test-Requirements)

#### 10.4 LICENSE.md
- **Zweck**: Lizenzinformationen
- **Inhalt**:
  - Project License
  - Third-Party Licenses
  - Attribution

## Diagramm-Übersicht

### Zentrale Diagramme (in mehreren Dokumenten verwendet)

1. **Layered Component Diagram** (ARCHITECTURE.md, DATA_FLOW.md)
2. **Agent Lifecycle Sequence** (AGENT_SYSTEM.md, DATA_FLOW.md)
3. **WebSocket Message Flow** (API_REFERENCE.md, DATA_FLOW.md)
4. **DTO Class Diagram** (DATA_MODELS.md, SCHEMA_SYSTEM.md)
5. **Logging Architecture** (MONITORING.md, CONFIGURATION.md)
6. **Token Tracking Flow** (AI_INTEGRATION.md, AGENT_SYSTEM.md)

### Spezifische Diagramme

- **Strategy Pattern** (DESIGN_PATTERNS.md, FILE_PROCESSING.md)
- **Context Directory Structure** (CONTEXT_MANAGEMENT.md)
- **Schema Provider Architecture** (SCHEMA_SYSTEM.md)
- **Frontend Architecture** (FRONTEND_GUIDE.md)
- **Deployment Architecture** (DEPLOYMENT.md)
- **Security Architecture** (SECURITY.md)
- **Test Pyramid** (TESTING.md)

## Verknüpfungs-Strategie

### Horizontale Verknüpfungen
- Dokumente auf gleicher Ebene verweisen aufeinander (z.B. ARCHITECTURE.md ↔ DESIGN_PATTERNS.md)

### Vertikale Verknüpfungen
- Übersichtsdokumente verweisen auf Detail-Dokumente (z.B. README.md → ARCHITECTURE.md → AGENT_SYSTEM.md)

### Querschnitts-Verknüpfungen
- Themenübergreifende Verweise (z.B. CONFIGURATION.md → SECURITY.md, MONITORING.md)

### Navigation
- Jedes Dokument hat:
  - **Breadcrumb** am Anfang (z.B. `Home > Docs > Architecture`)
  - **Table of Contents** für lange Dokumente
  - **Related Documents** am Ende
  - **Back to Top** Links bei langen Dokumenten

## Implementierungs-Reihenfolge

### Phase 1: Kern-Dokumentation (Priorität: Hoch)
1. ARCHITECTURE.md
2. API_REFERENCE.md
3. DATA_MODELS.md
4. CONFIGURATION.md
5. DEPLOYMENT.md

### Phase 2: System-Komponenten (Priorität: Hoch)
6. AGENT_SYSTEM.md
7. AI_INTEGRATION.md
8. SCHEMA_SYSTEM.md
9. CONTEXT_MANAGEMENT.md
10. FILE_PROCESSING.md

### Phase 3: Betrieb und Entwicklung (Priorität: Mittel)
11. MONITORING.md
12. SECURITY.md
13. DEVELOPMENT_GUIDE.md
14. TESTING.md
15. ERROR_HANDLING.md

### Phase 4: Frontend und Integration (Priorität: Mittel)
16. FRONTEND_GUIDE.md
17. UI_COMPONENTS.md
18. INTEGRATION_GUIDE.md
19. DATA_FLOW.md

### Phase 5: Referenz und Zusatz (Priorität: Niedrig)
20. DESIGN_PATTERNS.md
21. CODE_EXAMPLES.md
22. MIGRATION_GUIDE.md
23. TROUBLESHOOTING.md
24. PERFORMANCE.md
25. FAQ.md
26. GLOSSARY.md
27. CHANGELOG.md
28. CONTRIBUTING.md

## Qualitätskriterien

### Jedes Dokument muss:
- ✅ Einen klaren Zweck haben
- ✅ Eine Inhaltsübersicht (TOC) haben
- ✅ Mindestens 3 Verknüpfungen zu anderen Dokumenten haben
- ✅ Mindestens 1 Mermaid-Diagramm enthalten (wo sinnvoll)
- ✅ Code-Beispiele enthalten (wo relevant)
- ✅ Aktuell und wartbar sein
- ✅ Konsistente Formatierung verwenden

### Diagramm-Qualität:
- ✅ Lesbar und nicht überladen
- ✅ Konsistente Farbgebung
- ✅ Beschriftungen auf Deutsch (wo möglich)
- ✅ Legende bei komplexen Diagrammen

### Verknüpfungs-Qualität:
- ✅ Relative Links verwenden
- ✅ Beschreibende Link-Texte
- ✅ Keine toten Links
- ✅ Bidirektionale Verknüpfungen wo sinnvoll

## Wartung und Updates

### Regelmäßige Reviews:
- **Monatlich**: Überprüfung auf Aktualität
- **Bei Releases**: Update von CHANGELOG.md und betroffenen Dokumenten
- **Bei Breaking Changes**: Update von MIGRATION_GUIDE.md
- **Bei neuen Features**: Update relevanter Dokumente

### Verantwortlichkeiten:
- **Entwickler**: Technische Dokumentation aktuell halten
- **Architekten**: Architektur-Dokumente pflegen
- **DevOps**: Deployment- und Monitoring-Dokumente
- **Frontend-Team**: Frontend-Dokumentation

## Tooling

### Empfohlene Tools:
- **Markdown-Editor**: VS Code mit Markdown-Extensions
- **Diagramm-Vorschau**: Mermaid Preview Extension
- **Link-Checker**: markdown-link-check
- **Spell-Checker**: Code Spell Checker Extension
- **Dokumentations-Generator**: Docusaurus oder MkDocs (optional)

## Nächste Schritte

1. ✅ Dokumentationsplan erstellen (dieser Plan)
2. ⏳ Plan mit Team reviewen
3. ⏳ Phase 1 Dokumente erstellen
4. ⏳ Feedback einholen und iterieren
5. ⏳ Weitere Phasen implementieren
6. ⏳ Automatisierte Link-Checks einrichten
7. ⏳ Dokumentations-Website aufsetzen (optional)

---

**Erstellt**: 2025-01-16  
**Version**: 1.0  
**Status**: Draft - Zur Review
