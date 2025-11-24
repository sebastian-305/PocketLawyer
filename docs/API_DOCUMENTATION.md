# PocketLawyer API Documentation

## Overview

PocketLawyer provides a WebSocket-based API for real-time contract analysis. The system supports multiple contract types (Rental, Employment) with both Basic and Premium analysis tiers.

## Architecture

The system follows a 3-phase agent lifecycle:

1. **Construction**: Lightweight agent creation with dependency injection
2. **Initialization**: Contract validation, type checking, and text storage
3. **Analysis**: Parallel execution of Basic and Premium analysis steps

### Key Components

- **Contract Analysis Agents**: Orchestrate the analysis workflow
- **Schema Provider**: Provides JSON schemas as Single Source of Truth
- **Schema Validator**: Validates AI responses against schemas
- **Prompt Provider**: Central management of all AI prompts
- **WebSocket Handler**: Manages real-time communication with clients

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
  "additionalProperties": false,
  "properties": {
    "vertragstyp": {
      "type": "string",
      "description": "Typ des Mietvertrags",
      "x-title": "Vertragstyp",
      "x-styling": {
        "displayClass": "font-bold",
        "importance": "high"
      }
    },
    "mieter": {
      "type": "string",
      "description": "Name des/der Mieter:in",
      "x-title": "Mieter",
      "x-styling": {
        "displayClass": "text-primary"
      }
    }
  },
  "required": ["vertragstyp", "mieter"]
}
```

**Schema Metadata**:

All schemas include the following metadata:
- `x-title`: German display name for each field (used by frontend for labels)
- `x-styling`: Display hints for frontend rendering
  - `displayClass`: CSS class suggestions
  - `importance`: Field importance level (high, medium, low)
  - `icon`: Optional icon identifier

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
  }
}
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
- Validation failures (Warning level)
- Errors and exceptions (Error level)

### Metrics to Track

- Analysis duration per step
- Success/failure rates
- AI service response times
- WebSocket connection duration
- Error rates by type

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

## Future Enhancements

### Planned Features

1. **Additional Contract Types**: Support for more contract types (purchase, service, etc.)
2. **Caching**: Cache analysis results for identical contracts
3. **Streaming Results**: Stream partial results as steps complete
4. **Multi-Language Support**: Support for contracts in multiple languages
5. **Advanced Validation**: Business rule validation beyond schema validation
6. **Analytics**: Track analysis patterns and common issues

### API Versioning

Future versions will use URL-based versioning:
- `/v1/rental-analysis`
- `/v2/rental-analysis`

## Support

For issues or questions:
- Check the integration tests in `PocketLawyer.Tests/Integration/`
- Review the design document in `.kiro/specs/agent-refactoring/design.md`
- Consult the requirements in `.kiro/specs/agent-refactoring/requirements.md`
