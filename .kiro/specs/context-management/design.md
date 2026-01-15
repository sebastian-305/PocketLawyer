# Design Document - Context Management System

## Overview

The Context Management System provides a hierarchical file-based approach to supplying contextual information (legal texts, reference documents) to AI agents during contract analysis. The system automatically discovers analysis steps through reflection, creates the necessary directory structure, and loads context files on demand with caching for performance.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Application Startup                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  ContextDirectoryInitializer (IHostedService)          │ │
│  │  - Scans agents for [ContextStep] attributes           │ │
│  │  - Creates directory structure                          │ │
│  │  - Generates README files                               │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    File System Structure                     │
│  data/Context/                                               │
│  ├── rental/                                                 │
│  │   ├── basic/                                             │
│  │   │   ├── general-data/                                  │
│  │   │   │   ├── README.md                                  │
│  │   │   │   └── *.txt, *.md                                │
│  │   │   └── common/                                        │
│  │   └── premium/                                           │
│  │       ├── costs/                                         │
│  │       └── common/                                        │
│  └── employment/                                            │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Runtime (Per Request)                     │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Agent Method Execution                                 │ │
│  │  [ContextStep("costs")]                                 │ │
│  │  private async Task<PremiumSectionDto> AnalyzeCosts()  │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  ContextProvider (Scoped)                              │ │
│  │  - Resolves context path from attribute                │ │
│  │  - Loads context via ContextFileReader                 │ │
│  │  - Caches for request duration                         │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌───────────────────────────
─────────────────────────┐ │
│  │  ContextFileReader                                      │ │
│  │  - Reads .txt and .md files                            │ │
│  │  - Concatenates with headers                           │ │
│  │  - Handles common directories                          │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Prompt Builder                                         │ │
│  │  - Receives context string                             │ │
│  │  - Formats with "## Reference Context" header          │ │
│  │  - Appends to AI prompt                                │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### Component Overview

| Component | Lifetime | Purpose |
|-----------|----------|---------|
| ContextStepAttribute | N/A | Marks methods that need context |
| ContextOptions | Singleton | Configuration settings |
| ContextDirectoryInitializer | Hosted Service | Creates directory structure at startup |
| IContextProvider | Scoped | Loads and caches context per request |
| ContextProvider | Scoped | Implementation of IContextProvider |
| IContextFileReader | Singleton | Reads files from directories |
| ContextFileReader | Singleton | Implementation of IContextFileReader |

### 1. ContextStepAttribute

Custom attribute to mark agent methods that require context loading.

**Location:** `Infrastructure/Context/ContextStepAttribute.cs`

```csharp
namespace PocketLawyer.Infrastructure.Context;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ContextStepAttribute : Attribute
{
    public string StepName { get; }
    public string? Tier { get; set; }
    
    public ContextStepAttribute(string stepName)
    {
        StepName = stepName ?? throw new ArgumentNullException(nameof(stepName));
    }
}
```

### 2. ContextOptions

**Location:** `Options/ContextOptions.cs`

```csharp
namespace PocketLawyer.Options;

public class ContextOptions
{
    public string BasePath { get; set; } = "data/Context";
    public bool CreateReadmeFiles { get; set; } = true;
    public long MaxContextSizeBytes { get; set; } = 5 * 1024 * 1024;
}
```

### 3. IContextProvider

**Location:** `Application/Interfaces/IContextProvider.cs`

```csharp
namespace PocketLawyer.Application.Interfaces;

public interface IContextProvider
{
    Task<string> GetContextAsync(string agentName, string tier, string stepName);
    Task<string> GetContextForCurrentStepAsync();
}
```

### 4. IContextFileReader

**Location:** `Application/Interfaces/IContextFileReader.cs`

```csharp
namespace PocketLawyer.Application.Interfaces;

public interface IContextFileReader
{
    Task<string> ReadDirectoryAsync(string directoryPath);
}
```

## Data Models

### ContextMetadata (Optional - for monitoring)

```csharp
namespace PocketLawyer.Infrastructure.Context;

public record ContextMetadata(
    string AgentName,
    string Tier,
    string StepName,
    int FileCount,
    long TotalSizeBytes,
    DateTimeOffset LoadedAt
);
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system-essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Directory creation idempotence
*For any* agent configuration, running the ContextDirectoryInitializer multiple times should result in the same directory structure without errors or data loss
**Validates: Requirements 3.4**

### Property 2: Context path normalization consistency
*For any* step name string, normalizing it twice should produce the same result as normalizing it once (idempotent)
**Validates: Requirements 8.2, 8.3, 8.4**

### Property 3: Context loading determinism
*For any* set of context files in a directory, loading the context multiple times should produce identical concatenated output (assuming files haven't changed)
**Validates: Requirements 4.3, 5.2**

### Property 4: Cache consistency
*For any* context request within the same HTTP request scope, the second request should return cached content identical to the first request
**Validates: Requirements 5.4**

### Property 5: Common context inclusion order
*For any* step-specific context request, if common directories exist at agent or tier level, their content should appear before step-specific content in the final output
**Validates: Requirements 9.2, 9.4**

### Property 6: File extension filtering
*For any* directory containing mixed file types, only files with .txt or .md extensions (excluding README.md) should be included in the context
**Validates: Requirements 4.1, 4.2**

### Property 7: Error resilience
*For any* context loading operation where some files fail to read, the operation should continue and return content from successfully read files
**Validates: Requirements 4.5**

### Property 8: Attribute discovery completeness
*For any* agent class with ContextStep attributes, the ContextDirectoryInitializer should create directories for all attributed methods
**Validates: Requirements 3.1, 3.2**

## Error Handling

### Startup Errors (ContextDirectoryInitializer)

| Error Scenario | Handling Strategy |
|----------------|-------------------|
| Base path creation fails | Log error, allow app to start (graceful degradation) |
| Individual directory creation fails | Log warning, continue with other directories |
| README file creation fails | Log warning, continue (non-critical) |
| Agent type discovery fails | Log error, skip that agent |

### Runtime Errors (ContextProvider)

| Error Scenario | Handling Strategy |
|----------------|-------------------|
| Directory doesn't exist | Return empty string, log debug message |
| File read fails | Log warning, continue with other files |
| Context exceeds max size | Log warning, return content anyway |
| Cache key collision | Overwrite (shouldn't happen with proper keys) |

### File Reading Errors (ContextFileReader)

| Error Scenario | Handling Strategy |
|----------------|-------------------|
| File locked by another process | Log warning, skip file |
| File encoding issues | Log warning, skip file |
| Permission denied | Log warning, skip file |

## Testing Strategy

### Unit Tests

**ContextStepAttribute Tests:**
- Attribute can be applied to methods
- StepName is required
- Tier is optional

**Path Normalization Tests:**
- Various step names normalize correctly
- Special characters are handled
- Idempotency of normalization

**ContextFileReader Tests:**
- Reads .txt files correctly
- Reads .md files correctly
- Ignores README.md
- Ignores unsupported extensions
- Handles empty directories
- Handles file read errors gracefully

**ContextProvider Tests:**
- Caching works correctly
- Common context is loaded in correct order
- Empty context returns empty string
- Cache keys are unique per agent/tier/step

### Integration Tests

**Directory Initialization Tests:**
- All agent directories are created
- README files are generated
- Idempotent execution
- Handles missing base path

**End-to-End Context Loading:**
- Context is loaded for attributed methods
- Multiple files are concatenated correctly
- Common context is included
- Performance is acceptable (<100ms for 1MB)

### Property-Based Tests

Property-based testing will use **FsCheck** library for C#.

**Test Configuration:**
- Minimum 100 iterations per property
- Custom generators for file paths, step names, and content

## Performance Considerations

### Caching Strategy

- **Scope:** Per-request (Scoped service lifetime)
- **Key:** `{agentName}:{tier}:{stepName}`
- **Invalidation:** Automatic at end of request
- **Memory:** Bounded by request duration

### File I/O Optimization

- Use `File.ReadAllTextAsync` for async I/O
- Read files sequentially (not parallel) to avoid disk thrashing
- No streaming needed for files <5MB

### Startup Performance

- Directory creation is fast (milliseconds)
- Reflection scan happens once at startup
- No impact on request handling

### Expected Performance Metrics

| Operation | Target | Notes |
|-----------|--------|-------|
| Directory initialization | <500ms | One-time at startup |
| Context loading (1MB) | <100ms | First load per request |
| Context loading (cached) | <1ms | Subsequent loads |
| File concatenation | <50ms | For 10 files |

## Configuration

### appsettings.json

```json
{
  "Context": {
    "BasePath": "data/Context",
    "CreateReadmeFiles": true,
    "MaxContextSizeBytes": 5242880
  }
}
```

### appsettings.Development.json

```json
{
  "Context": {
    "BasePath": "data/Context",
    "CreateReadmeFiles": true,
    "MaxContextSizeBytes": 10485760
  }
}
```

### Environment Variables (Optional)

- `CONTEXT_BASE_PATH`: Override base path
- `CONTEXT_MAX_SIZE`: Override max size in bytes

## Integration with Existing System

### Changes to Program.cs

```csharp
// Add Context options
builder.Services
    .AddOptions<ContextOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Context"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Register Context services
builder.Services.AddSingleton<IContextFileReader, ContextFileReader>();
builder.Services.AddScoped<IContextProvider, ContextProvider>();
builder.Services.AddHostedService<ContextDirectoryInitializer>();
```

### Changes to Agent Methods

Agents will be updated to use context:

```csharp
[ContextStep("costs")]
private async Task<PremiumSectionDto> AnalyzeCostsAsync()
{
    var context = await _contextProvider.GetContextForCurrentStepAsync();
    
    var prompt = Domain.PromptProvider.GetPrompt("Rental", "Premium", "Costs");
    
    // Add context to prompt if available
    if (!string.IsNullOrWhiteSpace(context))
    {
        prompt += $"\n\n## Reference Context\n\n{context}";
    }
    
    // Continue with AI call...
}
```

### Changes to PromptProvider (Optional Enhancement)

Could add a method to automatically include context:

```csharp
public static string GetPromptWithContext(
    string agentName, 
    string tier, 
    string stepName, 
    string context)
{
    var basePrompt = GetPrompt(agentName, tier, stepName);
    
    if (string.IsNullOrWhiteSpace(context))
        return basePrompt;
    
    return $"{basePrompt}\n\n## Reference Context\n\n{context}";
}
```

## Migration Strategy

### Phase 1: Infrastructure Setup
1. Create attribute, interfaces, and implementations
2. Add configuration to appsettings
3. Register services in Program.cs
4. Run application to generate directory structure

### Phase 2: Agent Integration
1. Add ContextStepAttribute to existing agent methods
2. Inject IContextProvider into agents
3. Update methods to load and use context
4. Test with empty context (no files yet)

### Phase 3: Content Population
1. Content managers add context files to directories
2. Test with real context files
3. Monitor performance and adjust if needed

### Phase 4: Optimization (if needed)
1. Add more aggressive caching if performance issues
2. Consider pre-loading common context at startup
3. Add context file validation

## Security Considerations

### File System Access

- Context files are read-only at runtime
- No user input affects file paths (prevents path traversal)
- Directory creation only at startup (not during requests)

### Content Validation

- No validation of context file content (trusted source)
- File size limits prevent memory exhaustion
- Only specific extensions allowed

### Logging

- File paths logged at Debug level only
- No sensitive content logged
- Performance metrics logged at Info level

## Monitoring and Observability

### Metrics to Track

- Context loading duration per step
- Context size per step
- Cache hit rate
- File read errors
- Directory initialization time

### Log Messages

**Startup:**
- `"Initializing context directories at {BasePath}"`
- `"Created context directory: {Path}"`
- `"Context directory initialization complete"`

**Runtime:**
- `"Loaded context for {AgentName}/{Tier}/{StepName} in {Duration}ms ({Size} chars)"`
- `"Returning cached context for {CacheKey}"`
- `"Context size exceeds recommended maximum"`

**Errors:**
- `"Failed to initialize context directories"`
- `"Failed to load context for {AgentName}/{Tier}/{StepName}"`
- `"Failed to read context file: {File}"`

## Future Enhancements

### Potential Improvements

1. **Hot Reload:** Watch file system for changes and invalidate cache
2. **Compression:** Compress large context files
3. **Versioning:** Support multiple versions of context files
4. **Validation:** Validate context file format/structure
5. **UI:** Admin interface to manage context files
6. **Templates:** Context file templates for common scenarios
7. **Localization:** Support multiple languages for context
8. **Analytics:** Track which context files are most useful

### Not Planned (Out of Scope)

- Database storage for context
- User-uploaded context files
- Dynamic context generation
- Context file encryption
