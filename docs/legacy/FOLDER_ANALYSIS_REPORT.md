# Folder-by-Folder Analysis Report
**Generated:** $(Get-Date)
**Purpose:** Detailed analysis of each repository folder for cleanup opportunities

---

## Root Directory Analysis

**Path:** `/`
**Purpose:** Project root containing solution files and configuration
**Status:** ⚠️ Needs Cleanup

### Current Contents:
- Solution files: `PocketLawyer.sln`, `PocketLawyer.csproj`
- Configuration: `appsettings.json`, `appsettings.Development.json`, `.editorconfig`, `.gitignore`, `.gitattributes`
- Documentation: `README.md`
- **Development files (to clean):**
  - 19 TASK_*.md files
  - 4 analysis/schema documentation files
  - 13 verification/test scripts (.csx, .ps1)
  - 1 Python file: `locustfile.py` (load testing)

### Cleanup Recommendations:
1. **Move to docs/legacy/:**
   - All TASK_*.md files (19 files)
   - FILE_OVERVIEW.md
   - JSON_SCHEMA_DEFINITION_ANALYSIS.md
   - SCHEMA_MIGRATION_OVERVIEW.md
   - SCHEMA_MIGRATION_README.md

2. **Delete:**
   - All verify_*.csx files (10 files)
   - test-schema-gen.csx
   - test-serialization.csx
   - update-test-properties.ps1

3. **Keep:**
   - All solution and project files
   - All configuration files
   - README.md (will be replaced with new version)
   - locustfile.py (load testing tool - may be useful)

### Production Readiness: 
After cleanup, root should contain only:
- Solution/project files
- Configuration files
- README.md
- .gitignore, .gitattributes, .editorconfig
- locustfile.py (optional)

---

## Application Folder Analysis

**Path:** `/Application`
**Purpose:** Application layer containing use cases, agents, and interfaces
**Status:** ✅ Clean

### Current Structure:
```
Application/
├── Agents/
│   ├── ContractAnalysisAgentBase.cs
│   ├── EmploymentContractAgent.cs
│   └── RentalContractAgent.cs
├── Exceptions/
│   ├── AiServiceException.cs
│   ├── AnalysisException.cs
│   ├── ContractAnalysisException.cs
│   ├── ContractValidationException.cs
│   ├── FileValidationException.cs
│   ├── PromptNotFoundException.cs
│   ├── SchemaNotFoundException.cs
│   ├── SchemaValidationException.cs
│   └── UnsupportedContractTypeException.cs
├── Interfaces/
│   ├── IAiClient.cs (contains 1 deprecated method)
│   ├── IContextFileReader.cs
│   ├── IContextProvider.cs
│   ├── IContractAnalysisAgent.cs
│   ├── IContractAnalyzerAgent.cs
│   ├── IContractAnalyzerUseCase.cs
│   ├── IFileDecoder.cs
│   ├── IFileDecoderService.cs
│   ├── IFileValidatorService.cs
│   ├── IGeminiContextManager.cs
│   ├── IKollektivvertragService.cs
│   ├── IKvDetectionService.cs
│   ├── IParameterSerializer.cs
│   ├── IResponseLogger.cs
│   ├── ISchemaProvider.cs
│   ├── ISchemaValidator.cs
│   └── IWebSocketAnalysisService.cs
├── Models/
│   ├── AiResponse.cs
│   └── InitializationResult.cs
└── UseCases/
    └── DetectKvUseCase.cs
```

### Findings:
- All files are production code
- Well-organized by responsibility
- One deprecated method in IAiClient.cs (SendPrompt) - but interface should be kept

### Cleanup Recommendations:
**None** - All files are production-ready

### Notes:
- IAiClient.SendPrompt method is marked obsolete but interface must remain for backward compatibility
- Clean architecture with clear separation of concerns
- No unused or deprecated files

### Production Readiness: ✅ Ready

---

## Domain Folder Analysis

**Path:** `/Domain`
**Purpose:** Domain layer containing business entities and DTOs
**Status:** ⚠️ Contains deprecated code + needs reorganization

### Current Structure:
```
Domain/
├── Dtos/
│   ├── AnalysisDto.cs
│   ├── ContractValidationDto.cs
│   ├── EmploymentBasicAnalysisDto.cs
│   ├── EmploymentContractAnalysisDto.cs
│   ├── EmploymentPremiumAnalysisDto.cs
│   ├── FileContentDto.cs
│   ├── IAnalysisDto.cs
│   ├── IValidationDto.cs
│   ├── KvDetectionAnswer.cs
│   ├── KvDetectionResult.cs
│   ├── RentalBasicAnalysisDto.cs
│   ├── RentalPremiumAnalysisDto.cs
│   ├── RentContractAnalysisDto.cs
│   └── WebSocketFileDto.cs
├── AgentType.cs
├── PromptBuilder.cs (DEPRECATED)
└── PromptProvider.cs
```

### Findings:
1. **Deprecated Code:**
   - `PromptBuilder.cs` - Marked with [Obsolete], replaced by PromptProvider

2. **Reorganization Needed:**
   - All DTOs are in flat structure
   - Should be organized into Employment/, Rental/, Shared/ subfolders

### Cleanup Recommendations:
1. **Delete after verification:**
   - PromptBuilder.cs (verify no production references first)

2. **Reorganize (Task 3):**
   - Create Domain/Dtos/Employment/ folder
   - Create Domain/Dtos/Rental/ folder
   - Create Domain/Dtos/Shared/ folder
   - Move DTOs to appropriate subfolders
   - Update namespaces

### Target Structure:
```
Domain/
├── Dtos/
│   ├── Employment/
│   │   ├── EmploymentBasicAnalysisDto.cs
│   │   ├── EmploymentContractAnalysisDto.cs
│   │   └── EmploymentPremiumAnalysisDto.cs
│   ├── Rental/
│   │   ├── RentalBasicAnalysisDto.cs
│   │   ├── RentalPremiumAnalysisDto.cs
│   │   └── RentContractAnalysisDto.cs
│   └── Shared/
│       ├── AnalysisDto.cs
│       ├── ContractValidationDto.cs
│       ├── FileContentDto.cs
│       ├── IAnalysisDto.cs
│       ├── IValidationDto.cs
│       ├── KvDetectionAnswer.cs
│       ├── KvDetectionResult.cs
│       └── WebSocketFileDto.cs
├── AgentType.cs
└── PromptProvider.cs
```

### Production Readiness: ⚠️ Needs cleanup and reorganization

---

## Infrastructure Folder Analysis

**Path:** `/Infrastructure`
**Purpose:** Infrastructure layer containing external service implementations
**Status:** ⚠️ Contains deprecated code

### Current Structure:
```
Infrastructure/
├── AiServices/
│   ├── Claude/
│   │   └── ClaudeService.cs (DEPRECATED)
│   ├── Gemini/
│   │   └── GeminiService.cs (DEPRECATED)
│   ├── Nebius/
│   │   └── (production AI service)
│   ├── SchemaNameDetectionService.cs
│   └── TimedAiClient.cs
├── Context/
│   ├── ContextDirectoryInitializer.cs
│   ├── ContextFileReader.cs
│   ├── ContextProvider.cs
│   └── ContextStepAttribute.cs
├── FileDecoding/
│   ├── Strategies/
│   └── FileDecoderService.cs
├── Logging/
│   ├── LoggingAiClientDecorator.cs
│   ├── MethodTracingActionFilter.cs
│   └── ParameterSerializer.cs
├── SchemaProviders/
│   ├── Employment/
│   ├── Rental/
│   ├── SchemaAttributes.cs
│   ├── SchemaComparator.cs
│   ├── SchemaExtensions.cs
│   ├── SchemaGenerator.cs
│   └── SchemaProvider.cs
├── DefaultPaths.cs
├── ExceptionTranslatorMiddleware.cs
├── FileValidatorService.cs
├── KvAiDetectionService.cs
├── KvFileService.cs
├── NoLoggingHttpClientBuilderFilter.cs
├── PromptBuilder_deprecated1.cs (DEPRECATED)
├── SchemaValidator.cs
├── WebSocketAgentAnalysisService.cs
├── WebSocketAnalysisHandler.cs
└── WebSocketHelpers.cs
```

### Findings:
**Deprecated Code (3 files):**
1. `PromptBuilder_deprecated1.cs` - Legacy prompt builder
2. `AiServices/Claude/ClaudeService.cs` - Legacy AI service
3. `AiServices/Gemini/GeminiService.cs` - Legacy AI service

All marked with [Obsolete] attributes and replaced by newer implementations.

### Cleanup Recommendations:
1. **Delete after verification:**
   - Infrastructure/PromptBuilder_deprecated1.cs
   - Infrastructure/AiServices/Claude/ClaudeService.cs
   - Infrastructure/AiServices/Gemini/GeminiService.cs
   - (Verify no production code references these files first)

2. **Consider deleting empty folders after cleanup:**
   - Infrastructure/AiServices/Claude/ (if only contains ClaudeService.cs)
   - Infrastructure/AiServices/Gemini/ (if only contains GeminiService.cs)

### Production Readiness: ⚠️ Needs deprecated code removal

---

## Web Folder Analysis

**Path:** `/Web`
**Purpose:** Web layer containing controllers and program entry point
**Status:** ✅ Clean

### Current Structure:
```
Web/
├── PocketController.cs
└── Program.cs
```

### Findings:
- Minimal, clean structure
- Only contains essential web layer files
- No deprecated or development files

### Cleanup Recommendations:
**None** - All files are production-ready

### Production Readiness: ✅ Ready

---

## wwwroot Folder Analysis

**Path:** `/wwwroot`
**Purpose:** Static web files (HTML, CSS, JavaScript, images)
**Status:** ⚠️ Contains test files and documentation

### Current Structure:
```
wwwroot/
├── Media/
│   ├── favicon.ico
│   ├── Frau_am_Laptop.jpg
│   ├── Frau_mit_Verträgen.jpg
│   ├── klara_full.png
│   ├── klara.png
│   └── Mann_mit_Notizbuch.jpg
├── index.html
├── main.js
├── network.js
├── render.js
├── praes.css
├── print.css
├── styles.css
├── test-dynamic-renderer.html (DELETE)
├── test-dynamic-renderer.mjs (DELETE)
├── test-render-generic.html (DELETE)
├── test-render-logic.mjs (DELETE)
├── test-schema-fetch.html (DELETE)
├── test-tier-accordion.html (DELETE)
├── test-tier-accordion.mjs (DELETE)
├── DYNAMIC_RENDERER_TEST_SUMMARY.md (DELETE)
├── RENDER_GENERIC_README.md (DELETE)
├── SCHEMA_FETCH_README.md (DELETE)
├── TASK_15_IMPLEMENTATION_SUMMARY.md (DELETE)
├── TASK_15.4_SUMMARY.md (DELETE)
├── TIER_ACCORDION_DEMO.md (DELETE)
└── TIER_ACCORDION_README.md (DELETE)
```

### Findings:
1. **Test Files (7 files):** test-*.html and test-*.mjs files for development testing
2. **Documentation (7 files):** *_README.md and *_SUMMARY.md files
3. **Production Files:** index.html, *.js, *.css, Media/ folder

### Cleanup Recommendations:
1. **Delete:**
   - All test-*.html files (4 files)
   - All test-*.mjs files (3 files)
   - All *_README.md files (4 files)
   - All *_SUMMARY.md files (3 files)

2. **Keep:**
   - index.html (will be modified in Task 7)
   - All .js files (main.js, network.js, render.js)
   - All .css files (styles.css, praes.css, print.css)
   - Media/ folder with all images

3. **Future additions (Task 7-8):**
   - employment.html
   - rental.html
   - js/ folder with organized modules

### Target Structure:
```
wwwroot/
├── css/
│   ├── styles.css
│   ├── praes.css
│   └── print.css
├── js/
│   ├── main.js
│   ├── employment.js
│   ├── rental.js
│   ├── network.js
│   └── render.js
├── Media/
│   └── (all existing images)
├── index.html
├── employment.html
└── rental.html
```

### Production Readiness: ⚠️ Needs cleanup and reorganization

---

## docs Folder Analysis

**Path:** `/docs`
**Purpose:** Project documentation
**Status:** ⚠️ Contains legacy documentation

### Current Structure:
```
docs/
├── diagrams/
│   ├── Architecture-Components.png
│   ├── basic-par.png
│   ├── Data-Flow.png
│   ├── diagrams_images.md
│   ├── diagrams.md
│   ├── Domainmodell.png
│   ├── plan1.png
│   ├── Plant_UML.png
│   ├── prem-par.png
│   ├── SequenceFull.png
│   ├── SequenceOnionLayer.png
│   ├── UI-Flow.png
│   └── UML.png
├── AGENT_REFACTORING_SUMMARY.md (MOVE TO LEGACY)
├── API_DOCUMENTATION.md (KEEP)
├── CONFIGURATION.md (KEEP)
├── DesignPatterns_de.md (KEEP)
├── KIRO_AENDERUNGEN.md (MOVE TO LEGACY)
├── KOLLEKTIVVERTRAG_FIELD_DOCUMENTATION.md (MOVE TO LEGACY)
├── KV_CONTENT_CONFIGURATION.md (MOVE TO LEGACY)
├── KV_DETECTION_VALIDATION.md (MOVE TO LEGACY)
├── Presentation Klausel Klara 1.pptx (KEEP)
├── REFACTORING_SUMMARY.md (MOVE TO LEGACY)
├── SCHEMA_GENERATOR_REFACTORING.md (MOVE TO LEGACY)
└── TEST_FAILURES_ANALYSIS.md (MOVE TO LEGACY)
```

### Findings:
1. **Production Documentation (4 files + 1 folder + 1 presentation):**
   - API_DOCUMENTATION.md
   - CONFIGURATION.md
   - DesignPatterns_de.md
   - diagrams/ folder
   - Presentation Klausel Klara 1.pptx

2. **Legacy Documentation (8 files):**
   - Development summaries and analysis files
   - Should be archived but not deleted

### Cleanup Recommendations:
1. **Create:** docs/legacy/ folder

2. **Move to docs/legacy/:**
   - AGENT_REFACTORING_SUMMARY.md
   - KIRO_AENDERUNGEN.md
   - KOLLEKTIVVERTRAG_FIELD_DOCUMENTATION.md
   - KV_CONTENT_CONFIGURATION.md
   - KV_DETECTION_VALIDATION.md
   - REFACTORING_SUMMARY.md
   - SCHEMA_GENERATOR_REFACTORING.md
   - TEST_FAILURES_ANALYSIS.md

3. **Keep in docs/:**
   - API_DOCUMENTATION.md
   - CONFIGURATION.md
   - DesignPatterns_de.md
   - diagrams/ (entire folder)
   - Presentation Klausel Klara 1.pptx

### Target Structure:
```
docs/
├── legacy/
│   ├── README_old.md (from root)
│   ├── (8 legacy docs from docs/)
│   ├── (19 TASK_*.md from root)
│   ├── (4 analysis .md from root)
│   └── (7 docs from wwwroot/)
├── diagrams/
│   └── (all existing diagrams)
├── API_DOCUMENTATION.md
├── CONFIGURATION.md
├── DesignPatterns_de.md
└── Presentation Klausel Klara 1.pptx
```

### Production Readiness: ⚠️ Needs reorganization

---

## data Folder Analysis

**Path:** `/data`
**Purpose:** Application data including context files, KV data, and samples
**Status:** ✅ Clean (production data)

### Current Structure:
```
data/
├── Context/
│   ├── employment/
│   ├── Neuer Ordner/
│   ├── Rent/
│   └── rental/
├── Context2/
│   └── (Austrian law text files)
├── KVS/
│   ├── Employment/
│   └── README.md
├── RawContextFiles/
│   ├── Context/
│   ├── KV/
│   ├── OHG/
│   ├── RealEstate_Law/
│   └── (law RTF files + transform.ps1)
├── Responses/
│   └── (sample JSON responses)
├── Samples/
│   └── (sample contract PDFs and test files)
└── tessdata/
    └── (Tesseract OCR language data)
```

### Findings:
- All folders contain production data
- Context files needed for AI analysis
- KV (Kollektivvertrag) data for collective agreement detection
- Sample contracts for testing
- Tesseract data for OCR functionality

### Cleanup Recommendations:
**None** - All data is needed for production

### Notes:
- "Neuer Ordner" (German for "New Folder") could be renamed for consistency
- Consider reviewing Responses/ folder - may contain development test data
- Samples/ folder contains test files but may be useful for demos

### Production Readiness: ✅ Ready (minor naming improvements optional)

---

## Restructuring Folder Analysis

**Path:** `/Restructuring`
**Purpose:** Old/experimental code from restructuring phase
**Status:** ❌ Entire folder for deletion

### Current Structure:
```
Restructuring/
├── Schemas/
│   └── RentSchemas/
└── BasicAnalysisDTO.cs
```

### Findings:
- Contains old DTO file (BasicAnalysisDTO.cs)
- Contains empty schema folders
- Appears to be leftover from code restructuring phase
- No clear production purpose

### Cleanup Recommendations:
1. **Verify:** Scan entire codebase for any references to:
   - `Restructuring/BasicAnalysisDTO.cs`
   - Any files in `Restructuring/Schemas/`

2. **Delete:** Entire Restructuring/ folder if no production references found

3. **Report:** If references found, document them and determine if they should be updated or if files should be kept

### Production Readiness: ❌ Should be deleted

---

## Other Folders (Build/IDE)

### .git/ - ✅ Keep (version control)
### .github/ - ✅ Keep (GitHub workflows/actions)
### .kiro/ - ✅ Keep (Kiro specifications)
### .vs/ - ✅ Keep (Visual Studio settings, in .gitignore)
### .vscode/ - ✅ Keep (VS Code settings)
### bin/ - ✅ Keep (build output, in .gitignore)
### obj/ - ✅ Keep (build intermediate files, in .gitignore)
### coveragereport/ - ✅ Keep (test coverage reports, likely in .gitignore)
### license-report-temp/ - ⚠️ Review (temporary folder, may be deletable)
### logs/ - ✅ Keep (application logs, likely in .gitignore)
### ContractAnalysisBlueprintPoC/ - ⚠️ Review (proof of concept code?)
### Options/ - ✅ Keep (configuration options classes)
### Properties/ - ✅ Keep (project properties)
### PocketLawyer.Tests/ - ✅ Keep (test project)

---

## Summary by Status

### ✅ Clean (No Action Needed):
- Application/
- Web/
- data/ (minor improvements optional)
- .git/, .github/, .kiro/, .vs/, .vscode/
- bin/, obj/, logs/, coveragereport/
- Options/, Properties/, PocketLawyer.Tests/

### ⚠️ Needs Cleanup:
- Root directory (19 TASK files + 4 docs + 13 scripts)
- Domain/ (1 deprecated class + DTO reorganization)
- Infrastructure/ (3 deprecated classes)
- wwwroot/ (7 test files + 7 docs)
- docs/ (8 legacy docs to archive)

### ❌ For Deletion:
- Restructuring/ (entire folder)

### ⚠️ Needs Review:
- license-report-temp/ (temporary folder?)
- ContractAnalysisBlueprintPoC/ (proof of concept?)

---

## Cleanup Priority

1. **High Priority:** Root directory cleanup (most visible)
2. **High Priority:** wwwroot/ cleanup (affects web deployment)
3. **Medium Priority:** docs/ reorganization (documentation structure)
4. **Medium Priority:** Deprecated code deletion (code quality)
5. **Medium Priority:** Restructuring/ deletion (unused code)
6. **Low Priority:** Review license-report-temp/ and ContractAnalysisBlueprintPoC/

---

## Validation Requirements

After cleanup, verify:
- [ ] Root directory contains only essential files
- [ ] No test files in wwwroot/
- [ ] All legacy docs in docs/legacy/
- [ ] No deprecated code in Domain/ or Infrastructure/
- [ ] Restructuring/ folder deleted
- [ ] dotnet build succeeds
- [ ] dotnet test passes
- [ ] No broken file references

---

## Estimated Impact

- **Files to move:** 38 files
- **Files to delete:** 24 files + 1 folder
- **Folders to create:** 1 (docs/legacy/)
- **Deprecated classes to remove:** 4 classes
- **Total cleanup items:** 67+ items

This cleanup will significantly improve repository organization and production readiness.
