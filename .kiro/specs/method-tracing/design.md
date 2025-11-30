# Design Document - Method Tracing System

## Overview

The Method Tracing System provides automatic logging of method invocations with parameter information at the Verbose/Trace level. The system uses ASP.NET Core Action Filters for controllers and the Decorator Pattern for services, with intelligent parameter serialization to keep logs readable and performant.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Application Startup                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Service Registration (Program.cs)                      │ │
│  │  - Register MethodTracingActionFilter globally          │ │
│  │  - Register ParameterSerializer as Singleton            │ │
│  │  - Wrap critical services with LoggingDecorators        │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Controller Execution                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  HTTP Request → Controller Action                       │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  MethodTracingActionFilter (OnActionExecuting)          │ │
│  │  - Check if Verbose logging enabled                     │ │
│  │  - Extract action name and parameters                   │ │
│  │  - Serialize parameters via ParameterSerializer         │ │
│  │  - Log "Entering {Controller}.{Action}"                 │ │
│  │  - Start stopwatch                                      │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Action Execution                                        │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  MethodTracingActionFilter (OnActionExecuted)           │ │
│  │  - Stop stopwatch                                       │ │
│  │  - Log "Exiting {Controller}.{Action} in {Duration}ms" │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    Service Execution                         │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Service Method Call (e.g., IAiClient.SendPrompt)      │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  LoggingDecorator (Before)                              │ │
│  │  - Check if Verbose logging enabled                     │ │
│  │  - Extract method name and parameters                   │ │
│  │  - Serialize parameters via ParameterSerializer         │ │
│  │  - Log "Entering {Service}.{Method}"                    │ │
│  │  - Start stopwatch                                      │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Inner Service (Actual Implementation)                  │ │
│  └────────────────────────────────────────────────────────┘ │
│                            │                                 │
│                            ▼                                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  LoggingDecorator (After)                               │ │
│  │  - Stop stopwatch                                       │ │
│  │  - Log "Exiting {Service}.{Method} in {Duration}ms"    │ │
│  │  - If exception: Log "Exception in {Service}.{Method}" │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### Component Overview

| Component | Lifetime | Purpose |
|-----------|----------|---------|
| MethodTracingActionFilter | Singleton | Logs controller action invocations |
| IParameterSerializer | Singleton | Serializes method parameters intelligently |
| ParameterSerializer | Singleton | Implementation of IParameterSerializer |
| LoggingAiClientDecorator | Keyed Scoped | Wraps IAiClient with logging |
| LoggingFileDecoderDecorator | Scoped | Wraps IFileDecoder with logging |

### 1. IParameterSerializer

**Location:** `Application/Interfaces/IParameterSerializer.cs`

```csharp
namespace PocketLawyer.Application.Interfaces;

public interface IParameterSerializer
{
    /// <summary>
    /// Serializes a parameter value to a loggable string representation.
    /// Handles strings, primitives, collections, byte arrays, and complex objects.
    /// Redacts sensitive parameters (password, token, key, secret).
    /// </summary>
    string Serialize(string parameterName, object? value);
    
    /// <summary>
    /// Serializes multiple parameters to a dictionary for structured logging.
    /// </summary>
    Dictionary<string, string> SerializeParameters(IDictionary<string, object?> parameters);
}
```

### 2. ParameterSerializer Implementation

**Location:** `Infrastructure/Logging/ParameterSerializer.cs`

```csharp
namespace PocketLawyer.Infrastructure.Logging;

public class ParameterSerializer : IParameterSerializer
{
    private const int MaxStringLength = 200;
    private const int MaxCollectionItems = 5;
    private const int MaxReflectionDepth = 2;
    
    private static readonly string[] SensitiveKeywords = 
    {
        "password", "secret", "token", "key", "apikey", "api_key"
    };
    
    public string Serialize(string parameterName, object? value)
    {
        // Check for sensitive parameters
        if (IsSensitiveParameter(parameterName))
        {
            return "[REDACTED]";
        }
        
        // Handle null
        if (value == null)
        {
            return "null";
        }
        
        // Handle strings
        if (value is string str)
        {
            return SerializeString(str);
        }
        
        // Handle byte arrays
        if (value is byte[] bytes)
        {
            return $"byte[{bytes.Length}]";
        }
        
        // Handle primitives
        if (value.GetType().IsPrimitive || value is decimal || value is DateTime || value is DateTimeOffset)
        {
            return value.ToString() ?? "null";
        }
        
        // Handle collections
        if (value is IEnumerable enumerable and not string)
        {
            return SerializeCollection(enumerable);
        }
        
        // Handle complex objects
        return SerializeObject(value);
    }
    
    public Dictionary<string, string> SerializeParameters(IDictionary<string, object?> parameters)
    {
        return parameters.ToDictionary(
            kvp => kvp.Key,
            kvp => Serialize(kvp.Key, kvp.Value)
        );
    }
    
    private bool IsSensitiveParameter(string parameterName)
    {
        return SensitiveKeywords.Any(keyword => 
            parameterName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
    
    private string SerializeString(string str)
    {
        if (str.Length <= MaxStringLength)
        {
            return $"\"{str}\"";
        }
        
        return $"\"{str.Substring(0, MaxStringLength)}...\" (truncated, total: {str.Length} chars)";
    }
    
    private string SerializeCollection(IEnumerable enumerable)
    {
        var items = enumerable.Cast<object>().Take(MaxCollectionItems + 1).ToList();
        var typeName = enumerable.GetType().Name;
        
        if (items.Count == 0)
        {
            return $"{typeName}[0]";
        }
        
        var itemsStr = string.Join(", ", items.Take(MaxCollectionItems).Select(i => i?.ToString() ?? "null"));
        
        if (items.Count > MaxCollectionItems)
        {
            return $"{typeName}[{items.Count}+]: [{itemsStr}, ...]";
        }
        
        return $"{typeName}[{items.Count}]: [{itemsStr}]";
    }
    
    private string SerializeObject(object obj)
    {
        var type = obj.GetType();
        var typeName = type.Name;
        
        // Try to get key properties (limit to avoid performance issues)
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Take(5)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();
        
        if (properties.Count == 0)
        {
            return typeName;
        }
        
        var propValues = properties
            .Select(p =>
            {
                try
                {
                    var value = p.GetValue(obj);
                    var valueStr = value?.ToString() ?? "null";
                    if (valueStr.Length > 50)
                    {
                        valueStr = valueStr.Substring(0, 50) + "...";
                    }
                    return $"{p.Name}={valueStr}";
                }
                catch
                {
                    return $"{p.Name}=<error>";
                }
            })
            .ToList();
        
        return $"{typeName} {{ {string.Join(", ", propValues)} }}";
    }
}
```

### 3. MethodTracingActionFilter

**Location:** `Infrastructure/Logging/MethodTracingActionFilter.cs`

```csharp
namespace PocketLawyer.Infrastructure.Logging;

public class MethodTracingActionFilter : IActionFilter
{
    private readonly ILogger<MethodTracingActionFilter> _logger;
    private readonly IParameterSerializer _serializer;
    
    public MethodTracingActionFilter(
        ILogger<MethodTracingActionFilter> logger,
        IParameterSerializer serializer)
    {
        _logger = logger;
        _serializer = serializer;
    }
    
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Short-circuit if Verbose logging is not enabled
        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }
        
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName ?? context.ActionDescriptor.RouteValues["action"];
        
        // Serialize parameters
        var parameters = _serializer.SerializeParameters(context.ActionArguments);
        
        // Start timing
        var stopwatch = Stopwatch.StartNew();
        context.HttpContext.Items["MethodTracing_Stopwatch"] = stopwatch;
        
        _logger.LogTrace(
            "Entering {ControllerName}.{ActionName} with parameters: {@Parameters}",
            controllerName,
            actionName,
            parameters);
    }
    
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Short-circuit if Verbose logging is not enabled
        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }
        
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName ?? context.ActionDescriptor.RouteValues["action"];
        
        // Get timing
        var stopwatch = context.HttpContext.Items["MethodTracing_Stopwatch"] as Stopwatch;
        var duration = stopwatch?.ElapsedMilliseconds ?? 0;
        
        if (context.Exception != null)
        {
            _logger.LogTrace(
                "Exception in {ControllerName}.{ActionName} after {Duration}ms: {ExceptionType}",
                controllerName,
                actionName,
                duration,
                context.Exception.GetType().Name);
        }
        else
        {
            _logger.LogTrace(
                "Exiting {ControllerName}.{ActionName} in {Duration}ms",
                controllerName,
                actionName,
                duration);
        }
    }
}
```

### 4. LoggingAiClientDecorator

**Location:** `Infrastructure/Logging/LoggingAiClientDecorator.cs`

```csharp
namespace PocketLawyer.Infrastructure.Logging;

public class LoggingAiClientDecorator : IAiClient
{
    private readonly IAiClient _inner;
    private readonly ILogger<LoggingAiClientDecorator> _logger;
    private readonly IParameterSerializer _serializer;
    
    public LoggingAiClientDecorator(
        IAiClient inner,
        ILogger<LoggingAiClientDecorator> logger,
        IParameterSerializer serializer)
    {
        _inner = inner;
        _logger = logger;
        _serializer = serializer;
    }
    
    public async Task<AiResponse> SendPromptWithMetadata(string prompt, BinaryData schema)
    {
        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            return await _inner.SendPromptWithMetadata(prompt, schema);
        }
        
        var serviceName = _inner.GetType().Name;
        var parameters = new Dictionary<string, object?>
        {
            ["prompt"] = prompt,
            ["schema"] = schema
        };
        
        var serializedParams = _serializer.SerializeParameters(parameters);
        
        _logger.LogTrace(
            "Entering {ServiceName}.SendPromptWithMetadata with parameters: {@Parameters}",
            serviceName,
            serializedParams);
        
        var sw = Stopwatch.StartNew();
        
        try
        {
            var result = await _inner.SendPromptWithMetadata(prompt, schema);
            
            _logger.LogTrace(
                "Exiting {ServiceName}.SendPromptWithMetadata in {Duration}ms",
                serviceName,
                sw.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogTrace(
                ex,
                "Exception in {ServiceName}.SendPromptWithMetadata after {Duration}ms: {ExceptionType}",
                serviceName,
                sw.ElapsedMilliseconds,
                ex.GetType().Name);
            
            throw;
        }
    }
}
```

## Data Models

No new data models required. Uses existing Serilog structured logging.

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system-essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Sensitive parameter redaction
*For any* parameter name containing "password", "secret", "token", or "key" (case-insensitive), the serialized value should be "[REDACTED]"
**Validates: Requirements 7.1, 7.2, 7.3, 7.4**

### Property 2: String truncation consistency
*For any* string longer than 200 characters, the serialized value should contain exactly the first 200 characters followed by "..." and the total length
**Validates: Requirements 2.1**

### Property 3: Byte array serialization format
*For any* byte array, the serialized value should be in the format "byte[{length}]" where length is the array length
**Validates: Requirements 2.2**

### Property 4: Null handling
*For any* null parameter value, the serialized value should be the string "null"
**Validates: Requirements 2.5**

### Property 5: Logging level short-circuit
*For any* method tracing operation when Verbose logging is disabled, no parameter serialization should occur
**Validates: Requirements 5.1, 5.2**

### Property 6: Timing accuracy
*For any* traced method execution, the logged duration should be within 10ms of the actual execution time
**Validates: Requirements 1.4, 3.3**

### Property 7: Log format consistency
*For any* method entry log, the format should be "Entering {ClassName}.{MethodName}" with structured parameters
**Validates: Requirements 6.1, 6.2, 6.5**

### Property 8: Exception logging
*For any* method that throws an exception, the exception type should be logged before the exception is re-thrown
**Validates: Requirements 3.4, 6.4**

## Error Handling

### Parameter Serialization Errors

| Error Scenario | Handling Strategy |
|----------------|-------------------|
| Property reflection fails | Return "<error>" for that property |
| ToString() throws exception | Return type name only |
| Circular reference detected | Limit depth to prevent stack overflow |

### Action Filter Errors

| Error Scenario | Handling Strategy |
|----------------|-------------------|
| Serialization fails | Log error, continue with action execution |
| Stopwatch not found in context | Log with duration=0 |
| Logger is null | Skip tracing (shouldn't happen with DI) |

## Testing Strategy

### Unit Tests

**ParameterSerializer Tests:**
- Sensitive parameters are redacted
- Strings are truncated at 200 chars
- Byte arrays show length
- Null values return "null"
- Primitives are serialized correctly
- Collections show count and first N items
- Complex objects show type and properties

**MethodTracingActionFilter Tests:**
- Logs entry with parameters
- Logs exit with duration
- Logs exceptions
- Short-circuits when Verbose disabled
- Stopwatch is stored and retrieved correctly

**LoggingDecorator Tests:**
- Logs entry and exit
- Measures duration accurately
- Logs exceptions and re-throws
- Short-circuits when Verbose disabled

### Integration Tests

**End-to-End Controller Tracing:**
- HTTP request triggers action filter
- Parameters are logged
- Duration is measured
- Logs appear in output

**End-to-End Service Tracing:**
- Service call triggers decorator
- Parameters are logged
- Duration is measured
- Exceptions are logged

### Property-Based Tests

Property-based testing will use **FsCheck** library for C#.

**Test Configuration:**
- Minimum 100 iterations per property
- Custom generators for strings, byte arrays, and objects

## Performance Considerations

### Short-Circuit Optimization

The most critical performance optimization is the short-circuit check:

```csharp
if (!_logger.IsEnabled(LogLevel.Trace))
{
    return; // No serialization, no reflection, no overhead
}
```

This ensures **zero overhead** when Verbose logging is disabled.

### Serialization Performance

| Operation | Cost | Mitigation |
|-----------|------|------------|
| String truncation | O(1) | Substring is fast |
| Byte array length | O(1) | Just read Length property |
| Collection enumeration | O(n) | Limit to first 5 items |
| Reflection | O(n) | Limit to 5 properties, depth 2 |

### Expected Overhead

| Scenario | Overhead |
|----------|----------|
| Verbose disabled | <0.1% (just IsEnabled check) |
| Verbose enabled, simple params | ~1-2ms per method |
| Verbose enabled, complex params | ~5-10ms per method |

## Configuration

### appsettings.json (Production)

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

### appsettings.Development.json (Development)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "PocketLawyer": "Verbose",
        "PocketLawyer.Infrastructure.Logging": "Verbose"
      }
    }
  }
}
```

### Selective Tracing

Enable tracing for specific namespaces:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "PocketLawyer.Application.Agents": "Verbose",
        "PocketLawyer.Infrastructure.AiServices": "Verbose"
      }
    }
  }
}
```

## Integration with Existing System

### Changes to Program.cs

```csharp
// Register ParameterSerializer
builder.Services.AddSingleton<IParameterSerializer, ParameterSerializer>();

// Register MethodTracingActionFilter globally
builder.Services.AddControllers(options =>
{
    options.Filters.Add<MethodTracingActionFilter>();
});

// Wrap IAiClient services with logging decorator
// Example for Gemini:
builder.Services.AddKeyedScoped<IAiClient>("gemini:inner:inner", (sp, key) =>
    sp.GetRequiredKeyedService<GeminiService>("gemini:inner"));

builder.Services.AddKeyedScoped<IAiClient>("gemini:inner", (sp, key) =>
    new LoggingAiClientDecorator(
        sp.GetRequiredKeyedService<IAiClient>("gemini:inner:inner"),
        sp.GetRequiredService<ILogger<LoggingAiClientDecorator>>(),
        sp.GetRequiredService<IParameterSerializer>()));

// Existing TimedAiClient decorator wraps the logging decorator
builder.Services.AddKeyedScoped<IAiClient>("gemini", (sp, key) =>
    new TimedAiClient(
        sp.GetRequiredKeyedService<IAiClient>("gemini:inner"),
        sp.GetRequiredService<ILogger<TimedAiClient>>()));
```

### Decorator Chain

```
Request → TimedAiClient → LoggingAiClientDecorator → GeminiService
          (measures time)  (traces calls)              (actual impl)
```

## Migration Strategy

### Phase 1: Core Infrastructure
1. Implement IParameterSerializer and ParameterSerializer
2. Write unit tests for serialization logic
3. Test with various parameter types

### Phase 2: Controller Tracing
1. Implement MethodTracingActionFilter
2. Register globally in Program.cs
3. Test with sample controller actions
4. Verify logs appear at Verbose level

### Phase 3: Service Tracing
1. Implement LoggingAiClientDecorator
2. Update service registration to use decorator
3. Test with AI service calls
4. Verify logs and performance

### Phase 4: Optimization
1. Verify short-circuit works (no overhead when disabled)
2. Measure performance impact
3. Adjust serialization limits if needed

## Security Considerations

### Sensitive Data

- Automatic redaction of password, secret, token, key parameters
- No logging of full request bodies (only parameter summaries)
- Verbose logging should be disabled in production

### Log Storage

- Trace logs can be large - ensure log rotation is configured
- Consider separate log files for trace logs
- Monitor disk usage

## Monitoring and Observability

### Metrics to Track

- Number of traced method calls per minute
- Average serialization time
- Log volume at Verbose level

### Log Messages

**Method Entry:**
- `"Entering {ClassName}.{MethodName} with parameters: {@Parameters}"`

**Method Exit:**
- `"Exiting {ClassName}.{MethodName} in {Duration}ms"`

**Exception:**
- `"Exception in {ClassName}.{MethodName} after {Duration}ms: {ExceptionType}"`

## Future Enhancements

### Potential Improvements

1. **Attribute-based control:** `[NoTrace]` attribute to exclude specific methods
2. **Custom serializers:** Allow registration of custom serializers for specific types
3. **Sampling:** Only trace X% of requests to reduce overhead
4. **Distributed tracing:** Integration with OpenTelemetry
5. **Call graph visualization:** Generate call graphs from trace logs

### Not Planned (Out of Scope)

- Automatic tracing of all methods (too much overhead)
- Tracing of private methods (use debugger instead)
- Real-time trace viewing UI
- Trace log analysis tools
