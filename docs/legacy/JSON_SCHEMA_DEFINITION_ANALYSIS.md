# Analyse: JSON-basierte Schema-Definitionen

## 🤔 Fragestellung

Können/Sollten die DTO-Schema-Definitionen in JSON-Dateien ausgelagert werden?

**Beispiel:** Statt Attributes in C# → JSON-Dateien pro Agent mit Schema-Metadaten

## 📋 Aktuelle Struktur (Attribute-basiert)

```csharp
[SchemaSection("Allgemeine Arbeitsdaten")]
public record EmploymentBasicGeneralDto(
    [property: JsonPropertyName("arbeitgeber")]
    [property: SchemaDisplay("Arbeitgeber",
        Description = "Name des Arbeitgebers.",
        Icon = "building",
        DisplayClass = "text-primary")]
    string Arbeitgeber,
    // ... weitere Properties
);
```

**Dateien:** 4 DTO-Dateien (Employment Basic/Premium, Rental Basic/Premium)

## 🔄 Alternative: JSON-basierte Definitionen

### Variante A: Separate Schema-JSON-Dateien

```
data/Schemas/
├── employment-basic.json
├── employment-premium.json
├── rental-basic.json
└── rental-premium.json
```

**employment-basic.json:**
```json
{
  "sections": {
    "General": {
      "sectionTitle": "Allgemeine Arbeitsdaten",
      "properties": {
        "arbeitgeber": {
          "type": "string",
          "displayName": "Arbeitgeber",
          "description": "Name des Arbeitgebers.",
          "icon": "building",
          "displayClass": "text-primary"
        },
        "arbeitnehmer": {
          "type": "string",
          "displayName": "Arbeitnehmer",
          "description": "Name des/der Arbeitnehmer:in.",
          "icon": "user",
          "displayClass": "text-primary"
        }
      }
    },
    "Compensation": { /* ... */ },
    "WorkingConditions": { /* ... */ },
    "Summary": { /* ... */ }
  }
}
```

### Variante B: Hybrid (DTOs + JSON-Metadaten)

```csharp
// Minimale DTOs ohne Attributes
public record EmploymentBasicGeneralDto(
    [property: JsonPropertyName("arbeitgeber")] string Arbeitgeber,
    [property: JsonPropertyName("arbeitnehmer")] string Arbeitnehmer,
    // ...
);
```

**employment-basic-metadata.json:**
```json
{
  "EmploymentBasicGeneralDto": {
    "sectionTitle": "Allgemeine Arbeitsdaten",
    "properties": {
      "arbeitgeber": {
        "displayName": "Arbeitgeber",
        "description": "Name des Arbeitgebers.",
        "icon": "building",
        "displayClass": "text-primary"
      }
    }
  }
}
```

## ⚖️ Vergleich

### Vorteile JSON-basiert

| Aspekt | Vorteil | Bewertung |
|--------|---------|-----------|
| **Nicht-Entwickler können editieren** | Designer/PMs können Texte/Icons ändern | ⭐⭐⭐⭐ |
| **Keine Recompilation** | Änderungen ohne Build-Prozess | ⭐⭐⭐ |
| **Zentralisierung** | Alle Metadaten an einem Ort | ⭐⭐⭐ |
| **Versionierung** | Einfacher Schema-Versionen zu verwalten | ⭐⭐ |
| **Mehrsprachigkeit** | Einfacher i18n (employment-basic.de.json, .en.json) | ⭐⭐⭐⭐⭐ |
| **Tooling** | JSON-Schema-Validierung, Editoren | ⭐⭐⭐ |

### Nachteile JSON-basiert

| Aspekt | Nachteil | Bewertung |
|--------|----------|-----------|
| **Keine Type-Safety** | Tippfehler erst zur Laufzeit erkannt | ⭐⭐⭐⭐⭐ |
| **Keine Compile-Zeit-Validierung** | Fehler erst beim Laden | ⭐⭐⭐⭐⭐ |
| **Refactoring schwieriger** | Property-Umbenennung nicht automatisch | ⭐⭐⭐⭐ |
| **Duplikation** | DTO-Properties + JSON-Keys müssen synchron sein | ⭐⭐⭐⭐ |
| **Komplexität** | Zusätzliche Loader/Parser-Logik nötig | ⭐⭐⭐ |
| **Debugging schwieriger** | Fehler in JSON schwerer zu tracken | ⭐⭐⭐ |
| **IntelliSense verloren** | Keine IDE-Unterstützung beim Editieren | ⭐⭐⭐⭐ |
| **Trennung von Code** | Metadaten weit weg von DTO-Definition | ⭐⭐⭐ |

## 🎯 Empfehlung

### ❌ **NICHT empfohlen** für dieses Projekt

**Hauptgründe:**

1. **Type-Safety ist kritisch**
   - Wir haben gerade 87% Code reduziert UND Type-Safety gewonnen
   - JSON würde Type-Safety wieder aufgeben
   - Fehler würden erst zur Laufzeit auftreten

2. **Single Source of Truth würde verletzt**
   - DTO-Properties müssen mit JSON-Keys synchron bleiben
   - Zurück zum ursprünglichen Problem (nur anders)

3. **Refactoring-Sicherheit geht verloren**
   - Property umbenennen → JSON muss manuell angepasst werden
   - Genau das Problem, das wir gerade gelöst haben

4. **Komplexität steigt**
   - JSON-Loader/Parser nötig
   - Fehlerbehandlung für fehlende/falsche JSON-Dateien
   - Mehr Code statt weniger

5. **Entwickler-Workflow verschlechtert sich**
   - Keine IntelliSense
   - Keine Compile-Zeit-Fehler
   - Debugging schwieriger

## ✅ **ABER:** Sinnvoll für bestimmte Use Cases

### Wann JSON-Definitionen Sinn machen:

1. **Mehrsprachigkeit (i18n)**
   ```
   data/Schemas/i18n/
   ├── employment-basic.de.json  # Deutsche Texte
   ├── employment-basic.en.json  # Englische Texte
   └── employment-basic.fr.json  # Französische Texte
   ```
   
   **Lösung:** Nur DisplayNames/Descriptions in JSON, Rest in Attributes

2. **Dynamische Schemas**
   - Wenn Schemas zur Laufzeit geändert werden müssen
   - Wenn verschiedene Mandanten unterschiedliche Schemas brauchen
   - **Nicht der Fall hier:** Schemas sind statisch pro Agent

3. **Non-Developer Editing**
   - Wenn Marketing/Design häufig Texte/Icons ändern
   - **Nicht der Fall hier:** Änderungen selten, durch Entwickler

## 🔧 Hybrid-Ansatz (Kompromiss)

Falls wirklich gewünscht, könnte man einen **Hybrid-Ansatz** wählen:

### Struktur

```csharp
// DTOs bleiben mit Attributes (Type-Safety!)
[SchemaSection("Allgemeine Arbeitsdaten")]
public record EmploymentBasicGeneralDto(
    [property: JsonPropertyName("arbeitgeber")]
    [property: SchemaDisplay("Arbeitgeber",
        Icon = "building",
        DisplayClass = "text-primary")]
    string Arbeitgeber
);
```

**PLUS optionale JSON-Overrides für i18n:**

```json
// data/Schemas/i18n/employment-basic.de.json
{
  "General": {
    "sectionTitle": "Allgemeine Arbeitsdaten",
    "properties": {
      "arbeitgeber": {
        "displayName": "Arbeitgeber",
        "description": "Name des Arbeitgebers."
      }
    }
  }
}
```

**SchemaGenerator erweitern:**
```csharp
public class SchemaGenerator
{
    private readonly ISchemaLocalization? _localization;
    
    public JsonObject GenerateSchema<T>(string? locale = null)
    {
        var schema = GenerateFromAttributes<T>();
        
        // Optional: Texte aus JSON überschreiben
        if (_localization != null && locale != null)
        {
            schema = _localization.ApplyTranslations(schema, typeof(T), locale);
        }
        
        return schema;
    }
}
```

**Vorteile:**
- ✅ Type-Safety bleibt erhalten (Attributes als Fallback)
- ✅ i18n möglich (JSON-Overrides)
- ✅ Refactoring-sicher (Attributes sind führend)
- ✅ Optional (JSON nur wenn nötig)

## 📊 Entscheidungsmatrix

| Kriterium | Attributes (aktuell) | JSON (vollständig) | Hybrid |
|-----------|---------------------|-------------------|--------|
| Type-Safety | ✅✅✅ | ❌ | ✅✅✅ |
| Refactoring | ✅✅✅ | ❌ | ✅✅✅ |
| i18n | ❌ | ✅✅✅ | ✅✅✅ |
| Non-Dev Editing | ❌ | ✅✅✅ | ✅✅ |
| Komplexität | ✅✅✅ | ❌ | ✅✅ |
| Wartbarkeit | ✅✅✅ | ❌❌ | ✅✅ |
| Performance | ✅✅✅ | ✅✅ | ✅✅ |

## 🎬 Fazit

### Für dieses Projekt: **Attributes beibehalten** ✅

**Gründe:**
1. Type-Safety ist wichtiger als Non-Dev-Editing
2. Schemas ändern sich selten
3. Keine Mehrsprachigkeit geplant
4. Entwickler-Workflow ist optimal
5. Gerade erst 87% Code reduziert - nicht wieder komplexer machen

### Falls später i18n benötigt wird:

→ **Hybrid-Ansatz** implementieren:
- Attributes bleiben (Type-Safety)
- JSON nur für Übersetzungen (optional)
- Beste aus beiden Welten

## 💡 Alternative: Wenn wirklich JSON gewünscht

Falls trotzdem JSON gewünscht ist, würde ich empfehlen:

1. **JSON-Schema für Validierung**
   ```json
   // schema-definition.schema.json
   {
     "$schema": "http://json-schema.org/draft-07/schema#",
     "type": "object",
     "properties": {
       "sections": { /* ... */ }
     }
   }
   ```

2. **Code-Generator**
   ```bash
   # Generiert C# DTOs aus JSON
   dotnet run --project SchemaGenerator -- generate employment-basic.json
   ```
   
   → Beste aus beiden Welten: JSON als Source, C# als Output

3. **Validierung im Build**
   ```xml
   <Target Name="ValidateSchemas" BeforeTargets="Build">
     <Exec Command="dotnet run --project SchemaValidator" />
   </Target>
   ```

## 📝 Zusammenfassung

| Ansatz | Empfehlung | Use Case |
|--------|------------|----------|
| **Attributes (aktuell)** | ✅ **Empfohlen** | Standard-Fall, Type-Safety wichtig |
| **JSON (vollständig)** | ❌ Nicht empfohlen | Nur wenn Schemas dynamisch sein müssen |
| **Hybrid** | ⚠️ Bei Bedarf | Wenn i18n benötigt wird |
| **JSON + Codegen** | ⚠️ Bei Bedarf | Wenn Non-Devs Schemas definieren sollen |

---

**Meine Empfehlung:** Attributes beibehalten. Die aktuelle Lösung ist optimal für dieses Projekt. Falls später i18n benötigt wird, kann man immer noch einen Hybrid-Ansatz hinzufügen.

---

## 🔮 Zukünftige Ideen (Backlog)

### Idee 1: Schema Builder UI (Blazor)

**Problem:** Anfänger sollen Schemas ändern können, ohne C#-Code anfassen zu müssen.

**Lösung:** Web-basiertes Admin-Interface für Schema-Definition

**Features:**
- ✅ Visueller Schema-Editor mit Drag & Drop
- ✅ Property-Editor mit Validierung (Pflichtfelder, Dropdowns für Icons/CSS-Klassen)
- ✅ Live-Preview (JSON-Schema + C#-Code + Frontend-Rendering)
- ✅ Fehler-Prävention (keine vergessenen Felder möglich)
- ✅ Code-Generator Integration (generiert C# DTOs mit Attributes)
- ✅ Export zu JSON + Git-Integration

**Technologie:**
- Blazor Server/WASM
- Nutzt bestehende SchemaGenerator-Infrastruktur
- Läuft unter `/schema-builder` Route

**Vorteile:**
- Anfänger-freundlich (keine C#-Kenntnisse nötig)
- Keine vergessenen Felder (UI erzwingt Vollständigkeit)
- Konsistenz (Dropdowns verhindern Tippfehler)
- Type-Safety bleibt erhalten (generierter Code)
- Versionierung (generierter Code geht in Git)

**Workflow:**
1. Anfänger öffnet Schema Builder UI
2. Wählt Schema aus (z.B. Employment Basic General)
3. Bearbeitet Properties visuell (Name, Type, Icon, etc.)
4. Live-Preview zeigt JSON + C# Code
5. Klickt "Save & Generate"
6. UI generiert C# DTO Code mit Attributes
7. Code wird committed

**Struktur:**
```
/Tools/SchemaBuilder/
├── Pages/
│   ├── SchemaList.razor          # Übersicht aller Schemas
│   ├── SchemaEditor.razor        # Editor für ein Schema
│   └── PropertyEditor.razor      # Editor für eine Property
├── Services/
│   ├── SchemaService.cs          # CRUD für Schemas
│   ├── CodeGenerator.cs          # Generiert C# DTOs
│   └── ValidationService.cs      # Validiert Schemas
└── Models/
    ├── SchemaDefinition.cs       # Model für Schema
    └── PropertyDefinition.cs     # Model für Property
```

**Status:** 💡 Idee (nicht priorisiert)  
**Aufwand:** ~3-5 Tage  
**Nutzen:** Hoch für Teams mit Anfängern

---

### Idee 2: JSON-basierte Schema-Definitionen mit Code-Generator

**Problem:** Schemas sollen in einfacher JSON-Struktur definiert werden können.

**Lösung:** JSON-Dateien als Source of Truth + automatischer Code-Generator

**Struktur:**
```
data/Schemas/
├── employment-basic-general.json
├── employment-basic-compensation.json
└── ...
```

**JSON-Format:**
```json
{
  "sectionTitle": "Allgemeine Arbeitsdaten",
  "properties": [
    {
      "name": "arbeitgeber",
      "type": "string",
      "displayName": "Arbeitgeber",
      "description": "Name des Arbeitgebers.",
      "icon": "building",
      "displayClass": "text-primary",
      "required": true
    }
  ]
}
```

**Code-Generator:**
```bash
# Generiert C# DTOs aus JSON
dotnet run --project Tools/SchemaCodeGen

# Output: Domain/Dtos/Generated/EmploymentBasicGeneralDto.cs
```

**Vorteile:**
- Einfache JSON-Struktur (anfänger-freundlich)
- JSON-Schema-Validierung möglich
- Type-Safety durch generierten Code
- Kann in Build-Prozess integriert werden

**Nachteile:**
- Zusätzlicher Build-Step nötig
- JSON muss mit DTOs synchron gehalten werden
- Komplexer als aktuelle Lösung

**Status:** 💡 Idee (nicht priorisiert)  
**Aufwand:** ~2-3 Tage  
**Nutzen:** Mittel (nur wenn UI nicht gewünscht)

---

### Idee 3: Hybrid-Ansatz für Mehrsprachigkeit (i18n)

**Problem:** Schemas sollen in mehreren Sprachen verfügbar sein.

**Lösung:** Attributes als Basis + optionale JSON-Overrides für Übersetzungen

**Struktur:**
```
data/Schemas/i18n/
├── employment-basic.de.json  # Deutsche Texte
├── employment-basic.en.json  # Englische Texte
└── employment-basic.fr.json  # Französische Texte
```

**JSON-Format (nur Übersetzungen):**
```json
{
  "General": {
    "sectionTitle": "General Employment Data",
    "properties": {
      "arbeitgeber": {
        "displayName": "Employer",
        "description": "Name of the employer."
      }
    }
  }
}
```

**SchemaGenerator erweitern:**
```csharp
public class SchemaGenerator
{
    private readonly ISchemaLocalization? _localization;
    
    public JsonObject GenerateSchema<T>(string? locale = null)
    {
        var schema = GenerateFromAttributes<T>();
        
        // Optional: Texte aus JSON überschreiben
        if (_localization != null && locale != null)
        {
            schema = _localization.ApplyTranslations(schema, typeof(T), locale);
        }
        
        return schema;
    }
}
```

**Vorteile:**
- Type-Safety bleibt erhalten (Attributes als Fallback)
- i18n möglich (JSON-Overrides)
- Refactoring-sicher (Attributes sind führend)
- Optional (JSON nur wenn nötig)

**Status:** 💡 Idee (nur bei Bedarf)  
**Aufwand:** ~2-3 Tage  
**Nutzen:** Hoch wenn Mehrsprachigkeit benötigt wird

---

### Vergleich der Ideen

| Idee | Anfänger-freundlich | Type-Safety | Aufwand | Nutzen | Empfehlung |
|------|---------------------|-------------|---------|--------|------------|
| **Schema Builder UI** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | 3-5 Tage | Hoch | ✅ **Beste Lösung** |
| **JSON + Code-Gen** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | 2-3 Tage | Mittel | ⚠️ Nur ohne UI |
| **Hybrid i18n** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | 2-3 Tage | Hoch* | ⚠️ Nur bei Bedarf |

\* *Nur wenn Mehrsprachigkeit benötigt wird*

---

**Fazit:** Die **Schema Builder UI** ist die beste Lösung für den Use Case "Anfänger sollen Schemas ändern können". Sie kombiniert Anfänger-Freundlichkeit mit Type-Safety und verhindert vergessene Felder durch UI-Validierung.
