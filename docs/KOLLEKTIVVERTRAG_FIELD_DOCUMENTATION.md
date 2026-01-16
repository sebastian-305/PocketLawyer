# Kollektivvertrag-Feld in der Basic-Analyse

## Übersicht

Das `kollektivvertrag`-Feld in der Basic-Analyse (`EmploymentBasicSummaryDto`) zeigt den Status der Kollektivvertragserkennung an.

## Mögliche Werte

### 1. Erkannter Kollektivvertrag
Wenn ein KV eindeutig oder mit hoher Wahrscheinlichkeit erkannt wurde:
```
"kollektivvertrag": "kollektivvertrag_handel"
```

### 2. Kein passender Kollektivvertrag
Wenn kein passender KV in der Datenbank gefunden wurde:
```
"kollektivvertrag": "Kein passender Kollektivvertrag gefunden"
```

### 3. Nicht eindeutig identifiziert
Wenn mehrere KVs möglich sind oder die AI-Abfragen unterschiedliche Ergebnisse liefern:
```
"kollektivvertrag": "Kollektivvertrag konnte nicht eindeutig identifiziert werden (mehrere mögliche Treffer)"
```

### 4. Fehler bei der Erkennung
Wenn ein technischer Fehler auftritt:
```
"kollektivvertrag": "Fehler bei der Kollektivvertragserkennung"
```

## Implementierung

### 1. KV-Erkennung (`KvAiDetectionService`)
- Sendet den Vertragstext und die Liste verfügbarer KVs an die AI (Claude)
- Führt 2 parallele Abfragen durch für höhere Zuverlässigkeit
- Loggt alle erkannten KVs: `[Agent xyz] Detected KVs from attempts: [kv1, kv2]`
- Behandelt Sonderfälle:
  - **"Multiple"**: Wird als "nicht eindeutig" behandelt
  - **"Keiner"**: Wird als "kein passender KV" behandelt
  - **Unterschiedliche Ergebnisse**: Wird als "nicht eindeutig" behandelt

### 2. Agent-Initialisierung (`EmploymentContractAgent.InitializeAsync`)
- Ruft `KvDetectionService.DetectAsync()` auf
- Speichert erkannten KV-Namen in `_detectedKvName`
- Speichert KV-Erkennungsnachricht in `_kvDetectionMessage`
- Lädt KV-Inhalt in `_detectedKvText` (für Premium-Analyse)

### 3. Summary-Analyse (`AnalyzeBasicSummaryAsync`)
- Fügt KV-Information zum Prompt hinzu:
  - Bei erkanntem KV: `## Erkannter Kollektivvertrag\n\n{_detectedKvName}`
  - Bei nicht erkanntem KV: `## Kollektivvertrag\n\n{_kvDetectionMessage}`
- Weist AI explizit an, den exakten Text zu verwenden
- AI befüllt das `kollektivvertrag`-Feld im JSON-Response

### 4. Context-Guidelines (`summary-guidelines.txt`)
- Enthält detaillierte Anweisungen für die AI
- Betont, dass der Text EXAKT übernommen werden muss
- Verbietet eigene Interpretation oder Erfindung von KV-Namen

## Logging

### Erfolgreiche Erkennung
```
[Agent xyz] Available KV directories: 54
[Agent xyz] KV Detection attempt 1: kollektivvertrag_handel
  Begründung: Der Vertrag erwähnt explizit Einzelhandel und die Position ist Verkäufer. Dies passt eindeutig zum Kollektivvertrag Handel.
[Agent xyz] KV Detection attempt 2: kollektivvertrag_handel
  Begründung: Die Branche ist klar als Handel identifiziert, Position und Tätigkeitsbeschreibung passen zum KV Handel.
[Agent xyz] Detection answers all equal: True
[Agent xyz] KV detected: kollektivvertrag_handel
[Agent xyz] Reasoning from AI:
  Attempt 1: Der Vertrag erwähnt explizit Einzelhandel und die Position ist Verkäufer. Dies passt eindeutig zum Kollektivvertrag Handel.
  Attempt 2: Die Branche ist klar als Handel identifiziert, Position und Tätigkeitsbeschreibung passen zum KV Handel.
[Agent xyz] KV Detection Result: Kollektivvertrag eindeutig erkannt: kollektivvertrag_handel
[Agent xyz] Detected KVs from attempts: [kollektivvertrag_handel, kollektivvertrag_handel]
[Agent xyz] Kollektivvertrag detected: kollektivvertrag_handel (AllEqual: True)
[Agent xyz] KV text loaded: 45678 characters from 3 files
```

### Nicht eindeutige Erkennung
```
[Agent xyz] Available KV directories: 54
[Agent xyz] KV Detection attempt 1: kollektivvertrag_handel
  Begründung: Der Vertrag könnte zum Handel passen, aber auch Gastronomie-Elemente sind erkennbar.
[Agent xyz] KV Detection attempt 2: kollektivvertrag_hotel_und_gastgewerbe
  Begründung: Die Tätigkeitsbeschreibung deutet auf Gastronomie hin, aber Handel ist auch möglich.
[Agent xyz] Detection answers all equal: False
[Agent xyz] KV Detection Result: Kollektivvertrag erkannt (nicht eindeutig): kollektivvertrag_handel
[Agent xyz] Detected KVs from attempts: [kollektivvertrag_handel, kollektivvertrag_hotel_und_gastgewerbe]
[Agent xyz] Kollektivvertrag detected: kollektivvertrag_handel (AllEqual: False)
```

### Multiple erkannt
```
[Agent xyz] Available KV directories: 54
[Agent xyz] KV Detection attempt 1: Multiple
  Begründung: Der Vertrag könnte sowohl zum KV Handel als auch zum KV Gastronomie passen, da beide Branchen erwähnt werden. Eine eindeutige Zuordnung ist nicht möglich.
[Agent xyz] KV Detection attempt 2: Multiple
  Begründung: Es werden mehrere Tätigkeitsbereiche genannt (Verkauf und Service), die verschiedenen KVs zugeordnet werden könnten. Ohne weitere Informationen ist keine eindeutige Zuordnung möglich.
[Agent xyz] Detection answers all equal: True
[Agent xyz] Multiple KVs detected, cannot determine unique KV
[Agent xyz] Detailed reasoning from AI:
  Attempt 1: Der Vertrag könnte sowohl zum KV Handel als auch zum KV Gastronomie passen, da beide Branchen erwähnt werden. Eine eindeutige Zuordnung ist nicht möglich.
  Attempt 2: Es werden mehrere Tätigkeitsbereiche genannt (Verkauf und Service), die verschiedenen KVs zugeordnet werden könnten. Ohne weitere Informationen ist keine eindeutige Zuordnung möglich.
[Agent xyz] KV Detection Result: Kollektivvertrag konnte nicht eindeutig identifiziert werden (mehrere mögliche Treffer)
[Agent xyz] Detected KVs from attempts: [Multiple, Multiple]
[Agent xyz] No Kollektivvertrag detected: Kollektivvertrag konnte nicht eindeutig identifiziert werden (mehrere mögliche Treffer)
```

### Kein KV gefunden
```
[Agent xyz] Available KV directories: 54
[Agent xyz] KV Detection attempt 1: Keiner
  Begründung: Der Vertrag bezieht sich auf eine IT-Firma, aber keiner der verfügbaren Kollektivverträge deckt den IT-Bereich ab.
[Agent xyz] KV Detection attempt 2: Keiner
  Begründung: Die Branche ist Software-Entwicklung, für die kein passender KV in der Liste vorhanden ist.
[Agent xyz] Detection answers all equal: True
[Agent xyz] No matching KV detected
[Agent xyz] Reasoning from AI:
  Attempt 1: Der Vertrag bezieht sich auf eine IT-Firma, aber keiner der verfügbaren Kollektivverträge deckt den IT-Bereich ab.
  Attempt 2: Die Branche ist Software-Entwicklung, für die kein passender KV in der Liste vorhanden ist.
[Agent xyz] KV Detection Result: Kein passender Kollektivvertrag gefunden
[Agent xyz] Detected KVs from attempts: [Keiner, Keiner]
[Agent xyz] No Kollektivvertrag detected: Kein passender Kollektivvertrag gefunden
```

## Frontend-Darstellung

Das Frontend sollte das `kollektivvertrag`-Feld entsprechend darstellen:

### Erkannter KV
```html
<div class="kv-info success">
  <i class="icon-check"></i>
  <strong>Kollektivvertrag:</strong> kollektivvertrag_handel
</div>
```

### Kein KV gefunden
```html
<div class="kv-info warning">
  <i class="icon-info"></i>
  <strong>Kollektivvertrag:</strong> Kein passender Kollektivvertrag gefunden
  <p class="hint">Es konnte kein spezifischer Kollektivvertrag identifiziert werden.</p>
</div>
```

### Nicht eindeutig
```html
<div class="kv-info warning">
  <i class="icon-alert"></i>
  <strong>Kollektivvertrag:</strong> Kollektivvertrag konnte nicht eindeutig identifiziert werden
  <p class="hint">Mehrere Kollektivverträge könnten zutreffen. Für eine genaue Zuordnung wird eine manuelle Prüfung empfohlen.</p>
</div>
```

## Schema-Definition

Das Feld ist im DTO definiert:

```csharp
[property: JsonPropertyName("kollektivvertrag")]
[property: SchemaDisplay("Kollektivvertrag",
    Description = "Erwähnung des erkannten Kollektivvertrags oder 'Kein Kollektivvertrag erkannt'.",
    Icon = "document",
    DisplayClass = "badge badge-info",
    Importance = "high")]
string Kollektivvertrag
```

Das JSON-Schema wird automatisch aus dem DTO generiert und enthält:
- **type**: string
- **description**: Erwähnung des erkannten Kollektivvertrags oder 'Kein Kollektivvertrag erkannt'
- **required**: true (Feld muss immer vorhanden sein)

## Testfälle

### Test 1: Eindeutige Erkennung
- **Input**: Arbeitsvertrag mit klarem Bezug zu einem KV
- **Expected**: `"kollektivvertrag": "kollektivvertrag_handel"`

### Test 2: Kein KV
- **Input**: Arbeitsvertrag ohne KV-Bezug
- **Expected**: `"kollektivvertrag": "Kein passender Kollektivvertrag gefunden"`

### Test 3: Multiple
- **Input**: Arbeitsvertrag, der zu mehreren KVs passen könnte
- **Expected**: `"kollektivvertrag": "Kollektivvertrag konnte nicht eindeutig identifiziert werden (mehrere mögliche Treffer)"`

### Test 4: Unterschiedliche AI-Antworten
- **Input**: AI-Abfrage 1 erkennt KV A, Abfrage 2 erkennt KV B
- **Expected**: `"kollektivvertrag": "kollektivvertrag_a"` (häufigster Wert) mit Log-Warnung

## Zukünftige Verbesserungen

1. **Konfidenz-Score**: Hinzufügen eines Konfidenz-Werts (0-100%)
2. **Alternative KVs**: Liste möglicher alternativer KVs bei nicht eindeutiger Erkennung
3. **Manuelle Korrektur**: API-Endpoint zum manuellen Setzen des KVs
4. **KV-Vorschläge**: Bei "Multiple" die konkreten KV-Namen zurückgeben
5. **Caching**: Erkannte KVs für ähnliche Verträge cachen

## Troubleshooting: Warum erkennt Claude immer "Multiple"?

### Mögliche Ursachen:

1. **Zu viele KVs in der Liste**: 54 KVs sind sehr viele für Claude zu verarbeiten
   - **Lösung**: KVs nach Branchen vorfiltern oder gruppieren
   
2. **Unklare Vertragsinformationen**: Der Vertrag enthält keine eindeutigen Branchenhinweise
   - **Lösung**: Prompt verbessern, um spezifische Merkmale zu suchen
   
3. **KV-Namen sind zu ähnlich**: Viele KVs haben ähnliche Namen
   - **Lösung**: Zusätzliche Metadaten zu KVs hinzufügen (Branche, Tätigkeitsbereich)
   
4. **Claude ist zu vorsichtig**: Das Modell bevorzugt "Multiple" bei Unsicherheit
   - **Lösung**: Prompt anpassen, um eine Entscheidung zu erzwingen

### Debugging-Schritte:

1. **Logs prüfen**: Schaue dir die Begründungen in den Logs an
   ```
   [Agent xyz] Detailed reasoning from AI:
     Attempt 1: [Begründung hier]
     Attempt 2: [Begründung hier]
   ```

2. **Vertrag analysieren**: Welche Branche/Position wird erwähnt?
   - Ist die Branche klar erkennbar?
   - Gibt es widersprüchliche Informationen?

3. **KV-Liste prüfen**: Gibt es passende KVs in der Liste?
   - Sind die KV-Namen aussagekräftig?
   - Fehlt ein relevanter KV?

4. **Prompt testen**: Teste mit einem sehr eindeutigen Vertrag
   - Beispiel: "Verkäufer bei REWE" sollte eindeutig "kollektivvertrag_handel" ergeben

### Verbesserungsvorschläge:

#### 1. KV-Vorfilterung
Filtere die KV-Liste basierend auf Schlüsselwörtern im Vertrag:
```csharp
// Beispiel: Nur relevante KVs an Claude senden
var relevantKvs = FilterKvsByKeywords(contractContent, allKvs);
```

#### 2. Zwei-Stufen-Erkennung
Erste Stufe: Branche erkennen → Zweite Stufe: Spezifischen KV wählen

#### 3. Zusätzliche Metadaten
Füge zu jedem KV Metadaten hinzu:
```json
{
  "name": "kollektivvertrag_handel",
  "branche": "Handel",
  "keywords": ["Verkauf", "Einzelhandel", "Großhandel"],
  "beschreibung": "Für Angestellte im Handel"
}
```

#### 4. Prompt-Optimierung
Füge Beispiele hinzu, die zeigen, wann ein KV passt:
```
Beispiel 1: Vertrag mit "Verkäufer bei REWE" → kollektivvertrag_handel
Beispiel 2: Vertrag mit "Koch im Restaurant" → kollektivvertrag_hotel_und_gastgewerbe
```
