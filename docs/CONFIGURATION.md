# PocketLawyer Configuration Guide

This document provides comprehensive configuration guidance for the PocketLawyer application, including all settings in `appsettings.json`, environment variables, and best practices for different deployment scenarios.

## Table of Contents

1. [Configuration Overview](#configuration-overview)
2. [appsettings.json Structure](#appsettingsjson-structure)
3. [Logging Configuration](#logging-configuration)
4. [AI Service Configuration](#ai-service-configuration)
5. [File Upload Configuration](#file-upload-configuration)
6. [Path Configuration](#path-configuration)
7. [Environment-Specific Configuration](#environment-specific-configuration)
8. [Security Best Practices](#security-best-practices)
9. [Production Deployment](#production-deployment)
10. [Troubleshooting](#troubleshooting)

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
