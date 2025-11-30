using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.Context;
using PocketLawyer.Options;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.Context;

/// <summary>
/// Unit tests for ContextProvider.
/// </summary>
public class ContextProviderTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ContextProvider _provider;
    private readonly Mock<ILogger<ContextFileReader>> _fileReaderLogger;
    private readonly Mock<ILogger<ContextProvider>> _providerLogger;

    public ContextProviderTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"context-provider-unit-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        var options = Microsoft.Extensions.Options.Options.Create(new ContextOptions { BasePath = _testDirectory });
        _fileReaderLogger = new Mock<ILogger<ContextFileReader>>();
        var fileReader = new ContextFileReader(_fileReaderLogger.Object);
        _providerLogger = new Mock<ILogger<ContextProvider>>();
        _provider = new ContextProvider(options, fileReader, _providerLogger.Object);
    }

    [Fact]
    public async Task GetContextAsync_EmptyDirectory_ReturnsEmptyString()
    {
        // Arrange - Create empty directory structure
        var agentName = "rental";
        var tier = "basic";
        var stepName = "test-step";
        
        var stepDir = Path.Combine(_testDirectory, agentName, tier, stepName);
        Directory.CreateDirectory(stepDir);

        // Act
        var result = await _provider.GetContextAsync(agentName, tier, stepName);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetContextAsync_NonExistentDirectory_ReturnsEmptyString()
    {
        // Arrange - Don't create any directories
        var agentName = "nonexistent";
        var tier = "basic";
        var stepName = "test-step";

        // Act
        var result = await _provider.GetContextAsync(agentName, tier, stepName);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetContextAsync_OnlyCommonDirectoriesEmpty_ReturnsEmptyString()
    {
        // Arrange - Create empty common directories
        var agentName = "rental";
        var tier = "basic";
        var stepName = "test-step";
        
        var agentCommonDir = Path.Combine(_testDirectory, agentName, "common");
        var tierCommonDir = Path.Combine(_testDirectory, agentName, tier, "common");
        var stepDir = Path.Combine(_testDirectory, agentName, tier, stepName);
        
        Directory.CreateDirectory(agentCommonDir);
        Directory.CreateDirectory(tierCommonDir);
        Directory.CreateDirectory(stepDir);

        // Act
        var result = await _provider.GetContextAsync(agentName, tier, stepName);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetContextAsync_WithContent_ReturnsNonEmptyString()
    {
        // Arrange - Create directory with content
        var agentName = "rental";
        var tier = "basic";
        var stepName = "test-step";
        
        var stepDir = Path.Combine(_testDirectory, agentName, tier, stepName);
        Directory.CreateDirectory(stepDir);
        
        await File.WriteAllTextAsync(Path.Combine(stepDir, "test.txt"), "Test content");

        // Act
        var result = await _provider.GetContextAsync(agentName, tier, stepName);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("Test content", result);
    }

    [Fact]
    public async Task GetContextAsync_NullAgentName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _provider.GetContextAsync(null!, "basic", "step"));
    }

    [Fact]
    public async Task GetContextAsync_NullTier_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _provider.GetContextAsync("rental", null!, "step"));
    }

    [Fact]
    public async Task GetContextAsync_NullStepName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _provider.GetContextAsync("rental", "basic", null!));
    }

    [Fact]
    public async Task GetContextAsync_EmptyAgentName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _provider.GetContextAsync("", "basic", "step"));
    }

    [Fact]
    public async Task GetContextAsync_WhitespaceAgentName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _provider.GetContextAsync("   ", "basic", "step"));
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
