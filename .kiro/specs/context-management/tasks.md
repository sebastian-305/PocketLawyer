# Implementation Plan - Context Management System

- [x] 1. Create core infrastructure and attribute
  - Create ContextStepAttribute class with StepName and optional Tier properties
  - Create ContextOptions class with BasePath, CreateReadmeFiles, and MaxContextSizeBytes
  - Create IContextProvider interface with GetContextAsync and GetContextForCurrentStepAsync methods
  - Create IContextFileReader interface with ReadDirectoryAsync method
  - _Requirements: 10.1, 10.2, 1.1_

- [x] 1.1 Write unit tests for ContextStepAttribute
  - Test attribute can be applied to methods only
  - Test StepName is required
  - Test Tier is optional
  - _Requirements: 10.1, 10.2_

- [x] 2. Implement ContextFileReader
  - [x] 2.1 Create ContextFileReader class implementing IContextFileReader
    - Implement ReadDirectoryAsync to read .txt and .md files
    - Filter out README.md files
    - Sort files alphabetically
    - Concatenate contents with filename headers
    - Handle file read errors gracefully (log warning, continue)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

  - [x] 2.2 Write property test for file extension filtering
    - **Property 6: File extension filtering**
    - **Validates: Requirements 4.1, 4.2**

  - [x] 2.3 Write property test for file concatenation
    - **Property 3: Context loading determinism**
    - **Validates: Requirements 4.3**

  - [x] 2.4 Write unit tests for error handling
    - Test unreadable files are skipped
    - Test empty directories return empty string
    - Test README.md is excluded
    - _Requirements: 4.5_

- [x] 3. Implement ContextProvider
  - [x] 3.1 Create ContextProvider class implementing IContextProvider
    - Inject ContextOptions, IContextFileReader, and ILogger
    - Implement per-request caching with Dictionary
    - Implement GetContextAsync with agent/tier/step parameters
    - Load agent-level common context
    - Load tier-level common context
    - Load step-specific context
    - Concatenate in correct order with headers
    - Log performance metrics
    - Warn if context exceeds max size
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 9.2, 9.4, 11.3, 11.4_

  - [x] 3.2 Implement GetContextForCurrentStepAsync using StackTrace
    - Use StackTrace to find calling method
    - Find ContextStep attribute on method
    - Extract agent name from declaring type
    - Infer tier from method name or attribute
    - Normalize step name
    - Call GetContextAsync with resolved parameters
    - _Requirements: 5.1, 5.6_

  - [x] 3.3 Write property test for cache consistency
    - **Property 4: Cache consistency**
    - **Validates: Requirements 5.4**

  - [x] 3.4 Write property test for common context ordering
    - **Property 5: Common context inclusion order**
    - **Validates: Requirements 9.2, 9.4**

  - [x] 3.5 Write unit tests for empty context
    - Test empty directory returns empty string
    - Test non-existent directory returns empty string
    - _Requirements: 5.3_

- [x] 4. Implement ContextDirectoryInitializer
  - [x] 4.1 Create ContextDirectoryInitializer class implementing IHostedService
    - Inject IServiceProvider, ContextOptions, and ILogger
    - Implement StartAsync to initialize directories
    - Create base directory from options
    - Discover all agent types implementing IContractAnalysisAgent
    - For each agent, scan for methods with ContextStep attribute
    - Extract agent name from type name
    - Infer tier from method name or attribute
    - Normalize step name
    - Create directory structure
    - Create README files if enabled
    - Create common directories for each tier
    - Log initialization progress
    - Handle errors gracefully (log, don't throw)
    - _Requirements: 1.3, 3.1, 3.2, 3.3, 3.4, 3.5_

  - [x] 4.2 Implement path normalization helper
    - Convert to lowercase
    - Replace spaces with hyphens
    - Remove special characters
    - Ensure idempotency
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

  - [x] 4.3 Implement README file generation
    - Create step-specific README template
    - Create common directory README template
    - Don't overwrite existing README files
    - _Requirements: 1.1_

  - [x] 4.4 Write property test for directory creation idempotence
    - **Property 1: Directory creation idempotence**
    - **Validates: Requirements 3.4**

  - [x] 4.5 Write property test for path normalization consistency
    - **Property 2: Context path normalization consistency**
    - **Validates: Requirements 8.2, 8.3, 8.4**

  - [x] 4.6 Write property test for attribute discovery completeness
    - **Property 8: Attribute discovery completeness**
    - **Validates: Requirements 3.1, 3.2**

  - [x] 4.7 Write unit tests for error handling
    - Test graceful handling of directory creation failures
    - Test graceful handling of README creation failures
    - Test application continues on errors
    - _Requirements: 3.5_

- [x] 5. Register services in Program.cs
  - Add ContextOptions configuration binding
  - Register IContextFileReader as Singleton
  - Register IContextProvider as Scoped
  - Register ContextDirectoryInitializer as HostedService
  - _Requirements: 1.1_

- [x] 6. Extend TextFileDecoder to support .md files
  - Update TextFileDecoder to accept .md extension
  - Add .md to supported extensions list
  - Test with markdown files
  - _Requirements: 4.2_

- [x] 7. Update RentalContractAgent with ContextStep attributes
  - [x] 7.1 Add ContextStep attributes to basic analysis methods
    - Mark AnalyzeGeneralDataAsync with [ContextStep("general-data")]
    - Mark AnalyzeRentConditionsAsync with [ContextStep("rent-conditions")]
    - Mark AnalyzeContractConditionsAsync with [ContextStep("contract-conditions")]
    - Mark AnalyzeSummaryAsync with [ContextStep("summary")]
    - _Requirements: 10.1_

  - [x] 7.2 Add ContextStep attributes to premium analysis methods
    - Mark AnalyzeCostsAsync with [ContextStep("costs")]
    - Mark AnalyzeTermsAsync with [ContextStep("terms")]
    - Mark AnalyzeUseAsync with [ContextStep("use")]
    - Mark AnalyzeMaintenanceAsync with [ContextStep("maintenance")]
    - Mark AnalyzeDutiesAsync with [ContextStep("duties")]
    - Mark AnalyzePrivacyAsync with [ContextStep("privacy")]
    - Mark AnalyzePremiumSummaryAsync with [ContextStep("premium-summary")]
    - _Requirements: 10.1_

  - [x] 7.3 Inject IContextProvider into RentalContractAgent
    - Add IContextProvider parameter to constructor
    - Store as private field
    - _Requirements: 5.1_

  - [x] 7.4 Update agent methods to load and use context
    - Call GetContextForCurrentStepAsync in each attributed method
    - Append context to prompt if not empty
    - Use format: "\n\n## Reference Context\n\n{context}"
    - _Requirements: 5.1, 6.1, 6.2, 6.3, 6.4_

- [x] 8. Update EmploymentContractAgent with ContextStep attributes
  - Add ContextStep attributes to all analysis methods
  - Inject IContextProvider
  - Update methods to load and use context
  - _Requirements: 10.1, 5.1, 6.1_

- [x] 9. Add configuration to appsettings
  - Add Context section to appsettings.json with default values
  - Add Context section to appsettings.Development.json
  - Document configuration options
  - _Requirements: 1.1, 1.2_

- [x] 10. Write integration tests
  - Test end-to-end context loading for a complete agent execution
  - Test directory initialization creates all expected directories
  - Test context files are loaded and included in prompts
  - Test performance is acceptable (<100ms for 1MB)
  - _Requirements: 11.1, 11.5_

- [x] 11. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 12. Write property test for context path construction
  - **Property: Path construction consistency**
  - For any agent name, tier, and step name, the constructed path should follow the format {BasePath}/{AgentName}/{Tier}/{StepName}
  - **Validates: Requirements 2.1**

- [x] 13. Write property test for error resilience
  - **Property 7: Error resilience**
  - **Validates: Requirements 4.5**

- [x] 14. Update documentation
  - Update CONFIGURATION.md with Context options
  - Add section explaining context directory structure
  - Add examples of context file usage
  - Document performance considerations
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [x] 15. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.
