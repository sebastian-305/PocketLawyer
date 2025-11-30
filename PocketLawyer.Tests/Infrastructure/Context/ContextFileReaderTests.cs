using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Infrastructure.Context;

namespace PocketLawyer.Tests.Infrastructure.Context;

/// <summary>
/// Unit tests for ContextFileReader.
/// Feature: context-management
/// </summary>
public class ContextFileReaderTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ContextFileReader _reader;
    private readonly Mock<ILogger<ContextFileReader>> _loggerMock;

    public ContextFileReaderTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"context-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        _loggerMock = new Mock<ILogger<ContextFileReader>>();
        _reader = new ContextFileReader(_loggerMock.Object);
    }

    /// <summary>
    /// Test that empty directories return empty string
    /// **Validates: Requirements 4.5**
    /// </summary>
    [Fact]
    public async Task ReadDirectoryAsync_EmptyDirectory_ReturnsEmptyString()
    {
        // Arrange
        var emptyDir = Path.Combine(_testDirectory, "empty");
        Directory.CreateDirectory(emptyDir);

        // Act
        var result = await _reader.ReadDirectoryAsync(emptyDir);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Test that non-existent directories return empty string
    /// **Validates: Requirements 4.5**
    /// </summary>
    [Fact]
    public async Task ReadDirectoryAsync_NonExistentDirectory_ReturnsEmptyString()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "does-not-exist");

        // Act
        var result = await _reader.ReadDirectoryAsync(nonExistentDir);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Test that README.md is excluded from context
    /// **Validates: Requirements 4.5**
    /// </summary>
    [Fact]
    public async Task ReadDirectoryAsync_WithReadmeMd_ExcludesReadme()
    {
        // Arrange
        var testDir = Path.Combine(_testDirectory, "readme-test");
        Directory.CreateDirectory(testDir);
        
        await File.WriteAllTextAsync(Path.Combine(testDir, "README.md"), "This is a README");
        await File.WriteAllTextAsync(Path.Combine(testDir, "context.txt"), "This is context");

        // Act
        var result = await _reader.ReadDirectoryAsync(testDir);

        // Assert
        Assert.DoesNotContain("README.md", result);
        Assert.Contains("context.txt", result);
        Assert.Contains("This is context", result);
    }

    /// <summary>
    /// Test that unreadable files are skipped gracefully
    /// **Validates: Requirements 4.5**
    /// </summary>
    [Fact]
    public async Task ReadDirectoryAsync_WithUnreadableFile_SkipsFileAndContinues()
    {
        // Arrange
        var testDir = Path.Combine(_testDirectory, "unreadable-test");
        Directory.CreateDirectory(testDir);
        
        var readableFile = Path.Combine(testDir, "readable.txt");
        var unreadableFile = Path.Combine(testDir, "unreadable.txt");
        
        await File.WriteAllTextAsync(readableFile, "Readable content");
        await File.WriteAllTextAsync(unreadableFile, "Unreadable content");

        // Lock the unreadable file
        using (var stream = File.Open(unreadableFile, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            // Act
            var result = await _reader.ReadDirectoryAsync(testDir);

            // Assert - Should contain readable file but not unreadable
            Assert.Contains("readable.txt", result);
            Assert.Contains("Readable content", result);
            
            // Verify warning was logged for unreadable file
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("unreadable.txt")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// Test that files are sorted alphabetically
    /// **Validates: Requirements 4.3**
    /// </summary>
    [Fact]
    public async Task ReadDirectoryAsync_MultipleFiles_SortsAlphabetically()
    {
        // Arrange
        var testDir = Path.Combine(_testDirectory, "sort-test");
        Directory.CreateDirectory(testDir);
        
        await File.WriteAllTextAsync(Path.Combine(testDir, "zebra.txt"), "Z");
        await File.WriteAllTextAsync(Path.Combine(testDir, "alpha.txt"), "A");
        await File.WriteAllTextAsync(Path.Combine(testDir, "beta.txt"), "B");

        // Act
        var result = await _reader.ReadDirectoryAsync(testDir);

        // Assert - Files should appear in alphabetical order
        var alphaIndex = result.IndexOf("alpha.txt");
        var betaIndex = result.IndexOf("beta.txt");
        var zebraIndex = result.IndexOf("zebra.txt");

        Assert.True(alphaIndex < betaIndex);
        Assert.True(betaIndex < zebraIndex);
    }

    /// <summary>
    /// Test that file contents are concatenated with headers
    /// **Validates: Requirements 4.4**
    /// </summary>
    [Fact]
    public async Task ReadDirectoryAsync_MultipleFiles_ConcatenatesWithHeaders()
    {
        // Arrange
        var testDir = Path.Combine(_testDirectory, "concat-test");
        Directory.CreateDirectory(testDir);
        
        await File.WriteAllTextAsync(Path.Combine(testDir, "file1.txt"), "Content 1");
        await File.WriteAllTextAsync(Path.Combine(testDir, "file2.txt"), "Content 2");

        // Act
        var result = await _reader.ReadDirectoryAsync(testDir);

        // Assert
        Assert.Contains("### file1.txt", result);
        Assert.Contains("Content 1", result);
        Assert.Contains("### file2.txt", result);
        Assert.Contains("Content 2", result);
    }

    /// <summary>
    /// Test that both .txt and .md files are included
    /// **Validates: Requirements 4.1, 4.2**
    /// </summary>
    [Fact]
    public async Task ReadDirectoryAsync_TxtAndMdFiles_IncludesBoth()
    {
        // Arrange
        var testDir = Path.Combine(_testDirectory, "extension-test");
        Directory.CreateDirectory(testDir);
        
        await File.WriteAllTextAsync(Path.Combine(testDir, "text.txt"), "Text content");
        await File.WriteAllTextAsync(Path.Combine(testDir, "markdown.md"), "Markdown content");
        await File.WriteAllTextAsync(Path.Combine(testDir, "other.pdf"), "PDF content");

        // Act
        var result = await _reader.ReadDirectoryAsync(testDir);

        // Assert
        Assert.Contains("text.txt", result);
        Assert.Contains("Text content", result);
        Assert.Contains("markdown.md", result);
        Assert.Contains("Markdown content", result);
        Assert.DoesNotContain("other.pdf", result);
        Assert.DoesNotContain("PDF content", result);
    }

    /// <summary>
    /// Test that null or empty directory path returns empty string
    /// **Validates: Requirements 4.5**
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReadDirectoryAsync_NullOrEmptyPath_ReturnsEmptyString(string? path)
    {
        // Act
        var result = await _reader.ReadDirectoryAsync(path!);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
