# Implementation Plan: Employment Contract Agent

## Overview

Dieser Plan beschreibt die schrittweise Implementierung des Employment Contract Agent basierend auf dem RentalContractAgent als Referenz. Die Implementierung erfolgt inkrementell mit regelmäßigen Checkpoints zur Validierung.

## Tasks

- [x] 1. Create DTO structures for Employment Contract Analysis
  - Create file `Domain/Dtos/EmploymentBasicAnalysisDto.cs`
  - Define `EmploymentBasicGeneralDto` record with employment-specific fields
  - Define `EmploymentBasicCompensationDto` record for salary and benefits
  - Define `EmploymentBasicWorkingConditionsDto` record for work conditions
  - Define `EmploymentBasicSummaryDto` record with KV mention
  - Define `EmploymentBasicAnalysisDto` record as combined DTO implementing IAnalysisDto
  - Create file `Domain/Dtos/EmploymentPremiumAnalysisDto.cs`
  - Define `EmploymentPremiumSummaryDto` record for premium summary
  - Define `EmploymentPremiumAnalysisDto` record as combined DTO implementing IAnalysisDto
  - Use JsonPropertyName attributes for all properties (camelCase for JSON)
  - _Requirements: 4.1, 4.2, 4.4, 4.5_

- [x] 1.1 Write unit tests for DTO serialization/deserialization
  - Test all DTOs can be serialized to JSON and deserialized back
  - Test edge cases with null/empty values
  - _Requirements: 4.1, 4.2_

- [x] 2. Create JSON Schemas for Employment Contract Analysis
  - [x] 2.1 Create Basic Analysis Schemas
    - Create schema for `Employment_Basic_General`
    - Create schema for `Employment_Basic_Compensation`
    - Create schema for `Employment_Basic_WorkingConditions`
    - Create schema for `Employment_Basic_Summary`
    - _Requirements: 4.1, 4.2, 4.3, 4.5_
  
  - [x] 2.2 Create Premium Analysis Schemas
    - Create schema for `Employment_Premium_CompensationBenefits`
    - Create schema for `Employment_Premium_WorkingTimeVacation`
    - Create schema for `Employment_Premium_TerminationNotice`
    - Create schema for `Employment_Premium_NonCompeteConfidentiality`
    - Create schema for `Employment_Premium_RightsDuties`
    - Create schema for `Employment_Premium_Summary`
    - _Requirements: 4.1, 4.2, 4.3, 4.5_

- [x] 2.3 Write unit tests for schema validation
  - Test schemas validate correct JSON structures
  - Test schemas reject invalid JSON structures
  - _Requirements: 4.1, 4.2_

- [x] 3. Implement KV Detection in Initialization Phase
  - [x] 3.1 Add KV-related fields to EmploymentContractAgent
    - Add `_detectedKvName` field
    - Add `_detectedKvText` field
    - Add `_kvDetectionService` dependency
    - _Requirements: 2.4, 2.5_
  
  - [x] 3.2 Update InitializeAsync to detect and store KV
    - Call `_kvDetectionService.DetectAsync()` after contract type validation
    - Store KV name and text in agent fields
    - Handle case when no KV is detected
    - _Requirements: 2.4, 2.5, 2.6_

- [x] 3.3 Write unit tests for KV detection in initialization
  - Test KV is detected and stored when present
  - Test initialization succeeds when no KV is found
  - Test KV detection errors are handled gracefully
  - _Requirements: 2.4, 2.5_

- [x] 4. Checkpoint - Ensure initialization tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Implement Basic Analysis Steps
  - [x] 5.1 Implement AnalyzeGeneralEmploymentDataAsync
    - Add `[ContextStep("general-employment-data")]` attribute
    - Load schema from Schema_Provider
    - Load context from Context_Provider
    - Call AI_Client with prompt and contract text
    - Validate response against schema
    - Deserialize to EmploymentBasicGeneralDto
    - Return error DTO on failure
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_
  
  - [x] 5.2 Write property test for AnalyzeGeneralEmploymentDataAsync
    - **Property 3: Token Tracking Completeness**
    - **Validates: Requirements 10.1, 10.3**
  
  - [x] 5.3 Implement AnalyzeCompensationAsync
    - Add `[ContextStep("compensation")]` attribute
    - Follow same pattern as AnalyzeGeneralEmploymentDataAsync
    - Return EmploymentBasicCompensationDto
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_
  
  - [x] 5.4 Implement AnalyzeWorkingConditionsAsync
    - Add `[ContextStep("working-conditions")]` attribute
    - Follow same pattern as AnalyzeGeneralEmploymentDataAsync
    - Return EmploymentBasicWorkingConditionsDto
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_
  
  - [x] 5.5 Implement AnalyzeBasicSummaryAsync
    - Add `[ContextStep("basic-summary")]` attribute
    - Include detected KV name in summary
    - Handle case when no KV was detected
    - Return EmploymentBasicSummaryDto
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 7.1, 7.2_

- [x] 5.6 Write unit tests for Basic Analysis steps
  - Test each step handles errors gracefully
  - Test error DTOs are returned on failure
  - Test context loading is called
  - _Requirements: 5.6, 11.1, 11.2_

- [x] 6. Implement Basic Analysis Orchestration
  - Update AnalyzeBasicAsync to call all basic steps in parallel
  - Use Task.WhenAll for parallel execution
  - Combine results into EmploymentBasicAnalysisDto
  - Create AnalysisMetadataDto
  - Log token summary
  - _Requirements: 3.1, 3.2, 3.3, 10.2_

- [x] 6.1 Write property test for error resilience
  - **Property 4: Error DTO Resilience**
  - **Validates: Requirements 11.2**

- [x] 6.2 Write unit tests for Basic Analysis orchestration
  - Test EnsureInitialized is called
  - Test all steps are executed
  - Test results are combined correctly
  - Test analysis continues when one step fails
  - _Requirements: 3.1, 3.3, 3.4, 11.3_

- [x] 7. Checkpoint - Ensure Basic Analysis tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. Implement Premium Analysis Steps
  - [x] 8.1 Implement AnalyzeCompensationBenefitsClausesAsync
    - Add `[ContextStep("compensation-benefits-clauses")]` attribute
    - Load schema and context
    - Add KV text to prompt if available
    - Call AI_Client and validate response
    - Return PremiumSectionDto
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 7.3_
  
  - [x] 8.2 Implement AnalyzeWorkingTimeVacationAsync
    - Add `[ContextStep("working-time-vacation")]` attribute
    - Follow same pattern with KV context
    - Return PremiumSectionDto
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 7.3_
  
  - [x] 8.3 Implement AnalyzeTerminationNoticeAsync
    - Add `[ContextStep("termination-notice")]` attribute
    - Follow same pattern with KV context
    - Return PremiumSectionDto
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 7.3_
  
  - [x] 8.4 Implement AnalyzeNonCompeteConfidentialityAsync
    - Add `[ContextStep("non-compete-confidentiality")]` attribute
    - Follow same pattern with KV context
    - Return PremiumSectionDto
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 7.3_
  
  - [x] 8.5 Implement AnalyzeRightsDutiesAsync
    - Add `[ContextStep("rights-duties")]` attribute
    - Follow same pattern with KV context
    - Return PremiumSectionDto
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 7.3_
  
  - [x] 8.6 Implement AnalyzePremiumSummaryAsync
    - Add `[ContextStep("premium-summary")]` attribute
    - Return tuple of (Gesamtfazit, Gesamtbewertung)
    - _Requirements: 9.1, 9.2, 9.4, 9.5_

- [x] 8.7 Write unit tests for Premium Analysis steps
  - Test KV context is included in prompts
  - Test error handling for each step
  - _Requirements: 7.3, 9.5, 11.2_

- [x] 9. Implement Premium Analysis Orchestration
  - Update AnalyzePremiumAsync to call all premium steps in parallel
  - Use Task.WhenAll for parallel execution
  - Combine results into EmploymentPremiumAnalysisDto
  - Create AnalysisMetadataDto
  - Log token summary
  - _Requirements: 8.1, 8.2, 8.3, 10.2_

- [x] 9.1 Write unit tests for Premium Analysis orchestration
  - Test EnsureInitialized is called
  - Test all steps are executed
  - Test results are combined correctly
  - Test analysis continues when one step fails
  - _Requirements: 8.1, 8.3, 8.4, 11.3, 11.4_

- [x] 10. Checkpoint - Ensure Premium Analysis tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Create Context Files for Analysis Steps
  - Create context file for `general-employment-data`
  - Create context file for `compensation`
  - Create context file for `working-conditions`
  - Create context file for `basic-summary`
  - Create context file for `compensation-benefits-clauses`
  - Create context file for `working-time-vacation`
  - Create context file for `termination-notice`
  - Create context file for `non-compete-confidentiality`
  - Create context file for `rights-duties`
  - Create context file for `premium-summary`
  - _Requirements: 5.2, 9.2_

- [x] 12. Organize KV Storage Location
  - Create directory structure `data/KVS/Employment/`
  - Move or copy relevant KV files to new location
  - Update KV file service configuration if needed
  - _Requirements: 6.1, 6.3, 6.4_

- [x] 13. Write property tests for agent construction
  - **Property 1: Unique Agent IDs**
  - **Validates: Requirements 1.2**
  - **Property 2: Null Dependency Rejection**
  - **Validates: Requirements 1.4**

- [x] 14. Write integration tests
  - Test complete Basic Analysis flow with real schemas
  - Test complete Premium Analysis flow with real schemas
  - Test KV detection integration
  - _Requirements: 2.4, 3.2, 3.3, 8.2, 8.3_

- [x] 15. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties
- Unit tests validate specific examples and edge cases
- Follow RentalContractAgent implementation pattern closely
- All analysis steps use the same error handling pattern
- Context files should be created based on legal domain knowledge
