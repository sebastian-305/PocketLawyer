# Design Document

## Overview

Dieses Design-Dokument beschreibt die technische Umsetzung von vier Systemverbesserungen für die PocketLawyer-Anwendung:

1. **Property-Displaynamen im Schema**: Erweiterung der JSON-Schemas um Frontend-spezifische Displaynamen
2. **Legacy Code Isolierung**: Entkopplung von PromptBuilder_Deprecated, ClaudeService und GeminiService
3. **Token-Tracking für Agenten**: Aggregierung und Logging von Token-Verbrauch pro Analyse
4. **Filesystem-Logging**: Persistente Speicherung von Logs in täglichen Dateien

## Architecture

### 1. Property-Displaynamen im Schema

#### Aktueller Zustand
- JSON-Schemas werden in `Infrastructure/SchemaProviders` definiert
- Schemas verwenden nur Standard-JSON-Schema-Properties
- Frontend muss Property-Namen aus technischen Keys ableiten

#### Zielarchitektur
- Schemas enthalten zusätzliches Feld `x-displayName` für jede Property
- `x-displayName` wird vom Frontend verwendet, von der AI ignoriert
- Alternative: Verwendung von `x-styling` mit verschachteltem `displayName`

#### Entscheidung
Wir verwenden `x-displayName` als direktes Property-Feld, da:
- Es einfacher zu implementieren ist
- Es dem JSON-Schema-Standard für Custom Extensions entspricht (x- Prefix)
- Es klarer kommuniziert, dass es sich um einen Displaynamen handelt

### 2. Legacy Code Isolierung

#### Betroffene Komponenten
- `Domain/PromptBuilder.cs` (veraltet, ersetzt durch PromptProvider)
- `Infrastructure/PromptBuilder_deprecated1.cs` (veraltet)
- `Infrastructure/AiServices/Claude/ClaudeService.cs` (Legacy)
- `Infrastructure/AiServices/Gemini/GeminiService.cs` (Legacy)

#### Aktuelle Architektur
- **PromptProvider** (`Domain/PromptProvider.cs`): Aktuelle Klasse für Prompt-Management
- **PromptBuilder** (`Domain/PromptBuilder.cs`): Veraltet, wird nicht mehr verwendet
- **PromptBuilder_Deprecated** (`Infrastructure/PromptBuilder_deprecated1.cs`): Veraltet

#### Isolierungsstrategie

**PromptBuilder & PromptBuilder_Deprecated:**
- Entfernung der Abhängigkeit zum `AgentType` Enum
- Ersetzung durch String-Parameter
- Markierung mit `[Obsolete]` Attribut
- Dokumentation als Legacy Code

**ClaudeService & GeminiService:**
- Markierung mit `[Obsolete]` Attribut
- Beibehaltung der aktuellen Registrierung in `Program.cs`
- Dokumentation als Legacy Services
- Keine neuen Features oder Refactorings

### 3. Token-Tracking für Agenten

#### Aktueller Zustand
- `NebiusService.SendPrompt()` loggt Token-Informationen mit Level Info
- Agenten haben keinen Zugriff auf Token-Informationen
- Keine Aggregierung von Token-Zahlen über mehrere AI-Anfragen

#### Zielarchitektur

**IAiClient Interface Erweiterung:**
```csharp
public interface IAiClient
{
    Task<AiResponse> SendPromptWithMetadata(string prompt, object? outputSchema);
}

public record AiResponse(
    string Content,
    int InputTokens,
    int OutputTokens,
    int TotalTokens
);
```

**Agent Token-Tracking:**
- Agents sammeln Token-Informationen von allen AI-Anfragen
- Aggregierung erfolgt in der Basis-Klasse `ContractAnalysisAgentBase`
- Logging nach Abschluss der Analyse mit Level Info

**NebiusService Änderungen:**
- Token-Logging auf Level Debug ändern
- Rückgabe von Token-Informationen im Response-Objekt

#### Backward Compatibility
- Bestehende `SendPrompt()` Methode bleibt erhalten
- Neue `SendPromptWithMetadata()` Methode für Token-Tracking
- Agents verwenden neue Methode, Legacy Code kann alte Methode nutzen

### 4. Filesystem-Logging

#### Konfiguration

**appsettings.json Erweiterung:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "logs"
    }
  }
}
```

#### Implementierung

**Logging Provider:**
- Verwendung von Serilog als Logging-Framework
- File Sink mit Rolling File Konfiguration
- Dateiformat: `{Path}/YYYY-MM-DD.log`

**Konfiguration in Program.cs:**
```csharp
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: LogEventLevel.Information,
            outputTemplate: "{Timestamp:yyyy-MM-DD HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
});
```

## Components and Interfaces

### 1. Schema Provider Erweiterungen

**Neue Hilfsmethode in Schema Providern:**
```csharp
protected JsonObject AddDisplayName(JsonObject property, string displayName)
{
    property["x-displayName"] = displayName;
    return property;
}
```

**Beispiel-Verwendung:**
```csharp
public JsonObject GetSchema()
{
    return new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["vermieter"] = AddDisplayName(new JsonObject
            {
                ["type"] = "string"
            }, "Vermieter"),
            ["mieter"] = AddDisplayName(new JsonObject
            {
                ["type"] = "string"
            }, "Mieter")
        }
    };
}
```

### 2. AI Client Interface

**Erweiterte Interface:**
```csharp
public interface IAiClient
{
    Task<string> SendPrompt(string prompt, object? outputSchema);
    Task<AiResponse> SendPromptWithMetadata(string prompt, object? outputSchema);
}

public record AiResponse(
    string Content,
    int InputTokens,
    int OutputTokens,
    int TotalTokens
);
```

### 3. Agent Base Class Erweiterung

**Token-Tracking in ContractAnalysisAgentBase:**
```csharp
public abstract class ContractAnalysisAgentBase
{
    protected int TotalInputTokens { get; private set; }
    protected int TotalOutputTokens { get; private set; }
    protected int TotalTokens => TotalInputTokens + TotalOutputTokens;

    protected void TrackTokens(AiResponse response)
    {
        TotalInputTokens += response.InputTokens;
        TotalOutputTokens += response.OutputTokens;
    }

    protected void LogTokenSummary()
    {
        _logger.LogInformation(
            "[Agent {AgentId}] Analysis completed. Total Input Tokens: {InputTokens}, Total Output Tokens: {OutputTokens}, Total Tokens: {TotalTokens}",
            AgentId, TotalInputTokens, TotalOutputTokens, TotalTokens);
    }
}
```

### 4. Logging Configuration

**FileLoggingOptions:**
```csharp
public class FileLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public string MinimumLevel { get; set; } = "Information";
    public string Path { get; set; } = "logs";
}
```

## Data Models

### AiResponse Record
```csharp
namespace PocketLawyer.Application.Models;

public record AiResponse(
    string Content,
    int InputTokens,
    int OutputTokens,
    int TotalTokens
);
```

### FileLoggingOptions
```csharp
namespace PocketLawyer.Options;

public class FileLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public string MinimumLevel { get; set; } = "Information";
    public string Path { get; set; } = "logs";
}
```

## Error Handling

### Schema Provider
- Keine Änderungen am Error Handling
- Bestehende `SchemaNotFoundException` bleibt erhalten

### AI Client
- Neue Methode wirft dieselben Exceptions wie alte Methode
- Zusätzliche Validierung: Token-Werte müssen >= 0 sein

### Agent Token-Tracking
- Bei Fehler in AI-Anfrage: Token-Werte werden nicht aggregiert
- Logging erfolgt nur bei erfolgreicher Analyse

### Filesystem-Logging
- Bei Fehler beim Schreiben: Fallback auf Console-Logging
- Keine Exception bei fehlenden Schreibrechten
- Automatische Verzeichniserstellung wenn nicht vorhanden

## Testing Strategy

### Unit Tests

**Schema Provider Tests:**
- Test: `x-displayName` wird korrekt zu Properties hinzugefügt
- Test: Schemas ohne `x-displayName` funktionieren weiterhin
- Test: Frontend kann `x-displayName` aus Schema extrahieren

**AI Client Tests:**
- Test: `SendPromptWithMetadata()` gibt korrektes `AiResponse` zurück
- Test: Token-Werte sind nicht-negativ
- Test: Alte `SendPrompt()` Methode funktioniert weiterhin

**Agent Token-Tracking Tests:**
- Test: Token-Aggregierung über mehrere AI-Anfragen
- Test: Token-Logging nach erfolgreicher Analyse
- Test: Kein Token-Logging bei fehlgeschlagener Analyse

**Filesystem-Logging Tests:**
- Test: Log-Dateien werden mit korrektem Dateinamen erstellt
- Test: Logs werden nur geschrieben wenn Enabled = true
- Test: MinimumLevel wird korrekt angewendet
- Test: Täglicher Rollover funktioniert

### Integration Tests

**End-to-End Token-Tracking:**
- Test: Vollständige Analyse mit Token-Tracking
- Test: Token-Summen stimmen mit einzelnen Anfragen überein

**Filesystem-Logging:**
- Test: Logs werden über mehrere Tage korrekt gespeichert
- Test: Konfigurationsänderungen werden zur Laufzeit angewendet

## Implementation Notes

### Phase 1: Property-Displaynamen
1. Hilfsmethode in Base Schema Provider erstellen
2. Alle existierenden Schema Provider aktualisieren
3. Frontend-Tests für `x-displayName` Extraktion

### Phase 2: Legacy Code Isolierung
1. `[Obsolete]` Attribute zu PromptBuilder, PromptBuilder_Deprecated, ClaudeService und GeminiService hinzufügen
2. AgentType Dependency in PromptBuilder und PromptBuilder_Deprecated entfernen
3. String-Parameter statt Enum verwenden
4. Dokumentation aktualisieren

### Phase 3: Token-Tracking
1. `AiResponse` Record erstellen
2. `IAiClient` Interface erweitern
3. `NebiusService` implementieren
4. `ContractAnalysisAgentBase` erweitern
5. Agents auf neue Methode umstellen
6. Token-Logging auf Debug ändern

### Phase 4: Filesystem-Logging
1. Serilog NuGet Package installieren
2. `FileLoggingOptions` erstellen
3. `Program.cs` Konfiguration hinzufügen
4. appsettings.json aktualisieren
5. Tests für Log-Dateierstellung

## Dependencies

### NuGet Packages
- **Serilog.AspNetCore** (>= 8.0.0): Für Filesystem-Logging
- **Serilog.Sinks.File** (>= 6.0.0): Für Rolling File Sink

### Existing Dependencies
- System.Text.Json: Für JSON-Schema-Manipulation
- Microsoft.Extensions.Logging: Für Logging-Abstraktion
- Microsoft.Extensions.Options: Für Konfiguration

## Performance Considerations

### Schema Provider
- Minimaler Overhead durch `x-displayName` (< 1% Schema-Größe)
- Keine Performance-Auswirkungen auf AI-Anfragen

### Token-Tracking
- Minimaler Memory-Overhead (3 Integer pro Agent-Instanz)
- Keine zusätzlichen AI-Anfragen
- Logging erfolgt nur einmal pro Analyse

### Filesystem-Logging
- Asynchrones Schreiben durch Serilog
- Buffering für bessere Performance
- Täglicher Rollover ohne Performance-Impact

## Security Considerations

### Filesystem-Logging
- Log-Dateien enthalten keine sensiblen Daten (PII wird bereits redacted)
- Zugriffsbeschränkung auf Log-Verzeichnis empfohlen
- Automatische Log-Rotation verhindert unbegrenztes Wachstum

### Legacy Code
- Keine neuen Security-Risiken durch Isolierung
- Bestehende Security-Maßnahmen bleiben erhalten

## Migration Strategy

### Backward Compatibility
- Alle Änderungen sind rückwärtskompatibel
- Bestehende API-Contracts bleiben unverändert
- Legacy Code funktioniert weiterhin

### Rollout Plan
1. Phase 1: Property-Displaynamen (keine Breaking Changes)
2. Phase 2: Legacy Code Isolierung (keine Breaking Changes)
3. Phase 3: Token-Tracking (neue Features, keine Breaking Changes)
4. Phase 4: Filesystem-Logging (neue Features, keine Breaking Changes)

### Rollback Strategy
- Jede Phase kann unabhängig zurückgerollt werden
- Konfigurationsänderungen können zur Laufzeit rückgängig gemacht werden
- Keine Datenbank-Migrationen erforderlich
