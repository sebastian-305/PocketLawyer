# KV-Erkennung: Validierung und Fehlerbehandlung

## Problem: AI erfindet KV-Namen

### Symptom
```
[Agent xyz] Kollektivvertrag detected: kollektivvertrag_metallindustrie
[Agent xyz] KV detected but no content found for: kollektivvertrag_metallindustrie
```

Der erkannte KV-Name existiert nicht im Dateisystem, weil die AI einen Namen "erfunden" hat, der nicht in der verfügbaren Liste war.

## Ursache

Die AI interpretiert den Vertragsinhalt und erstellt einen "logischen" KV-Namen (z.B. "kollektivvertrag_metallindustrie" für einen Metallarbeiter-Vertrag), anstatt einen Namen aus der bereitgestellten Liste zu wählen.

## Lösung

### 1. Verbesserter Prompt

Der Prompt wurde stark erweitert, um die AI zu zwingen, nur Namen aus der Liste zu verwenden:

```
=== VERFÜGBARE KOLLEKTIVVERTRÄGE (NUR DIESE NAMEN VERWENDEN!) ===
kollektivvertrag_handel, kollektivvertrag_hotel_und_gastgewerbe, ...
=== ENDE DER LISTE ===

KRITISCHE REGELN:
1. VERWENDE NUR NAMEN AUS DER OBIGEN LISTE! Erfinde KEINE eigenen Namen!
2. Kopiere den Namen EXAKT so, wie er in der Liste steht
3. Wenn KEIN Name aus der Liste passt, schreibe "Keiner"
```

### 2. Validierung im Code

Der `KvAiDetectionService` validiert jetzt, ob der erkannte KV-Name in der verfügbaren Liste ist:

```csharp
// Validate that detected KVs are in the available list
for (int i = 0; i < kvNames.Length; i++)
{
    var kvName = kvNames[i];
    if (!string.Equals(kvName, "Multiple", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(kvName, "Keiner", StringComparison.OrdinalIgnoreCase) &&
        !kvsFolders.Contains(kvName, StringComparer.OrdinalIgnoreCase))
    {
        _logger.LogError("AI returned invalid KV name: {KvName} - This name is NOT in the available list!", kvName);
        
        // Replace invalid name with "Keiner"
        kvNames[i] = "Keiner";
        begruendungen[i] = $"FEHLER: AI hat ungültigen KV-Namen '{kvName}' zurückgegeben...";
    }
}
```

### 3. Detailliertes Logging

Bei ungültigen KV-Namen wird jetzt geloggt:

```
[ERROR] AI returned invalid KV name: kollektivvertrag_metallindustrie - This name is NOT in the available list!
[ERROR] Available KVs: kollektivvertrag_handel, kollektivvertrag_hotel_und_gastgewerbe, ...
[ERROR] AI reasoning: Der Vertrag bezieht sich auf Metallverarbeitung...
[INFO] KV Detection attempt 1: Keiner
  Begründung: FEHLER: AI hat ungültigen KV-Namen 'kollektivvertrag_metallindustrie' zurückgegeben...
```

## Fehlerbehandlung

### Automatische Korrektur

Wenn die AI einen ungültigen Namen zurückgibt:
1. **Fehler wird geloggt** mit dem ungültigen Namen und der Begründung
2. **Name wird ersetzt** mit "Keiner"
3. **Begründung wird erweitert** um den Fehlerhinweis
4. **Verarbeitung läuft weiter** - kein Absturz

### Ergebnis

Das System behandelt ungültige KV-Namen als "Kein passender KV gefunden" und loggt den Fehler für Debugging-Zwecke.

## Beispiel-Logs

### Vor der Validierung (Problem)
```
[INFO] KV Detection attempt 1: kollektivvertrag_metallindustrie
  Begründung: Der Vertrag bezieht sich auf Metallverarbeitung und Maschinenbau.
[INFO] Kollektivvertrag detected: kollektivvertrag_metallindustrie
[WARN] KV detected but no content found for: kollektivvertrag_metallindustrie
```

### Nach der Validierung (Gelöst)
```
[ERROR] AI returned invalid KV name: kollektivvertrag_metallindustrie - This name is NOT in the available list!
[ERROR] Available KVs: kollektivvertrag_handel, kollektivvertrag_hotel_und_gastgewerbe, ...
[ERROR] AI reasoning: Der Vertrag bezieht sich auf Metallverarbeitung und Maschinenbau.
[INFO] KV Detection attempt 1: Keiner
  Begründung: FEHLER: AI hat ungültigen KV-Namen 'kollektivvertrag_metallindustrie' zurückgegeben, der nicht in der Liste ist. Original-Begründung: Der Vertrag bezieht sich auf Metallverarbeitung und Maschinenbau.
[INFO] No matching KV detected
[INFO] KV Detection Result: Kein passender Kollektivvertrag gefunden
```

## Häufige Fehler

### 1. AI erfindet Namen
**Problem**: `kollektivvertrag_metallindustrie` (nicht in Liste)  
**Lösung**: Validierung ersetzt mit "Keiner"

### 2. AI verwendet falsche Schreibweise
**Problem**: `Kollektivvertrag_Handel` (Großbuchstaben)  
**Lösung**: Case-insensitive Vergleich in Validierung

### 3. AI verwendet Abkürzungen
**Problem**: `kv_handel` (Abkürzung)  
**Lösung**: Validierung erkennt, dass Name nicht in Liste ist

## Verbesserungsvorschläge

### 1. Fuzzy Matching
Bei ungültigen Namen könnte man versuchen, den ähnlichsten Namen aus der Liste zu finden:

```csharp
// Beispiel: Levenshtein-Distanz
var closestMatch = FindClosestMatch(kvName, kvsFolders);
if (closestMatch != null && similarity > 0.8)
{
    _logger.LogWarning("AI returned '{Invalid}', using closest match '{Valid}'", kvName, closestMatch);
    kvNames[i] = closestMatch;
}
```

### 2. KV-Metadaten
Füge zu jedem KV Metadaten hinzu (Branche, Keywords), um der AI mehr Kontext zu geben:

```json
{
  "name": "kollektivvertrag_handel",
  "branche": "Handel",
  "keywords": ["Verkauf", "Einzelhandel", "Großhandel", "Verkäufer"],
  "beschreibung": "Für Angestellte im Handel"
}
```

### 3. Zwei-Stufen-Erkennung
1. **Stufe 1**: Erkenne Branche aus Vertrag
2. **Stufe 2**: Wähle passenden KV aus gefilterten KVs der Branche

### 4. Feedback-Loop
Wenn die AI häufig ungültige Namen zurückgibt, könnte man:
- Den Prompt weiter verbessern
- Beispiele für häufige Fehler hinzufügen
- Ein anderes AI-Modell testen

## Testing

### Test 1: Gültiger KV-Name
```
Input: Vertrag mit "Verkäufer bei REWE"
Expected: "kollektivvertrag_handel"
Result: ✓ Pass
```

### Test 2: Ungültiger KV-Name
```
Input: Vertrag mit "Metallarbeiter"
AI Output: "kollektivvertrag_metallindustrie"
Expected: "Keiner" (nach Validierung)
Result: ✓ Pass (mit Error-Log)
```

### Test 3: Kein passender KV
```
Input: Vertrag mit "Software-Entwickler"
Expected: "Keiner"
Result: ✓ Pass
```

## Monitoring

Überwache folgende Metriken:
1. **Anzahl ungültiger KV-Namen** pro Tag
2. **Häufigste ungültige Namen** (welche Namen erfindet die AI?)
3. **Erfolgsrate** der KV-Erkennung
4. **Häufigkeit von "Multiple"** vs. eindeutige Erkennung

Diese Daten helfen, den Prompt und die Validierung weiter zu verbessern.
