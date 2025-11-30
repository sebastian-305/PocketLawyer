using Microsoft.Extensions.Options;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using System.Reflection;

namespace PocketLawyer.Infrastructure.Context;

/// <summary>
/// Hosted service that initializes the context directory structure at application startup.
/// Scans all agent types for ContextStep attributes and creates corresponding directories.
/// </summary>
public class ContextDirectoryInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ContextOptions _options;
    private readonly ILogger<ContextDirectoryInitializer> _logger;

    public ContextDirectoryInitializer(
        IServiceProvider serviceProvider,
        IOptions<ContextOptions> options,
        ILogger<ContextDirectoryInitializer> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing context directories at {BasePath}", _options.BasePath);

        try
        {
            // Create base directory if it doesn't exist
            if (!Directory.Exists(_options.BasePath))
            {
                Directory.CreateDirectory(_options.BasePath);
                _logger.LogInformation("Created base context directory: {BasePath}", _options.BasePath);
            }

            // Discover all agent types implementing IContractAnalysisAgent
            var agentTypes = DiscoverAgentTypes();
            _logger.LogInformation("Discovered {Count} agent types", agentTypes.Count);

            // Process each agent type
            foreach (var agentType in agentTypes)
            {
                try
                {
                    await ProcessAgentTypeAsync(agentType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing agent type {AgentType}", agentType.Name);
                    // Continue with other agents
                }
            }

            _logger.LogInformation("Context directory initialization complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize context directories");
            // Don't throw - allow application to start
        }

        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // No cleanup needed
        return Task.CompletedTask;
    }

    /// <summary>
    /// Discovers all types that implement IContractAnalysisAgent.
    /// </summary>
    private List<Type> DiscoverAgentTypes()
    {
        var agentInterface = typeof(IContractAnalysisAgent);
        var agentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                    return Array.Empty<Type>();
                }
            })
            .Where(type => type.IsClass && !type.IsAbstract && agentInterface.IsAssignableFrom(type))
            .ToList();

        return agentTypes;
    }

    /// <summary>
    /// Processes a single agent type, scanning for ContextStep attributes and creating directories.
    /// </summary>
    private async Task ProcessAgentTypeAsync(Type agentType)
    {
        var agentName = ExtractAgentName(agentType.Name);
        _logger.LogDebug("Processing agent type: {AgentType} -> {AgentName}", agentType.Name, agentName);

        // Get all methods with ContextStep attribute
        var methods = agentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<ContextStepAttribute>() != null)
            .ToList();

        if (methods.Count == 0)
        {
            _logger.LogDebug("No ContextStep attributes found for agent {AgentName}", agentName);
            return;
        }

        _logger.LogInformation("Found {Count} methods with ContextStep attribute for agent {AgentName}", 
            methods.Count, agentName);

        // Track tiers we've seen to create common directories
        var tiersProcessed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Process each method
        foreach (var method in methods)
        {
            try
            {
                var attribute = method.GetCustomAttribute<ContextStepAttribute>();
                if (attribute == null) continue;

                // Infer tier from attribute or method name
                var tier = attribute.Tier ?? InferTierFromMethodName(method.Name);
                var stepName = NormalizeName(attribute.StepName);

                // Create step directory
                var stepPath = Path.Combine(_options.BasePath, agentName, tier, stepName);
                await CreateDirectoryWithReadmeAsync(stepPath, agentName, tier, stepName, isCommon: false);

                // Track this tier
                tiersProcessed.Add(tier);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process method {MethodName} for agent {AgentName}", 
                    method.Name, agentName);
                // Continue with other methods
            }
        }

        // Create common directories for each tier
        foreach (var tier in tiersProcessed)
        {
            try
            {
                var commonPath = Path.Combine(_options.BasePath, agentName, tier, "common");
                await CreateDirectoryWithReadmeAsync(commonPath, agentName, tier, "common", isCommon: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create common directory for agent {AgentName}, tier {Tier}", 
                    agentName, tier);
            }
        }

        // Create agent-level common directory
        try
        {
            var agentCommonPath = Path.Combine(_options.BasePath, agentName, "common");
            await CreateDirectoryWithReadmeAsync(agentCommonPath, agentName, null, "common", isCommon: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create agent-level common directory for agent {AgentName}", agentName);
        }
    }

    /// <summary>
    /// Creates a directory and optionally a README file.
    /// </summary>
    private async Task CreateDirectoryWithReadmeAsync(
        string directoryPath, 
        string agentName, 
        string? tier, 
        string stepName, 
        bool isCommon)
    {
        // Create directory if it doesn't exist
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _logger.LogInformation("Created context directory: {DirectoryPath}", directoryPath);
        }

        // Create README file if enabled and doesn't exist
        if (_options.CreateReadmeFiles)
        {
            var readmePath = Path.Combine(directoryPath, "README.md");
            if (!File.Exists(readmePath))
            {
                try
                {
                    var readmeContent = GenerateReadmeContent(agentName, tier, stepName, isCommon);
                    await File.WriteAllTextAsync(readmePath, readmeContent);
                    _logger.LogDebug("Created README file: {ReadmePath}", readmePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create README file: {ReadmePath}", readmePath);
                    // Continue - README creation is non-critical
                }
            }
        }
    }

    /// <summary>
    /// Generates README content for a context directory.
    /// </summary>
    private string GenerateReadmeContent(string agentName, string? tier, string stepName, bool isCommon)
    {
        if (isCommon)
        {
            if (tier == null)
            {
                // Agent-level common directory
                return $@"# Common Context for {agentName} Agent

This directory contains context files that are shared across all tiers and steps for the {agentName} agent.

## Usage

Place `.txt` or `.md` files in this directory. These files will be automatically loaded and included in all AI prompts for this agent, regardless of tier or step.

## File Format

- Supported extensions: `.txt`, `.md`
- Files are concatenated in alphabetical order
- Each file's content is prefixed with its filename as a header

## Examples

- Legal texts that apply to all analysis steps
- Reference documents
- Common definitions and terminology
";
            }
            else
            {
                // Tier-level common directory
                return $@"# Common Context for {agentName} Agent - {tier} Tier

This directory contains context files that are shared across all steps in the {tier} tier for the {agentName} agent.

## Usage

Place `.txt` or `.md` files in this directory. These files will be automatically loaded and included in all AI prompts for {tier} tier analysis steps.

## File Format

- Supported extensions: `.txt`, `.md`
- Files are concatenated in alphabetical order
- Each file's content is prefixed with its filename as a header

## Examples

- Legal texts specific to {tier} tier analysis
- Reference documents for {tier} tier
- Common definitions for {tier} tier steps
";
            }
        }
        else
        {
            // Step-specific directory
            return $@"# Context for {agentName} Agent - {tier} Tier - {stepName} Step

This directory contains context files specific to the ""{stepName}"" analysis step in the {tier} tier.

## Usage

Place `.txt` or `.md` files in this directory. These files will be automatically loaded and included in AI prompts when the {stepName} step is executed.

## File Format

- Supported extensions: `.txt`, `.md`
- Files are concatenated in alphabetical order
- Each file's content is prefixed with its filename as a header

## Examples

- Legal texts relevant to {stepName} analysis
- Reference documents for {stepName}
- Specific regulations or guidelines for {stepName}

## Context Loading Order

When this step executes, context is loaded in the following order:
1. Agent-level common context (if exists)
2. Tier-level common context (if exists)
3. Step-specific context (this directory)
";
        }
    }

    /// <summary>
    /// Extracts agent name from type name.
    /// </summary>
    private static string ExtractAgentName(string typeName)
    {
        // Remove "Agent" suffix if present
        if (typeName.EndsWith("Agent", StringComparison.OrdinalIgnoreCase))
        {
            typeName = typeName.Substring(0, typeName.Length - 5);
        }
        
        // Remove "Contract" suffix if present
        if (typeName.EndsWith("Contract", StringComparison.OrdinalIgnoreCase))
        {
            typeName = typeName.Substring(0, typeName.Length - 8);
        }

        return NormalizeName(typeName);
    }

    /// <summary>
    /// Infers tier from method name.
    /// </summary>
    private static string InferTierFromMethodName(string methodName)
    {
        if (methodName.Contains("Premium", StringComparison.OrdinalIgnoreCase))
        {
            return "premium";
        }
        
        if (methodName.Contains("Basic", StringComparison.OrdinalIgnoreCase))
        {
            return "basic";
        }

        return "basic";
    }

    /// <summary>
    /// Normalizes a name to a valid directory name.
    /// Converts to lowercase, replaces spaces with hyphens, removes special characters.
    /// </summary>
    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Convert to lowercase
        var normalized = name.ToLowerInvariant();

        // Replace spaces and underscores with hyphens
        normalized = normalized.Replace(" ", "-").Replace("_", "-");

        // Remove special characters (keep only alphanumeric and hyphens)
        normalized = new string(normalized.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Remove consecutive hyphens
        while (normalized.Contains("--"))
        {
            normalized = normalized.Replace("--", "-");
        }

        // Trim hyphens from start and end
        normalized = normalized.Trim('-');

        return normalized;
    }
}
