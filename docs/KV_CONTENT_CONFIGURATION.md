# Kollektivvertrag-Inhalt: Konfiguration

## Problem: KV-Inhalt zu groß für Token-Limit

### Symptom
```
System.ClientModel.ClientResultException: Service request failed.
Status: 400 (Bad Request)
...
This model's maximum context length is 131072 tokens. However, you requested 178701 tokens...
```

Der Kollektivvertrag-Inhalt ist zu groß und überschreitet das Token-Limit des AI-Modells.

## Ursache

Einige Kollektivverträge sind sehr umfangreich:
- **Beispiel**: `kollektivvertrag_arbeiter_innen_bei_den_sozialversicherungstragern`
- **Größe**: 970.000+ Zeichen (ca. 6 Dateien)
- **Token-Schätzung**: ~240.000 Tokens (bei 4 Zeichen pro Token)
- **Limit**: 131.072 Tokens (Nebius/Llama-3.3-70B)

Zusammen mit:
- Vertragtext
- Context-Dateien
- Prompt-Anweisungen
- Schema-Definition

...wird das Token-Limit überschritten.

## Lösung: Konfigurierbare KV-Einbindung

### 1. Neue Konfigurationsoption

In `appsettings.json`:

```json
{
  "Context": {
    "BasePath": "data/Context",
    "CreateReadmeFiles": true,
    "MaxContextSizeBytes": 5242880,
    "IncludeKvContentInAnalysis": false
  }
}
```

### 2. Option-Beschreibung

**`IncludeKvContentInAnalysis`** (bool, default: `true`)

- **`true`**: KV-Inhalt wird in Premium-Analyse-Prompts eingebunden
  - **Vorteil**: AI kann KV-spezifische Regelungen direkt referenzieren
  - **Nachteil**: Kann Token-Limit überschreiten bei großen KVs
  
- **`false`**: KV-Inhalt wird NICHT eingebunden
  - **Vorteil**: Vermeidet Token-Limit-Probleme
  - **Nachteil**: AI kann keine KV-spezifischen Details analysieren

### 3. Verhalten

#### KV-Erkennung (immer aktiv)
- KV wird **immer** erkannt (unabhängig von der Option)
- KV-Name wird in Basic-Summary angezeigt
- KV-Dateien werden geladen (für spätere Verwendung)

#### Basic-Analyse (KV-Inhalt nie verwendet)
- KV-Name wird in Summary erwähnt
- KV-Inhalt wird **nicht** in Prompts eingebunden
- Keine Token-Probleme

#### Premium-Analyse (abhängig von Option)
- **`IncludeKvContentInAnalysis: true`**:
  - KV-Inhalt wird in alle Premium-Prompts eingebunden
  - AI kann KV-spezifische Regelungen analysieren
  
- **`IncludeKvContentInAnalysis: false`**:
  - KV-Inhalt wird **nicht** eingebunden
  - AI analysiert nur basierend auf allgemeinen Gesetzen
  - Kein Token-Limit-Problem

## Implementierung

### Code-Änderungen

#### 1. `ContextOptions.cs`
```csharp
public class ContextOptions
{
    public string BasePath { get; set; } = "data/Context";
    public bool CreateReadmeFiles { get; set; } = true;
    public long MaxContextSizeBytes { get; set; } = 5 * 1024 * 1024;
    
    /// <summary>
    /// Controls whether detected Kollektivvertrag content should be included in analysis prompts.
    /// </summary>
    public bool IncludeKvContentInAnalysis { get; set; } = true;
}
```

#### 2. `EmploymentContractAgent.cs`
```csharp
private readonly ContextOptions _contextOptions;

// Constructor
public EmploymentContractAgent(
    ...,
    IOptions<ContextOptions> contextOptions,
    ...)
{
    _contextOptions = contextOptions?.Value ?? throw new ArgumentNullException(nameof(contextOptions));
}

// Helper method
private string AddKvContextIfEnabled(string prompt)
{
    if (!_contextOptions.IncludeKvContentInAnalysis)
    {
        _logger.LogDebug("[Agent {AgentId}] KV content inclusion is disabled in configuration", AgentId);
        return prompt;
    }
    
    if (!string.IsNullOrWhiteSpace(_detectedKvText))
    {
        prompt += $"\n\n## Kollektivvertrag Context\n\n{_detectedKvText}";
        _logger.LogDebug("[Agent {AgentId}] KV context added: {KvLength} chars", AgentId, _detectedKvText.Length);
    }
    
    return prompt;
}
```

#### 3. Premium-Analyse-Methoden
Alle 6 Premium-Methoden verwenden jetzt:
```csharp
// Add KV text as additional context if available and enabled
prompt = AddKvContextIfEnabled(prompt);
```

### Logging

#### Mit KV-Inhalt (enabled)
```
[DBG] [Agent xyz] KV context added: 970123 chars
```

#### Ohne KV-Inhalt (disabled)
```
[DBG] [Agent xyz] KV content inclusion is disabled in configuration
```

## Empfehlungen

### Wann `IncludeKvContentInAnalysis: true` verwenden?

✅ **Verwende `true` wenn:**
- KVs sind klein (< 100.000 Zeichen)
- Detaillierte KV-Analyse ist wichtig
- Token-Limit ist ausreichend (> 200.000 Tokens)
- Premium-Analyse soll KV-spezifische Regelungen prüfen

### Wann `IncludeKvContentInAnalysis: false` verwenden?

✅ **Verwende `false` wenn:**
- KVs sind sehr groß (> 500.000 Zeichen)
- Token-Limit-Fehler auftreten
- Allgemeine Analyse ausreichend ist
- Performance wichtiger ist als KV-Details

## Alternative Lösungen

### 1. KV-Zusammenfassung
Statt vollständigen KV-Text, nur relevante Abschnitte einbinden:
```csharp
// Beispiel: Nur Gehalt-relevante Paragraphen für Compensation-Analyse
var relevantKvSections = ExtractRelevantSections(_detectedKvText, "Gehalt");
```

### 2. KV-Chunking
KV in kleinere Chunks aufteilen und nur relevante Chunks einbinden:
```csharp
// Beispiel: Nur Paragraphen 1-10 für Basic-Analyse
var kvChunk = GetKvChunk(_detectedKvText, startParagraph: 1, endParagraph: 10);
```

### 3. Größeres Modell
Modell mit höherem Token-Limit verwenden:
- **Llama-3.3-70B**: 131.072 Tokens
- **GPT-4**: 128.000 Tokens
- **Claude-3**: 200.000 Tokens
- **Gemini-1.5-Pro**: 1.000.000 Tokens

### 4. KV-Datenbank
KV-Inhalte in Vektor-Datenbank speichern und nur relevante Abschnitte abrufen:
```csharp
// Beispiel: RAG (Retrieval-Augmented Generation)
var relevantKvParagraphs = await _kvVectorDb.SearchAsync(query: "Gehalt Überstunden");
```

## Testing

### Test 1: Mit KV-Inhalt (enabled)
```json
{
  "Context": {
    "IncludeKvContentInAnalysis": true
  }
}
```
**Expected**: KV-Inhalt wird in Premium-Prompts eingebunden  
**Result**: ✓ Pass (wenn KV klein genug ist)

### Test 2: Ohne KV-Inhalt (disabled)
```json
{
  "Context": {
    "IncludeKvContentInAnalysis": false
  }
}
```
**Expected**: KV-Inhalt wird NICHT eingebunden  
**Result**: ✓ Pass (keine Token-Limit-Fehler)

### Test 3: Großer KV mit enabled
```json
{
  "Context": {
    "IncludeKvContentInAnalysis": true
  }
}
```
**Input**: KV mit 970.000+ Zeichen  
**Expected**: Token-Limit-Fehler  
**Result**: ✓ Pass (Fehler tritt auf wie erwartet)

### Test 4: Großer KV mit disabled
```json
{
  "Context": {
    "IncludeKvContentInAnalysis": false
  }
}
```
**Input**: KV mit 970.000+ Zeichen  
**Expected**: Keine Token-Limit-Fehler  
**Result**: ✓ Pass (Analyse läuft durch)

## Monitoring

Überwache folgende Metriken:
1. **Token-Verbrauch** pro Analyse
2. **Häufigkeit von Token-Limit-Fehlern**
3. **KV-Größen** (welche KVs sind problematisch?)
4. **Analyse-Qualität** mit vs. ohne KV-Inhalt

## Zukünftige Verbesserungen

1. **Automatische Erkennung**: System erkennt automatisch, ob KV zu groß ist
2. **Intelligentes Chunking**: Nur relevante KV-Abschnitte einbinden
3. **KV-Zusammenfassungen**: Vorab-Zusammenfassungen von KVs erstellen
4. **Adaptive Strategie**: Basierend auf KV-Größe automatisch entscheiden
