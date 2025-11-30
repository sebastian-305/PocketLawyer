# PocketLawyer Configuration Guide

This document provides comprehensive configuration guidance for the PocketLawyer application, including all settings in `appsettings.json`, environment variables, and best practices for different deployment scenarios.

## Table of Contents

1. [Configuration Overview](#configuration-overview)
2. [appsettings.json Structure](#appsettingsjson-structure)
3. [Logging Configuration](#logging-configuration)
4. [Method Tracing Configuration](#method-tracing-configuration)
5. [AI Service Configuration](#ai-service-configuration)
6. [File Upload Configuration](#file-upload-configuration)
7. [Path Configuration](#path-configuration)
8. [Context Management Configuration](#context-management-configuration)
9. [Environment-Specific Configuration](#environment-specific-configuration)
10. [Security Best Practices](#security-best-practices)
11. [Production Deployment](#production-deployment)
12. [Troubleshooting](#troubleshooting)

## Configuration Overview

PocketLawyer uses ASP.NET Core's configuration system with the following hierarchy (later sources override earlier ones):

1. `appsettings.json` - Base configuration
2. `appsettings.{Environment}.json` - Environment-specific overrides
3. User Secrets - Development secrets (not committed to source control)
4. Environment Variables - Runtime configuration
5. Command-line arguments - Highest priority

## appsettings.json Structure

### Complete Configuration Example

```json
{
  "Nebius": {
    "Key": "your-api-key-here",
    "ModelId": "meta-llama/Meta-Llama-3.1-70B-Instruct",
    "BaseUrl": "https://api.studio.nebius.ai/v1/"
  },
  "Upload": {
    "MaxSizeMB": 20,
    "AllowedExtensions": [".pdf", ".txt", ".jpg", ".png"]
  },
  "Paths": {
    "TesseractData": "data/tessdata",
    "Responses": "data/Responses",
    "KVS": "data/KVS"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "PocketLawyer": "Information",
      "PocketLawyer.Infrastructure.AiServices": "Debug"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "logs"
    }
  },
  "AllowedHosts": "*"
}
```

## Logging Configuration

### Console Logging

Console logging is always enabled and configured through the `Logging:LogLevel` section:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Log Levels** (from most to least verbose):
- `Trace`: Very detailed logs, typically only enabled in development
- `Debug`: Debugging information (e.g., individual AI token counts)
- `Information`: General informational messages (e.g., analysis completion, token summaries)
- `Warning`: Warning messages (e.g., validation issues)
- `Error`: Error messages (e.g., AI service failures)
- `Critical`: Critical failures (e.g., application crashes)

### Filesystem Logging

Filesystem logging is configured through the `Logging:File` section:

```json
{
  "Logging": {
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "logs"
    }
  }
}
```

**Configuration Options**:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Enabled` | bool | `true` | Enable/disable filesystem logging |
| `MinimumLevel` | string | `"Information"` | Minimum log level for file logs |
| `Path` | string | `"logs"` | Directory for log files (relative or absolute) |

**Log File Format**:
- Files are created daily: `YYYY-MM-DD.log`
- Example: `logs/2024-01-15.log`
- Automatic rollover at midnight (local time)

**Log Entry Format**:
```
{Timestamp:yyyy-MM-DD HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}
```

**Example Log Entries**:
```
2024-01-15 14:23:45.123 +01:00 [INF] [Agent rental-abc123] Analysis completed. Total Input Tokens: 12450, Total Output Tokens: 3821, Total Tokens: 16271
2024-01-15 14:23:45.456 +01:00 [WRN] Schema validation warning: Optional field missing
2024-01-15 14:23:46.789 +01:00 [ERR] AI service error: Rate limit exceeded
System.Net.Http.HttpRequestException: Response status code does not indicate success: 429 (Too Many Requests).
   at System.Net.Http.HttpResponseMessage.EnsureSuccessStatusCode()
   at PocketLawyer.Infrastructure.AiServices.Nebius.NebiusService.SendPromptWithMetadata(String prompt, Object outputSchema)
```

### Logging Scenarios

#### Development Environment

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Debug",
      "Path": "logs"
    }
  }
}
```

**Use Case**: Maximum verbosity for debugging, including individual AI token counts.

#### Production Environment

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "/var/log/pocketlawyer"
    }
  }
}
```

**Use Case**: Balanced logging for production with aggregated token summaries and important events.

#### High-Traffic Production

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "PocketLawyer": "Information"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Warning",
      "Path": "/var/log/pocketlawyer"
    }
  }
}
```

**Use Case**: Minimal logging to reduce I/O overhead, only warnings and errors.

#### Troubleshooting Specific Components

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "PocketLawyer.Infrastructure.AiServices": "Debug",
      "PocketLawyer.Application.Agents": "Debug"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Debug",
      "Path": "logs"
    }
  }
}
```

**Use Case**: Debug specific components without flooding logs with framework messages.

### Log File Management

**Retention Policy**:
```bash
# Linux/macOS: Delete logs older than 30 days
find /var/log/pocketlawyer -name "*.log" -mtime +30 -delete

# Windows PowerShell: Delete logs older than 30 days
Get-ChildItem -Path "C:\logs\pocketlawyer" -Filter "*.log" | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Remove-Item
```

**Monitoring Disk Usage**:
```bash
# Linux/macOS
du -sh /var/log/pocketlawyer

# Windows PowerShell
Get-ChildItem -Path "C:\logs\pocketlawyer" -Recurse | Measure-Object -Property Length -Sum
```

**Log Rotation with Cron** (Linux):
```bash
# /etc/cron.daily/pocketlawyer-logs
#!/bin/bash
find /var/log/pocketlawyer -name "*.log" -mtime +30 -delete
```

## Method Tracing Configuration

### Overview

Method tracing provides automatic logging of method invocations with parameter information at the Verbose/Trace level. This feature enables detailed debugging and performance analysis without requiring manual logging code in every method.

**Key Features**:
- Automatic logging of controller action invocations
- Automatic logging of service method calls (for decorated services)
- Intelligent parameter serialization (handles large data, sensitive parameters)
- Execution duration tracking
- Exception logging
- Minimal performance impact when disabled (<0.1% overhead)

### Enabling Method Tracing

Method tracing is controlled through Serilog's log level configuration. To enable tracing, set the log level to `Verbose` for the desired namespaces.

#### Enable for All PocketLawyer Code

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "PocketLawyer": "Verbose"
      }
    }
  }
}
```

#### Enable for Specific Namespaces

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "PocketLawyer.Application.Agents": "Verbose",
        "PocketLawyer.Infrastructure.AiServices": "Verbose",
        "PocketLawyer.Infrastructure.Logging": "Verbose"
      }
    }
  }
}
```

#### Enable for Controllers Only

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "PocketLawyer.Web": "Verbose"
      }
    }
  }
}
```

### Disabling Method Tracing

To disable method tracing, set the log level to `Debug` or higher (Information, Warning, Error, Critical):

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "PocketLawyer": "Information"
      }
    }
  }
}
```

**Important**: When method tracing is disabled, there is virtually no performance overhead (<0.1%) due to short-circuit optimization.

### Log Format

Method tracing produces structured logs with the following formats:

#### Method Entry
```
Entering {ClassName}.{MethodName} with parameters: {@Parameters}
```

**Example**:
```
2024-01-15 14:23:45.123 +01:00 [VRB] Entering PocketController.AnalyzeRentalContract with parameters: {"file": "FileContentDto { FileName=contract.pdf, ContentType=application/pdf, Content=byte[15234] }", "tier": "\"basic\""}
```

#### Method Exit
```
Exiting {ClassName}.{MethodName} in {Duration}ms
```

**Example**:
```
2024-01-15 14:23:47.456 +01:00 [VRB] Exiting PocketController.AnalyzeRentalContract in 2333ms
```

#### Exception
```
Exception in {ClassName}.{MethodName} after {Duration}ms: {ExceptionType}
```

**Example**:
```
2024-01-15 14:23:47.789 +01:00 [VRB] Exception in GeminiService.SendPromptWithMetadata after 1234ms: HttpRequestException
```

### Parameter Serialization

The method tracing system intelligently serializes parameters to keep logs readable:

#### String Parameters
- Strings longer than 200 characters are truncated
- Format: `"first 200 chars..." (truncated, total: 1234 chars)`

#### Byte Arrays
- Only the length is logged, not the content
- Format: `byte[15234]`

#### Collections
- Shows collection type, count, and first 5 items
- Format: `List[10]: [item1, item2, item3, item4, item5, ...]`

#### Complex Objects
- Shows type name and up to 5 properties
- Format: `FileContentDto { FileName=contract.pdf, ContentType=application/pdf, Content=byte[15234] }`

#### Sensitive Parameters
- Parameters containing "password", "secret", "token", or "key" (case-insensitive) are redacted
- Format: `[REDACTED]`

### Configuration Examples

#### Development Environment (Maximum Verbosity)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "PocketLawyer": "Verbose",
        "PocketLawyer.Infrastructure.Logging": "Verbose"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  },
  "Logging": {
    "File": {
      "Enabled": true,
      "MinimumLevel": "Verbose",
      "Path": "logs"
    }
  }
}
```

**Use Case**: Full method tracing for debugging, including all controller actions and service calls.

#### Production Environment (Tracing Disabled)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "PocketLawyer": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  },
  "Logging": {
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "/var/log/pocketlawyer"
    }
  }
}
```

**Use Case**: No method tracing overhead, only standard application logs.

#### Troubleshooting AI Services

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "PocketLawyer.Infrastructure.AiServices": "Verbose",
        "PocketLawyer.Application.Agents": "Verbose"
      }
    }
  }
}
```

**Use Case**: Trace only AI service calls and agent methods to debug AI integration issues.

#### Troubleshooting Specific Controller

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "PocketLawyer.Web.PocketController": "Verbose"
      }
    }
  }
}
```

**Use Case**: Trace only a specific controller to debug API endpoint issues.

### Performance Impact

Method tracing is designed to have minimal performance impact:

| Scenario | Overhead | Notes |
|----------|----------|-------|
| Verbose disabled | <0.1% | Short-circuit optimization prevents serialization |
| Verbose enabled, simple params | ~1-2ms per method | Fast serialization of primitives and strings |
| Verbose enabled, complex params | ~5-10ms per method | Reflection limited to 5 properties, depth 2 |

**Recommendations**:
- **Development**: Enable Verbose logging for all PocketLawyer code
- **Staging**: Enable Verbose logging for specific namespaces as needed
- **Production**: Disable Verbose logging (use Information or Warning level)
- **Troubleshooting**: Temporarily enable Verbose for specific namespaces

### Security Considerations

#### Sensitive Parameter Redaction

The method tracing system automatically redacts sensitive parameters to prevent credentials and secrets from appearing in logs.

**Redacted Parameter Names** (case-insensitive):
- `password`
- `secret`
- `token`
- `key`
- `apikey`
- `api_key`

**Example**:
```csharp
// Method signature
public async Task<string> Authenticate(string username, string password)

// Logged parameters
{"username": "john.doe", "password": "[REDACTED]"}
```

#### Production Security

**Important**: Always disable Verbose logging in production to prevent:
- Exposure of sensitive data in logs
- Excessive log volume
- Performance degradation
- Increased storage costs

### Troubleshooting Method Tracing

#### Issue: Method tracing logs not appearing

**Check 1**: Verify Verbose logging is enabled
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "PocketLawyer": "Verbose"
      }
    }
  }
}
```

**Check 2**: Verify file logging minimum level
```json
{
  "Logging": {
    "File": {
      "MinimumLevel": "Verbose"
    }
  }
}
```

**Check 3**: Check console output for trace logs (look for `[VRB]` prefix)

#### Issue: Too many trace logs

**Solution 1**: Reduce scope to specific namespaces
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "PocketLawyer.Application.Agents": "Verbose"
      }
    }
  }
}
```

**Solution 2**: Disable method tracing entirely
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "PocketLawyer": "Information"
      }
    }
  }
}
```

#### Issue: Performance degradation with tracing enabled

**Solution 1**: Verify short-circuit is working (check that `IsEnabled(LogLevel.Trace)` is called)

**Solution 2**: Reduce tracing scope to specific namespaces

**Solution 3**: Disable tracing in production

#### Issue: Sensitive data appearing in logs

**Check**: Verify parameter names contain sensitive keywords (password, secret, token, key)

**Solution**: Rename parameters to include sensitive keywords:
```csharp
// Before
public async Task<string> Authenticate(string user, string pass)

// After
public async Task<string> Authenticate(string user, string password)
```

### Dynamic Configuration Changes

Serilog supports dynamic configuration reloading. Changes to `appsettings.json` will be applied without restarting the application (typically within 30 seconds).

**Example**: Enable tracing for troubleshooting
1. Edit `appsettings.json` to add `"PocketLawyer": "Verbose"`
2. Save the file
3. Wait ~30 seconds for configuration reload
4. Trace logs will start appearing
5. Edit `appsettings.json` to remove Verbose level
6. Save the file
7. Trace logs will stop appearing

**Note**: This feature requires `reloadOnChange: true` in `Program.cs` (enabled by default).

## AI Service Configuration

### Nebius API Configuration

```json
{
  "Nebius": {
    "Key": "your-api-key-here",
    "ModelId": "meta-llama/Meta-Llama-3.1-70B-Instruct",
    "BaseUrl": "https://api.studio.nebius.ai/v1/"
  }
}
```

**Configuration Options**:

| Option | Type | Required | Description |
|--------|------|----------|-------------|
| `Key` | string | Yes | Nebius API key for authentication |
| `ModelId` | string | Yes | Model identifier (e.g., `meta-llama/Meta-Llama-3.1-70B-Instruct`) |
| `BaseUrl` | string | Yes | Base URL for Nebius API |

**Security**: Never commit API keys to source control. Use User Secrets or environment variables.

### Token Tracking

Token tracking is automatically enabled for all AI requests. No additional configuration is required.

**Logged Metrics**:
- Input Tokens: Tokens sent to AI (prompt + schema)
- Output Tokens: Tokens generated by AI (response)
- Total Tokens: Sum of input and output tokens

**Log Levels**:
- Debug: Individual token counts per AI request
- Info: Aggregated token summary per analysis tier

**Example Log Output**:
```
2024-01-15 14:23:45.123 +01:00 [INF] [Agent rental-abc123] Analysis completed. Total Input Tokens: 12450, Total Output Tokens: 3821, Total Tokens: 16271
```

## File Upload Configuration

```json
{
  "Upload": {
    "MaxSizeMB": 20,
    "AllowedExtensions": [".pdf", ".txt", ".jpg", ".png"]
  }
}
```

**Configuration Options**:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `MaxSizeMB` | int | `20` | Maximum file size in megabytes |
| `AllowedExtensions` | string[] | `[".pdf", ".txt", ".jpg", ".png"]` | Allowed file extensions |

**Supported File Types**:
- `.pdf`: PDF documents (extracted with PdfPig)
- `.txt`: Plain text files
- `.jpg`, `.jpeg`, `.png`: Images (OCR with Tesseract)

**Recommendations**:
- **Development**: Keep default 20MB limit
- **Production**: Consider reducing to 10MB for faster uploads
- **High-Security**: Restrict to `.pdf` and `.txt` only

## Path Configuration

```json
{
  "Paths": {
    "TesseractData": "data/tessdata",
    "Responses": "data/Responses",
    "KVS": "data/KVS"
  }
}
```

**Configuration Options**:

| Option | Type | Description |
|--------|------|-------------|
| `TesseractData` | string | Path to Tesseract language data files |
| `Responses` | string | Path to mock response files (development) |
| `KVS` | string | Path to Kollektivvertrag (collective agreement) files |

**Path Types**:
- Relative paths: Resolved from application root directory
- Absolute paths: Used as-is

**Production Recommendations**:
```json
{
  "Paths": {
    "TesseractData": "/opt/pocketlawyer/data/tessdata",
    "Responses": "/opt/pocketlawyer/data/Responses",
    "KVS": "/opt/pocketlawyer/data/KVS"
  }
}
```

## Context Management Configuration

The Context Management System provides step-specific contextual information (such as legal texts and reference documents) to AI agents during contract analysis. Context files are automatically loaded based on the agent, analysis tier, and step being executed.

### Configuration

```json
{
  "Context": {
    "BasePath": "data/Context",
    "CreateReadmeFiles": true,
    "MaxContextSizeBytes": 5242880
  }
}
```

**Configuration Options**:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `BasePath` | string | `"data/Context"` | Base directory for all context files |
| `CreateReadmeFiles` | bool | `true` | Automatically create README files in context directories |
| `MaxContextSizeBytes` | long | `5242880` (5MB) | Maximum total size of context files per step (warning threshold) |

### Directory Structure

The Context Management System automatically creates a hierarchical directory structure based on agent methods marked with the `[ContextStep]` attribute:

```
data/Context/
├── rental/
│   ├── basic/
│   │   ├── general-data/
│   │   │   ├── README.md
│   │   │   └── *.txt, *.md
│   │   ├── rent-conditions/
│   │   ├── contract-conditions/
│   │   ├── summary/
│   │   └── common/              # Shared across all basic steps
│   └── premium/
│       ├── costs/
│       ├── terms/
│       ├── use/
│       ├── maintenance/
│       ├── duties/
│       ├── privacy/
│       ├── premium-summary/
│       └── common/              # Shared across all premium steps
└── employment/
    ├── basic/
    │   └── common/
    └── premium/
        └── common/
```

### Context File Format

**Supported File Types**:
- `.txt`: Plain text files
- `.md`: Markdown files
- `README.md` files are automatically excluded from context loading

**File Naming**:
- Use descriptive names (e.g., `mrg-articles.txt`, `abgb-sections.md`)
- Files are loaded in alphabetical order
- Multiple files are concatenated with filename headers

**Example Context File** (`data/Context/rental/basic/rent-conditions/mrg-articles.txt`):
```
§ 16 MRG - Mietzinsbildung
(1) Der Hauptmietzins darf den Richtwert nicht übersteigen...

§ 17 MRG - Betriebskosten
(1) Der Vermieter kann vom Mieter die Betriebskosten verlangen...
```

### Context Loading Behavior

**Automatic Loading**:
- Context is automatically loaded when an agent method marked with `[ContextStep]` is executed
- No manual code required in agent methods (handled by `IContextProvider`)

**Common Context**:
- Files in `common/` directories are included in all steps at that level
- Loading order: agent-level common → tier-level common → step-specific

**Caching**:
- Context is cached per HTTP request (scoped lifetime)
- Subsequent calls to the same step within a request return cached content
- Cache is automatically cleared at the end of the request

**Performance**:
- Context loading completes in <100ms for files up to 1MB
- Warning logged if total context size exceeds `MaxContextSizeBytes`

### Adding Context Files

**Step 1**: Identify the agent, tier, and step
- Agent: `rental` or `employment`
- Tier: `basic` or `premium`
- Step: e.g., `rent-conditions`, `costs`, `general-data`

**Step 2**: Navigate to the appropriate directory
```bash
cd data/Context/rental/basic/rent-conditions
```

**Step 3**: Add your context files
```bash
# Copy legal text files
cp ~/legal-texts/mrg-articles.txt .
cp ~/legal-texts/abgb-sections.md .
```

**Step 4**: Verify files are loaded
- Run the application
- Execute an analysis for that step
- Check logs for context loading messages:
```
[DBG] Loaded context for rental/basic/rent-conditions in 45ms (12450 chars)
```

### Configuration Examples

#### Development Environment

```json
{
  "Context": {
    "BasePath": "data/Context",
    "CreateReadmeFiles": true,
    "MaxContextSizeBytes": 10485760
  }
}
```

**Use Case**: Larger context size limit (10MB) for development and testing with extensive legal texts.

#### Production Environment

```json
{
  "Context": {
    "BasePath": "/opt/pocketlawyer/data/Context",
    "CreateReadmeFiles": false,
    "MaxContextSizeBytes": 5242880
  }
}
```

**Use Case**: Absolute path for production deployment, README creation disabled (directories already exist), standard 5MB limit.

#### High-Performance Environment

```json
{
  "Context": {
    "BasePath": "data/Context",
    "CreateReadmeFiles": false,
    "MaxContextSizeBytes": 2097152
  }
}
```

**Use Case**: Reduced context size limit (2MB) for faster loading and lower memory usage in high-traffic scenarios.

### Logging

Context loading produces the following log messages:

**Startup**:
```
[INF] Initializing context directories at data/Context
[DBG] Created context directory: data/Context/rental/basic/general-data
[INF] Context directory initialization complete
```

**Runtime**:
```
[DBG] Loaded context for rental/basic/rent-conditions in 45ms (12450 chars)
[DBG] Returning cached context for rental:basic:rent-conditions
[WRN] Context size (6291456 bytes) exceeds recommended maximum (5242880 bytes) for rental/premium/costs
```

**Errors**:
```
[WRN] Failed to read context file: data/Context/rental/basic/rent-conditions/locked-file.txt
[DBG] No context files found for employment/basic/general-data
```

### Troubleshooting

**Issue**: Context not being loaded
- **Check**: Agent method is marked with `[ContextStep("step-name")]` attribute
- **Check**: Directory exists at `{BasePath}/{agent}/{tier}/{step-name}`
- **Check**: Directory contains `.txt` or `.md` files (excluding `README.md`)
- **Check**: Application has read permissions for context directory

**Issue**: Context files not found
- **Check**: File extensions are `.txt` or `.md` (case-sensitive on Linux)
- **Check**: Files are not named `README.md` (automatically excluded)
- **Check**: Directory path matches normalized step name (lowercase, hyphens)

**Issue**: Context size warning
- **Solution**: Split large files into smaller, more focused files
- **Solution**: Increase `MaxContextSizeBytes` in configuration
- **Solution**: Move common content to `common/` directory to share across steps

**Issue**: Outdated context being used
- **Cause**: Context is cached per request
- **Solution**: Context is automatically refreshed on the next request
- **Note**: No application restart required for context file changes

### Performance Considerations

**File Size Recommendations**:
- Individual files: <1MB for optimal performance
- Total context per step: <5MB (default limit)
- Use multiple smaller files instead of one large file

**Optimization Tips**:
- Place frequently used content in `common/` directories (loaded once per request)
- Use `.txt` files for plain text (faster than `.md` parsing)
- Remove unnecessary whitespace and formatting from legal texts
- Monitor context loading duration in logs (should be <100ms)

### Security Considerations

**File System Access**:
- Context files are read-only at runtime
- No user input affects context file paths (prevents path traversal attacks)
- Directory creation only occurs at application startup

**Content Validation**:
- No validation of context file content (trusted source)
- File size limits prevent memory exhaustion
- Only `.txt` and `.md` extensions allowed (prevents execution of scripts)

### Migration from Manual Context Loading

If you previously loaded context manually in agent methods, you can remove that code:

**Before**:
```csharp
private async Task<PremiumSectionDto> AnalyzeCostsAsync()
{
    var contextPath = Path.Combine("data", "Context", "rental", "premium", "costs");
    var context = await LoadContextFiles(contextPath);
    
    var prompt = PromptProvider.GetPrompt("Rental", "Premium", "Costs");
    if (!string.IsNullOrWhiteSpace(context))
    {
        prompt += $"\n\n## Reference Context\n\n{context}";
    }
    
    // Continue with AI call...
}
```

**After**:
```csharp
[ContextStep("costs")]
private async Task<PremiumSectionDto> AnalyzeCostsAsync()
{
    var context = await _contextProvider.GetContextForCurrentStepAsync();
    
    var prompt = PromptProvider.GetPrompt("Rental", "Premium", "Costs");
    if (!string.IsNullOrWhiteSpace(context))
    {
        prompt += $"\n\n## Reference Context\n\n{context}";
    }
    
    // Continue with AI call...
}
```

**Benefits**:
- Automatic directory structure creation
- Consistent context loading across all agents
- Built-in caching and performance optimization
- Centralized configuration

## Environment-Specific Configuration

### Development (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Debug",
      "Path": "logs"
    }
  }
}
```

### Staging (appsettings.Staging.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "/var/log/pocketlawyer-staging"
    }
  },
  "Upload": {
    "MaxSizeMB": 15
  }
}
```

### Production (appsettings.Production.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "/var/log/pocketlawyer"
    }
  },
  "Upload": {
    "MaxSizeMB": 10
  },
  "Paths": {
    "TesseractData": "/opt/pocketlawyer/data/tessdata",
    "Responses": "/opt/pocketlawyer/data/Responses",
    "KVS": "/opt/pocketlawyer/data/KVS"
  }
}
```

## Security Best Practices

### User Secrets (Development)

**Initialize User Secrets**:
```bash
dotnet user-secrets init
```

**Set Nebius API Key**:
```bash
dotnet user-secrets set "Nebius:Key" "your-api-key-here"
```

**List User Secrets**:
```bash
dotnet user-secrets list
```

**Remove User Secret**:
```bash
dotnet user-secrets remove "Nebius:Key"
```

### Environment Variables (Production)

**Linux/macOS**:
```bash
export Nebius__Key="your-api-key-here"
export Nebius__ModelId="meta-llama/Meta-Llama-3.1-70B-Instruct"
export Logging__File__Path="/var/log/pocketlawyer"
```

**Windows PowerShell**:
```powershell
$env:Nebius__Key = "your-api-key-here"
$env:Nebius__ModelId = "meta-llama/Meta-Llama-3.1-70B-Instruct"
$env:Logging__File__Path = "C:\logs\pocketlawyer"
```

**Docker**:
```yaml
version: '3.8'
services:
  pocketlawyer:
    image: pocketlawyer:latest
    environment:
      - Nebius__Key=${NEBIUS_API_KEY}
      - Nebius__ModelId=meta-llama/Meta-Llama-3.1-70B-Instruct
      - Logging__File__Path=/var/log/pocketlawyer
    volumes:
      - ./logs:/var/log/pocketlawyer
```

### Secrets Management

**Azure Key Vault**:
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

**AWS Secrets Manager**:
```csharp
// Program.cs
builder.Configuration.AddSecretsManager(
    configurator: options =>
    {
        options.SecretFilter = entry => entry.Name.StartsWith("PocketLawyer");
    });
```

## Production Deployment

### Recommended Configuration

```json
{
  "Nebius": {
    "Key": "${NEBIUS_API_KEY}",
    "ModelId": "meta-llama/Meta-Llama-3.1-70B-Instruct",
    "BaseUrl": "https://api.studio.nebius.ai/v1/"
  },
  "Upload": {
    "MaxSizeMB": 10,
    "AllowedExtensions": [".pdf", ".txt"]
  },
  "Paths": {
    "TesseractData": "/opt/pocketlawyer/data/tessdata",
    "Responses": "/opt/pocketlawyer/data/Responses",
    "KVS": "/opt/pocketlawyer/data/KVS"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "Path": "/var/log/pocketlawyer"
    }
  },
  "AllowedHosts": "yourdomain.com"
}
```

### Performance Tuning

**High-Traffic Scenarios**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "PocketLawyer": "Information"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Warning",
      "Path": "/var/log/pocketlawyer"
    }
  }
}
```

**Reduced Logging Overhead**:
- Set `MinimumLevel` to `"Warning"` or `"Error"`
- Disable Debug-level logging in production
- Use asynchronous logging (Serilog handles this automatically)

### Monitoring and Alerting

**Log Aggregation**:
- Use ELK Stack (Elasticsearch, Logstash, Kibana)
- Use Splunk or Datadog for centralized logging
- Parse token consumption metrics for cost tracking

**Metrics to Monitor**:
- Token consumption per analysis (from logs)
- Analysis duration per tier
- Error rates by type
- File upload sizes and types
- WebSocket connection duration

## Troubleshooting

### Common Issues

**Issue**: Logs not being written to file
- **Check**: `Logging:File:Enabled` is `true`
- **Check**: Application has write permissions to log directory
- **Check**: Log directory exists (Serilog creates it automatically)

**Issue**: Too many log files
- **Solution**: Implement log retention policy (see [Log File Management](#log-file-management))

**Issue**: High disk usage from logs
- **Solution**: Reduce `MinimumLevel` to `"Warning"` or `"Error"`
- **Solution**: Implement log rotation and retention

**Issue**: Missing token tracking logs
- **Check**: `Logging:LogLevel:Default` is at least `"Information"`
- **Check**: Agents are using `SendPromptWithMetadata` instead of `SendPrompt`

**Issue**: API key not found
- **Check**: User Secrets are configured (development)
- **Check**: Environment variables are set (production)
- **Check**: Configuration hierarchy (environment variables override appsettings.json)

### Debug Configuration

**Enable Verbose Logging**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Debug",
      "PocketLawyer": "Trace"
    },
    "File": {
      "Enabled": true,
      "MinimumLevel": "Trace",
      "Path": "logs"
    }
  }
}
```

**Verify Configuration Loading**:
```csharp
// Program.cs
var config = builder.Configuration.GetSection("Logging:File").Get<FileLoggingOptions>();
Console.WriteLine($"File Logging Enabled: {config?.Enabled}");
Console.WriteLine($"Minimum Level: {config?.MinimumLevel}");
Console.WriteLine($"Path: {config?.Path}");
```

## Additional Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Serilog Documentation](https://serilog.net/)
- [User Secrets in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Environment Variables in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#environment-variables)
