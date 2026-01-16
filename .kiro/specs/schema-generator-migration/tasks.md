# Implementation Plan: Schema Generator Migration

## Overview

This plan migrates the schema generation system from manual JSON schema definitions to automatic generation from DTOs using the existing SchemaGenerator infrastructure. The migration proceeds in four phases (Employment Basic → Employment Premium → Rental Basic → Rental Premium), with validation at each step to ensure generated schemas match manual schemas exactly.

## Tasks

- [x] 1. Create schema comparison infrastructure
  - Create SchemaComparator class with Compare method
  - Implement recursive property comparison
  - Implement required array comparison (order-independent)
  - Implement x-* metadata comparison
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10_

- [x] 1.1 Write unit tests for SchemaComparator
  - Test identical schemas return IsIdentical = true
  - Test type mismatches are detected
  - Test missing properties are detected
  - Test required array differences are detected
  - Test x-* metadata differences are detected
  - Test nested object comparison
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10_

- [x] 2. Phase 1: Migrate Employment Basic General schema
  - [x] 2.1 Add SchemaSection attribute to EmploymentBasicGeneralDto class
    - Add [SchemaSection("Allgemeine Arbeitsdaten")]
    - _Requirements: 1.7, 1.9_
  
  - [x] 2.2 Add SchemaDisplay attributes to EmploymentBasicGeneralDto properties
    - Add attributes for arbeitgeber (Arbeitgeber, building icon, text-primary)
    - Add attributes for arbeitnehmer (Arbeitnehmer, user icon, text-primary)
    - Add attributes for position (Position, briefcase icon, text-secondary)
    - Add attributes for startdatum (Startdatum, calendar icon, text-info, format=date)
    - Add attributes for vertragsart (Vertragsart, badge class, importance=medium)
    - Add attributes for probezeit (Probezeit, clock icon, text-secondary)
    - Add attributes for vertragsende (Vertragsende, calendar icon, text-info, format=date)
    - Include all descriptions from manual schema
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.9_
  
  - [x] 2.3 Update EmploymentGeneralSchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition with SchemaGenerator call
    - Keep GetSchema() method signature unchanged
    - _Requirements: 2.1, 2.3, 2.7_
  
  - [x] 2.4 Write comparison test for EmploymentGeneralSchemaProvider
    - **Property 1: Schema Equivalence After Migration**
    - **Validates: Requirements 2.2, 3.1-3.11**
    - Compare generated schema to original manual schema
    - Verify all properties, types, required fields, and metadata match
    - _Requirements: 2.2, 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10, 3.11_
  
  - [x] 2.5 Write API compatibility test for EmploymentGeneralSchemaProvider
    - **Property 2: API Compatibility Preservation**
    - **Validates: Requirements 2.7**
    - Verify GetSchema() method exists and returns JsonObject
    - _Requirements: 2.7, 2.8_

- [x] 3. Phase 1: Migrate Employment Basic Compensation schema
  - [x] 3.1 Add SchemaSection attribute to EmploymentBasicCompensationDto
    - Add [SchemaSection("Vergütung und Zusatzleistungen")]
    - _Requirements: 1.7, 1.9_
  
  - [x] 3.2 Add SchemaDisplay attributes to EmploymentBasicCompensationDto properties
    - Add attributes for gehaltBrutto (Bruttogehalt, currency icon, text-primary font-semibold, importance=high)
    - Add attributes for gehaltNettoCa (Nettogehalt (ca.), currency icon, text-secondary)
    - Add attributes for zahlungsweise (Zahlungsweise, calendar icon, text-secondary)
    - Add attributes for sonderzahlungen (Sonderzahlungen, gift icon, text-info, importance=medium)
    - Add attributes for zuschlaege (Zuschläge, plus icon, text-secondary)
    - Add attributes for zusatzleistungen (Zusatzleistungen, star icon, badge-list class)
    - Include all descriptions from manual schema
    - _Requirements: 1.1, 1.2, 1.3, 1.9_
  
  - [x] 3.3 Update EmploymentCompensationSchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition with SchemaGenerator call
    - _Requirements: 2.1, 2.3, 2.7_
  
  - [x] 3.4 Write comparison test for EmploymentCompensationSchemaProvider
    - Compare generated schema to original manual schema
    - Verify array type handling for zusatzleistungen
    - _Requirements: 2.2, 3.11_

- [x] 4. Phase 1: Migrate Employment Basic Working Conditions schema
  - [x] 4.1 Add SchemaSection attribute to EmploymentBasicWorkingConditionsDto
    - Add [SchemaSection("Arbeitsbedingungen")]
    - _Requirements: 1.7, 1.9_
  
  - [x] 4.2 Add SchemaDisplay attributes to EmploymentBasicWorkingConditionsDto properties
    - Add attributes for all 6 properties with appropriate icons and styling
    - Include all descriptions from manual schema
    - _Requirements: 1.1, 1.2, 1.3, 1.9_
  
  - [x] 4.3 Update EmploymentWorkingConditionsSchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition with SchemaGenerator call
    - _Requirements: 2.1, 2.3, 2.7_
  
  - [x] 4.4 Write comparison test for EmploymentWorkingConditionsSchemaProvider
    - Compare generated schema to original manual schema
    - _Requirements: 2.2, 3.11_

- [x] 5. Phase 1: Migrate Employment Basic Summary schema
  - [x] 5.1 Add SchemaSection attribute to EmploymentBasicSummaryDto
    - Add [SchemaSection("Zusammenfassung")]
    - _Requirements: 1.7, 1.9_
  
  - [x] 5.2 Add SchemaDisplay attributes to EmploymentBasicSummaryDto properties
    - Add attributes for kurzfazit (Kurzfazit, text-lg font-semibold, importance=high)
    - Add attributes for kollektivvertrag (Kollektivvertrag, document icon, badge badge-info, importance=high)
    - Add attributes for gesamtbewertung (Gesamtbewertung, badge badge-lg, importance=high)
    - Add attributes for riskLevel (Risikostufe, alert icon, risk-indicator class, importance=high)
    - Include all descriptions and enum values from manual schema
    - _Requirements: 1.1, 1.2, 1.3, 1.8, 1.9_
  
  - [x] 5.3 Update EmploymentBasicSummarySchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition with SchemaGenerator call
    - _Requirements: 2.1, 2.3, 2.7_
  
  - [x] 5.4 Write comparison test for EmploymentBasicSummarySchemaProvider
    - Compare generated schema to original manual schema
    - Verify enum handling for riskLevel
    - _Requirements: 1.8, 2.2, 3.11_

- [x] 6. Phase 1 Checkpoint - Verify Employment Basic migration
  - Run all comparison tests for Employment Basic schemas
  - Run existing integration tests to verify AI service compatibility
  - Verify all tests pass before proceeding to Phase 2
  - _Requirements: 6.1, 6.5, 6.6_

- [x] 7. Phase 2: Migrate Employment Premium Summary schema
  - [x] 7.1 Add SchemaSection attribute to EmploymentPremiumSummaryDto
    - Add appropriate section title
    - _Requirements: 1.7, 1.10_
  
  - [x] 7.2 Add SchemaDisplay attributes to EmploymentPremiumSummaryDto properties
    - Add attributes for gesamtfazit, gesamtbewertung, handlungsempfehlungen
    - Include all descriptions and styling from manual schema
    - _Requirements: 1.1, 1.2, 1.3, 1.10_
  
  - [x] 7.3 Update EmploymentPremiumSummarySchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition with SchemaGenerator call
    - _Requirements: 2.1, 2.4, 2.7_
  
  - [x] 7.4 Write comparison test for EmploymentPremiumSummarySchemaProvider
    - Compare generated schema to original manual schema
    - _Requirements: 2.2, 3.11_

- [x] 8. Phase 2: Migrate Employment Clause Analysis schema
  - [x] 8.1 Analyze EmploymentClauseAnalysisSchemaProvider structure
    - Determine if it uses PremiumSectionDto or custom structure
    - Document any special handling needed
    - _Requirements: 2.4_
  
  - [x] 8.2 Add attributes to relevant DTOs for clause analysis
    - Add SchemaSection and SchemaDisplay attributes as needed
    - _Requirements: 1.10, 2.4_
  
  - [x] 8.3 Update EmploymentClauseAnalysisSchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition with SchemaGenerator call
    - _Requirements: 2.1, 2.4, 2.7_
  
  - [x] 8.4 Write comparison test for EmploymentClauseAnalysisSchemaProvider
    - Compare generated schema to original manual schema
    - _Requirements: 2.2, 3.11_

- [x] 9. Phase 2 Checkpoint - Verify Employment Premium migration
  - Run all comparison tests for Employment Premium schemas
  - Run existing integration tests
  - Verify all tests pass before proceeding to Phase 3
  - _Requirements: 6.2, 6.5, 6.6_

- [x] 10. Phase 3: Migrate Rental Basic schemas (4 schemas)
  - [x] 10.1 Add attributes to RentalBasicGeneralDto
    - Add SchemaSection attribute
    - Add SchemaDisplay attributes to all properties
    - Include all metadata from manual schema
    - _Requirements: 1.1, 1.2, 1.3, 1.7, 1.11_
  
  - [x] 10.2 Update RentalGeneralSchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition
    - _Requirements: 2.1, 2.5, 2.7_
  
  - [x] 10.3 Write comparison test for RentalGeneralSchemaProvider
    - _Requirements: 2.2, 3.11_
  
  - [x] 10.4 Add attributes to RentalBasicRentDto
    - Add SchemaSection attribute
    - Add SchemaDisplay attributes to all properties
    - Handle decimal types correctly
    - _Requirements: 1.1, 1.2, 1.3, 1.7, 1.11_
  
  - [x] 10.5 Update RentalRentConditionsSchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition
    - _Requirements: 2.1, 2.5, 2.7_
  
  - [x] 10.6 Write comparison test for RentalRentConditionsSchemaProvider
    - Verify decimal type mapping to "number"
    - _Requirements: 2.2, 3.11_
  
  - [x] 10.7 Add attributes to RentalBasicConditionsDto
    - Add SchemaSection attribute
    - Add SchemaDisplay attributes to all properties
    - _Requirements: 1.1, 1.2, 1.3, 1.7, 1.11_
  
  - [x] 10.8 Update RentalContractConditionsSchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition
    - _Requirements: 2.1, 2.5, 2.7_
  
  - [x] 10.9 Write comparison test for RentalContractConditionsSchemaProvider
    - _Requirements: 2.2, 3.11_
  
  - [x] 10.10 Add attributes to RentalBasicSummaryDto
    - Add SchemaSection attribute
    - Add SchemaDisplay attributes to all properties
    - _Requirements: 1.1, 1.2, 1.3, 1.7, 1.11_
  
  - [x] 10.11 Update RentalBasicSummarySchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition
    - _Requirements: 2.1, 2.5, 2.7_
  
  - [x] 10.12 Write comparison test for RentalBasicSummarySchemaProvider
    - _Requirements: 2.2, 3.11_

- [x] 11. Phase 3 Checkpoint - Verify Rental Basic migration
  - Run all comparison tests for Rental Basic schemas
  - Run existing integration tests
  - Verify all tests pass before proceeding to Phase 4
  - _Requirements: 6.3, 6.5, 6.6_

- [x] 12. Phase 4: Migrate Rental Premium schemas (2 schemas)
  - [x] 12.1 Add attributes to RentalPremiumSummaryDto
    - Add SchemaSection attribute
    - Add SchemaDisplay attributes to all properties
    - _Requirements: 1.1, 1.2, 1.3, 1.7, 1.12_
  
  - [x] 12.2 Update RentalPremiumSummarySchemaProvider to use SchemaGenerator
    - Replace manual JsonObject definition
    - _Requirements: 2.1, 2.6, 2.7_
  
  - [x] 12.3 Write comparison test for RentalPremiumSummarySchemaProvider
    - _Requirements: 2.2, 3.11_
  
  - [x] 12.4 Analyze and migrate ClauseAnalysisSchemaProvider (Rental)
    - Add attributes to relevant DTOs
    - Update SchemaProvider to use SchemaGenerator
    - _Requirements: 1.12, 2.1, 2.6, 2.7_
  
  - [x] 12.5 Write comparison test for ClauseAnalysisSchemaProvider (Rental)
    - _Requirements: 2.2, 3.11_

- [x] 13. Phase 4 Checkpoint - Verify Rental Premium migration
  - Run all comparison tests for Rental Premium schemas
  - Run existing integration tests
  - Verify all tests pass before cleanup
  - _Requirements: 6.4, 6.5, 6.6_

- [x] 14. Final cleanup and documentation
  - [x] 14.1 Remove manual schema definitions from all SchemaProvider classes
    - Remove JsonObject field declarations
    - Keep only SchemaGenerator instantiation and GetSchema() method
    - _Requirements: 7.1_
  
  - [x] 14.2 Update SchemaProvider class documentation
    - Update XML comments to reference SchemaGenerator
    - Document that schemas are generated from DTOs
    - _Requirements: 7.2_
  
  - [x] 14.3 Update SCHEMA_GENERATOR_REFACTORING.md
    - Mark migration as complete
    - Document lessons learned
    - Update status sections
    - _Requirements: 7.3_
  
  - [x] 14.4 Verify no orphaned code remains
    - Search for unused JsonObject schema definitions
    - Remove any temporary comparison code
    - _Requirements: 7.4_
  
  - [x] 14.5 Run full test suite
    - Run all unit tests
    - Run all integration tests
    - Verify 100% pass rate
    - _Requirements: 7.5, 7.6_

- [x] 15. Final validation checkpoint
  - Verify all 16 SchemaProviders migrated successfully
  - Verify all comparison tests pass
  - Verify all existing tests continue to pass
  - Verify AI service integration still works
  - Verify frontend UI still renders correctly
  - _Requirements: 6.7, 7.5, 7.6_

## Notes

- All tasks are required for comprehensive migration with full test coverage
- Each phase includes a checkpoint to ensure quality before proceeding
- Schema comparison tests are critical for validating migration correctness
- Existing integration tests provide regression coverage
- The SchemaGenerator infrastructure is already implemented and tested (12/12 tests passing)
- Total estimated effort: 9-14 hours including testing and validation
