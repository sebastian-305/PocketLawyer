# PocketLawyer API Documentation

## Overview

PocketLawyer provides a WebSocket-based API for real-time contract analysis. The system supports multiple contract types (Rental, Employment) with both Basic and Premium analysis tiers.

## Architecture

The system follows a 3-phase agent lifecycle:

1. **Construction**: Lightweight agent creation with dependency injection
2. **Initialization**: Contract validation, type checking, and text storage
3. **Analysis**: Parallel execution of Basic and Premium analysis steps

### Key Components

- **Contract Analysis Agents**: Orchestrate the analysis workflow with token tracking
- **Schema Provider**: Provides JSON schemas as Single Source of Truth with `x-displayName` and `x-sectionTitle` metadata
- **Schema Validator**: Validates AI responses against schemas
- **Prompt Provider**: Central management of all AI prompts
- **WebSocket Handler**: Manages real-time communication with clients
- **Token Tracking**: Aggregates and logs AI token consumption per analysis
- **Filesystem Logging**: Persistent logging with daily rollover using Serilog

## WebSocket Endpoints

### Rental Contract Analysis

**Endpoint**: `GET /rental-analysis`

**Protocol**: WebSocket (WSS in production)

**Flow**:
1. Client establishes WebSocket connection
2. Client sends file data with contract type
3. Server validates file and decodes content
4. Server creates appropriate agent based on contract type
5. Server initializes agent (readability check, type detection, text storage)
6. Server sends confirmation message (agent ready)
7. Server executes Basic analysis (4 parallel steps) and sends results
8. Server executes Premium analysis (7 parallel steps) and sends results
9. Server closes connection with NormalClosure status

**Error Handling**:
- If initialization fails (unreadable, wrong type), server sends error message and closes connection
- If analysis step fails, other steps continue; failed step returns error message instead of data
- All errors are customer-friendly and localized in German

**Message Format**:

```json
// Upload Message (Client → Server)
{
  "fileName": "mietvertrag.pdf",
  "fileContent": "base64-encoded-content",
  "contractType": "Rental"
}

// Confirmation Message (Server → Client)
{
  "type": "confirmation",
  "message": "Vertrag akzeptiert, Analyse gestartet"
}

// Basic Analysis Result (Server → Client)
{
  "type": "result",
  "data": {
    "result": {
      "allgemeineVertragsdaten": { ... },
      "mietkonditionen": { ... },
      "vertragsbedingungen": { ... },
      "summary": { ... }
    },
    "metadata": {
      "contractId": "uuid",
      "countryCode": "AT",
      "language": "de",
      "analyzedAt": "2024-01-01T12:00:00Z",
      "isPreview": false,
      "modelVersion": "nebius-1.0"
    }
  }
}

// Premium Analysis Result (Server → Client)
{
  "type": "result",
  "data": {
    "result": {
      "mietzahlungUndNebenkosten": { ... },
      "mietdauerUndKuendigung": { ... },
      "gebrauchDerMietsache": { ... },
      "instandhaltungUndReparaturen": { ... },
      "rechteUndPflichten": { ... },
      "datenschutzUndKommunikation": { ... },
      "gesamtfazit": "...",
      "gesamtbewertung": "Gruen"
    },
    "metadata": { ... }
  }
}

// Error Message (Server → Client)
{
  "type": "error",
  "message": "Das Dokument konnte nicht gelesen werden."
}
```

### Employment Contract Analysis

**Endpoint**: `GET /employment-analysis`

**Protocol**: WebSocket (WSS in production)

**Flow**: Same as Rental Contract Analysis

**Message Format**: Same structure as Rental, with employment-specific data fields

## HTTP Endpoints

### Schema Retrieval

**Endpoint**: `GET /api/schemas/{contractType}/{tier}/{step}`

**Description**: Retrieves JSON schema for a specific analysis step

**Parameters**:
- `contractType`: Contract type (e.g., "Rental", "Employment")
- `tier`: Analysis tier ("Basic" or "Premium")
- `step`: Analysis step name (e.g., "General", "Costs")

**Response**:

```json
{
  "type": "object",
  "x-sectionTitle": "Allgemeine Vertragsdaten",
  "additionalProperties": false,
  "properties": {
    "vertragstyp": {
      "type": "string",
      "description": "Typ des Mietvertrags",
      "x-displayName": "Vertragstyp",
      "x-styling": {
        "displayClass": "font-bold",
        "importance": "high"
      }
    },
    "mieter": {
      "type": "string",
      "description": "Name des/der Mieter:in",
      "x-displayName": "Mieter",
      "x-styling": {
        "displayClass": "text-primary"
      }
    },
    "vermieter": {
      "type": "string",
      "description": "Name des/der Vermieter:in",
      "x-displayName": "Vermieter"
    }
  },
  "required": ["vertragstyp", "mieter", "vermieter"]
}
```

**Schema Metadata**:

All schemas include the following metadata:
- `x-displayName`: German display name for each property (used by frontend for labels)
- `x-sectionTitle`: German title for each section (used by frontend for section headers)
- `x-styling`: Display hints for frontend rendering (optional)
  - `displayClass`: CSS class suggestions
  - `importance`: Field importance level (high, medium, low)
  - `icon`: Optional icon identifier

**Note**: The frontend is fully data-driven and uses schema metadata exclusively. No hardcoded translations or mappings are required.

**Status Codes**:
- `200 OK`: Schema found and returned
- `404 Not Found`: Schema not found for given parameters
- `500 Internal Server Error`: Server error

## Data Models

### Analysis DTO Structure

All analysis results follow this structure:

```typescript
interface AnalysisDto<T> {
  result: T;
  metadata: AnalysisMetadata;
}

interface AnalysisMetadata {
  contractId: string;
  countryCode: string;
  language: string;
  analyzedAt: string; // ISO 8601 datetime
  isPreview: boolean;
  modelVersion: string;
}
```

### Rental Contract DTOs

#### Basic Analysis

```typescript
interface RentalBasicAnalysisDto {
  allgemeineVertragsdaten: {
    vertragstyp: string;
    mieter: string;
    vermieter: string;
    mietobjektAdresse: string;
    vertragsbeginn: string; // ISO 8601 date
    vertragsart: string;
  };
  mietkonditionen: {
    kaltmiete: number;
    nebenkosten: number;
    warmmiete: number;
    kaution: number;
    zahlungsweise: string;
    nebenkostenregelung: string;
  };
  vertragsbedingungen: {
    kuendigungsfrist: string;
    tierhaltung: string;
    untervermietung: string;
    besonderheiten: string;
  };
  summary: {
    kurzfazit: string;
    gesamtfazit: string;
    gesamtbewertung: string;
    riskLevel: "Gruen" | "Gelb" | "Rot";
  };
}
```

#### Premium Analysis

```typescript
interface RentalPremiumAnalysisDto {
  mietzahlungUndNebenkosten: PremiumSection;
  mietdauerUndKuendigung: PremiumSection;
  gebrauchDerMietsache: PremiumSection;
  instandhaltungUndReparaturen: PremiumSection;
  rechteUndPflichten: PremiumSection;
  datenschutzUndKommunikation: PremiumSection;
  gesamtfazit: string;
  gesamtbewertung: "Gruen" | "Gelb" | "Rot";
}

interface PremiumSection {
  zusammenfassung: string;
  bewertung: string;
  rechtsgrundlage: string;
  risikoScore: "Zulaessig" | "Nachteilhaft" | "Unzulaessig";
  riskLevel: "Gruen" | "Gelb" | "Rot";
}
```

### Employment Contract DTOs

```typescript
interface EmploymentBasicAnalysisDto {
  allgemeineVertragsdaten: {
    arbeitgeber: string;
    arbeitnehmer: string;
    position: string;
    vertragsbeginn: string;
    vertragsart: string;
    probezeit: string;
  };
  arbeitsbedingungen: {
    arbeitszeit: string;
    gehalt: string;
    urlaubsanspruch: string;
    kuendigungsfrist: string;
    arbeitsort: string;
  };
}
```

## Error Handling

### Error Types

The system uses a hierarchical exception structure:

- `ContractAnalysisException`: Base exception for all contract analysis errors
  - `ContractValidationException`: Contract validation failures
  - `UnsupportedContractTypeException`: Unknown contract type
  - `SchemaNotFoundException`: Schema not found
  - `PromptNotFoundException`: Prompt not found
  - `AiServiceException`: AI service errors

### Error Messages

All error messages sent to clients are customer-friendly and localized in German:

- **Unreadable Contract**: "Das Dokument konnte nicht gelesen werden. Bitte stellen Sie sicher, dass es sich um eine gültige PDF-, Text- oder Bilddatei handelt."
- **Wrong Contract Type**: "Der hochgeladene Vertrag entspricht nicht dem erwarteten Vertragstyp."
- **Analysis Step Failed**: "Ein Teil der Analyse konnte nicht durchgeführt werden. Die übrigen Ergebnisse sind verfügbar."
- **AI Service Unavailable**: "Der KI-Service ist vorübergehend nicht verfügbar. Bitte versuchen Sie es später erneut."
- **Validation Failed**: "Die Analyse konnte nicht validiert werden. Bitte versuchen Sie es erneut."
- **Unsupported Contract Type**: "Der angegebene Vertragstyp wird nicht unterstützt."
- **Schema Not Found**: "Das erforderliche Schema konnte nicht gefunden werden."
- **Agent Not Initialized**: "Der Agent wurde nicht korrekt initialisiert."

### Error Response Format

```json
{
  "type": "error",
  "message": "Customer-friendly error message in German"
}
```

## Performance Characteristics

### Parallel Execution

- All analysis steps within a tier (Basic/Premium) execute in parallel using `Task.WhenAll`
- Failed steps don't block other steps from completing
- Failed steps return error messages instead of throwing exceptions

### Timing

- Agent construction: < 1ms (dependency injection only)
- Initialization: ~1-2 seconds (AI calls for readability and type detection)
- Basic analysis: ~5-10 seconds (4 parallel steps)
- Premium analysis: ~10-20 seconds (7 parallel steps)

### Scalability

- Agents are stateless per-request (scoped lifetime)
- Can scale horizontally across multiple instances
- WebSocket connections are independent

## Security Considerations

### Input Validation

- File size limits enforced (default: 20MB)
- File type validation (PDF, text, images)
- Contract text sanitization

### Data Protection

- Contract text not logged or persisted
- DSGVO-compliant data handling
- Secure WebSocket connections (WSS in production)

### Rate Limiting

Consider implementing rate limiting for:
- WebSocket connections per IP
- AI service calls per user
- File uploads per time window

## Token Tracking

### Overview

All contract analysis agents automatically track AI token consumption during analysis. Token information is aggregated across all analysis steps and logged at the end of each analysis tier (Basic/Premium).

### Token Metrics

- **Input Tokens**: Number of tokens sent to the AI (prompt + schema)
- **Output Tokens**: Number of tokens generated by the AI (response)
- **Total Tokens**: Sum of input and output tokens

### Implementation

**AiResponse Record**:
```csharp
public record AiResponse(
    string Content,
    int InputTokens,
    int OutputTokens,
    int TotalTokens
);
```

**IAiClient Interface**:
```csharp
public interface IAiClient
{
    Task<string> SendPrompt(string prompt, object? outputSchema);
    Task<AiResponse> SendPromptWithMetadata(string prompt, object? outputSchema);
}
```

**Agent Token Tracking**:
```csharp
// In ContractAnalysisAgentBase
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
```

### Logging Levels

- **Debug**: Individual token counts per AI request (NebiusService)
- **Info**: Aggregated token summary per analysis tier (Agent)

### Example Log Output

```
[2024-01-15 14:23:45.123 +01:00] [INF] [Agent rental-abc123] Analysis completed. Total Input Tokens: 12450, Total Output Tokens: 3821, Total Tokens: 16271
```

## Configuration

### Required Settings

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
    "Responses": "data/Responses"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
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

### Filesystem Logging Configuration

**FileLoggingOptions**:
- `Enabled` (bool): Enable/disable filesystem logging (default: `true`)
- `MinimumLevel` (string): Minimum log level for file logs (default: `"Information"`)
  - Possible values: `"Debug"`, `"Information"`, `"Warning"`, `"Error"`
- `Path` (string): Directory for log files (default: `"logs"`)

**Log File Format**:
- Files are created daily with format: `YYYY-MM-DD.log`
- Example: `logs/2024-01-15.log`
- Automatic rollover at midnight

**Log Entry Format**:
```
{Timestamp:yyyy-MM-DD HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}
```

**Example**:
```
2024-01-15 14:23:45.123 +01:00 [INF] [Agent rental-abc123] Analysis completed. Total Input Tokens: 12450, Total Output Tokens: 3821, Total Tokens: 16271
2024-01-15 14:23:45.456 +01:00 [WRN] Schema validation warning: Optional field missing
2024-01-15 14:23:46.789 +01:00 [ERR] AI service error: Rate limit exceeded
```

### Environment Variables

For production, use environment variables or secrets management:

- `NEBIUS_API_KEY`: Nebius API key
- `ASPNETCORE_ENVIRONMENT`: Environment name
- `ASPNETCORE_URLS`: Listening URLs

## Monitoring and Logging

### Logged Events

- Agent initialization (Info level)
- Analysis step start/completion with duration (Info level)
- Token consumption per analysis tier (Info level)
- Individual AI request tokens (Debug level)
- Validation failures (Warning level)
- Errors and exceptions (Error level)

### Metrics to Track

- Analysis duration per step
- Token consumption per analysis (input, output, total)
- Success/failure rates
- AI service response times
- WebSocket connection duration
- Error rates by type

### Log File Management

- **Retention**: Implement log retention policy (e.g., keep last 30 days)
- **Size**: Monitor log directory size
- **Rotation**: Automatic daily rotation
- **Backup**: Consider backing up logs for compliance

## Client Integration

### JavaScript Example

```javascript
const ws = new WebSocket('ws://localhost:5112/rental-analysis');

ws.onopen = () => {
  // Send file data
  ws.send(JSON.stringify({
    fileName: 'contract.pdf',
    fileContent: base64Content,
    contractType: 'Rental'
  }));
};

ws.onmessage = (event) => {
  const message = JSON.parse(event.data);
  
  switch (message.type) {
    case 'confirmation':
      console.log('Analysis started:', message.message);
      break;
    case 'result':
      console.log('Analysis result:', message.data);
      break;
    case 'error':
      console.error('Analysis error:', message.message);
      break;
  }
};

ws.onerror = (error) => {
  console.error('WebSocket error:', error);
};

ws.onclose = (event) => {
  console.log('Connection closed:', event.code, event.reason);
};
```

### C# Client Example

```csharp
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var ws = new ClientWebSocket();
await ws.ConnectAsync(new Uri("ws://localhost:5112/rental-analysis"), CancellationToken.None);

// Send file data
var uploadMessage = new {
    fileName = "contract.pdf",
    fileContent = Convert.ToBase64String(fileBytes),
    contractType = "Rental"
};
var json = JsonSerializer.Serialize(uploadMessage);
var bytes = Encoding.UTF8.GetBytes(json);
await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

// Receive messages
var buffer = new byte[1024 * 4];
while (ws.State == WebSocketState.Open)
{
    var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
    var data = JsonSerializer.Deserialize<JsonElement>(message);
    
    var messageType = data.GetProperty("type").GetString();
    Console.WriteLine($"Received: {messageType}");
}
```

## Deprecated Components

The following components are marked as deprecated and will be removed in future versions:

### Legacy Prompt Builders

**PromptBuilder** (`Domain/PromptBuilder.cs`):
- Status: `[Obsolete]`
- Replacement: Use `PromptProvider` (`Domain/PromptProvider.cs`)
- Reason: Replaced by static PromptProvider with better organization

**PromptBuilder_Deprecated** (`Infrastructure/PromptBuilder_deprecated1.cs`):
- Status: `[Obsolete]`
- Replacement: Use `PromptProvider`
- Reason: Old implementation, no longer maintained

### Legacy AI Services

**ClaudeService** (`Infrastructure/AiServices/Claude/ClaudeService.cs`):
- Status: `[Obsolete]`
- Replacement: Use `NebiusService` (`Infrastructure/AiServices/Nebius/NebiusService.cs`)
- Reason: Consolidated on Nebius API for better performance and token tracking

**GeminiService** (`Infrastructure/AiServices/Gemini/GeminiService.cs`):
- Status: `[Obsolete]`
- Replacement: Use `NebiusService`
- Reason: Consolidated on Nebius API for better performance and token tracking

### Migration Guide

**From PromptBuilder to PromptProvider**:
```csharp
// Old (Deprecated)
var prompt = PromptBuilder.BuildAnalysisPrompt(contractText, agentType);

// New
var prompt = PromptProvider.GetPrompt("Rental", "Basic", "General");
```

**From ClaudeService/GeminiService to NebiusService**:
```csharp
// Old (Deprecated)
services.AddKeyedScoped<IAiClient, ClaudeService>("claude");

// New
services.AddScoped<IAiClient, NebiusService>();
```

**Token Tracking Migration**:
```csharp
// Old (No token tracking)
var response = await _aiClient.SendPrompt(prompt, schema);

// New (With token tracking)
var aiResponse = await _aiClient.SendPromptWithMetadata(prompt, schema);
TrackTokens(aiResponse);
var response = aiResponse.Content;
```

## Future Enhancements

### Planned Features

1. **Additional Contract Types**: Support for more contract types (purchase, service, etc.)
2. **Caching**: Cache analysis results for identical contracts
3. **Streaming Results**: Stream partial results as steps complete
4. **Multi-Language Support**: Support for contracts in multiple languages
5. **Advanced Validation**: Business rule validation beyond schema validation
6. **Analytics**: Track analysis patterns and common issues
7. **Token Cost Tracking**: Calculate costs based on token consumption
8. **Log Analytics**: Dashboard for log analysis and monitoring

### API Versioning

Future versions will use URL-based versioning:
- `/v1/rental-analysis`
- `/v2/rental-analysis`

## Support

For issues or questions:
- Check the integration tests in `PocketLawyer.Tests/Integration/`
- Review the design document in `.kiro/specs/agent-refactoring/design.md`
- Consult the requirements in `.kiro/specs/agent-refactoring/requirements.md`
