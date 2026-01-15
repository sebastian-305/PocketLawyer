# Design Document: Schema Generator Migration

## Overview

This design describes the migration from a dual-definition schema system (manual JSON schemas + DTOs) to a Single Source of Truth approach where DTOs with custom attributes automatically generate JSON schemas using the existing SchemaGenerator infrastructure.

The migration eliminates approximately 1,500+ lines of duplicate schema definition code across 16 SchemaProvider classes while maintaining 100% backward compatibility with AI services (Nebius/Gemini) and frontend UI components.

**Key Benefits:**
- Single source of truth: DTOs become the authoritative definition
- Type safety: Compiler enforces schema structure
- Automatic synchronization: Schema changes happen automatically when DTOs change
- Reduced maintenance: No need to update schemas in two places
- Zero breaking changes: Generated schemas are identical to manual schemas

**Migration Scope:**
- 4 Employment Basic schemas (General, Compensation, WorkingConditions, Summary)
- 2 Employment Premium schemas (Summary, ClauseAnalysis)
- 4 Rental Basic schemas (General, Rent, Conditions, Summary)
- 2 Rental Premium schemas (Summary, ClauseAnalysis)

## Architecture

### Current Architecture (Before Migration)

```
┌─────────────────────────────────────────────────────────────┐
│                    Manual Schema Definition                  │
│  EmploymentGeneralSchemaProvider.cs                         │
│  - JsonObject with hardcoded properties                     │
│  - Manual x-displayName, x-styling, descriptions            │
│  - Manual required array                                     │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ GetSchema()
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                    AI Service Request                        │
│  Uses schema to guide AI response generation                │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ AI Response (JSON)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                    DTO Deserialization                       │
│  EmploymentBasicGeneralDto                                  │
│  - JsonPropertyName attributes                              │
│  - C# type definitions                                       │
└─────────────────────────────────────────────────────────────┘

Problem: Schema and DTO definitions are separate and must be kept in sync manually
```

### Target Architecture (After Migration)

```
┌─────────────────────────────────────────────────────────────┐
│                    DTO with Attributes (SST)                 │
│  [SchemaSection("Allgemeine Arbeitsdaten")]                 │
│  public record EmploymentBasicGeneralDto(                   │
│    [JsonPropertyName("arbeitgeber")]                        │
│    [SchemaDisplay("Arbeitgeber", Icon="building")]          │
│    string Arbeitgeber,                                       │
│    ...                                                        │
│  )                                                            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ SchemaGenerator.GenerateSchema<T>()
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                    Generated JSON Schema                     │
│  {                                                            │
│    "type": "object",                                         │
│    "x-sectionTitle": "Allgemeine Arbeitsdaten",             │
│    "properties": { ... },                                    │
│    "required": [ ... ]                                       │
│  }                                                            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ GetSchema() (SchemaProvider wrapper)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                    AI Service Request                        │
│  Uses generated schema (identical to manual)                │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ AI Response (JSON)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                    DTO Deserialization                       │
│  Same DTO used for both schema generation and               │
│  deserialization (Single Source of Truth)                   │
└─────────────────────────────────────────────────────────────┘

Solution: DTO is the single source of truth for both schema and deserialization
```

## Components and Interfaces

### 1. SchemaGenerator (Already Implemented)

**Location:** `Infrastructure/SchemaProviders/SchemaGenerator.cs`

**Responsibility:** Generate JSON schemas from DTOs using reflection

**Key Methods:**
- `JsonObject GenerateSchema<T>()` - Generate schema from generic type
- `JsonObject GenerateSchema(Type type)` - Generate schema from Type instance
- `JsonObject GeneratePropertySchema(PropertyInfo prop)` - Generate schema for individual property
- `JsonObject GenerateTypeSchema(Type type)` - Generate schema for nested types

**Features:**
- Reads `JsonPropertyName` attributes for property names
- Reads `SchemaDisplay` attributes for metadata (x-displayName, x-styling, description, format, etc.)
- Reads `SchemaSection` attributes for section titles
- Handles nullable types correctly (["string", "null"])
- Handles arrays and nested objects
- Handles enums with enum value arrays
- Enforces Nebius strict mode (additionalProperties: false)
- Respects `JsonIgnore` attributes

### 2. Custom Attributes (Already Implemented)

**Location:** `Infrastructure/SchemaProviders/SchemaAttributes.cs`

#### SchemaDisplayAttribute

Applied to DTO properties to define display and validation metadata.

**Properties:**
- `DisplayName` (required) - Frontend label (x-displayName)
- `Description` - AI guidance text (description)
- `Format` - JSON Schema format (date, email, uri)
- `Icon` - Frontend icon name (x-styling.icon)
- `DisplayClass` - Frontend CSS class (x-styling.displayClass)
- `Importance` - Frontend importance level (x-styling.importance)
- `MaxLength` - String max length constraint
- `MinLength` - String min length constraint
- `Examples` - Example values array

**Usage:**
```csharp
[property: SchemaDisplay("Arbeitgeber",
    Description = "Name des Arbeitgebers.",
    Icon = "building",
    DisplayClass = "text-primary")]
string Arbeitgeber
```

#### SchemaSectionAttribute

Applied to DTO classes to define section titles.

**Properties:**
- `SectionTitle` (required) - Frontend section title (x-sectionTitle)

**Usage:**
```csharp
[SchemaSection("Allgemeine Arbeitsdaten")]
public record EmploymentBasicGeneralDto(...)
```

### 3. SchemaProvider Classes (To Be Modified)

**Current Implementation:**
```csharp
public sealed class EmploymentGeneralSchemaProvider
{
    private readonly JsonObject _schema = new() { /* manual definition */ };
    
    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
```

**Target Implementation:**
```csharp
public sealed class EmploymentGeneralSchemaProvider
{
    private readonly SchemaGenerator _generator = new();
    
    public JsonObject GetSchema() => 
        _generator.GenerateSchema<EmploymentBasicGeneralDto>();
}
```

**Affected Classes:**
- Employment Basic: 4 providers
- Employment Premium: 2 providers
- Rental Basic: 4 providers
- Rental Premium: 2 providers

### 4. DTO Classes (To Be Enhanced)

**Current State:** DTOs have only `JsonPropertyName` attributes

**Target State:** DTOs have both `JsonPropertyName` and `SchemaDisplay`/`SchemaSection` attributes

**Example Transformation:**

**Before:**
```csharp
public record EmploymentBasicGeneralDto(
    [property: JsonPropertyName("arbeitgeber")] string Arbeitgeber,
    [property: JsonPropertyName("position")] string Position
);
```

**After:**
```csharp
[SchemaSection("Allgemeine Arbeitsdaten")]
public record EmploymentBasicGeneralDto(
    [property: JsonPropertyName("arbeitgeber")]
    [property: SchemaDisplay("Arbeitgeber",
        Description = "Name des Arbeitgebers.",
        Icon = "building",
        DisplayClass = "text-primary")]
    string Arbeitgeber,
    
    [property: JsonPropertyName("position")]
    [property: SchemaDisplay("Position",
        Description = "Bezeichnung der Position/Tätigkeit.",
        Icon = "briefcase",
        DisplayClass = "text-secondary")]
    string Position
);
```

## Data Models

### DTO Attribute Mapping

This table shows how manual schema fields map to DTO attributes:

| Manual Schema Field | DTO Attribute | Attribute Property | Notes |
|---------------------|---------------|-------------------|-------|
| `x-sectionTitle` | `[SchemaSection]` | `SectionTitle` | Class-level attribute |
| `x-displayName` | `[SchemaDisplay]` | `DisplayName` | Required parameter |
| `description` | `[SchemaDisplay]` | `Description` | AI guidance |
| `format` | `[SchemaDisplay]` | `Format` | date, email, uri |
| `maxLength` | `[SchemaDisplay]` | `MaxLength` | String constraint |
| `minLength` | `[SchemaDisplay]` | `MinLength` | String constraint |
| `examples` | `[SchemaDisplay]` | `Examples` | Example values |
| `x-styling.icon` | `[SchemaDisplay]` | `Icon` | Frontend icon |
| `x-styling.displayClass` | `[SchemaDisplay]` | `DisplayClass` | Frontend CSS |
| `x-styling.importance` | `[SchemaDisplay]` | `Importance` | high/medium/low |
| `type` | Inferred | - | From C# property type |
| `required` | Inferred | - | From required keyword or non-nullable |
| `enum` | Inferred | - | From C# enum type |
| `additionalProperties` | Auto-added | - | Always false |

### Schema Comparison Model

For validation during migration, we need to compare generated schemas with manual schemas:

```csharp
public record SchemaComparisonResult
{
    public bool IsIdentical { get; init; }
    public List<string> Differences { get; init; } = new();
    public JsonObject ManualSchema { get; init; }
    public JsonObject GeneratedSchema { get; init; }
}

public class SchemaComparator
{
    public SchemaComparisonResult Compare(JsonObject manual, JsonObject generated)
    {
        var differences = new List<string>();
        
        // Compare type
        if (manual["type"]?.ToString() != generated["type"]?.ToString())
            differences.Add($"Type mismatch: {manual["type"]} vs {generated["type"]}");
        
        // Compare additionalProperties
        if (manual["additionalProperties"]?.ToString() != generated["additionalProperties"]?.ToString())
            differences.Add("additionalProperties mismatch");
        
        // Compare x-sectionTitle
        if (manual["x-sectionTitle"]?.ToString() != generated["x-sectionTitle"]?.ToString())
            differences.Add($"x-sectionTitle mismatch");
        
        // Compare properties
        CompareProperties(manual["properties"] as JsonObject, 
                         generated["properties"] as JsonObject, 
                         differences);
        
        // Compare required arrays
        CompareRequiredArrays(manual["required"] as JsonArray,
                             generated["required"] as JsonArray,
                             differences);
        
        return new SchemaComparisonResult
        {
            IsIdentical = differences.Count == 0,
            Differences = differences,
            ManualSchema = manual,
            GeneratedSchema = generated
        };
    }
    
    private void CompareProperties(JsonObject? manual, JsonObject? generated, List<string> differences)
    {
        // Compare each property recursively
        // Check: type, description, x-displayName, x-styling, format, enum, etc.
    }
    
    private void CompareRequiredArrays(JsonArray? manual, JsonArray? generated, List<string> differences)
    {
        // Compare required field arrays (order-independent)
    }
}
```

### Migration Phase Model

```csharp
public enum MigrationPhase
{
    EmploymentBasic,
    EmploymentPremium,
    RentalBasic,
    RentalPremium
}

public record MigrationStep
{
    public string DtoName { get; init; }
    public string SchemaProviderName { get; init; }
    public MigrationPhase Phase { get; init; }
    public bool IsCompleted { get; init; }
    public SchemaComparisonResult? ValidationResult { get; init; }
}
```


## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property Reflection

After analyzing all acceptance criteria, I identified the following testable properties:

**Already Tested in SchemaGenerator (12/12 tests passing):**
- Enum preservation (1.8)
- Type mapping correctness (4.3-4.10)
- Nebius strict mode compliance (4.1, 4.2, 4.11)
- Metadata preservation (5.1-5.5)
- JsonPropertyName handling (8.6)
- JsonIgnore handling (8.7)

**New Properties for Migration:**
- Schema comparison (2.2, 3.1-3.11) - The critical property for this migration
- API compatibility (2.7) - Verify GetSchema() interface preserved

**Redundancy Analysis:**
- Requirements 3.1-3.10 all describe aspects of schema comparison - these combine into one comprehensive property
- Requirements 4.3-4.10 all describe type mapping - already tested as one property in SchemaGenerator tests
- Requirements 5.1-5.5 all describe metadata preservation - already tested as one property in SchemaGenerator tests

### Property 1: Schema Equivalence After Migration

*For any* DTO that has been migrated from manual schema to SchemaGenerator, the generated schema SHALL be structurally and semantically identical to the original manual schema.

**Validates: Requirements 2.2, 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10, 3.11**

**Verification Approach:**
- Compare JSON structure recursively
- Verify all property names match
- Verify all type definitions match (including nullable types)
- Verify all required arrays match (order-independent)
- Verify all x-displayName values match
- Verify all x-styling objects match (icon, displayClass, importance)
- Verify all x-sectionTitle values match
- Verify all description values match
- Verify all format values match
- Verify all enum arrays match
- Verify additionalProperties is false in both

**Implementation:**
```csharp
// For each migrated schema
var manual = oldSchemaProvider.GetSchema();
var generated = schemaGenerator.GenerateSchema<DtoType>();
var comparison = schemaComparator.Compare(manual, generated);
Assert.True(comparison.IsIdentical, 
    $"Schema mismatch: {string.Join(", ", comparison.Differences)}");
```

### Property 2: API Compatibility Preservation

*For any* SchemaProvider that has been migrated, the public interface SHALL remain unchanged with the GetSchema() method returning a JsonObject.

**Validates: Requirements 2.7**

**Verification Approach:**
- Verify GetSchema() method exists
- Verify return type is JsonObject
- Verify method is public
- Verify no breaking changes to method signature

**Implementation:**
```csharp
// Verify interface compatibility
var provider = new EmploymentGeneralSchemaProvider();
JsonObject schema = provider.GetSchema(); // Should compile and execute
Assert.NotNull(schema);
Assert.Equal("object", schema["type"]?.ToString());
```

### Existing SchemaGenerator Properties (Already Tested)

The following properties are already verified by the existing SchemaGenerator test suite (12/12 passing):

**Property 3: Enum Preservation**
*For any* DTO property with an enum type, the generated schema SHALL include an enum array with all possible enum values.
**Validates: Requirements 1.8**

**Property 4: Type Mapping Correctness**
*For any* C# type, the SchemaGenerator SHALL map it to the correct JSON Schema type according to Nebius strict mode requirements.
**Validates: Requirements 4.3, 4.4, 4.5, 4.6, 4.7, 4.8, 4.9, 4.10**

**Property 5: Nebius Strict Mode Compliance**
*For any* generated schema, it SHALL contain only allowed JSON Schema keywords and SHALL set additionalProperties to false.
**Validates: Requirements 4.1, 4.2, 4.11**

**Property 6: Metadata Preservation**
*For any* DTO with SchemaDisplay or SchemaSection attributes, the generated schema SHALL include all corresponding x-* fields and standard fields.
**Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5**

**Property 7: JsonPropertyName Handling**
*For any* DTO property without a JsonPropertyName attribute, the SchemaGenerator SHALL exclude it from the generated schema.
**Validates: Requirements 8.6**

**Property 8: JsonIgnore Handling**
*For any* DTO property marked with JsonIgnore, the SchemaGenerator SHALL exclude it from the generated schema.
**Validates: Requirements 8.7**

## Error Handling

### Schema Generation Errors

**Scenario:** DTO has invalid structure (no JsonPropertyName attributes)
**Handling:** SchemaGenerator silently skips properties without JsonPropertyName
**Rationale:** Allows DTOs to have internal properties not exposed in schemas

**Scenario:** DTO has circular references
**Handling:** Not currently handled - would cause stack overflow
**Mitigation:** DTOs in this codebase are simple records without circular references
**Future:** Could add cycle detection if needed

**Scenario:** Attribute has null or empty DisplayName
**Handling:** C# compiler enforces non-null through required parameter
**Rationale:** Compile-time safety prevents runtime errors

### Schema Comparison Errors

**Scenario:** Generated schema differs from manual schema
**Handling:** SchemaComparator returns detailed list of differences
**Action:** Developer must investigate and fix DTO attributes or SchemaGenerator logic
**Rationale:** Migration should not proceed if schemas don't match

**Scenario:** Manual schema has fields not supported by SchemaGenerator
**Handling:** Comparison will flag the difference
**Action:** Either extend SchemaGenerator or adjust manual schema
**Rationale:** Ensures complete feature parity

### Migration Errors

**Scenario:** Tests fail after migrating a SchemaProvider
**Handling:** Revert the migration, investigate root cause
**Action:** Fix DTO attributes or SchemaGenerator, then retry
**Rationale:** Phased migration allows safe rollback

**Scenario:** AI service rejects generated schema
**Handling:** Should not occur - schemas are identical to manual schemas
**Action:** If it occurs, investigate schema differences and Nebius requirements
**Rationale:** Generated schemas are proven equivalent to working manual schemas

## Testing Strategy

### Dual Testing Approach

This migration requires both unit tests and property-based tests:

**Unit Tests:**
- Verify specific DTO attribute configurations produce expected schemas
- Test edge cases (nullable types, arrays, nested objects, enums)
- Test schema comparison logic with known inputs
- Test each migrated SchemaProvider individually
- Verify API compatibility for each SchemaProvider

**Property-Based Tests:**
- Verify schema equivalence across all migrated DTOs (Property 1)
- Verify type mapping correctness across all C# types (Property 4 - already tested)
- Verify metadata preservation across all attribute combinations (Property 6 - already tested)

### Testing Configuration

**Property-Based Test Configuration:**
- Library: Use existing C# property-based testing library (if available) or FsCheck
- Iterations: Minimum 100 iterations per property test
- Tag format: `// Feature: schema-generator-migration, Property {N}: {description}`

**Unit Test Organization:**
```
PocketLawyer.Tests/
  Infrastructure/
    SchemaGeneratorTests.cs (✅ Already exists - 12/12 passing)
    SchemaComparatorTests.cs (New - for schema comparison logic)
    SchemaProviders/
      Employment/
        Basic/
          EmploymentGeneralSchemaProviderTests.cs (New - verify migration)
          EmploymentCompensationSchemaProviderTests.cs (New)
          EmploymentWorkingConditionsSchemaProviderTests.cs (New)
          EmploymentBasicSummarySchemaProviderTests.cs (New)
        Premium/
          EmploymentPremiumSummarySchemaProviderTests.cs (New)
          EmploymentClauseAnalysisSchemaProviderTests.cs (New)
      Rental/
        Basic/
          RentalGeneralSchemaProviderTests.cs (New)
          RentalRentConditionsSchemaProviderTests.cs (New)
          RentalContractConditionsSchemaProviderTests.cs (New)
          RentalBasicSummarySchemaProviderTests.cs (New)
        Premium/
          RentalPremiumSummarySchemaProviderTests.cs (New)
          ClauseAnalysisSchemaProviderTests.cs (New)
```

### Test Coverage Requirements

**For Each Migrated SchemaProvider:**
1. Unit test verifying GetSchema() returns valid JsonObject
2. Unit test comparing generated schema to original manual schema (Property 1)
3. Unit test verifying all required fields are present
4. Unit test verifying all x-* metadata is preserved
5. Integration test verifying AI service accepts the schema (existing tests should cover this)

**For SchemaComparator:**
1. Unit test for identical schemas (should return IsIdentical = true)
2. Unit test for schemas with type differences
3. Unit test for schemas with missing properties
4. Unit test for schemas with different required arrays
5. Unit test for schemas with different x-* metadata
6. Unit test for nested object comparison
7. Unit test for array type comparison

### Migration Validation Process

For each migration phase:

1. **Pre-Migration:**
   - Capture manual schema output from existing SchemaProvider
   - Document all x-* fields and their values
   - Run existing tests to establish baseline

2. **During Migration:**
   - Add attributes to DTO
   - Update SchemaProvider to use SchemaGenerator
   - Generate schema and compare to manual schema
   - Fix any differences in DTO attributes

3. **Post-Migration:**
   - Run schema comparison test (must pass)
   - Run all existing integration tests (must pass)
   - Verify AI service still accepts schemas
   - Document any issues or learnings

4. **Cleanup:**
   - Remove manual schema definition code
   - Update documentation
   - Commit changes

### Regression Testing

**Existing Tests Must Continue to Pass:**
- All AI service integration tests
- All DTO deserialization tests
- All contract analysis workflow tests
- All frontend integration tests

**Rationale:** Generated schemas are identical to manual schemas, so all existing functionality should work unchanged.

## Migration Implementation Plan

### Phase 1: Employment Basic (4 schemas)

**DTOs to Enhance:**
1. `EmploymentBasicGeneralDto` - 7 properties
2. `EmploymentBasicCompensationDto` - 6 properties
3. `EmploymentBasicWorkingConditionsDto` - 6 properties
4. `EmploymentBasicSummaryDto` - 4 properties

**SchemaProviders to Migrate:**
1. `EmploymentGeneralSchemaProvider`
2. `EmploymentCompensationSchemaProvider`
3. `EmploymentWorkingConditionsSchemaProvider`
4. `EmploymentBasicSummarySchemaProvider`

**Estimated Effort:** 2-3 hours
- Add ~80 lines of attributes to DTOs
- Modify 4 SchemaProvider classes (~20 lines each)
- Write 4 comparison tests
- Validate and cleanup

### Phase 2: Employment Premium (2 schemas)

**DTOs to Enhance:**
1. `EmploymentPremiumSummaryDto` - 3 properties
2. `PremiumSectionDto` (if not already done) - varies

**SchemaProviders to Migrate:**
1. `EmploymentPremiumSummarySchemaProvider`
2. `EmploymentClauseAnalysisSchemaProvider`

**Estimated Effort:** 1-2 hours
- Add ~30 lines of attributes
- Modify 2 SchemaProvider classes
- Write 2 comparison tests
- Validate and cleanup

### Phase 3: Rental Basic (4 schemas)

**DTOs to Enhance:**
1. `RentalBasicGeneralDto` - varies
2. `RentalBasicRentDto` - varies
3. `RentalBasicConditionsDto` - varies
4. `RentalBasicSummaryDto` - varies

**SchemaProviders to Migrate:**
1. `RentalGeneralSchemaProvider`
2. `RentalRentConditionsSchemaProvider`
3. `RentalContractConditionsSchemaProvider`
4. `RentalBasicSummarySchemaProvider`

**Estimated Effort:** 2-3 hours
- Similar to Employment Basic
- Add attributes to DTOs
- Modify 4 SchemaProvider classes
- Write 4 comparison tests
- Validate and cleanup

### Phase 4: Rental Premium (2 schemas)

**DTOs to Enhance:**
1. `RentalPremiumSummaryDto` - varies
2. Premium section DTOs (if applicable)

**SchemaProviders to Migrate:**
1. `RentalPremiumSummarySchemaProvider`
2. `ClauseAnalysisSchemaProvider`

**Estimated Effort:** 1-2 hours
- Similar to Employment Premium
- Add attributes to DTOs
- Modify 2 SchemaProvider classes
- Write 2 comparison tests
- Validate and cleanup

### Total Estimated Effort

**Development:** 6-10 hours
**Testing & Validation:** 2-3 hours
**Documentation:** 1 hour
**Total:** 9-14 hours

### Risk Mitigation

**Risk:** Generated schema differs from manual schema
**Mitigation:** Schema comparison tests catch this immediately
**Fallback:** Revert to manual schema, investigate difference

**Risk:** AI service rejects generated schema
**Mitigation:** Schemas are identical to working manual schemas
**Fallback:** Existing integration tests will catch this

**Risk:** Frontend UI breaks due to missing x-* fields
**Mitigation:** Schema comparison verifies all x-* fields preserved
**Fallback:** Existing frontend tests will catch this

**Risk:** Performance degradation from reflection
**Mitigation:** SchemaGenerator uses caching (if needed)
**Measurement:** Benchmark schema generation time
**Acceptable:** <10ms per schema generation

## Benefits Summary

### Code Reduction

**Before Migration:**
- 16 SchemaProvider classes with manual JsonObject definitions
- ~1,500 lines of duplicate schema code
- DTOs with only JsonPropertyName attributes

**After Migration:**
- 16 SchemaProvider classes with 3-line implementations
- ~200 lines of attribute code on DTOs
- Net reduction: ~1,300 lines of code

### Maintenance Improvement

**Before:** Change requires updates in 2 places (DTO + SchemaProvider)
**After:** Change requires update in 1 place (DTO only)
**Benefit:** 50% reduction in maintenance effort for schema changes

### Type Safety

**Before:** No compile-time verification of schema correctness
**After:** Compiler enforces DTO structure, attributes are type-checked
**Benefit:** Schema errors caught at compile-time instead of runtime

### Consistency

**Before:** Manual synchronization between DTO and schema (error-prone)
**After:** Automatic synchronization (schemas generated from DTOs)
**Benefit:** Impossible for DTO and schema to be out of sync

### Testability

**Before:** Must test DTO deserialization and schema separately
**After:** Single source of truth means one set of tests
**Benefit:** Simpler test maintenance, better coverage

## Conclusion

This migration eliminates the Single Source of Truth violation in the schema generation system while maintaining 100% backward compatibility. The SchemaGenerator infrastructure is already implemented and tested. The migration is low-risk, incremental, and provides significant long-term benefits in code maintainability, type safety, and consistency.

The phased approach allows for safe rollback at any point, and comprehensive testing ensures no breaking changes are introduced. The estimated 9-14 hours of effort will eliminate ~1,300 lines of duplicate code and reduce future maintenance burden by 50%.
