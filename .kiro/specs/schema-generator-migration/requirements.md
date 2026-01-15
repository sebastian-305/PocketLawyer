# Requirements Document

## Introduction

This specification defines the migration of the schema generation system from a dual-definition approach (manual JSON schemas + DTOs) to a Single Source of Truth approach where DTOs with custom attributes automatically generate JSON schemas. The SchemaGenerator infrastructure is already implemented and tested (12/12 tests passing). This migration eliminates code duplication, improves type safety, and reduces maintenance burden while preserving all functionality including Nebius strict mode compatibility and frontend metadata.

## Glossary

- **DTO**: Data Transfer Object - C# record types used for deserializing AI responses
- **JSON Schema**: Structured definition of expected JSON format sent to AI services
- **SchemaProvider**: Class that provides JSON schema definitions for AI requests
- **SchemaGenerator**: Reflection-based utility that generates JSON schemas from DTOs with custom attributes
- **Nebius Strict Mode**: AI service requirement that schemas use only standard JSON Schema keywords
- **Frontend Metadata**: Custom x-* fields in schemas used by the UI (x-displayName, x-styling, x-sectionTitle)
- **AI Metadata**: Standard JSON Schema fields used by AI services (description, format, enum, maxLength)
- **Single Source of Truth (SST)**: Design principle where each piece of knowledge has exactly one authoritative representation

## Requirements

### Requirement 1: DTO Attribute Enhancement

**User Story:** As a developer, I want DTOs to contain all schema metadata through attributes, so that schema definitions are co-located with type definitions.

#### Acceptance Criteria

1. WHEN a DTO property needs a display name, THE System SHALL use the SchemaDisplay attribute with DisplayName parameter
2. WHEN a DTO property needs AI guidance, THE System SHALL use the SchemaDisplay attribute with Description parameter
3. WHEN a DTO property needs frontend styling, THE System SHALL use the SchemaDisplay attribute with Icon, DisplayClass, or Importance parameters
4. WHEN a DTO property needs format validation, THE System SHALL use the SchemaDisplay attribute with Format parameter
5. WHEN a DTO property needs length constraints, THE System SHALL use the SchemaDisplay attribute with MaxLength or MinLength parameters
6. WHEN a DTO property needs example values, THE System SHALL use the SchemaDisplay attribute with Examples parameter
7. WHEN a DTO class needs a section title, THE System SHALL use the SchemaSection attribute with SectionTitle parameter
8. WHEN a DTO property has an enum type, THE System SHALL preserve the enum constraint in the generated schema
9. FOR ALL Employment Basic DTOs (EmploymentBasicGeneralDto, EmploymentBasicCompensationDto, EmploymentBasicWorkingConditionsDto, EmploymentBasicSummaryDto), THE System SHALL add appropriate SchemaDisplay and SchemaSection attributes
10. FOR ALL Employment Premium DTOs (EmploymentPremiumSummaryDto and nested types), THE System SHALL add appropriate SchemaDisplay and SchemaSection attributes
11. FOR ALL Rental Basic DTOs (RentalBasicGeneralDto, RentalBasicRentDto, RentalBasicConditionsDto, RentalBasicSummaryDto), THE System SHALL add appropriate SchemaDisplay and SchemaSection attributes
12. FOR ALL Rental Premium DTOs (RentalPremiumSummaryDto and nested types), THE System SHALL add appropriate SchemaDisplay and SchemaSection attributes

### Requirement 2: SchemaProvider Migration

**User Story:** As a developer, I want SchemaProvider classes to use SchemaGenerator instead of manual schemas, so that schemas are automatically synchronized with DTOs.

#### Acceptance Criteria

1. WHEN a SchemaProvider needs to provide a schema, THE System SHALL call SchemaGenerator.GenerateSchema with the corresponding DTO type
2. WHEN SchemaGenerator generates a schema, THE System SHALL produce output identical to the manual schema definition
3. FOR ALL Employment Basic SchemaProviders (EmploymentGeneralSchemaProvider, EmploymentCompensationSchemaProvider, EmploymentWorkingConditionsSchemaProvider, EmploymentBasicSummarySchemaProvider), THE System SHALL replace manual schema definitions with SchemaGenerator calls
4. FOR ALL Employment Premium SchemaProviders (EmploymentPremiumSummarySchemaProvider, EmploymentClauseAnalysisSchemaProvider), THE System SHALL replace manual schema definitions with SchemaGenerator calls
5. FOR ALL Rental Basic SchemaProviders (RentalGeneralSchemaProvider, RentalRentConditionsSchemaProvider, RentalContractConditionsSchemaProvider, RentalBasicSummarySchemaProvider), THE System SHALL replace manual schema definitions with SchemaGenerator calls
6. FOR ALL Rental Premium SchemaProviders (RentalPremiumSummarySchemaProvider, ClauseAnalysisSchemaProvider), THE System SHALL replace manual schema definitions with SchemaGenerator calls
7. WHEN a SchemaProvider is migrated, THE System SHALL maintain the same public interface (GetSchema method)
8. WHEN a SchemaProvider is migrated, THE System SHALL maintain backward compatibility with existing callers

### Requirement 3: Schema Validation and Verification

**User Story:** As a developer, I want to verify that generated schemas match manual schemas exactly, so that the migration introduces no breaking changes.

#### Acceptance Criteria

1. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all property names match
2. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all property types match
3. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all required fields match
4. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all x-displayName values match
5. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all x-styling values match
6. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all x-sectionTitle values match
7. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all description values match
8. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all format values match
9. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that all enum values match
10. WHEN comparing a generated schema to a manual schema, THE System SHALL verify that additionalProperties is set to false
11. FOR ALL migrated schemas, THE System SHALL pass schema comparison validation before removing manual definitions

### Requirement 4: Nebius Strict Mode Compliance

**User Story:** As a system integrator, I want generated schemas to comply with Nebius strict mode requirements, so that AI services accept the schemas without errors.

#### Acceptance Criteria

1. WHEN generating a schema, THE System SHALL set additionalProperties to false
2. WHEN generating a schema, THE System SHALL use only allowed JSON Schema keywords (type, properties, required, items, enum, description, format, maxLength, minLength, minimum, maximum, examples)
3. WHEN generating a schema for a string property, THE System SHALL use type "string"
4. WHEN generating a schema for an integer property, THE System SHALL use type "integer"
5. WHEN generating a schema for a number property, THE System SHALL use type "number"
6. WHEN generating a schema for a boolean property, THE System SHALL use type "boolean"
7. WHEN generating a schema for an array property, THE System SHALL use type "array" with items definition
8. WHEN generating a schema for an object property, THE System SHALL use type "object" with properties definition
9. WHEN generating a schema for a nullable property, THE System SHALL use type array with both the base type and "null"
10. WHEN generating a schema for an enum property, THE System SHALL include an enum array with all possible values
11. WHEN generating a schema, THE System SHALL NOT use anyOf, oneOf, allOf, or $ref keywords

### Requirement 5: Frontend Metadata Preservation

**User Story:** As a frontend developer, I want all UI metadata to be preserved in generated schemas, so that the user interface continues to function correctly.

#### Acceptance Criteria

1. WHEN a DTO property has a SchemaDisplay attribute with DisplayName, THE System SHALL include x-displayName in the generated schema
2. WHEN a DTO property has a SchemaDisplay attribute with Icon, THE System SHALL include x-styling.icon in the generated schema
3. WHEN a DTO property has a SchemaDisplay attribute with DisplayClass, THE System SHALL include x-styling.displayClass in the generated schema
4. WHEN a DTO property has a SchemaDisplay attribute with Importance, THE System SHALL include x-styling.importance in the generated schema
5. WHEN a DTO class has a SchemaSection attribute, THE System SHALL include x-sectionTitle in the generated schema
6. WHEN a generated schema contains x-* fields, THE AI service SHALL ignore them (as per JSON Schema specification)
7. WHEN the frontend receives a generated schema, THE System SHALL render UI elements identically to manual schemas

### Requirement 6: Phased Migration Strategy

**User Story:** As a project manager, I want the migration to proceed in phases, so that risks are minimized and progress is incremental.

#### Acceptance Criteria

1. WHEN starting the migration, THE System SHALL migrate Employment Basic schemas first
2. WHEN Employment Basic migration is complete, THE System SHALL migrate Employment Premium schemas second
3. WHEN Employment Premium migration is complete, THE System SHALL migrate Rental Basic schemas third
4. WHEN Rental Basic migration is complete, THE System SHALL migrate Rental Premium schemas fourth
5. WHEN a phase is complete, THE System SHALL verify all tests pass before proceeding to the next phase
6. WHEN a phase is complete, THE System SHALL verify schema comparison tests pass before proceeding to the next phase
7. WHEN all phases are complete, THE System SHALL remove all manual schema definitions

### Requirement 7: Code Cleanup and Documentation

**User Story:** As a maintainer, I want obsolete code removed and documentation updated, so that the codebase reflects the new architecture.

#### Acceptance Criteria

1. WHEN all SchemaProviders are migrated, THE System SHALL remove manual JsonObject schema definitions from SchemaProvider classes
2. WHEN all SchemaProviders are migrated, THE System SHALL update SchemaProvider class documentation to reference SchemaGenerator
3. WHEN the migration is complete, THE System SHALL update the SCHEMA_GENERATOR_REFACTORING.md document to reflect completion status
4. WHEN the migration is complete, THE System SHALL verify no orphaned schema definition code remains
5. WHEN the migration is complete, THE System SHALL verify all existing tests continue to pass
6. WHEN the migration is complete, THE System SHALL verify all integration points continue to function

### Requirement 8: Type Safety and Maintainability

**User Story:** As a developer, I want compile-time verification of schema definitions, so that schema errors are caught early.

#### Acceptance Criteria

1. WHEN a DTO property is renamed, THE System SHALL automatically update the generated schema
2. WHEN a DTO property type is changed, THE System SHALL automatically update the generated schema type
3. WHEN a DTO property is added, THE System SHALL automatically include it in the generated schema
4. WHEN a DTO property is removed, THE System SHALL automatically exclude it from the generated schema
5. WHEN a SchemaDisplay attribute has invalid parameters, THE Compiler SHALL produce a compilation error
6. WHEN a JsonPropertyName attribute is missing, THE SchemaGenerator SHALL skip that property
7. WHEN a property is marked with JsonIgnore, THE SchemaGenerator SHALL exclude it from the schema
8. FOR ALL schema changes, THE System SHALL require only DTO modifications (no separate schema file changes)
