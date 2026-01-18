# Onion-Architektur Refactoring Plan

**Datum:** 18. Januar 2026  
**Ziel:** Behebung der Dependency-Rule-Verletzungen in der Domain-Schicht

---

## Problem-Analyse

Die Domain-Schicht hat derzeit **unzulässige Abhängigkeiten** zu Application und Infrastructure:

### Gefundene Verletzungen

1. **Domain → Application**
   - `Domain/PromptProvider.cs` verwendet `PocketLawyer.Application.Exceptions`
   - Verwendet: `PromptNotFoundException`

2. **Domain → Infrastructure**
   - `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs` verwendet `PocketLawyer.Infrastructure.SchemaProviders`
   - `Domain/Dtos/Employment/EmploymentPremiumAnalysisDto.cs` verwendet `PocketLawyer.Infrastructure.SchemaProviders`
   - `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs` verwendet `PocketLawyer.Infrastructure.SchemaProviders`
   - `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs` verwendet `PocketLawyer.Infrastructure.SchemaProviders`
   - Verwendet: `SchemaDisplayAttribute`, `SchemaSectionAttribute`

---

## Lösungsansätze

### Lösung 1: PromptProvider verschieben (EMPFOHLEN)

**Problem:** `PromptProvider` ist in der Domain-Schicht, verwendet aber Application-Exceptions.

**Analyse:**
- `PromptProvider` ist eine **statische Utility-Klasse** für Prompt-Verwaltung
- Wirft `PromptNotFoundException` wenn ein Prompt nicht gefunden wird
- Hat keine Domain-Logik, sondern ist eine **technische Infrastruktur-Komponente**

**Lösung:** Verschiebe `PromptProvider` von Domain nach Application oder Infrastructure

#### Option A: Nach Application verschieben
```
Domain/PromptProvider.cs → Application/PromptProvider.cs
```

**Vorteile:**
- PromptProvider ist näher an den Agents (Application-Schicht)
- Kann Application.Exceptions direkt verwenden
- Logisch sinnvoll: Prompts sind Teil der Anwendungslogik

**Nachteile:**
- Keine

#### Option B: Nach Infrastructure verschieben
```
Domain/PromptProvider.cs → Infrastructure/PromptProvider.cs
```

**Vorteile:**
- PromptProvider ist eine technische Komponente
- Könnte später durch externe Prompt-Verwaltung ersetzt werden

**Nachteile:**
- Weniger logisch als Application

**EMPFEHLUNG: Option A - Nach Application verschieben**

---

### Lösung 2: Schema-Attribute in Domain verschieben (EMPFOHLEN)

**Problem:** DTOs in Domain verwenden Attribute aus Infrastructure.

**Analyse der aktuellen Situation:**
- **Infrastructure → Domain:** SchemaProvider verwendet DTOs (✅ erlaubt)
  - `EmploymentGeneralSchemaProvider` verwendet `EmploymentBasicGeneralDto`
  - `SchemaGenerator.GenerateSchema<T>()` liest DTO-Typen via Reflection
- **Domain → Infrastructure:** DTOs verwenden Attribute aus Infrastructure (❌ NICHT erlaubt)
  - `EmploymentBasicGeneralDto` verwendet `SchemaDisplayAttribute`, `SchemaSectionAttribute`
  - Diese Attribute sind in `Infrastructure/SchemaProviders/SchemaAttributes.cs` definiert

**Die Abhängigkeitsrichtung:**
```
Infrastructure (SchemaProvider) → Domain (DTOs)  ✅ Korrekt
Domain (DTOs) → Infrastructure (Attributes)      ❌ Verletzung!
```

**Warum ist das ein Problem?**
- DTOs sind **Domain-Modelle** und sollten keine Infrastruktur-Abhängigkeiten haben
- Die Attribute sind aber nur **Metadaten-Deklarationen** ohne Infrastruktur-Logik
- Sie gehören konzeptionell zur Domain, nicht zur Infrastructure

**Lösung:** Verschiebe die Attribute von Infrastructure nach Domain

```
Infrastructure/SchemaProviders/SchemaAttributes.cs → Domain/Attributes/SchemaAttributes.cs
```

**Begründung:**
- Attribute sind **Deklarationen**, keine Implementierungen
- Sie beschreiben die DTOs, gehören also zur Domain
- SchemaGenerator (Infrastructure) kann Attribute aus Domain lesen (erlaubte Abhängigkeit)
- Vergleichbar mit `System.ComponentModel.DataAnnotations` - auch in Domain verwendbar
- Nach dem Verschieben: Infrastructure → Domain (Attribute lesen) ✅ erlaubt

**Vorteile:**
- Domain bleibt unabhängig von Infrastructure
- Attribute bleiben bei den DTOs, die sie beschreiben
- SchemaGenerator kann weiterhin die Attribute via Reflection lesen
- Saubere Onion-Architektur

**Nachteile:**
- Keine

---

## Implementierungsplan

### Phase 1: PromptProvider verschieben

#### Schritt 1: Neue Datei erstellen
```csharp
// Application/PromptProvider.cs
using PocketLawyer.Application.Exceptions;

namespace PocketLawyer.Application;

public static class PromptProvider
{
    // ... gesamter Inhalt von Domain/PromptProvider.cs
}
```

#### Schritt 2: Alle Referenzen aktualisieren
Suche und ersetze in allen Dateien:
- `using PocketLawyer.Domain;` → `using PocketLawyer.Application;` (nur wo PromptProvider verwendet wird)
- Oder: Explizite using-Direktive hinzufügen

**Betroffene Dateien:**
- `Application/Agents/EmploymentContractAgent.cs`
- `Application/Agents/RentalContractAgent.cs`
- `Infrastructure/KvAiDetectionService.cs`
- Alle anderen Dateien, die `PromptProvider` verwenden

#### Schritt 3: Alte Datei löschen
```
Domain/PromptProvider.cs
```

#### Schritt 4: Tests ausführen
```bash
dotnet build
dotnet test
```

---

### Phase 2: Schema-Attribute verschieben

#### Schritt 1: Neues Verzeichnis und Datei erstellen
```csharp
// Domain/Attributes/SchemaAttributes.cs
namespace PocketLawyer.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SchemaDisplayAttribute : Attribute
{
    // ... gesamter Inhalt aus Infrastructure/SchemaProviders/SchemaAttributes.cs
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SchemaSectionAttribute : Attribute
{
    // ... gesamter Inhalt aus Infrastructure/SchemaProviders/SchemaAttributes.cs
}
```

#### Schritt 2: Using-Direktiven in DTOs aktualisieren
In allen DTO-Dateien:
```csharp
// ALT:
using PocketLawyer.Infrastructure.SchemaProviders;

// NEU:
using PocketLawyer.Domain.Attributes;
```

**Betroffene Dateien:**
- `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`
- `Domain/Dtos/Employment/EmploymentPremiumAnalysisDto.cs`
- `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`
- `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

#### Schritt 3: Using-Direktiven in Infrastructure aktualisieren
In `Infrastructure/SchemaProviders/SchemaGenerator.cs`:
```csharp
using PocketLawyer.Domain.Attributes;
```

#### Schritt 4: Alte Datei löschen
```
Infrastructure/SchemaProviders/SchemaAttributes.cs
```

#### Schritt 5: Tests ausführen
```bash
dotnet build
dotnet test
```

---

## Validierung

Nach dem Refactoring sollten folgende Abhängigkeiten bestehen:

### Domain-Schicht
```
Domain/
├── Attributes/
│   └── SchemaAttributes.cs (keine externen Abhängigkeiten)
├── Dtos/
│   ├── Employment/ (verwendet Domain.Attributes)
│   ├── Rental/ (verwendet Domain.Attributes)
│   └── Shared/
└── (keine Application- oder Infrastructure-Abhängigkeiten mehr!)
```

### Application-Schicht
```
Application/
├── PromptProvider.cs (verwendet Application.Exceptions)
├── Agents/ (verwendet Domain.Dtos, Application.Interfaces)
└── ...
```

### Infrastructure-Schicht
```
Infrastructure/
├── SchemaProviders/
│   └── SchemaGenerator.cs (verwendet Domain.Attributes - erlaubt!)
└── ...
```

---

## Vorteile des Refactorings

1. **Saubere Architektur**
   - Domain-Schicht ist unabhängig von Application und Infrastructure
   - Abhängigkeiten fließen nur nach innen

2. **Bessere Testbarkeit**
   - Domain-Logik kann isoliert getestet werden
   - Keine Mocks für Infrastructure-Komponenten nötig

3. **Wartbarkeit**
   - Klare Trennung der Verantwortlichkeiten
   - Einfacher zu verstehen und zu erweitern

4. **Flexibilität**
   - Domain-Modelle können in anderen Projekten wiederverwendet werden
   - Infrastructure kann ausgetauscht werden ohne Domain zu ändern

---

## Risiken und Mitigationen

### Risiko 1: Breaking Changes
**Mitigation:** Alle using-Direktiven systematisch aktualisieren, Tests ausführen

### Risiko 2: Vergessene Referenzen
**Mitigation:** Compiler-Fehler zeigen alle fehlenden Referenzen an

### Risiko 3: Test-Failures
**Mitigation:** Schrittweise vorgehen, nach jedem Schritt testen

---

## Zeitaufwand

- **Phase 1 (PromptProvider):** ~30 Minuten
- **Phase 2 (Schema-Attribute):** ~30 Minuten
- **Testing & Validierung:** ~30 Minuten
- **Gesamt:** ~1,5 Stunden

---

## Nächste Schritte

1. ✅ Review dieses Plans
2. ⏳ Phase 1 implementieren (PromptProvider verschieben)
3. ⏳ Phase 2 implementieren (Schema-Attribute verschieben)
4. ⏳ Dokumentation aktualisieren (ARCHITECTURE.md)
5. ⏳ Commit mit aussagekräftiger Message

---

**Erstellt von:** Kiro AI  
**Status:** Bereit zur Implementierung  
**Priorität:** Mittel (Architektur-Verbesserung, keine funktionalen Änderungen)
