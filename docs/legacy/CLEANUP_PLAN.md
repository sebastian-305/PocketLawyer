# Repository Cleanup Plan
**Generated:** $(Get-Date)
**Purpose:** Comprehensive analysis and cleanup plan for production readiness

## Executive Summary

This document provides a complete analysis of files and code identified for cleanup in the Klausel Klara repository. The cleanup is organized into categories with specific recommendations for each item.

**Total Items Identified:** 67 files/folders + 4 deprecated code classes

---

## Category 1: Task Documentation Files (Root Directory)

**Location:** Root directory
**Pattern:** TASK_*.md
**Action:** Move to docs/legacy/
**Reason:** Development progress documentation, not needed for production

### Files Identified (25 files):

1. `TASK_10.10_COMPLETION_SUMMARY.md` - Development task completion documentation
2. `TASK_10.11_COMPLETION_SUMMARY.md` - Development task completion documentation
3. `TASK_10.12_COMPLETION_SUMMARY.md` - Development task completion documentation
4. `TASK_10.7_COMPLETION_SUMMARY.md` - Development task completion documentation
5. `TASK_10.8_COMPLETION_SUMMARY.md` - Development task completion documentation
6. `TASK_10.9_COMPLETION_SUMMARY.md` - Development task completion documentation
7. `TASK_11_PHASE3_CHECKPOINT_SUMMARY.md` - Development phase checkpoint documentation
8. `TASK_12.1_COMPLETION_SUMMARY.md` - Development task completion documentation
9. `TASK_12.3_COMPLETION_SUMMARY.md` - Development task completion documentation
10. `TASK_12.4_COMPLETION_SUMMARY.md` - Development task completion documentation
11. `TASK_12.5_COMPLETION_SUMMARY.md` - Development task completion documentation
12. `TASK_13_PHASE4_CHECKPOINT_SUMMARY.md` - Development phase checkpoint documentation
13. `TASK_15_FINAL_VALIDATION_SUMMARY.md` - Development validation documentation
14. `TASK_5_COMPLETION_SUMMARY.md` - Development task completion documentation
15. `TASK_8.1_ANALYSIS.md` - Development task analysis documentation
16. `TASK_8.1_SUMMARY.md` - Development task summary documentation
17. `TASK_8.2_COMPLETION_SUMMARY.md` - Development task completion documentation
18. `TASK_8.3_COMPLETION_SUMMARY.md` - Development task completion documentation
19. `TASK_9_COMPLETION_SUMMARY.md` - Development task completion documentation

---

## Category 2: Analysis and Schema Documentation (Root Directory)

**Location:** Root directory
**Pattern:** Various .md files
**Action:** Move to docs/legacy/
**Reason:** Development analysis documentation, not needed for production

### Files Identified (4 files):

1. `FILE_OVERVIEW.md` - Development file structure documentation
2. `JSON_SCHEMA_DEFINITION_ANALYSIS.md` - Schema analysis documentation
3. `SCHEMA_MIGRATION_OVERVIEW.md` - Schema migration documentation
4. `SCHEMA_MIGRATION_README.md` - Schema migration documentation

---

## Category 3: Verification Scripts (Root Directory)

**Location:** Root directory
**Pattern:** verify_*.csx, test-*.csx, *.ps1
**Action:** Delete
**Reason:** Development testing scripts, not needed for production

### Files Identified (13 files):

1. `verify_clause_analysis_schema.csx` - Schema verification script
2. `verify_migration.csx` - Migration verification script
3. `verify_premium_section_schema.csx` - Schema verification script
4. `verify_premium_summary_schema.csx` - Schema verification script
5. `verify_rental_clause_analysis_schema.csx` - Schema verification script
6. `verify_rental_conditions_schema.csx` - Schema verification script
7. `verify_rental_contract_conditions_schema.csx` - Schema verification script
8. `verify_rental_premium_summary_schema.csx` - Schema verification script
9. `verify_rental_summary_schema.csx` - Schema verification script
10. `verify_summary_schema.csx` - Schema verification script
11. `test-schema-gen.csx` - Schema generation test script
12. `test-serialization.csx` - Serialization test script
13. `update-test-properties.ps1` - PowerShell test update script

---

## Category 4: Test HTML Files and Documentation (wwwroot)

**Location:** wwwroot/
**Pattern:** test-*.html, test-*.mjs, *_README.md, *_SUMMARY.md
**Action:** Delete
**Reason:** Development test files and documentation, not needed for production

### Files Identified (14 files):

1. `test-dynamic-renderer.html` - Dynamic renderer test page
2. `test-dynamic-renderer.mjs` - Dynamic renderer test module
3. `test-render-generic.html` - Generic render test page
4. `test-render-logic.mjs` - Render logic test module
5. `test-schema-fetch.html` - Schema fetch test page
6. `test-tier-accordion.html` - Tier accordion test page
7. `test-tier-accordion.mjs` - Tier accordion test module
8. `DYNAMIC_RENDERER_TEST_SUMMARY.md` - Test summary documentation
9. `RENDER_GENERIC_README.md` - Render generic documentation
10. `SCHEMA_FETCH_README.md` - Schema fetch documentation
11. `TASK_15_IMPLEMENTATION_SUMMARY.md` - Task implementation documentation
12. `TASK_15.4_SUMMARY.md` - Task summary documentation
13. `TIER_ACCORDION_DEMO.md` - Tier accordion demo documentation
14. `TIER_ACCORDION_README.md` - Tier accordion documentation

---

## Category 5: Restructuring Folder

**Location:** Restructuring/
**Pattern:** Entire folder
**Action:** Delete entire folder
**Reason:** Old/experimental code from restructuring phase, no longer needed

### Contents Identified:

1. `Restructuring/BasicAnalysisDTO.cs` - Old DTO file
2. `Restructuring/Schemas/` - Old schema folder (empty subdirectories)
3. `Restructuring/Schemas/RentSchemas/` - Old rent schema folder

**Note:** Verify no production code references before deletion

---

## Category 6: Legacy Documentation (docs/)

**Location:** docs/
**Pattern:** Various development documentation files
**Action:** Move to docs/legacy/
**Reason:** Development documentation to be archived

### Files Identified (8 files):

1. `AGENT_REFACTORING_SUMMARY.md` - Agent refactoring documentation
2. `KIRO_AENDERUNGEN.md` - Kiro changes documentation (German)
3. `KOLLEKTIVVERTRAG_FIELD_DOCUMENTATION.md` - Collective agreement field documentation
4. `KV_CONTENT_CONFIGURATION.md` - KV content configuration documentation
5. `KV_DETECTION_VALIDATION.md` - KV detection validation documentation
6. `REFACTORING_SUMMARY.md` - Refactoring summary documentation
7. `SCHEMA_GENERATOR_REFACTORING.md` - Schema generator refactoring documentation
8. `TEST_FAILURES_ANALYSIS.md` - Test failures analysis documentation

### Files to Keep in docs/:

1. `API_DOCUMENTATION.md` - Production API documentation
2. `CONFIGURATION.md` - Production configuration documentation
3. `DesignPatterns_de.md` - Design patterns documentation
4. `diagrams/` - All diagram files (production documentation)
5. `Presentation Klausel Klara 1.pptx` - Project presentation

---

## Category 7: Deprecated Code (C# Classes)

**Location:** Various
**Pattern:** Classes marked with [Obsolete] attribute or deprecation comments
**Action:** Review for deletion after verifying no production usage
**Reason:** Deprecated code that should be removed

### Classes Identified (4 classes):

1. **Infrastructure/PromptBuilder_deprecated1.cs**
   - Class: `PromptBuilder_Deprecated`
   - Marked: `[Obsolete("This class is deprecated. Use PromptProvider instead.")]`
   - Reason: Legacy prompt builder, replaced by PromptProvider
   - Action: Delete after verifying no references

2. **Domain/PromptBuilder.cs**
   - Class: `PromptBuilder`
   - Marked: `[Obsolete("This class is deprecated. Use PromptProvider instead.")]`
   - Reason: Legacy prompt builder, replaced by PromptProvider
   - Action: Delete after verifying no references

3. **Infrastructure/AiServices/Gemini/GeminiService.cs**
   - Class: `GeminiService`
   - Marked: `[Obsolete("This service is deprecated. Use NebiusService instead.")]`
   - Reason: Legacy AI service, replaced by NebiusService
   - Action: Delete after verifying no references

4. **Infrastructure/AiServices/Claude/ClaudeService.cs**
   - Class: `ClaudeService`
   - Marked: `[Obsolete("This service is deprecated. Use NebiusService instead.")]`
   - Reason: Legacy AI service, replaced by NebiusService
   - Action: Delete after verifying no references

### Deprecated Methods:

1. **Application/Interfaces/IAiClient.cs**
   - Method: `SendPrompt(string prompt, object? outputSchema)`
   - Marked: `[Obsolete("Use SendPromptWithMetadata instead...")]`
   - Reason: Replaced by SendPromptWithMetadata for token usage tracking
   - Action: Keep interface for now (implementations may still use it)

---

## Folder-by-Folder Analysis

### Application Folder
**Status:** ✅ Clean
**Findings:** No cleanup needed. All files are production code.
**Recommendation:** No action required.

### Domain Folder
**Status:** ⚠️ Contains deprecated code
**Findings:** 
- `Domain/PromptBuilder.cs` - Deprecated class marked with [Obsolete]
- `Domain/Dtos/` - All DTOs need reorganization (separate task)
**Recommendation:** 
- Delete PromptBuilder.cs after verifying no production references
- Reorganize DTOs into Employment/, Rental/, Shared/ subfolders (Task 3)

### Infrastructure Folder
**Status:** ⚠️ Contains deprecated code
**Findings:**
- `Infrastructure/PromptBuilder_deprecated1.cs` - Deprecated class
- `Infrastructure/AiServices/Gemini/GeminiService.cs` - Deprecated service
- `Infrastructure/AiServices/Claude/ClaudeService.cs` - Deprecated service
**Recommendation:** 
- Delete all three deprecated files after verifying no production references
- Keep all other infrastructure code

### Web Folder
**Status:** ✅ Clean
**Findings:** Only contains Program.cs and PocketController.cs
**Recommendation:** No action required.

### wwwroot Folder
**Status:** ⚠️ Contains test files and documentation
**Findings:**
- 7 test HTML/JS files
- 7 documentation markdown files
**Recommendation:** 
- Delete all test-*.html and test-*.mjs files
- Delete all *_README.md and *_SUMMARY.md files
- Keep production files: index.html, main.js, network.js, render.js, *.css, Media/

### docs Folder
**Status:** ⚠️ Contains legacy documentation
**Findings:**
- 8 development documentation files to archive
- 4 production documentation files to keep
- diagrams/ folder to keep
**Recommendation:**
- Create docs/legacy/ folder
- Move 8 development docs to docs/legacy/
- Keep API_DOCUMENTATION.md, CONFIGURATION.md, DesignPatterns_de.md, diagrams/

### data Folder
**Status:** ✅ Clean (production data)
**Findings:** Contains context files, KV data, sample contracts, tessdata
**Recommendation:** No action required. All files are needed for production.

### Restructuring Folder
**Status:** ❌ Entire folder for deletion
**Findings:** Contains old/experimental code from restructuring phase
**Recommendation:** Delete entire folder after verifying no production references

### Root Directory
**Status:** ⚠️ Contains many development files
**Findings:**
- 19 TASK_*.md files
- 4 analysis/schema documentation files
- 13 verification/test scripts
**Recommendation:**
- Move 23 markdown files to docs/legacy/
- Delete 13 script files

---

## Summary Statistics

| Category | Count | Action |
|----------|-------|--------|
| Task Documentation Files | 19 | Move to docs/legacy/ |
| Analysis Documentation Files | 4 | Move to docs/legacy/ |
| Verification Scripts | 13 | Delete |
| Test HTML/JS Files | 7 | Delete |
| wwwroot Documentation Files | 7 | Delete |
| Legacy Documentation (docs/) | 8 | Move to docs/legacy/ |
| Restructuring Folder | 1 folder | Delete entire folder |
| Deprecated C# Classes | 4 | Delete after verification |
| **Total Files** | **63 files + 1 folder** | Various actions |

---

## Cleanup Execution Order

1. **Phase 1: Create Archive Structure**
   - Create docs/legacy/ folder
   - Backup current README.md to docs/legacy/README_old.md

2. **Phase 2: Move Documentation**
   - Move 19 TASK_*.md files from root to docs/legacy/
   - Move 4 analysis .md files from root to docs/legacy/
   - Move 8 legacy docs from docs/ to docs/legacy/
   - Move 7 wwwroot documentation files to docs/legacy/

3. **Phase 3: Delete Scripts**
   - Delete 13 verification/test scripts from root

4. **Phase 4: Delete Test Files**
   - Delete 7 test HTML/JS files from wwwroot/

5. **Phase 5: Verify and Delete Deprecated Code**
   - Scan codebase for references to deprecated classes
   - Delete deprecated C# files if no production references found
   - Report any files that cannot be deleted due to references

6. **Phase 6: Delete Restructuring Folder**
   - Verify no production code references Restructuring/ folder
   - Delete entire Restructuring/ folder

7. **Phase 7: Verification**
   - Run dotnet build to ensure compilation succeeds
   - Run dotnet test to ensure all tests pass
   - Generate deletion log

---

## Risk Assessment

### Low Risk (Safe to Delete)
- All TASK_*.md files (documentation only)
- All verify_*.csx and test-*.csx scripts (development tools)
- All test-*.html and test-*.mjs files (development tests)
- All *_README.md and *_SUMMARY.md in wwwroot (documentation)

### Medium Risk (Verify Before Deletion)
- Restructuring/ folder (may have references)
- Deprecated C# classes (may still be referenced)

### No Risk (Keep)
- All files in Application/ (production code)
- All files in Web/ (production code)
- Most files in Infrastructure/ (except deprecated classes)
- All files in data/ (production data)
- Production documentation in docs/
- Production files in wwwroot/ (index.html, *.js, *.css, Media/)

---

## Validation Checklist

After cleanup execution:

- [ ] All TASK_*.md files moved to docs/legacy/
- [ ] All analysis .md files moved to docs/legacy/
- [ ] All verification scripts deleted from root
- [ ] All test files deleted from wwwroot/
- [ ] All wwwroot documentation moved to docs/legacy/
- [ ] All legacy docs moved to docs/legacy/
- [ ] Deprecated C# classes deleted (or reported as still in use)
- [ ] Restructuring/ folder deleted (or reported as still in use)
- [ ] dotnet build succeeds
- [ ] dotnet test passes
- [ ] Deletion log generated
- [ ] No broken references in production code

---

## Notes

- This plan validates Requirements 4.1-4.8 (Code Cleanup Identification)
- This plan validates Requirements 5.1-5.8 (Repository Folder Review)
- Execution of this plan will be performed in subsequent tasks
- All deletions should be logged for audit purposes
- Any files that cannot be deleted due to production references should be reported
