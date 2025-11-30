using Microsoft.Extensions.Options;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using System.Diagnostics;
using System.Reflection;

namespace PocketLawyer.Infrastructure.Context;

public class ContextProvider : IContextProvider
{
    private readonly ContextOptions _options;
    private readonly IContextFileReader _fileReader;
    private readonly ILogger<ContextProvider> _logger;
    private readonly Dictionary<string, string> _cache;

    public ContextProvider(
        IOptions<ContextOptions> options,
        IContextFileReader fileReader,
        ILogger<ContextProvider> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = new Dictionary<string, string>();
    }

    public async Task<string> GetContextAsync(string agentName, string tier, string stepName)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            throw new ArgumentNullException(nameof(agentName));
        if (string.IsNullOrWhiteSpace(tier))
            throw new ArgumentNullException(nameof(tier));
        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentNullException(nameof(stepName));

        var cacheKey = $"{agentName}:{tier}:{stepName}";
        
        // Check cache first
        if (_cache.TryGetValue(cacheKey, out var cachedContext))
        {
            _logger.LogDebug("Returning cached context for {CacheKey}", cacheKey);
            return cachedContext;
        }

        var stopwatch = Stopwatch.StartNew();
        var contentBuilder = new System.Text.StringBuilder();

        // Normalize names for directory paths
        var normalizedAgentName = NormalizeName(agentName);
        var normalizedTier = NormalizeName(tier);
        var normalizedStepName = NormalizeName(stepName);

        // 1. Load agent-level common context
        var agentCommonPath = Path.Combine(_options.BasePath, normalizedAgentName, "common");
        var agentCommonContext = await _fileReader.ReadDirectoryAsync(agentCommonPath);
        if (!string.IsNullOrWhiteSpace(agentCommonContext))
        {
            contentBuilder.AppendLine(agentCommonContext);
        }

        // 2. Load tier-level common context
        var tierCommonPath = Path.Combine(_options.BasePath, normalizedAgentName, normalizedTier, "common");
        var tierCommonContext = await _fileReader.ReadDirectoryAsync(tierCommonPath);
        if (!string.IsNullOrWhiteSpace(tierCommonContext))
        {
            if (contentBuilder.Length > 0)
                contentBuilder.AppendLine();
            contentBuilder.AppendLine(tierCommonContext);
        }

        // 3. Load step-specific context
        var stepPath = Path.Combine(_options.BasePath, normalizedAgentName, normalizedTier, normalizedStepName);
        var stepContext = await _fileReader.ReadDirectoryAsync(stepPath);
        if (!string.IsNullOrWhiteSpace(stepContext))
        {
            if (contentBuilder.Length > 0)
                contentBuilder.AppendLine();
            contentBuilder.AppendLine(stepContext);
        }

        var result = contentBuilder.ToString().TrimEnd();
        stopwatch.Stop();

        // Log performance metrics
        var sizeInBytes = System.Text.Encoding.UTF8.GetByteCount(result);
        _logger.LogDebug(
            "Loaded context for {AgentName}/{Tier}/{StepName} in {Duration}ms ({Size} chars, {Bytes} bytes)",
            agentName, tier, stepName, stopwatch.ElapsedMilliseconds, result.Length, sizeInBytes);

        // Warn if context exceeds max size
        if (sizeInBytes > _options.MaxContextSizeBytes)
        {
            _logger.LogWarning(
                "Context size ({Bytes} bytes) exceeds recommended maximum ({MaxBytes} bytes) for {AgentName}/{Tier}/{StepName}",
                sizeInBytes, _options.MaxContextSizeBytes, agentName, tier, stepName);
        }

        // Cache the result
        _cache[cacheKey] = result;

        return result;
    }

    public async Task<string> GetContextForCurrentStepAsync()
    {
        // Use StackTrace to find the calling method
        var stackTrace = new StackTrace();
        MethodBase? callingMethod = null;
        
        // Start from frame 1 (skip this method) and look for a method with ContextStepAttribute
        for (int i = 1; i < stackTrace.FrameCount; i++)
        {
            var frame = stackTrace.GetFrame(i);
            var method = frame?.GetMethod();
            
            if (method != null && method.GetCustomAttribute<ContextStepAttribute>() != null)
            {
                callingMethod = method;
                break;
            }
        }

        if (callingMethod == null)
        {
            _logger.LogWarning("GetContextForCurrentStepAsync called from a method without ContextStepAttribute");
            return string.Empty;
        }

        // Get the ContextStep attribute
        var attribute = callingMethod.GetCustomAttribute<ContextStepAttribute>();
        if (attribute == null)
        {
            return string.Empty;
        }

        // Extract agent name from declaring type
        var declaringType = callingMethod.DeclaringType;
        if (declaringType == null)
        {
            _logger.LogWarning("Could not determine declaring type for method {MethodName}", callingMethod.Name);
            return string.Empty;
        }

        var agentName = ExtractAgentName(declaringType.Name);

        // Infer tier from attribute or method name
        var tier = attribute.Tier ?? InferTierFromMethodName(callingMethod.Name);

        // Normalize step name
        var stepName = NormalizeName(attribute.StepName);

        _logger.LogDebug(
            "Resolved context parameters from stack trace: Agent={AgentName}, Tier={Tier}, Step={StepName}",
            agentName, tier, stepName);

        // Call GetContextAsync with resolved parameters
        return await GetContextAsync(agentName, tier, stepName);
    }

    private static string ExtractAgentName(string typeName)
    {
        // Remove "Agent" suffix if present
        // e.g., "RentalContractAgent" -> "Rental"
        if (typeName.EndsWith("Agent", StringComparison.OrdinalIgnoreCase))
        {
            typeName = typeName.Substring(0, typeName.Length - 5);
        }
        
        // Remove "Contract" suffix if present
        if (typeName.EndsWith("Contract", StringComparison.OrdinalIgnoreCase))
        {
            typeName = typeName.Substring(0, typeName.Length - 8);
        }

        return typeName;
    }

    private static string InferTierFromMethodName(string methodName)
    {
        // Check if method name contains "Premium" or "Basic"
        if (methodName.Contains("Premium", StringComparison.OrdinalIgnoreCase))
        {
            return "Premium";
        }
        
        if (methodName.Contains("Basic", StringComparison.OrdinalIgnoreCase))
        {
            return "Basic";
        }

        // Default to "basic" if cannot infer
        return "basic";
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
    }
}
