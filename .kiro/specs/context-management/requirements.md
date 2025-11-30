# Requirements Document

## Introduction

This document specifies requirements for a context management system that provides step-specific contextual information (such as legal texts) to AI agents during contract analysis. Each analysis step will have its own directory containing relevant context files that are automatically loaded and included in AI prompts.

## Glossary

- **Context Management System**: The infrastructure that manages and loads contextual information for AI agent steps
- **Context Directory**: A file system directory containing context files (e.g., legal texts, reference documents) for a specific analysis step
- **Agent**: A contract analysis agent (e.g., RentalContractAgent, EmploymentContractAgent)
- **Analysis Tier**: Either "basic" or "premium" analysis level
- **Analysis Step**: A specific phase of contract analysis (e.g., "general-info", "rent-conditions", "clause-analysis")
- **Context File**: A text file containing reference information to be included in AI prompts
- **Context Loader**: Component responsible for reading context files from the file system
- **Context Provider**: Service that supplies context content to agents based on the current step

## Requirements

### Requirement 1

**User Story:** As a system administrator, I want to configure a base context directory path in appsettings, so that I can organize all agent context files in a centralized location.

#### Acceptance Criteria

1. WHEN the application starts THEN the Context Management System SHALL read the base context path from appsettings
2. WHEN the base context path is not configured THEN the Context Management System SHALL use a default path of "data/Context"
3. WHEN the base context path does not exist THEN the Context Management System SHALL create the directory structure automatically
4. WHEN the base context path is invalid THEN the Context Management System SHALL throw a configuration exception with a clear error message
5. WHERE the base context path is relative THEN the Context Management System SHALL resolve it relative to the application root directory

### Requirement 2

**User Story:** As a developer, I want a hierarchical directory structure for agent contexts, so that context files are organized by agent, tier, and step.

#### Acceptance Criteria

1. WHEN an agent requests context THEN the Context Management System SHALL look for files in the path "{BaseContextPath}/{AgentName}/{Tier}/{StepName}"
2. WHEN the agent name is "rental" THEN the Context Management System SHALL use the directory "rental"
3. WHEN the agent name is "employment" THEN the Context Management System SHALL use the directory "employment"
4. WHEN the tier is basic THEN the Context Management System SHALL use the directory "basic"
5. WHEN the tier is premium THEN the Context Management System SHALL use the directory "premium"
6. WHEN the step name is provided THEN the Context Management System SHALL use a normalized directory name (lowercase, hyphens)

### Requirement 3

**User Story:** As a developer, I want the context directory structure to be automatically created based on agent step attributes, so that I don't need to manually create directories.

#### Acceptance Criteria

1. WHEN the application starts THEN the Context Management System SHALL scan all agent classes for methods marked with ContextStep attributes
2. WHEN a method has a ContextStep attribute THEN the Context Management System SHALL create a directory for that step
3. WHEN the ContextStep attribute specifies a step name THEN the Context Management System SHALL use that name for the directory
4. WHEN a directory already exists THEN the Context Management System SHALL not modify or recreate it
5. WHEN directory creation fails THEN the Context Management System SHALL log a warning but continue application startup
6. WHERE an agent method is not marked with ContextStep THEN the Context Management System SHALL not create a directory for it

### Requirement 4

**User Story:** As a content manager, I want to place multiple context files in a step directory, so that I can organize context information into logical files.

#### Acceptance Criteria

1. WHEN a step directory contains multiple files THEN the Context Loader SHALL read all files with .txt or .md extensions
2. WHEN reading context files THEN the Context Loader SHALL support only .txt and .md file extensions for performance reasons
3. WHEN multiple files exist THEN the Context Loader SHALL concatenate their contents with clear separators
4. WHEN concatenating files THEN the Context Loader SHALL include the filename as a header for each section
5. WHEN a file cannot be read THEN the Context Loader SHALL log a warning and continue with remaining files
6. WHERE PDF files are needed THEN the content manager SHALL convert them to .txt files before placing them in context directories

### Requirement 5

**User Story:** As a developer, I want agents to automatically receive context for their current step, so that context is seamlessly integrated into AI prompts.

#### Acceptance Criteria

1. WHEN an agent executes a method marked with ContextStep THEN the Context Provider SHALL load context for that step automatically
2. WHEN context files exist for a step THEN the Context Provider SHALL return the concatenated content
3. WHEN no context files exist for a step THEN the Context Provider SHALL return an empty string
4. WHEN context is loaded THEN the Context Provider SHALL cache the content for the duration of the request
5. WHERE context files are modified THEN the Context Provider SHALL reload the content on the next request
6. WHEN a ContextStep attribute specifies a tier THEN the Context Provider SHALL load context from the appropriate tier directory

### Requirement 6

**User Story:** As a developer, I want context to be included in AI prompts with clear formatting, so that the AI can distinguish context from instructions.

#### Acceptance Criteria

1. WHEN context is added to a prompt THEN the Context Management System SHALL include a clear section header
2. WHEN context content is included THEN the Context Management System SHALL use the format "## Reference Context\n\n{content}"
3. WHEN multiple context files are included THEN the Context Management System SHALL separate them with "### {filename}\n\n{content}"
4. WHEN context is empty THEN the Context Management System SHALL not add any context section to the prompt
5. WHERE context contains special characters THEN the Context Management System SHALL preserve the original formatting

### Requirement 7

**User Story:** As a system administrator, I want to monitor context loading through logs, so that I can verify context files are being used correctly.

#### Acceptance Criteria

1. WHEN context files are loaded THEN the Context Loader SHALL log the file paths at Debug level
2. WHEN context is empty for a step THEN the Context Loader SHALL log an informational message
3. WHEN a context file cannot be read THEN the Context Loader SHALL log a warning with the file path and error
4. WHEN context is successfully loaded THEN the Context Loader SHALL log the total character count at Debug level
5. WHERE context loading fails THEN the Context Loader SHALL include the agent name, tier, and step in the log message

### Requirement 8

**User Story:** As a developer, I want context paths to be derived from step identifiers automatically, so that the system is maintainable and consistent.

#### Acceptance Criteria

1. WHEN a step has an identifier THEN the Context Management System SHALL convert it to a directory name
2. WHEN converting step identifiers THEN the Context Management System SHALL use lowercase characters
3. WHEN a step identifier contains spaces THEN the Context Management System SHALL replace them with hyphens
4. WHEN a step identifier contains special characters THEN the Context Management System SHALL remove or replace them with hyphens
5. WHEN a step identifier is "Rent Conditions" THEN the Context Management System SHALL use the directory name "rent-conditions"

### Requirement 9

**User Story:** As a content manager, I want to share common context across multiple steps, so that I don't need to duplicate reference materials.

#### Acceptance Criteria

1. WHEN a "common" directory exists at the tier level THEN the Context Loader SHALL include its contents in all steps
2. WHEN both common and step-specific context exist THEN the Context Loader SHALL include common context first
3. WHEN a "common" directory exists at the agent level THEN the Context Loader SHALL include its contents in all tiers and steps
4. WHEN multiple common directories exist THEN the Context Loader SHALL load them in order: agent-level, tier-level, step-level
5. WHERE common context is large THEN the Context Loader SHALL cache it separately to avoid redundant file reads

### Requirement 10

**User Story:** As a developer, I want to mark agent methods with a ContextStep attribute, so that the system knows which methods need context loading.

#### Acceptance Criteria

1. WHEN a developer creates an analysis method THEN the developer SHALL mark it with the ContextStep attribute
2. WHEN the ContextStep attribute is applied THEN it SHALL specify the step name as a parameter
3. WHEN the step name contains spaces or special characters THEN the Context Management System SHALL normalize it to a valid directory name
4. WHERE a method is not marked with ContextStep THEN the Context Management System SHALL not attempt to load context for it
5. WHEN the ContextStep attribute is used THEN it SHALL be applicable only to methods

### Requirement 11

**User Story:** As a developer, I want context loading to be performant, so that it doesn't significantly impact analysis response times.

#### Acceptance Criteria

1. WHEN context is requested THEN the Context Loader SHALL complete loading within 100ms for files up to 1MB
2. WHEN context files are large THEN the Context Loader SHALL use streaming reads to minimize memory usage
3. WHEN the same context is requested multiple times in a request THEN the Context Provider SHALL return cached content
4. WHEN context files exceed 5MB total THEN the Context Loader SHALL log a performance warning
5. WHERE context loading is slow THEN the Context Loader SHALL include timing information in debug logs
