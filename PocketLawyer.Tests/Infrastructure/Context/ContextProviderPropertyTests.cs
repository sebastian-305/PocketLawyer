using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.Context;
using PocketLawyer.Options;
using Options = Microsoft.Extensions.Options.Options;

namespace PocketLawyer.Tests.Infrastructure.Context;

/// <summary>
/// Property-based tests for ContextProvider.
/// Feature: context-management
/// </summary>
public class ContextProviderPropertyTests : IDisposable
{
    private readonly string _testDirectory;

    public ContextProviderPropertyTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"context-provider-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// **Feature: context-management, Property 4: Cache consistency**
    /// For any context request within the same HTTP request scope, the second request 
    /// should return cached content identical to the first request
    /// **Validates: Requirements 5.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CacheConsistency_SecondRequestReturnsCachedContent()
    {
        var agentNameGen = Gen.Elements("rental", "employment", "test-agent");
        var tierGen = Gen.Elements("basic", "premium");
        var stepNameGen = Gen.Elements("general-data", "costs", "summary", "test-step");
        
        var paramsGen = from agent in agentNameGen
                        from tier in tierGen
                        from step in stepNameGen
                        select (agent, tier, step);

        return Prop.ForAll(
            paramsGen.ToArbitrary(),
            (contextParams) =>
            {
                var (agentName, tier, stepName) = contextParams;

                // Arrange - Create test directory structure with some content
                var normalizedAgent = agentName.ToLowerInvariant().Replace(" ", "-");
                var normalizedTier = tier.ToLowerInvariant().Replace(" ", "-");
                var normalizedStep = stepName.ToLowerInvariant().Replace(" ", "-");
                
                var stepDir = Path.Combine(_testDirectory, normalizedAgent, normalizedTier, normalizedStep);
                Directory.CreateDirectory(stepDir);
                
                var testFile = Path.Combine(stepDir, "test.txt");
                File.WriteAllTextAsync(testFile, $"Test content for {agentName}/{tier}/{stepName}").GetAwaiter().GetResult();

                // Create ContextProvider with real file reader
                var options = Microsoft.Extensions.Options.Options.Create(new ContextOptions { BasePath = _testDirectory });
                var fileReaderLogger = new Mock<ILogger<ContextFileReader>>();
                var fileReader = new ContextFileReader(fileReaderLogger.Object);
                var providerLogger = new Mock<ILogger<ContextProvider>>();
                var provider = new ContextProvider(options, fileReader, providerLogger.Object);

                // Act - Load context twice
                var result1 = provider.GetContextAsync(agentName, tier, stepName).GetAwaiter().GetResult();
                var result2 = provider.GetContextAsync(agentName, tier, stepName).GetAwaiter().GetResult();

                // Assert - Both results should be identical
                var resultsIdentical = result1 == result2;
                
                // Also verify that the content is not empty (we created a file)
                var contentNotEmpty = !string.IsNullOrEmpty(result1);

                // Cleanup
                if (Directory.Exists(stepDir))
                {
                    Directory.Delete(Path.Combine(_testDirectory, normalizedAgent), true);
                }

                return resultsIdentical && contentNotEmpty;
            });
    }

    /// <summary>
    /// **Feature: context-management, Property 4: Cache consistency**
    /// For any sequence of context requests with different parameters, each unique 
    /// combination should be cached independently
    /// **Validates: Requirements 5.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CacheConsistency_DifferentParametersHaveIndependentCache()
    {
        var agentNameGen = Gen.Elements("rental", "employment");
        var tierGen = Gen.Elements("basic", "premium");
        var stepNameGen = Gen.Elements("step1", "step2");

        return Prop.ForAll(
            agentNameGen.ToArbitrary(),
            tierGen.ToArbitrary(),
            stepNameGen.ToArbitrary(),
            (agent1, tier1, step1) =>
            {
                // Generate second set of parameters that's different
                var agent2 = agent1 == "rental" ? "employment" : "rental";
                var tier2 = tier1 == "basic" ? "premium" : "basic";
                var step2 = step1 == "step1" ? "step2" : "step1";

                // Arrange - Create test directory structures with different content
                var dir1 = Path.Combine(_testDirectory, agent1, tier1, step1);
                var dir2 = Path.Combine(_testDirectory, agent2, tier2, step2);
                Directory.CreateDirectory(dir1);
                Directory.CreateDirectory(dir2);
                
                File.WriteAllTextAsync(Path.Combine(dir1, "test.txt"), "Content 1").GetAwaiter().GetResult();
                File.WriteAllTextAsync(Path.Combine(dir2, "test.txt"), "Content 2").GetAwaiter().GetResult();

                // Create ContextProvider
                var options = Microsoft.Extensions.Options.Options.Create(new ContextOptions { BasePath = _testDirectory });
                var fileReaderLogger = new Mock<ILogger<ContextFileReader>>();
                var fileReader = new ContextFileReader(fileReaderLogger.Object);
                var providerLogger = new Mock<ILogger<ContextProvider>>();
                var provider = new ContextProvider(options, fileReader, providerLogger.Object);

                // Act - Load both contexts
                var result1 = provider.GetContextAsync(agent1, tier1, step1).GetAwaiter().GetResult();
                var result2 = provider.GetContextAsync(agent2, tier2, step2).GetAwaiter().GetResult();
                
                // Load first one again to verify cache
                var result1Again = provider.GetContextAsync(agent1, tier1, step1).GetAwaiter().GetResult();

                // Assert - Results should be different but first should be cached
                var resultsDifferent = result1 != result2;
                var firstResultCached = result1 == result1Again;
                var result1ContainsContent1 = result1.Contains("Content 1");
                var result2ContainsContent2 = result2.Contains("Content 2");

                // Cleanup
                if (Directory.Exists(Path.Combine(_testDirectory, agent1)))
                {
                    Directory.Delete(Path.Combine(_testDirectory, agent1), true);
                }
                if (Directory.Exists(Path.Combine(_testDirectory, agent2)))
                {
                    Directory.Delete(Path.Combine(_testDirectory, agent2), true);
                }

                return resultsDifferent && firstResultCached && result1ContainsContent1 && result2ContainsContent2;
            });
    }

    /// <summary>
    /// **Feature: context-management, Property 5: Common context inclusion order**
    /// For any step-specific context request, if common directories exist at agent or tier level, 
    /// their content should appear before step-specific content in the final output
    /// **Validates: Requirements 9.2, 9.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CommonContextInclusionOrder_AgentThenTierThenStep()
    {
        var agentNameGen = Gen.Elements("rental", "employment");
        var tierGen = Gen.Elements("basic", "premium");
        var stepNameGen = Gen.Elements("step1", "step2");
        
        var paramsGen = from agent in agentNameGen
                        from tier in tierGen
                        from step in stepNameGen
                        select (agent, tier, step);

        return Prop.ForAll(
            paramsGen.ToArbitrary(),
            (contextParams) =>
            {
                var (agentName, tier, stepName) = contextParams;

                // Arrange - Create directory structure with common and step-specific content
                var agentDir = Path.Combine(_testDirectory, agentName);
                var tierDir = Path.Combine(agentDir, tier);
                var stepDir = Path.Combine(tierDir, stepName);
                var agentCommonDir = Path.Combine(agentDir, "common");
                var tierCommonDir = Path.Combine(tierDir, "common");
                
                Directory.CreateDirectory(stepDir);
                Directory.CreateDirectory(agentCommonDir);
                Directory.CreateDirectory(tierCommonDir);
                
                // Create files with identifiable content
                File.WriteAllTextAsync(Path.Combine(agentCommonDir, "agent-common.txt"), "AGENT_COMMON_CONTENT").GetAwaiter().GetResult();
                File.WriteAllTextAsync(Path.Combine(tierCommonDir, "tier-common.txt"), "TIER_COMMON_CONTENT").GetAwaiter().GetResult();
                File.WriteAllTextAsync(Path.Combine(stepDir, "step-specific.txt"), "STEP_SPECIFIC_CONTENT").GetAwaiter().GetResult();

                // Create ContextProvider
                var options = Microsoft.Extensions.Options.Options.Create(new ContextOptions { BasePath = _testDirectory });
                var fileReaderLogger = new Mock<ILogger<ContextFileReader>>();
                var fileReader = new ContextFileReader(fileReaderLogger.Object);
                var providerLogger = new Mock<ILogger<ContextProvider>>();
                var provider = new ContextProvider(options, fileReader, providerLogger.Object);

                // Act - Load context
                var result = provider.GetContextAsync(agentName, tier, stepName).GetAwaiter().GetResult();

                // Assert - Check order: agent common should appear before tier common, which should appear before step-specific
                var agentCommonIndex = result.IndexOf("AGENT_COMMON_CONTENT", StringComparison.Ordinal);
                var tierCommonIndex = result.IndexOf("TIER_COMMON_CONTENT", StringComparison.Ordinal);
                var stepSpecificIndex = result.IndexOf("STEP_SPECIFIC_CONTENT", StringComparison.Ordinal);

                // All content should be present
                var allContentPresent = agentCommonIndex >= 0 && tierCommonIndex >= 0 && stepSpecificIndex >= 0;
                
                // Order should be: agent common < tier common < step specific
                var correctOrder = agentCommonIndex < tierCommonIndex && tierCommonIndex < stepSpecificIndex;

                // Cleanup
                if (Directory.Exists(agentDir))
                {
                    Directory.Delete(agentDir, true);
                }

                return allContentPresent && correctOrder;
            });
    }

    /// <summary>
    /// **Feature: context-management, Property 5: Common context inclusion order**
    /// For any step-specific context request, if only some common directories exist, 
    /// the existing ones should still be included in the correct order
    /// **Validates: Requirements 9.2, 9.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CommonContextInclusionOrder_PartialCommonDirectories()
    {
        var agentNameGen = Gen.Elements("rental", "employment");
        var tierGen = Gen.Elements("basic", "premium");
        var stepNameGen = Gen.Elements("step1", "step2");
        var hasAgentCommonGen = Gen.Elements(true, false);
        var hasTierCommonGen = Gen.Elements(true, false);
        
        var paramsGen = from agent in agentNameGen
                        from tier in tierGen
                        from step in stepNameGen
                        from hasAgentCommon in hasAgentCommonGen
                        from hasTierCommon in hasTierCommonGen
                        select (agent, tier, step, hasAgentCommon, hasTierCommon);

        return Prop.ForAll(
            paramsGen.ToArbitrary(),
            (contextParams) =>
            {
                var (agentName, tier, stepName, hasAgentCommon, hasTierCommon) = contextParams;

                // Arrange - Create directory structure with optional common directories
                var agentDir = Path.Combine(_testDirectory, agentName);
                var tierDir = Path.Combine(agentDir, tier);
                var stepDir = Path.Combine(tierDir, stepName);
                
                Directory.CreateDirectory(stepDir);
                
                // Optionally create common directories
                if (hasAgentCommon)
                {
                    var agentCommonDir = Path.Combine(agentDir, "common");
                    Directory.CreateDirectory(agentCommonDir);
                    File.WriteAllTextAsync(Path.Combine(agentCommonDir, "agent-common.txt"), "AGENT_COMMON_CONTENT").GetAwaiter().GetResult();
                }
                
                if (hasTierCommon)
                {
                    var tierCommonDir = Path.Combine(tierDir, "common");
                    Directory.CreateDirectory(tierCommonDir);
                    File.WriteAllTextAsync(Path.Combine(tierCommonDir, "tier-common.txt"), "TIER_COMMON_CONTENT").GetAwaiter().GetResult();
                }
                
                File.WriteAllTextAsync(Path.Combine(stepDir, "step-specific.txt"), "STEP_SPECIFIC_CONTENT").GetAwaiter().GetResult();

                // Create ContextProvider
                var options = Microsoft.Extensions.Options.Options.Create(new ContextOptions { BasePath = _testDirectory });
                var fileReaderLogger = new Mock<ILogger<ContextFileReader>>();
                var fileReader = new ContextFileReader(fileReaderLogger.Object);
                var providerLogger = new Mock<ILogger<ContextProvider>>();
                var provider = new ContextProvider(options, fileReader, providerLogger.Object);

                // Act - Load context
                var result = provider.GetContextAsync(agentName, tier, stepName).GetAwaiter().GetResult();

                // Assert - Check that existing content appears in correct order
                var agentCommonIndex = result.IndexOf("AGENT_COMMON_CONTENT", StringComparison.Ordinal);
                var tierCommonIndex = result.IndexOf("TIER_COMMON_CONTENT", StringComparison.Ordinal);
                var stepSpecificIndex = result.IndexOf("STEP_SPECIFIC_CONTENT", StringComparison.Ordinal);

                // Step-specific content should always be present
                var stepContentPresent = stepSpecificIndex >= 0;
                
                // If agent common exists, it should be present and before tier common (if exists) and step
                var agentCommonCorrect = !hasAgentCommon || 
                    (agentCommonIndex >= 0 && 
                     (!hasTierCommon || agentCommonIndex < tierCommonIndex) && 
                     agentCommonIndex < stepSpecificIndex);
                
                // If tier common exists, it should be present and before step
                var tierCommonCorrect = !hasTierCommon || 
                    (tierCommonIndex >= 0 && tierCommonIndex < stepSpecificIndex);

                // Cleanup
                if (Directory.Exists(agentDir))
                {
                    Directory.Delete(agentDir, true);
                }

                return stepContentPresent && agentCommonCorrect && tierCommonCorrect;
            });
    }

    /// <summary>
    /// **Feature: context-management, Property: Path construction consistency**
    /// For any agent name, tier, and step name, the constructed path should follow 
    /// the format {BasePath}/{AgentName}/{Tier}/{StepName}
    /// **Validates: Requirements 2.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property PathConstructionConsistency_FollowsExpectedFormat()
    {
        var agentNameGen = Gen.Elements("rental", "employment", "test-agent", "Rental", "Employment");
        var tierGen = Gen.Elements("basic", "premium", "Basic", "Premium");
        var stepNameGen = Gen.Elements("general-data", "costs", "summary", "General Data", "Rent Conditions");
        
        var paramsGen = from agent in agentNameGen
                        from tier in tierGen
                        from step in stepNameGen
                        select (agent, tier, step);

        return Prop.ForAll(
            paramsGen.ToArbitrary(),
            (contextParams) =>
            {
                var (agentName, tier, stepName) = contextParams;

                // Arrange - Create test directory structure
                var normalizedAgent = agentName.ToLowerInvariant().Replace(" ", "-").Replace("_", "-");
                var normalizedTier = tier.ToLowerInvariant().Replace(" ", "-").Replace("_", "-");
                var normalizedStep = stepName.ToLowerInvariant().Replace(" ", "-").Replace("_", "-");
                
                // Expected path format: {BasePath}/{AgentName}/{Tier}/{StepName}
                var expectedPath = Path.Combine(_testDirectory, normalizedAgent, normalizedTier, normalizedStep);
                
                // Create the directory and a test file
                Directory.CreateDirectory(expectedPath);
                var testFile = Path.Combine(expectedPath, "test.txt");
                File.WriteAllTextAsync(testFile, $"Content for {agentName}/{tier}/{stepName}").GetAwaiter().GetResult();

                // Create ContextProvider
                var options = Microsoft.Extensions.Options.Options.Create(new ContextOptions { BasePath = _testDirectory });
                var fileReaderLogger = new Mock<ILogger<ContextFileReader>>();
                var fileReader = new ContextFileReader(fileReaderLogger.Object);
                var providerLogger = new Mock<ILogger<ContextProvider>>();
                var provider = new ContextProvider(options, fileReader, providerLogger.Object);

                // Act - Load context
                var result = provider.GetContextAsync(agentName, tier, stepName).GetAwaiter().GetResult();

                // Assert - If the path was constructed correctly, we should get the content
                var contentLoaded = !string.IsNullOrEmpty(result);
                var contentMatches = result.Contains($"Content for {agentName}/{tier}/{stepName}");

                // Verify the path exists (which means it was constructed correctly)
                var pathExists = Directory.Exists(expectedPath);

                // Cleanup
                if (Directory.Exists(Path.Combine(_testDirectory, normalizedAgent)))
                {
                    Directory.Delete(Path.Combine(_testDirectory, normalizedAgent), true);
                }

                return contentLoaded && contentMatches && pathExists;
            });
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
