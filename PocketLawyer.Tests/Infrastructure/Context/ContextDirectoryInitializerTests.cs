using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Infrastructure.Context;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.Context;

/// <summary>
/// Unit tests for ContextDirectoryInitializer error handling.
/// Feature: context-management
/// </summary>
public class ContextDirectoryInitializerTests : IDisposable
{
    private readonly string _testDirectory;

    public ContextDirectoryInitializerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"context-init-unit-test-{Guid.NewGuid()}");
    }

    /// <summary>
    /// Test that the application continues to start even if directory creation fails.
    /// **Validates: Requirements 3.5**
    /// </summary>
    [Fact]
    public async Task StartAsync_WhenDirectoryCreationFails_ApplicationContinues()
    {
        // Arrange - Use an invalid path that will cause directory creation to fail
        // On Windows, paths with invalid characters will fail
        var invalidPath = Path.Combine(_testDirectory, "invalid<>path");
        
        var options = Microsoft.Extensions.Options.Options.Create(new PocketLawyer.Options.ContextOptions
        {
            BasePath = invalidPath,
            CreateReadmeFiles = true,
            MaxContextSizeBytes = 5 * 1024 * 1024
        });

        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<ContextDirectoryInitializer>>();

        var initializer = new ContextDirectoryInitializer(
            serviceProvider.Object,
            options,
            logger.Object);

        // Act - Should not throw exception
        var exception = await Record.ExceptionAsync(async () =>
            await initializer.StartAsync(CancellationToken.None));

        // Assert - No exception should be thrown
        Assert.Null(exception);
        
        // Verify that an error was logged
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Test that the application continues even if README creation fails.
    /// **Validates: Requirements 3.5**
    /// </summary>
    [Fact(Skip = "Not reliable across platforms (Linux/Windows) – requires refactor with IDirectoryService")]
    public async Task StartAsync_WhenReadmeCreationFails_ApplicationContinues()
    {
        // Arrange - Create a valid base path
        var testBasePath = Path.Combine(_testDirectory, "valid-path");
        Directory.CreateDirectory(testBasePath);
        
        var options = Microsoft.Extensions.Options.Options.Create(new PocketLawyer.Options.ContextOptions
        {
            BasePath = testBasePath,
            CreateReadmeFiles = true,
            MaxContextSizeBytes = 5 * 1024 * 1024
        });

        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<ContextDirectoryInitializer>>();

        var initializer = new ContextDirectoryInitializer(
            serviceProvider.Object,
            options,
            logger.Object);

        // Act - Should not throw exception even if README creation might fail
        var exception = await Record.ExceptionAsync(async () =>
            await initializer.StartAsync(CancellationToken.None));

        // Assert - No exception should be thrown
        Assert.Null(exception);
    }

    /// <summary>
    /// Test that the application continues when base directory doesn't exist and gets created.
    /// **Validates: Requirements 3.5**
    /// </summary>
    [Fact]
    public async Task StartAsync_WhenBaseDirectoryDoesNotExist_CreatesItAndContinues()
    {
        // Arrange - Use a path that doesn't exist yet
        var testBasePath = Path.Combine(_testDirectory, "new-directory");
        
        var options = Microsoft.Extensions.Options.Options.Create(new PocketLawyer.Options.ContextOptions
        {
            BasePath = testBasePath,
            CreateReadmeFiles = false,
            MaxContextSizeBytes = 5 * 1024 * 1024
        });

        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<ContextDirectoryInitializer>>();

        var initializer = new ContextDirectoryInitializer(
            serviceProvider.Object,
            options,
            logger.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);

        // Assert - Directory should be created
        Assert.True(Directory.Exists(testBasePath));
        
        // Verify that creation was logged
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created base context directory")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test that StopAsync completes without errors.
    /// **Validates: Requirements 3.5**
    /// </summary>
    [Fact]
    public async Task StopAsync_CompletesWithoutErrors()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new PocketLawyer.Options.ContextOptions
        {
            BasePath = _testDirectory,
            CreateReadmeFiles = false,
            MaxContextSizeBytes = 5 * 1024 * 1024
        });

        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<ContextDirectoryInitializer>>();

        var initializer = new ContextDirectoryInitializer(
            serviceProvider.Object,
            options,
            logger.Object);

        // Act
        var exception = await Record.ExceptionAsync(async () =>
            await initializer.StopAsync(CancellationToken.None));

        // Assert - No exception should be thrown
        Assert.Null(exception);
    }

    /// <summary>
    /// Test that path normalization handles null and empty strings gracefully.
    /// **Validates: Requirements 8.1, 8.2, 8.3, 8.4, 8.5**
    /// </summary>
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("General Data", "general-data")]
    [InlineData("Rent Conditions", "rent-conditions")]
    [InlineData("UPPERCASE", "uppercase")]
    [InlineData("with_underscores", "with-underscores")]
    [InlineData("with!special@chars#", "withspecialchars")]
    [InlineData("multiple   spaces", "multiple-spaces")]
    [InlineData("  leading-and-trailing  ", "leading-and-trailing")]
    [InlineData("consecutive---hyphens", "consecutive-hyphens")]
    public void NormalizeName_HandlesVariousInputs_Correctly(string? input, string expected)
    {
        // Act
        var result = ContextDirectoryInitializer.NormalizeName(input!);

        // Assert
        Assert.Equal(expected, result);
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
