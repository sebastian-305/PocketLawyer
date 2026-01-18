# Design Document

## Overview

This design document outlines the technical approach for preparing the Klausel Klara repository for production readiness. The project involves reorganizing DTOs, creating separate front-end pages for each contract type, cleaning up legacy files, and restructuring documentation. The design emphasizes maintainability, clear separation of concerns, and production-ready code quality.

## Architecture

### High-Level Structure

The repository cleanup follows a systematic approach:

1. **Front-End Separation**: Create dedicated pages for employment and rental contracts
2. **DTO Reorganization**: Implement folder-based organization by agent type
3. **File Cleanup**: Remove development artifacts and legacy code
4. **Documentation Restructuring**: Archive old docs and create clean production documentation

### Folder Structure (After Reorganization)

```
Domain/
  Dtos/
    Employment/
      EmploymentBasicAnalysisDto.cs
      EmploymentContractAnalysisDto.cs
      EmploymentPremiumAnalysisDto.cs
    Rental/
      RentalBasicAnalysisDto.cs
      RentalPremiumAnalysisDto.cs
      RentContractAnalysisDto.cs
    Shared/
      IAnalysisDto.cs
      AnalysisDto.cs
      ContractValidationDto.cs
      FileContentDto.cs
      WebSocketFileDto.cs
      KvDetectionAnswer.cs
      KvDetectionResult.cs
      IValidationDto.cs

wwwroot/
  index.html (landing page with contract type selection)
  employment.html (dedicated employment contract page)
  rental.html (dedicated rental contract page)
  js/
    main.js
    employment.js
    rental.js
    network.js
    render.js
  css/
    styles.css
    praes.css
    print.css
  Media/
    (existing images)

docs/
  legacy/
    (all old documentation files)
    (all TASK_*.md files)
    (all analysis markdown files)
  diagrams/
    (existing diagrams)
  API_DOCUMENTATION.md (kept)
  CONFIGURATION.md (kept)
  DesignPatterns_de.md (kept)

(root directory cleaned of TASK_*.md and verify_*.csx files)
```

## Components and Interfaces

### Front-End Components

#### 1. Landing Page (index.html)
- **Purpose**: Entry point with contract type selection
- **Features**:
  - Contract type selector (Employment/Rental)
  - Redirect to appropriate dedicated page
  - Marketing content about Klausel Klara
  - Consistent branding and styling

#### 2. Employment Contract Page (employment.html)
- **Purpose**: Dedicated page for employment contract analysis
- **Features**:
  - Employment-specific file upload
  - WebSocket connection to /employment-analysis
  - Employment-focused content and messaging
  - Link to rental contract page
  - Analysis results display

#### 3. Rental Contract Page (rental.html)
- **Purpose**: Dedicated page for rental contract analysis
- **Features**:
  - Rental-specific file upload
  - WebSocket connection to /rental-analysis
  - Rental-focused content and messaging
  - Link to employment contract page
  - Analysis results display

#### 4. JavaScript Modules

**main.js** (shared utilities)
- Common initialization logic
- Shared helper functions
- Bootstrap initialization

**employment.js**
- Employment-specific WebSocket handling
- Employment form submission
- Employment result rendering

**rental.js**
- Rental-specific WebSocket handling
- Rental form submission
- Rental result rendering

**network.js** (existing, kept)
- WebSocket connection management
- Message handling

**render.js** (existing, kept)
- Generic result rendering
- Schema-based UI generation

### DTO Organization

#### Employment DTOs
Located in `Domain/Dtos/Employment/`:
- `EmploymentBasicAnalysisDto.cs`
- `EmploymentContractAnalysisDto.cs`
- `EmploymentPremiumAnalysisDto.cs`

Namespace: `PocketLawyer.Domain.Dtos.Employment`

#### Rental DTOs
Located in `Domain/Dtos/Rental/`:
- `RentalBasicAnalysisDto.cs`
- `RentalPremiumAnalysisDto.cs`
- `RentContractAnalysisDto.cs`

Namespace: `PocketLawyer.Domain.Dtos.Rental`

#### Shared DTOs
Located in `Domain/Dtos/Shared/`:
- `IAnalysisDto.cs` (interface)
- `AnalysisDto.cs` (generic wrapper)
- `ContractValidationDto.cs`
- `FileContentDto.cs`
- `WebSocketFileDto.cs`
- `KvDetectionAnswer.cs`
- `KvDetectionResult.cs`
- `IValidationDto.cs`

Namespace: `PocketLawyer.Domain.Dtos.Shared`

### Cleanup Strategy

#### Files to Delete

**Root Directory:**
- All `TASK_*.md` files (move to docs/legacy first)
- All `verify_*.csx` files
- `test-schema-gen.csx`
- `test-serialization.csx`
- `update-test-properties.ps1`
- `FILE_OVERVIEW.md` (move to docs/legacy)
- `JSON_SCHEMA_DEFINITION_ANALYSIS.md` (move to docs/legacy)
- `SCHEMA_MIGRATION_*.md` files (move to docs/legacy)

**wwwroot Directory:**
- `test-dynamic-renderer.html`
- `test-dynamic-renderer.mjs`
- `test-render-generic.html`
- `test-render-logic.mjs`
- `test-schema-fetch.html`
- `test-tier-accordion.html`
- `test-tier-accordion.mjs`
- All `*_README.md` and `*_SUMMARY.md` files (move to docs/legacy)

**Restructuring Folder:**
- Entire `Restructuring/` folder (after verifying no production dependencies)

**docs Directory:**
- Move to docs/legacy:
  - `AGENT_REFACTORING_SUMMARY.md`
  - `KIRO_AENDERUNGEN.md`
  - `KOLLEKTIVVERTRAG_FIELD_DOCUMENTATION.md`
  - `KV_CONTENT_CONFIGURATION.md`
  - `KV_DETECTION_VALIDATION.md`
  - `REFACTORING_SUMMARY.md`
  - `SCHEMA_GENERATOR_REFACTORING.md`
  - `TEST_FAILURES_ANALYSIS.md`

**Files to Keep:**
- `docs/API_DOCUMENTATION.md`
- `docs/CONFIGURATION.md`
- `docs/DesignPatterns_de.md`
- `docs/diagrams/` (entire folder)

## Data Models

### DTO Namespace Structure

```csharp
// Employment DTOs
namespace PocketLawyer.Domain.Dtos.Employment
{
    public class EmploymentBasicAnalysisDto { }
    public class EmploymentContractAnalysisDto : IAnalysisDto { }
    public class EmploymentPremiumAnalysisDto { }
}

// Rental DTOs
namespace PocketLawyer.Domain.Dtos.Rental
{
    public class RentalBasicAnalysisDto { }
    public class RentalPremiumAnalysisDto { }
    public class RentContractAnalysisDto : IAnalysisDto { }
}

// Shared DTOs
namespace PocketLawyer.Domain.Dtos.Shared
{
    public interface IAnalysisDto { }
    public class AnalysisDto<T> where T : IAnalysisDto { }
    public class ContractValidationDto { }
    public class FileContentDto { }
    public class WebSocketFileDto { }
    public class KvDetectionAnswer { }
    public class KvDetectionResult { }
    public interface IValidationDto { }
}
```

### Using Statement Updates

All files that reference DTOs will need updated using statements:

```csharp
// Before
using PocketLawyer.Domain.Dtos;

// After (for employment-specific code)
using PocketLawyer.Domain.Dtos.Employment;
using PocketLawyer.Domain.Dtos.Shared;

// After (for rental-specific code)
using PocketLawyer.Domain.Dtos.Rental;
using PocketLawyer.Domain.Dtos.Shared;

// After (for code using both)
using PocketLawyer.Domain.Dtos.Employment;
using PocketLawyer.Domain.Dtos.Rental;
using PocketLawyer.Domain.Dtos.Shared;
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Navigation Link Bidirectionality
*For any* pair of contract type pages (employment and rental), each page should contain a navigation link to the other page.
**Validates: Requirements 1.2**

### Property 2: Content Preservation from Original
*For any* content section in the original index.html that is marked for reuse, that content should appear in at least one of the new dedicated pages.
**Validates: Requirements 1.3**

### Property 3: Styling Consistency Across Pages
*For any* CSS color variable or class used across multiple pages, the color values should be identical across all pages.
**Validates: Requirements 2.2, 2.3**

### Property 4: Bootstrap Class Validity
*For any* HTML element with Bootstrap classes, the classes should be valid Bootstrap 5 classes.
**Validates: Requirements 2.4**

### Property 5: DTO Folder Structure Completeness
*For all* DTO organization folders (Employment, Rental, Shared), each folder should exist within Domain/Dtos/ after reorganization.
**Validates: Requirements 3.1, 3.2, 3.3**

### Property 6: DTO File Location Correctness
*For any* DTO file, its location should match its categorization (Employment DTOs in Employment/, Rental DTOs in Rental/, Shared DTOs in Shared/).
**Validates: Requirements 3.4, 3.5, 3.6**

### Property 7: Namespace-Location Consistency
*For any* moved DTO file, its namespace declaration should match its folder location (e.g., files in Domain/Dtos/Employment/ should have namespace PocketLawyer.Domain.Dtos.Employment).
**Validates: Requirements 3.7, 8.1**

### Property 8: Using Statement Correctness
*For any* C# file that references DTOs, its using statements should include the correct namespaces for all referenced DTO types.
**Validates: Requirements 3.8, 8.2**

### Property 9: Pattern-Based File Identification
*For any* file pattern (TASK_*.md, test-*.html, verify_*.csx, Restructuring/*), all files matching that pattern should be identified in the cleanup list.
**Validates: Requirements 4.1, 4.2, 4.3, 4.4, 4.5**

### Property 10: Cleanup List Completeness
*For any* file identified for deletion, the cleanup list should contain that file's path and a reason for deletion.
**Validates: Requirements 4.7**

### Property 11: Cleanup List Categorization
*For any* file in the cleanup list, it should have a category assigned (documentation, test files, deprecated code, or scripts).
**Validates: Requirements 4.8**

### Property 12: Legacy Documentation Relocation
*For any* documentation file that should be archived, it should exist in docs/legacy/ after reorganization.
**Validates: Requirements 6.2, 6.3, 6.4**

### Property 13: README Content Completeness
*For the* new README.md file, it should contain sections for project overview, architecture, setup instructions, and usage guide.
**Validates: Requirements 6.5, 6.6**

### Property 14: File Deletion Completeness
*For any* file in the cleanup list, it should not exist in the repository after deletion (unless it was skipped due to being referenced).
**Validates: Requirements 7.1**

### Property 15: Production Code Reference Safety
*For any* file that is deleted, no production code file should contain a reference to that deleted file.
**Validates: Requirements 7.2**

### Property 16: Deletion Log Completeness
*For any* file that is deleted, the deletion log should contain an entry with that file's path.
**Validates: Requirements 7.3**

### Property 17: Referenced File Preservation
*For any* file in the cleanup list that is referenced by production code, it should still exist after the deletion process and should appear in a "skipped" report.
**Validates: Requirements 7.4**

### Property 18: Compilation Error Reporting
*For any* compilation error encountered, the error report should include the file location and a description of the error.
**Validates: Requirements 8.5**

### Property 19: Build Error Reporting
*For any* build or test failure, the system should report the failure with detailed information.
**Validates: Requirements 9.3, 9.5**


## Error Handling

### File Operation Errors

**File Not Found**
- **Scenario**: Expected file does not exist during move/delete operations
- **Handling**: Log warning, skip operation, continue with remaining files
- **User Feedback**: Include in operation report with "File not found" status

**Permission Denied**
- **Scenario**: Insufficient permissions to move/delete files
- **Handling**: Log error, skip operation, continue with remaining files
- **User Feedback**: Include in operation report with "Permission denied" status

**File In Use**
- **Scenario**: File is locked by another process
- **Handling**: Log warning, skip operation, retry once after delay
- **User Feedback**: Include in operation report with "File in use" status

### Compilation Errors

**Namespace Mismatch**
- **Scenario**: Namespace doesn't match file location after move
- **Handling**: Attempt automatic fix, report if unable to resolve
- **User Feedback**: Detailed error with file path and expected namespace

**Missing Using Statement**
- **Scenario**: File references DTO but lacks correct using statement
- **Handling**: Attempt automatic fix by adding required using statements
- **User Feedback**: Report added using statements in operation log

**Build Failure**
- **Scenario**: Project fails to compile after reorganization
- **Handling**: Halt operation, report all compilation errors
- **User Feedback**: Detailed error report with file locations and error messages

### Git History Errors

**Git Not Available**
- **Scenario**: Git command not found or repository not initialized
- **Handling**: Skip git history search, proceed with current files only
- **User Feedback**: Warning that git history search was skipped

**Git History Parse Error**
- **Scenario**: Unable to parse git log output
- **Handling**: Log error, skip git history search
- **User Feedback**: Warning with error details

### Validation Errors

**Invalid HTML Structure**
- **Scenario**: HTML file cannot be parsed
- **Handling**: Log error, skip validation for that file
- **User Feedback**: Warning with file path and parse error

**Missing Required Content**
- **Scenario**: Required content section not found in source file
- **Handling**: Log warning, create placeholder content
- **User Feedback**: Warning that placeholder content was used


## Testing Strategy

### Dual Testing Approach

This project requires both unit tests and property-based tests to ensure comprehensive coverage:

**Unit Tests**: Verify specific examples, edge cases, and error conditions
- Test specific file operations (create, move, delete)
- Test specific namespace transformations
- Test error handling scenarios
- Test HTML parsing for specific content

**Property Tests**: Verify universal properties across all inputs
- Test that all files matching a pattern are identified
- Test that all moved files have correct namespaces
- Test that all using statements are updated correctly
- Test that all cleanup list entries have required fields

Together, these approaches provide comprehensive coverage where unit tests catch concrete bugs and property tests verify general correctness.

### Property-Based Testing Configuration

**Testing Library**: FsCheck (for C# property-based testing)

**Test Configuration**:
- Minimum 100 iterations per property test
- Each property test references its design document property
- Tag format: **Feature: repository-production-readiness, Property {number}: {property_text}**

**Property Test Examples**:

```csharp
// Property 7: Namespace-Location Consistency
[Property(MaxTest = 100)]
[Tag("Feature: repository-production-readiness, Property 7: Namespace-Location Consistency")]
public Property MovedDtoFiles_ShouldHaveMatchingNamespaces()
{
    return Prop.ForAll(
        ArbitraryDtoFile(),
        dtoFile => {
            var expectedNamespace = GetNamespaceFromPath(dtoFile.Path);
            var actualNamespace = ParseNamespace(dtoFile.Content);
            return actualNamespace == expectedNamespace;
        });
}

// Property 9: Pattern-Based File Identification
[Property(MaxTest = 100)]
[Tag("Feature: repository-production-readiness, Property 9: Pattern-Based File Identification")]
public Property FilePatterns_ShouldIdentifyAllMatchingFiles()
{
    return Prop.ForAll(
        ArbitraryFilePattern(),
        pattern => {
            var identifiedFiles = IdentifyFilesMatchingPattern(pattern);
            var actualFiles = GetAllFilesMatchingPattern(pattern);
            return identifiedFiles.SetEquals(actualFiles);
        });
}
```

### Unit Testing Strategy

**File Operations**:
- Test creating employment.html with expected structure
- Test creating rental.html with expected structure
- Test moving specific DTO files to correct folders
- Test deleting specific files from cleanup list

**Namespace Updates**:
- Test updating namespace in EmploymentBasicAnalysisDto.cs
- Test updating namespace in RentalPremiumAnalysisDto.cs
- Test adding using statements to files referencing moved DTOs

**Error Handling**:
- Test handling of missing files
- Test handling of permission denied errors
- Test handling of compilation errors
- Test handling of git command failures

**Content Validation**:
- Test extracting navigation links from HTML
- Test extracting content sections from index.html
- Test validating Bootstrap classes
- Test parsing CSS color values

### Integration Testing

**End-to-End Workflow**:
1. Create test repository with sample files
2. Run full reorganization process
3. Verify all files are in correct locations
4. Verify all namespaces are updated
5. Verify project compiles successfully
6. Verify all tests pass

**Build Verification**:
- Test that `dotnet build` succeeds after reorganization
- Test that `dotnet test` succeeds after reorganization
- Test that no compilation errors are present

### Test Data

**Sample Files**:
- Sample DTO files with various namespace patterns
- Sample HTML files with various content structures
- Sample CSS files with various color definitions
- Sample markdown files for documentation testing

**Test Repository Structure**:
```
TestRepo/
  Domain/
    Dtos/
      EmploymentBasicAnalysisDto.cs
      RentalPremiumAnalysisDto.cs
      IAnalysisDto.cs
  wwwroot/
    index.html
    test-example.html
  docs/
    API_DOCUMENTATION.md
  TASK_1.md
  verify_test.csx
```

### Validation Criteria

**Success Criteria**:
- All property tests pass (100 iterations each)
- All unit tests pass
- Integration test completes without errors
- Project builds successfully in Release configuration
- All existing tests continue to pass

**Failure Criteria**:
- Any property test fails
- Any unit test fails
- Compilation errors after reorganization
- Existing tests fail after reorganization
- Files are in incorrect locations
