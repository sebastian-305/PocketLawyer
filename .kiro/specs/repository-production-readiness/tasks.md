# Implementation Plan: Repository Production Readiness

## Overview

This implementation plan breaks down the repository cleanup and production-readiness project into discrete, manageable tasks. The approach follows a systematic order: first analyze and plan, then reorganize DTOs, create front-end pages, clean up files, restructure documentation, and finally verify the build. Each task builds on previous work and includes validation steps to ensure correctness.

## Tasks

- [x] 1. Analyze repository and create cleanup plan
  - Scan repository for all files matching cleanup patterns (TASK_*.md, test-*.html, verify_*.csx, Restructuring/*)
  - Identify deprecated code with [Obsolete] attributes or deprecation comments
  - Create comprehensive cleanup list with file paths, categories, and deletion reasons
  - Generate folder-by-folder analysis report with cleanup recommendations
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.7, 4.8, 5.1-5.8_

- [x] 2. Search git history for old front-end content
  - Execute git log command to search for commits containing "Mietvertrag" or "Arbeitsvertrag" HTML files
  - Parse git history to identify old separate front-end pages
  - Extract relevant text content from old files if found
  - Document findings for reuse in new pages
  - _Requirements: 1.4, 1.5_

- [x] 3. Reorganize DTO folder structure
  - [x] 3.1 Create DTO subfolders
    - Create Domain/Dtos/Employment/ folder
    - Create Domain/Dtos/Rental/ folder
    - Create Domain/Dtos/Shared/ folder
    - _Requirements: 3.1, 3.2, 3.3_
  
  - [x] 3.2 Move Employment DTOs
    - Move EmploymentBasicAnalysisDto.cs to Domain/Dtos/Employment/
    - Move EmploymentContractAnalysisDto.cs to Domain/Dtos/Employment/
    - Move EmploymentPremiumAnalysisDto.cs to Domain/Dtos/Employment/
    - Update namespace to PocketLawyer.Domain.Dtos.Employment in each file
    - _Requirements: 3.4, 3.7_
  
  - [x] 3.3 Move Rental DTOs
    - Move RentalBasicAnalysisDto.cs to Domain/Dtos/Rental/
    - Move RentalPremiumAnalysisDto.cs to Domain/Dtos/Rental/
    - Move RentContractAnalysisDto.cs to Domain/Dtos/Rental/
    - Update namespace to PocketLawyer.Domain.Dtos.Rental in each file
    - _Requirements: 3.5, 3.7_
  
  - [x] 3.4 Move Shared DTOs
    - Move IAnalysisDto.cs to Domain/Dtos/Shared/
    - Move AnalysisDto.cs to Domain/Dtos/Shared/
    - Move ContractValidationDto.cs to Domain/Dtos/Shared/
    - Move FileContentDto.cs to Domain/Dtos/Shared/
    - Move WebSocketFileDto.cs to Domain/Dtos/Shared/
    - Move KvDetectionAnswer.cs to Domain/Dtos/Shared/
    - Move KvDetectionResult.cs to Domain/Dtos/Shared/
    - Move IValidationDto.cs to Domain/Dtos/Shared/
    - Update namespace to PocketLawyer.Domain.Dtos.Shared in each file
    - _Requirements: 3.6, 3.7_
  
  - [x] 3.5 Write property test for namespace-location consistency
    - **Property 7: Namespace-Location Consistency**
    - **Validates: Requirements 3.7, 8.1**

- [x] 4. Update using statements across codebase
  - Scan all C# files for references to moved DTOs
  - Update using statements from PocketLawyer.Domain.Dtos to specific subnamespaces
  - Add using PocketLawyer.Domain.Dtos.Employment where employment DTOs are used
  - Add using PocketLawyer.Domain.Dtos.Rental where rental DTOs are used
  - Add using PocketLawyer.Domain.Dtos.Shared where shared DTOs are used
  - _Requirements: 3.8, 8.2_

- [x] 4.1 Write property test for using statement correctness
  - **Property 8: Using Statement Correctness**
  - **Validates: Requirements 3.8, 8.2**

- [x] 5. Verify compilation after DTO reorganization
  - Run dotnet build to check for compilation errors
  - Fix any namespace or using statement issues
  - Run dotnet test to ensure all tests still pass
  - _Requirements: 8.3, 8.4_

- [x] 6. Checkpoint - Ensure DTO reorganization is complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Create separate front-end pages
  - [x] 7.1 Create employment contract page
    - Create wwwroot/employment.html
    - Copy relevant structure from index.html
    - Update content to be employment-specific
    - Add navigation link to rental.html
    - Update form to connect to /employment-analysis WebSocket
    - _Requirements: 1.1, 1.2, 1.3_
  
  - [x] 7.2 Create rental contract page
    - Create wwwroot/rental.html
    - Copy relevant structure from index.html
    - Update content to be rental-specific
    - Add navigation link to employment.html
    - Update form to connect to /rental-analysis WebSocket
    - _Requirements: 1.1, 1.2, 1.3_
  
  - [x] 7.3 Update landing page
    - Modify index.html to serve as landing page
    - Add prominent links to employment.html and rental.html
    - Keep marketing content and branding
    - Remove or simplify file upload form (redirect to specific pages)
    - _Requirements: 1.1, 1.2_
  
  - [x] 7.4 Write property test for navigation link bidirectionality
    - **Property 1: Navigation Link Bidirectionality**
    - **Validates: Requirements 1.2**
  
  - [x] 7.5 Write property test for content preservation
    - **Property 2: Content Preservation from Original**
    - **Validates: Requirements 1.3**

- [x] 8. Create JavaScript modules for front-end
  - [x] 8.1 Create wwwroot/js/ folder structure
    - Create wwwroot/js/ directory
    - Move main.js to wwwroot/js/main.js
    - Move network.js to wwwroot/js/network.js
    - Move render.js to wwwroot/js/render.js
    - _Requirements: Front-end organization_
  
  - [x] 8.2 Create employment.js module
    - Create wwwroot/js/employment.js
    - Implement employment-specific WebSocket handling
    - Implement employment form submission logic
    - Implement employment result rendering
    - _Requirements: 1.1_
  
  - [x] 8.3 Create rental.js module
    - Create wwwroot/js/rental.js
    - Implement rental-specific WebSocket handling
    - Implement rental form submission logic
    - Implement rental result rendering
    - _Requirements: 1.1_
  
  - [x] 8.4 Update HTML script references
    - Update employment.html to reference employment.js
    - Update rental.html to reference rental.js
    - Update index.html to reference main.js
    - Ensure all script paths are correct
    - _Requirements: 1.1_

- [x] 9. Fix front-end styling issues
  - [x] 9.1 Audit CSS color values
    - Review styles.css for color definitions
    - Review praes.css for color definitions
    - Identify color mismatches and inconsistencies
    - Document intended color scheme
    - _Requirements: 2.1_
  
  - [x] 9.2 Fix color scheme consistency
    - Update color values to match design intent
    - Ensure consistent colors across all pages
    - Test color contrast for accessibility
    - _Requirements: 2.2, 2.3_
  
  - [x] 9.3 Organize CSS files
    - Move CSS files to wwwroot/css/ folder
    - Update HTML link references
    - Ensure Bootstrap 5 compatibility
    - _Requirements: 2.4_
  
  - [x] 9.4 Write property test for styling consistency
    - **Property 3: Styling Consistency Across Pages**
    - **Validates: Requirements 2.2, 2.3**
  
  - [x] 9.5 Write property test for Bootstrap class validity
    - **Property 4: Bootstrap Class Validity**
    - **Validates: Requirements 2.4**

- [x] 10. Checkpoint - Ensure front-end changes are complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Reorganize documentation structure
  - [x] 11.1 Create legacy documentation folder
    - Create docs/legacy/ folder
    - _Requirements: 6.1_
  
  - [x] 11.2 Move documentation to legacy folder
    - Move docs/AGENT_REFACTORING_SUMMARY.md to docs/legacy/
    - Move docs/KIRO_AENDERUNGEN.md to docs/legacy/
    - Move docs/KOLLEKTIVVERTRAG_FIELD_DOCUMENTATION.md to docs/legacy/
    - Move docs/KV_CONTENT_CONFIGURATION.md to docs/legacy/
    - Move docs/KV_DETECTION_VALIDATION.md to docs/legacy/
    - Move docs/REFACTORING_SUMMARY.md to docs/legacy/
    - Move docs/SCHEMA_GENERATOR_REFACTORING.md to docs/legacy/
    - Move docs/TEST_FAILURES_ANALYSIS.md to docs/legacy/
    - _Requirements: 6.2_
  
  - [x] 11.3 Move root documentation files to legacy
    - Move all TASK_*.md files from root to docs/legacy/
    - Move FILE_OVERVIEW.md to docs/legacy/
    - Move JSON_SCHEMA_DEFINITION_ANALYSIS.md to docs/legacy/
    - Move all SCHEMA_MIGRATION_*.md files to docs/legacy/
    - _Requirements: 6.3, 6.4_
  
  - [x] 11.4 Preserve current README
    - Copy README.md to docs/legacy/README_old.md
    - _Requirements: 6.7_
  
  - [x] 11.5 Write property test for legacy documentation relocation
    - **Property 12: Legacy Documentation Relocation**
    - **Validates: Requirements 6.2, 6.3, 6.4**

- [x] 12. Create new production README
  - Create new README.md with clean structure
  - Include project overview section
  - Include architecture section
  - Include setup instructions section
  - Include usage guide section
  - Include links to API_DOCUMENTATION.md and other kept docs
  - _Requirements: 6.5, 6.6_

- [x] 12.1 Write property test for README content completeness
  - **Property 13: README Content Completeness**
  - **Validates: Requirements 6.5, 6.6**

- [x] 13. Execute file cleanup
  - [x] 13.1 Verify files are not referenced
    - For each file in cleanup list, scan codebase for references
    - Mark files as safe to delete or skip if referenced
    - Create report of skipped files with reasons
    - _Requirements: 7.2, 7.4_
  
  - [x] 13.2 Delete root directory files
    - Delete all verify_*.csx files from root
    - Delete test-schema-gen.csx from root
    - Delete test-serialization.csx from root
    - Delete update-test-properties.ps1 from root
    - Log each deletion
    - _Requirements: 7.1, 7.3_
  
  - [x] 13.3 Delete wwwroot test files
    - Delete test-dynamic-renderer.html
    - Delete test-dynamic-renderer.mjs
    - Delete test-render-generic.html
    - Delete test-render-logic.mjs
    - Delete test-schema-fetch.html
    - Delete test-tier-accordion.html
    - Delete test-tier-accordion.mjs
    - Delete all *_README.md and *_SUMMARY.md files from wwwroot
    - Log each deletion
    - _Requirements: 7.1, 7.3_
  
  - [x] 13.4 Delete Restructuring folder
    - Verify no production code references Restructuring folder contents
    - Delete entire Restructuring/ folder
    - Log deletion
    - _Requirements: 7.1, 7.3_
  
  - [x] 13.5 Create deletion log
    - Generate deletion_log.md with all deleted files
    - Include timestamp and reason for each deletion
    - Include list of skipped files with reasons
    - _Requirements: 7.3, 7.4_
  
  - [x] 13.6 Write property test for file deletion completeness
    - **Property 14: File Deletion Completeness**
    - **Validates: Requirements 7.1**
  
  - [x] 13.7 Write property test for production code reference safety
    - **Property 15: Production Code Reference Safety**
    - **Validates: Requirements 7.2**
  
  - [x] 13.8 Write property test for deletion log completeness
    - **Property 16: Deletion Log Completeness**
    - **Validates: Requirements 7.3**
  
  - [x] 13.9 Write property test for referenced file preservation
    - **Property 17: Referenced File Preservation**
    - **Validates: Requirements 7.4**

- [x] 14. Verify build after cleanup
  - Run dotnet build in Release configuration
  - Verify no compilation errors
  - Run dotnet test to ensure all tests pass
  - _Requirements: 7.5, 9.1, 9.2_

- [x] 15. Checkpoint - Ensure cleanup is complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 16. Final production build verification
  - [x] 16.1 Execute release build
    - Run dotnet build --configuration Release
    - Capture and report any build errors
    - _Requirements: 9.1, 9.3_
  
  - [x] 16.2 Execute full test suite
    - Run dotnet test
    - Capture and report any test failures
    - _Requirements: 9.2, 9.3, 9.5_
  
  - [x] 16.3 Generate production readiness report
    - Document all completed changes
    - List all moved files with new locations
    - List all deleted files
    - Confirm build success and test passage
    - _Requirements: 9.4_
  
  - [x] 16.4 Write property test for build error reporting
    - **Property 19: Build Error Reporting**
    - **Validates: Requirements 9.3, 9.5**

- [x] 17. Final checkpoint - Confirm production readiness
  - Ensure all tests pass, ask the user if questions arise.
  - Confirm repository is ready for final submission.

## Notes

- All tasks are required for comprehensive production readiness
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation throughout the process
- Property tests validate universal correctness properties
- Unit tests validate specific examples and edge cases
- The implementation follows a logical order: analyze → reorganize → create → clean → verify
