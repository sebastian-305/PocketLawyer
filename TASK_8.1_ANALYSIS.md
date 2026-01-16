# Task 8.1 Analysis: EmploymentClauseAnalysisSchemaProvider Structure

## Executive Summary

The `EmploymentClauseAnalysisSchemaProvider` uses the **shared `PremiumSectionDto`** structure, which is defined in `Domain/Dtos/RentalPremiumAnalysisDto.cs` and used by both Employment and Rental premium analyses.

## Key Findings

### 1. DTO Structure Used

**DTO:** `PremiumSectionDto` (shared between Employment and Rental)

**Location:** `Domain/Dtos/RentalPremiumAnalysisDto.cs` (lines 54-62)

**Current Definition:**
```csharp
public record PremiumSectionDto(
    [property: JsonPropertyName("zusammenfassung")] string Zusammenfassung,
    [property: JsonPropertyName("bewertung")] string Bewertung,
    [property: JsonPropertyName("rechtsgrundlage")] string Rechtsgrundlage,
    [property: JsonPropertyName("risikoScore")] string RisikoScore,
    [property: JsonPropertyName("riskLevel")] string RiskLevel
);
```

**Status:** ❌ **No SchemaDisplay or SchemaSection attributes currently present**

### 2. Schema Provider Structure

**File:** `Infrastructure/SchemaProviders/Employment/Premium/EmploymentClauseAnalysisSchemaProvider.cs`

**Special Characteristics:**
- **Dynamic Section Titles**: Uses a `GetSchema(string stepName)` method that accepts a step name parameter
- **Section Title Mapping**: Has a dictionary mapping step names to German section titles:
  - `CompensationBenefits` → "Vergütungs- und Zusatzleistungsklauseln"
  - `WorkingTimeVacation` → "Arbeitszeit- und Urlaubsklauseln"
  - `TerminationNotice` → "Kündigungs- und Kündigungsfristklauseln"
  - `NonCompeteConfidentiality` → "Wettbewerbs- und Vertraulichkeitsklauseln"
  - `RightsDuties` → "Rechte und Pflichten"
- **Fallback Behavior**: Uses step name as section title if no mapping exists
- **Backward Compatibility**: Also has a parameterless `GetSchema()` method

### 3. Schema Properties Analysis

The manual schema defines 5 properties with the following metadata:

#### Property 1: `zusammenfassung`
- **Type:** string
- **Display Name:** "Zusammenfassung"
- **Description:** "Zusammenfassung der relevanten Klauseln in diesem Bereich."
- **Styling:** 
  - `displayClass`: "text-base"
  - `importance`: "high"
- **Required:** Yes

#### Property 2: `bewertung`
- **Type:** string
- **Display Name:** "Bewertung"
- **Description:** "Rechtliche Bewertung der Klauseln mit Hinweisen auf Risiken oder Vorteile unter Berücksichtigung des Kollektivvertrags."
- **Styling:**
  - `displayClass`: "text-base font-semibold"
  - `importance`: "high"
- **Required:** Yes

#### Property 3: `rechtsgrundlage`
- **Type:** string
- **Display Name:** "Rechtsgrundlage"
- **Description (Employment):** "Relevante Rechtsgrundlagen und Paragraphen (z.B. AngG, AVRAG, AZG, Kollektivvertrag)."
- **Description (Rental):** "Relevante Rechtsgrundlagen und Paragraphen (z.B. MRG, ABGB)."
- **Styling:**
  - `displayClass`: "text-sm text-secondary"
  - `importance`: "medium"
  - `icon`: "law"
- **Required:** Yes
- **Note:** Description differs between Employment and Rental (different legal frameworks)

#### Property 4: `risikoScore`
- **Type:** string
- **Enum (Employment):** ["Vorteilhaft", "Neutral", "Nachteilhaft"]
- **Enum (Rental):** ["Zulaessig", "Nachteilhaft", "Unzulaessig"]
- **Display Name:** "Risiko-Score"
- **Description (Employment):** "Risikobewertung: Vorteilhaft (günstig für Arbeitnehmer), Neutral (ausgeglichen), Nachteilhaft (nachteilig für Arbeitnehmer)."
- **Description (Rental):** "Risikobewertung: Zulaessig (rechtlich unbedenklich), Nachteilhaft (nachteilig für Mieter), Unzulaessig (rechtlich problematisch)."
- **Styling:**
  - `displayClass`: "badge"
  - `importance`: "high"
  - `icon`: "shield"
- **Required:** Yes
- **Note:** Enum values and descriptions differ between Employment and Rental

#### Property 5: `riskLevel`
- **Type:** string
- **Enum:** ["Gruen", "Gelb", "Rot"]
- **Display Name:** "Risikostufe"
- **Description:** "Risikostufe: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch)."
- **Styling:**
  - `displayClass`: "risk-indicator"
  - `importance`: "high"
  - `icon`: "alert"
- **Required:** Yes

### 4. Special Handling Requirements

#### Challenge: Dynamic Section Titles

The current schema provider uses **dynamic section titles** based on the step name. This presents a unique challenge for migration:

**Current Approach:**
```csharp
public JsonObject GetSchema(string stepName)
{
    var schema = (JsonObject)_baseSchema.DeepClone();
    if (SectionTitles.TryGetValue(stepName, out var title))
    {
        schema["x-sectionTitle"] = title;
    }
    return schema;
}
```

**Migration Options:**

**Option A: Keep Dynamic Behavior in SchemaProvider** ✅ **RECOMMENDED**
- Add SchemaDisplay attributes to PremiumSectionDto (without SchemaSection)
- Keep the dynamic section title logic in the SchemaProvider
- Generate base schema from DTO, then add x-sectionTitle dynamically

**Pros:**
- Maintains existing API (GetSchema(stepName) method)
- Preserves dynamic behavior
- Simpler migration

**Cons:**
- Section title not in DTO (but this is intentional - it varies by context)

**Option B: Create Multiple DTOs**
- Create separate DTOs for each step (e.g., CompensationBenefitsDto, WorkingTimeVacationDto)
- Each DTO has its own SchemaSection attribute
- More complex, not recommended

#### Challenge: Shared DTO Between Employment and Rental

`PremiumSectionDto` is used by both:
- **Employment Premium:** 5 different clause analysis steps
- **Rental Premium:** 6 different clause analysis steps (Costs, Terms, Use, Maintenance, Duties, Privacy)

**Implication:** The attributes we add to `PremiumSectionDto` will be used by **both** Employment and Rental clause analysis schemas. This is actually beneficial - it ensures consistency across both contract types.

### 5. Enum Handling - CRITICAL DISCOVERY

The schema uses string enums for `risikoScore` and `riskLevel`, but there's a **critical difference** between Employment and Rental:

**Employment `risikoScore` enum:**
- Values: ["Vorteilhaft", "Neutral", "Nachteilhaft"]
- Description: "Vorteilhaft (günstig für Arbeitnehmer), Neutral (ausgeglichen), Nachteilhaft (nachteilig für Arbeitnehmer)"

**Rental `risikoScore` enum:**
- Values: ["Zulaessig", "Nachteilhaft", "Unzulaessig"]
- Description: "Zulaessig (rechtlich unbedenklich), Nachteilhaft (nachteilig für Mieter), Unzulaessig (rechtlich problematisch)"

**Both use the same `riskLevel` enum:**
- Values: ["Gruen", "Gelb", "Rot"]
- Description: "Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch)"

**C# Enums defined in code:**
```csharp
public enum ClauseRiskScore
{
    Zulaessig,
    Nachteilhaft,
    Unzulaessig
}

public enum OverallRiskLevel
{
    Gruen,
    Gelb,
    Rot
}
```

**PROBLEM:** The shared `PremiumSectionDto` is used by both Employment and Rental, but they need **different enum values** for `risikoScore`!

**SOLUTION:** Keep `risikoScore` and `riskLevel` as `string` types in the DTO (not enum types). The enum constraints must be added **dynamically in each SchemaProvider** based on the contract type (Employment vs Rental). This is the only way to support different enum values for the same DTO property.

## Migration Plan for Task 8.2

### Step 1: Add SchemaDisplay Attributes to PremiumSectionDto

Add attributes to all 5 properties in `Domain/Dtos/RentalPremiumAnalysisDto.cs`:

**Important:** Since this DTO is shared between Employment and Rental, we need to use **generic descriptions** that work for both contexts. The contract-specific details (like legal frameworks and enum values) will be added dynamically in each SchemaProvider.

```csharp
public record PremiumSectionDto(
    [property: JsonPropertyName("zusammenfassung")]
    [property: SchemaDisplay("Zusammenfassung",
        Description = "Zusammenfassung der relevanten Klauseln in diesem Bereich.",
        DisplayClass = "text-base",
        Importance = "high")]
    string Zusammenfassung,
    
    [property: JsonPropertyName("bewertung")]
    [property: SchemaDisplay("Bewertung",
        Description = "Rechtliche Bewertung der Klauseln mit Hinweisen auf Risiken oder Vorteile.",
        DisplayClass = "text-base font-semibold",
        Importance = "high")]
    string Bewertung,
    
    [property: JsonPropertyName("rechtsgrundlage")]
    [property: SchemaDisplay("Rechtsgrundlage",
        Description = "Relevante Rechtsgrundlagen und Paragraphen.",
        Icon = "law",
        DisplayClass = "text-sm text-secondary",
        Importance = "medium")]
    string Rechtsgrundlage,
    
    [property: JsonPropertyName("risikoScore")]
    [property: SchemaDisplay("Risiko-Score",
        Description = "Risikobewertung der Klauseln.",
        Icon = "shield",
        DisplayClass = "badge",
        Importance = "high")]
    string RisikoScore,
    
    [property: JsonPropertyName("riskLevel")]
    [property: SchemaDisplay("Risikostufe",
        Description = "Risikostufe: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
        Icon = "alert",
        DisplayClass = "risk-indicator",
        Importance = "high")]
    string RiskLevel
);
```

**Note:** 
- Descriptions are kept generic to work for both Employment and Rental
- Contract-specific descriptions will be added dynamically in each SchemaProvider
- The `using PocketLawyer.Infrastructure.SchemaProviders;` statement must be added

### Step 2: Handle Enum Constraints - CRITICAL

The SchemaGenerator doesn't automatically add enum constraints for string properties. Additionally, **Employment and Rental use different enum values** for `risikoScore`:

- **Employment:** ["Vorteilhaft", "Neutral", "Nachteilhaft"]
- **Rental:** ["Zulaessig", "Nachteilhaft", "Unzulaessig"]

**Required Approach:**
- Generate base schema from DTO using SchemaGenerator
- **Each SchemaProvider must add its own enum constraints** based on contract type
- Employment providers add Employment-specific enums
- Rental providers add Rental-specific enums
- This is the only way to support different enum values for the same shared DTO

### Step 3: Update EmploymentClauseAnalysisSchemaProvider

Modify the provider to use SchemaGenerator while preserving dynamic section title behavior and adding Employment-specific descriptions and enums:

```csharp
public sealed class EmploymentClauseAnalysisSchemaProvider
{
    private readonly SchemaGenerator _generator = new();
    
    private static readonly Dictionary<string, string> SectionTitles = new()
    {
        ["CompensationBenefits"] = "Vergütungs- und Zusatzleistungsklauseln",
        ["WorkingTimeVacation"] = "Arbeitszeit- und Urlaubsklauseln",
        ["TerminationNotice"] = "Kündigungs- und Kündigungsfristklauseln",
        ["NonCompeteConfidentiality"] = "Wettbewerbs- und Vertraulichkeitsklauseln",
        ["RightsDuties"] = "Rechte und Pflichten"
    };

    public JsonObject GetSchema(string stepName)
    {
        // Generate base schema from DTO
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        
        // Add dynamic section title
        if (SectionTitles.TryGetValue(stepName, out var title))
        {
            schema["x-sectionTitle"] = title;
        }
        else
        {
            schema["x-sectionTitle"] = stepName;
        }
        
        // Customize descriptions and add enum constraints for Employment context
        if (schema["properties"] is JsonObject properties)
        {
            // Update bewertung description to include Kollektivvertrag reference
            if (properties["bewertung"] is JsonObject bewertung)
            {
                bewertung["description"] = "Rechtliche Bewertung der Klauseln mit Hinweisen auf Risiken oder Vorteile unter Berücksichtigung des Kollektivvertrags.";
            }
            
            // Update rechtsgrundlage description with Employment-specific legal frameworks
            if (properties["rechtsgrundlage"] is JsonObject rechtsgrundlage)
            {
                rechtsgrundlage["description"] = "Relevante Rechtsgrundlagen und Paragraphen (z.B. AngG, AVRAG, AZG, Kollektivvertrag).";
            }
            
            // Add Employment-specific enum and description for risikoScore
            if (properties["risikoScore"] is JsonObject risikoScore)
            {
                risikoScore["enum"] = new JsonArray { "Vorteilhaft", "Neutral", "Nachteilhaft" };
                risikoScore["description"] = "Risikobewertung: Vorteilhaft (günstig für Arbeitnehmer), Neutral (ausgeglichen), Nachteilhaft (nachteilig für Arbeitnehmer).";
            }
            
            // Add enum for riskLevel (same for both Employment and Rental)
            if (properties["riskLevel"] is JsonObject riskLevel)
            {
                riskLevel["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" };
            }
        }
        
        return schema;
    }

    public JsonObject GetSchema() => _generator.GenerateSchema<PremiumSectionDto>();
}
```

**Note:** The Rental `ClauseAnalysisSchemaProvider` will need similar changes but with Rental-specific descriptions and enum values.

### Step 3b: Update Rental ClauseAnalysisSchemaProvider (for completeness)

The Rental version will be similar but with Rental-specific customizations:

```csharp
public sealed class ClauseAnalysisSchemaProvider
{
    private readonly SchemaGenerator _generator = new();
    
    private static readonly Dictionary<string, string> SectionTitles = new()
    {
        ["Costs"] = "Mietzahlungs- und Nebenkostenklauseln",
        ["Terms"] = "Mietdauer und Kündigungsklauseln",
        ["Use"] = "Nutzungsklauseln",
        ["Maintenance"] = "Instandhaltungs- und Reparaturklauseln",
        ["Duties"] = "Pflichten und Obliegenheiten",
        ["Privacy"] = "Datenschutz und Privatsphäre"
    };

    public JsonObject GetSchema(string stepName)
    {
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        
        if (SectionTitles.TryGetValue(stepName, out var title))
        {
            schema["x-sectionTitle"] = title;
        }
        else
        {
            schema["x-sectionTitle"] = stepName;
        }
        
        // Customize for Rental context
        if (schema["properties"] is JsonObject properties)
        {
            // Update rechtsgrundlage description with Rental-specific legal frameworks
            if (properties["rechtsgrundlage"] is JsonObject rechtsgrundlage)
            {
                rechtsgrundlage["description"] = "Relevante Rechtsgrundlagen und Paragraphen (z.B. MRG, ABGB).";
            }
            
            // Add Rental-specific enum and description for risikoScore
            if (properties["risikoScore"] is JsonObject risikoScore)
            {
                risikoScore["enum"] = new JsonArray { "Zulaessig", "Nachteilhaft", "Unzulaessig" };
                risikoScore["description"] = "Risikobewertung: Zulaessig (rechtlich unbedenklich), Nachteilhaft (nachteilig für Mieter), Unzulaessig (rechtlich problematisch).";
            }
            
            // Add enum for riskLevel (same for both)
            if (properties["riskLevel"] is JsonObject riskLevel)
            {
                riskLevel["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" };
            }
        }
        
        return schema;
    }

    public JsonObject GetSchema() => _generator.GenerateSchema<PremiumSectionDto>();
}
```

### Step 4: Write Comparison Tests

Create tests to verify:
1. Base schema (without section title) matches manual schema structure
2. Schema with each step name has correct section title
3. Enum constraints are present for risikoScore and riskLevel with correct values
4. All metadata (x-displayName, x-styling, descriptions) matches
5. Employment-specific descriptions are correct
6. Rental-specific descriptions are correct (for Rental provider)

**Test file:** `PocketLawyer.Tests/Infrastructure/SchemaProviders/Employment/Premium/EmploymentClauseAnalysisSchemaProviderTests.cs`

Key test cases:
- Test each of the 5 step names produces correct section title
- Test risikoScore has Employment-specific enum values
- Test rechtsgrundlage has Employment-specific description
- Test bewertung includes Kollektivvertrag reference
- Compare generated schema to original manual schema for each step

## Impact Analysis

### Files to Modify

1. **Domain/Dtos/RentalPremiumAnalysisDto.cs**
   - Add SchemaDisplay attributes to PremiumSectionDto
   - Add using statement for SchemaProviders namespace

2. **Infrastructure/SchemaProviders/Employment/Premium/EmploymentClauseAnalysisSchemaProvider.cs**
   - Replace manual schema with SchemaGenerator
   - Preserve dynamic section title logic
   - Add enum constraints manually

3. **Infrastructure/SchemaProviders/Rental/Premium/ClauseAnalysisSchemaProvider.cs**
   - Replace manual schema with SchemaGenerator
   - Preserve dynamic section title logic
   - Add Rental-specific descriptions and enum constraints

### Tests to Create

1. **PocketLawyer.Tests/Infrastructure/SchemaProviders/Employment/Premium/EmploymentClauseAnalysisSchemaProviderTests.cs**
   - Test base schema generation
   - Test each of 5 step names produces correct section title
   - Test Employment-specific enum constraints and descriptions
   - Test schema comparison for each step

2. **PocketLawyer.Tests/Infrastructure/SchemaProviders/Rental/Premium/ClauseAnalysisSchemaProviderTests.cs**
   - Test base schema generation
   - Test each of 6 step names produces correct section title
   - Test Rental-specific enum constraints and descriptions
   - Test schema comparison for each step

## Conclusion

The `EmploymentClauseAnalysisSchemaProvider` uses the shared `PremiumSectionDto` structure with special handling for dynamic section titles. The migration requires:

1. ✅ Adding SchemaDisplay attributes to PremiumSectionDto with **generic descriptions** (benefits both Employment and Rental)
2. ✅ Preserving dynamic section title logic in SchemaProvider
3. ✅ **Dynamically customizing descriptions** for Employment vs Rental contexts in each SchemaProvider
4. ✅ **Dynamically adding different enum constraints** for Employment vs Rental in each SchemaProvider
5. ✅ Writing comprehensive comparison tests for both Employment and Rental providers

**Key Insight:** The shared DTO approach works well, but requires each SchemaProvider to add contract-specific customizations (descriptions and enum values) after generating the base schema. This is necessary because Employment and Rental have different legal frameworks and risk assessment scales.

The migration is more complex than previous tasks due to the shared DTO and context-specific requirements, but the approach is sound and maintains the benefits of the Single Source of Truth architecture.
