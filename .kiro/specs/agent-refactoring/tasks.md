# Implementation Plan

- [x] 1. Set up core interfaces and base classes





  - Create IContractAnalysisAgent interface with InitializeAsync, AnalyzeBasicAsync, AnalyzePremiumAsync methods
  - Create InitializationResult record
  - Create ContractAnalysisAgentBase abstract class with 3-phase lifecycle
  - Create exception hierarchy (ContractAnalysisException, ContractValidationException, etc.)
  - _Requirements: 2.1, 2.2, 2.3_

- [x] 2. Implement Schema Provider infrastructure





  - [x] 2.1 Create ISchemaProvider interface

    - Define GetSchema(contractType, tier, step) returning BinaryData
    - Define GetSchemaForEndpoint(contractType, tier, step) returning JsonObject
    - _Requirements: 6.1, 6.7_

  - [x] 2.2 Create Rental Basic schema providers


    - Implement RentalGeneralSchemaProvider with x-styling metadata
    - Implement RentalRentConditionsSchemaProvider with x-styling metadata
    - Implement RentalContractConditionsSchemaProvider with x-styling metadata
    - Implement RentalBasicSummarySchemaProvider with x-styling metadata
    - _Requirements: 6.2, 6.3, 8.3_

  - [x] 2.3 Create Rental Premium schema providers


    - Implement ClauseAnalysisSchemaProvider for all premium sections
    - Implement RentalPremiumSummarySchemaProvider
    - _Requirements: 6.2, 6.3, 8.4, 8.5_

  - [x] 2.4 Create central SchemaProvider factory


    - Implement SchemaProvider with dictionary-based factory pattern
    - Register all schema providers
    - Implement GetSchema() for Nebius (BinaryData)
    - Implement GetSchemaForEndpoint() for Frontend (JsonObject)
    - _Requirements: 6.1, 6.7_

  - [x] 2.5 Write property test for schema retrieval



    - **Property 12: Schema retrieval for analysis steps**
    - **Validates: Requirements 4.5, 5.5**

  - [x] 2.6 Write property test for schema structure



    - **Property 18: Schema contains required fields**
    - **Validates: Requirements 6.2**

  - [x] 2.7 Write property test for styling metadata



    - **Property 19: Schema contains styling information**
    - **Validates: Requirements 6.3**

- [x] 3. Implement Schema Validator




  - [x] 3.1 Create ISchemaValidator interface


    - Define Validate(schema, response) returning bool
    - Define ValidateWithDetails(schema, response) returning ValidationResult
    - _Requirements: 6.4, 6.5, 6.6_

  - [x] 3.2 Implement SchemaValidator using NJsonSchema


    - Implement Validate() method
    - Implement ValidateWithDetails() with error reporting
    - _Requirements: 6.4, 6.5, 6.6_

  - [x] 3.3 Write property test for schema validation



    - **Property 13: AI responses are validated**
    - **Validates: Requirements 4.7, 5.7**

  - [x] 3.4 Write property test for validation error details



    - **Property 20: Schema validation returns details**
    - **Validates: Requirements 6.6**

- [x] 4. Implement Prompt Provider





  - [x] 4.1 Create PromptProvider static class


    - Define system prompts in region
    - Define Rental Basic prompts in region
    - Define Rental Premium prompts in region
    - Implement GetPrompt(contractType, tier, step) method
    - Use regions and comments for organization
    - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5_

  - [x] 4.2 Write property test for prompt retrieval



    - **Property 39: Central prompt retrieval**
    - **Validates: Requirements 12.1, 12.5**

- [x] 5. Implement Rental Contract Agent






  - [x] 5.1 Create RentalContractAgent class





    - Inherit from ContractAnalysisAgentBase
    - Set ExpectedContractType to "Mietvertrag"
    - Inject dependencies (IAiClient, ISchemaProvider, ISchemaValidator, ILogger)
    - _Requirements: 7.1, 7.3_

  - [x] 5.2 Implement InitializeAsync method




    - Check contract readability using AI
    - Detect contract type using AI
    - Compare detected type with expected type
    - Store contract text on success
    - Set IsInitialized to true
    - Return InitializationResult
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7_

  - [x] 5.3 Write property test for initialization
    - **Property 9: Successful initialization stores text**
    - **Validates: Requirements 3.6, 3.7**

  - [x] 5.4 Write property test for unreadable contracts
    - **Property 7: Unreadable contracts are rejected**
    - **Validates: Requirements 3.2, 3.3**

  - [x] 5.5 Write property test for contract type mismatch





    - **Property 8: Contract type mismatch detection**
    - **Validates: Requirements 3.4, 3.5**

  - [x] 5.6 Implement Basic Analysis methods


    - Implement AnalyzeBasicAsync() orchestration method
    - Implement AnalyzeGeneralDataAsync() step
    - Implement AnalyzeRentConditionsAsync() step
    - Implement AnalyzeContractConditionsAsync() step
    - Implement AnalyzeSummaryAsync() step
    - Use Task.WhenAll for parallel execution
    - Handle step failures gracefully with error messages
    - Log step duration on Info level
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8, 4.9, 10.1, 10.2, 10.3, 10.4, 10.6, 10.7_

  - [x] 5.7 Write property test for uninitialized agent


    - **Property 5: Uninitialized agents reject analysis**
    - **Validates: Requirements 2.4**

  - [x] 5.8 Write property test for stored text usage


    - **Property 11: Basic analysis uses stored text**
    - **Validates: Requirements 4.4**

  - [x] 5.9 Write property test for failed steps




    - **Property 31: Failed steps don't stop others**
    - **Property 32: Failed steps return error messages**
    - **Validates: Requirements 10.3, 10.4**

  - [x] 5.10 Implement Premium Analysis methods


    - Implement AnalyzePremiumAsync() orchestration method
    - Implement AnalyzeCostsAsync() step
    - Implement AnalyzeTermsAsync() step
    - Implement AnalyzeUseAsync() step
    - Implement AnalyzeMaintenanceAsync() step
    - Implement AnalyzeDutiesAsync() step
    - Implement AnalyzePrivacyAsync() step
    - Implement AnalyzePremiumSummaryAsync() step
    - Use Task.WhenAll for parallel execution
    - Handle step failures gracefully with error messages
    - Log step duration on Info level
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8, 5.9, 5.10, 10.1, 10.2, 10.3, 10.4, 10.6, 10.7_

  - [x] 5.11 Write property test for premium independence


    - **Property 17: Premium analysis independence**
    - **Validates: Requirements 5.11**

- [x] 6. Implement Employment Contract Agent (skeleton)




  - Create EmploymentContractAgent class
  - Inherit from ContractAnalysisAgentBase
  - Set ExpectedContractType to "Arbeitsvertrag"
  - Implement InitializeAsync method (similar to Rental)
  - Implement AnalyzeBasicAsync stub (to be completed later)
  - Implement AnalyzePremiumAsync stub (to be completed later)
  - _Requirements: 7.2, 7.3, 7.5_

- [x] 7. Implement WebSocket Handler




  - [x] 7.1 Create WebSocketAnalysisHandler class


    - Inject ILogger, IFileValidatorService, IServiceProvider
    - Implement HandleAsync(HttpContext) method
    - _Requirements: 11.1_

  - [x] 7.2 Implement file reception

    - Implement ReceiveFileAsync() to receive file + contract type
    - Parse WebSocketFileDto with contract type field
    - _Requirements: 1.1, 1.2, 11.1_

  - [x] 7.3 Write property test for WebSocket file reception



    - **Property 1: WebSocket receives contract with type**
    - **Validates: Requirements 1.1, 1.2**

  - [x] 7.4 Implement agent creation factory

    - Implement CreateAgent(contractType) method
    - Return RentalContractAgent for "Rental"
    - Return EmploymentContractAgent for "Employment"
    - Throw UnsupportedContractTypeException for unknown types
    - _Requirements: 7.1, 7.2, 7.4_

  - [x] 7.5 Write unit test for agent creation


    - **Property 22: Correct agent creation by type**
    - **Validates: Requirements 7.1, 7.2**

  - [x] 7.6 Write property test for unknown contract types

    - **Property 23: Unknown contract types rejected**
    - **Validates: Requirements 7.4**

  - [x] 7.7 Implement initialization flow

    - Call agent.InitializeAsync(contractText)
    - Check InitializationResult.Success
    - Send error and close connection if failed
    - Send confirmation message if successful
    - _Requirements: 3.8, 11.3_

  - [x] 7.8 Write property test for initialization confirmation

    - **Property 10: Initialization sends confirmation**
    - **Property 35: Confirmation message after initialization**
    - **Validates: Requirements 3.8, 11.3**

  - [x] 7.9 Implement analysis execution flow

    - Call agent.AnalyzeBasicAsync()
    - Send basic result over WebSocket
    - Call agent.AnalyzePremiumAsync()
    - Send premium result over WebSocket
    - Close WebSocket with NormalClosure
    - _Requirements: 4.10, 5.10, 11.4, 11.5, 11.6_

  - [x] 7.10 Write property test for results over WebSocket

    - **Property 16: Basic results sent over WebSocket**
    - **Property 36: Results sent over WebSocket**
    - **Validates: Requirements 4.10, 11.4, 11.5**

  - [x] 7.11 Implement error handling

    - Catch all exceptions in HandleAsync
    - Send error message over WebSocket
    - Close connection with appropriate status
    - _Requirements: 11.7_

  - [x] 7.12 Write property test for error handling

    - **Property 38: Errors sent over WebSocket**
    - **Validates: Requirements 11.7**

- [x] 8. Implement DTOs and Metadata




  - [x] 8.1 Create Analysis DTO interfaces and records


    - Create IAnalysisDto interface
    - Create AnalysisDto<TResult> record
    - Create AnalysisMetadataDto record
    - _Requirements: 8.1, 8.2_

  - [x] 8.2 Create Rental Basic DTOs


    - Create RentalBasicGeneralDto record
    - Create RentalBasicRentDto record
    - Create RentalBasicConditionsDto record
    - Create RentalBasicSummaryDto record
    - Create RentalBasicAnalysisDto record
    - _Requirements: 8.3_

  - [x] 8.3 Create Rental Premium DTOs


    - Create ClauseRiskScore enum
    - Create OverallRiskLevel enum
    - Create PremiumSectionDto record
    - Create RentalPremiumAnalysisDto record
    - _Requirements: 8.4, 8.5_

  - [x] 8.4 Write property test for DTO structure


    - **Property 24: Analysis DTOs contain metadata**
    - **Property 25: Rental basic DTO structure**
    - **Property 26: Rental premium DTO structure**
    - **Property 27: Premium section structure**
    - **Validates: Requirements 8.1, 8.2, 8.3, 8.4, 8.5**

- [x] 9. Update Dependency Injection and Configuration





  - Register ISchemaProvider as singleton
  - Register ISchemaValidator as singleton
  - Register RentalContractAgent as scoped with keyed service "rental"
  - Register EmploymentContractAgent as scoped with keyed service "employment"
  - Register WebSocketAnalysisHandler as scoped
  - Update existing Nebius configuration
  - _Requirements: 9.1_

- [x] 10. Create Controller Endpoint for Schema Retrieval



  - Create GET endpoint /api/schemas/{contractType}/{tier}/{step}
  - Inject ISchemaProvider
  - Call GetSchemaForEndpoint()
  - Return JsonObject as JSON response
  - Handle SchemaNotFoundException with 404
  - _Requirements: 6.7_

- [x] 11. Update WebSocket Routes





  - Update /rental-analysis route to use new WebSocketAnalysisHandler
  - Update /employment-analysis route to use new WebSocketAnalysisHandler
  - Remove old agent implementations
  - _Requirements: 11.1_

- [x] 12. Checkpoint - Ensure all tests pass





  - Ensure all tests pass, ask the user if questions arise.

- [x] 13. Integration Testing















  - Test complete Rental contract analysis flow end-to-end
  - Test Employment contract analysis flow (basic skeleton)
  - Test error scenarios (invalid file, wrong contract type, AI failures)
  - Test WebSocket communication (messages, closure)
  - Test parallel step execution
  - Test schema endpoint

- [x] 14. Documentation and Cleanup





  - Update API documentation
  - Add XML comments to public interfaces
  - Remove old agent code
  - Update README with new architecture

- [x] 15. Frontend: Generic Dynamic Renderer









  - [x] 15.1 Add x-title metadata to all JSON schemas






    - Add `x-title` field to all property definitions in schema providers
    - x-title contains the German display name for each field
    - Example: `["x-title"] = "Vertragstyp"` for "vertragstyp" field
    - Update all Rental Basic schema providers (General, Rent, Conditions, Summary)
    - Update all Rental Premium schema providers (ClauseAnalysis, Summary)
    - Ensure x-title is included alongside x-styling metadata
    - _Requirements: Frontend Integration, 6.3_

  - [x] 15.2 Create schema fetching utility for frontend






    - Create `fetchSchema(contractType, tier, step)` function in network.js
    - Function calls `/api/schemas/{contractType}/{tier}/{step}` endpoint
    - Returns schema with x-title and x-styling metadata
    - Cache schemas in memory to avoid repeated requests
    - Handle 404 errors gracefully
    - _Requirements: Frontend Integration, 6.7_

  - [x] 15.3 Create generic render function for analysis results






    - Create `renderAnalysisResult(data, schema, container)` function in render.js
    - Function should dynamically build UI from JSON structure using schema metadata
    - Use x-title from schema for field labels
    - Handle nested objects recursively
    - No x-styling information processing yet (future enhancement)
    - _Requirements: Frontend Integration_

  - [x] 15.4 Implement accordion structure for analysis tiers





    - Create two main accordion sections: "Basis Analyse" and "Premium Analyse"
    - Each section should be collapsible/expandable
    - Use Bootstrap accordion component for consistency
    - Basic analysis should be expanded by default
    - Premium analysis should be collapsed by default
    - _Requirements: Frontend Integration_

  - [x] 15.5 Implement accordion structure for data sections



    - Each root key in the result object becomes a sub-accordion section
    - Sections should be collapsible/expandable independently
    - Use x-title from schema for section headers
    - Fallback to key name if x-title not available
    - _Requirements: Frontend Integration_

  - [x] 15.6 Implement data rendering logic



    - Render primitive values (strings, numbers, booleans) as text with x-title labels
    - Render objects as nested accordion sections
    - Render arrays as lists
    - Handle null/undefined values gracefully
    - Format dates and numbers appropriately
    - Use x-title from schema for all field labels
    - _Requirements: Frontend Integration_

  - [x] 15.7 Update main.js to handle new WebSocket message flow



    - Update WebSocket message handler to recognize new message types
    - Handle "confirmation" message type (display loading, no pre-validation needed)
    - Handle "result" message type with data.result and data.metadata
    - Remove old pre-validation logic (isReadable, isContract checks)
    - Update message flow: confirmation → basic result → premium result
    - _Requirements: Frontend Integration, 11.3, 11.4, 11.5_

  - [x] 15.8 Update main.js to use generic renderer



    - Fetch schemas for Basic and Premium analysis steps
    - Replace calls to `rentRes()` and `emploRes()` with generic `renderAnalysisResult()`
    - Pass analysis data along with corresponding schema to renderer
    - Handle schema fetching errors gracefully
    - Maintain backward compatibility during transition
    - _Requirements: Frontend Integration_

  - [x] 15.9 Add CSS styling for dynamic renderer



    - Style accordion sections for readability
    - Add spacing and padding for nested content
    - Style field labels (from x-title) distinctly from values
    - Ensure print styles work with new structure
    - Maintain responsive design
    - _Requirements: Frontend Integration_

  - [x] 15.10 Test dynamic renderer with different data structures



    - Test with Rental contract analysis results (Basic and Premium)
    - Test with Employment contract analysis results
    - Test with missing/null fields
    - Test with deeply nested objects
    - Test accordion expand/collapse functionality
    - Test with missing x-title metadata (fallback behavior)
    - Test schema fetching and caching
    - Test new WebSocket message flow (confirmation → results)
    - _Requirements: Frontend Integration_

- [x] 16. Update Architecture Diagrams and Documentation









  - [x] 16.1 Update architecture diagrams in docs/Diagrams/diagrams.md



    - Update sequence diagrams to reflect new 3-phase agent lifecycle
    - Update component diagrams to show SchemaProvider, SchemaValidator, PromptProvider
    - Update data flow diagrams to show parallel execution of analysis steps
    - Add diagram for WebSocket message flow (confirmation → basic → premium)
    - Update UML diagrams to reflect new class structure
    - Remove references to old agent implementations
    - _Requirements: Documentation_

  - [x] 16.2 Review and update API documentation



    - Verify all endpoints are documented correctly
    - Update WebSocket message format examples
    - Add examples for schema endpoint responses with x-title metadata
    - Update error handling documentation
    - Verify client integration examples are accurate
    - _Requirements: Documentation_

  - [x] 16.3 Review and update README.md



    - Verify architecture description matches implementation
    - Update workflow descriptions with correct message flow
    - Check all code examples are accurate
    - Update performance characteristics if needed
    - Verify configuration examples are complete
    - _Requirements: Documentation_

  - [x] 16.4 Review and update design document



    - Verify all components are documented
    - Check that correctness properties match implementation
    - Update any outdated code examples
    - Ensure testing strategy reflects actual tests
    - _Requirements: Documentation_

  - [x] 16.5 Create final documentation summary



    - Document all changes made during refactoring
    - List breaking changes for frontend
    - Provide migration guide from old to new architecture
    - Document new features (parallel execution, schema-driven rendering)
    - Create troubleshooting guide for common issues
    - _Requirements: Documentation_

