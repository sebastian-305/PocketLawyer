using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Infrastructure.Context;

namespace PocketLawyer.Tests.Infrastructure.Context;

/// <summary>
/// Property-based tests for ContextFileReader.
/// Feature: context-management
/// </summary>
public class ContextFileReaderPropertyTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ContextFileReader _reader;

    public ContextFileReaderPropertyTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"context-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        var logger = new Mock<ILogger<ContextFileReader>>();
        _reader = new ContextFileReader(logger.Object);
    }

    /// <summary>
    /// **Feature: context-management, Property 6: File extension filtering**
    /// For any directory containing mixed file types, only files with .txt or .md extensions 
    /// (excluding README.md) should be included in the context
    /// **Validates: Requirements 4.1, 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property FileExtensionFiltering_OnlyIncludesTxtAndMdFiles()
    {
        // Generate a mix of file extensions
        var fileExtensions = new[] { ".txt", ".md", ".pdf", ".docx", ".json", ".xml", ".log" };
        var fileCountGen = Gen.Choose(1, 10);
        
        var filesGen = from count in fileCountGen
                       from extensions in Gen.ArrayOf(count, Gen.Elements(fileExtensions))
                       select extensions.Select((ext, i) => $"file{i}{ext}").ToArray();

        return Prop.ForAll(
            filesGen.ToArbitrary(),
            files =>
            {
                // Arrange - Create test directory with mixed file types
                var testDir = Path.Combine(_testDirectory, Guid.NewGuid().ToString());
                Directory.CreateDirectory(testDir);

                foreach (var file in files)
                {
                    var filePath = Path.Combine(testDir, file);
                    File.WriteAllTextAsync(filePath, $"Content of {file}").GetAwaiter().GetResult();
                }

                // Act
                var result = _reader.ReadDirectoryAsync(testDir).GetAwaiter().GetResult();

                // Assert - Only .txt and .md files should be included
                var expectedFiles = files.Where(f => 
                    f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || 
                    f.EndsWith(".md", StringComparison.OrdinalIgnoreCase)).ToList();

                if (expectedFiles.Count == 0)
                {
                    return string.IsNullOrEmpty(result);
                }

                // Check that all expected files are mentioned in the result
                var allExpectedFilesPresent = expectedFiles.All(f => result.Contains(f));
                
                // Check that no unexpected files are mentioned
                var unexpectedFiles = files.Except(expectedFiles).ToList();
                var noUnexpectedFilesPresent = unexpectedFiles.All(f => !result.Contains(f));

                // Cleanup
                Directory.Delete(testDir, true);

                return allExpectedFilesPresent && noUnexpectedFilesPresent;
            });
    }

    /// <summary>
    /// **Feature: context-management, Property 6: File extension filtering**
    /// For any directory, README.md files should always be excluded from the context
    /// **Validates: Requirements 4.1, 4.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property FileExtensionFiltering_ExcludesReadmeMd()
    {
        var fileCountGen = Gen.Choose(0, 5);
        
        var filesGen = from count in fileCountGen
                       select Enumerable.Range(0, count).Select(i => $"file{i}.txt").ToArray();

        return Prop.ForAll(
            filesGen.ToArbitrary(),
            files =>
            {
                // Arrange - Create test directory with README.md and other files
                var testDir = Path.Combine(_testDirectory, Guid.NewGuid().ToString());
                Directory.CreateDirectory(testDir);

                // Always create README.md
                File.WriteAllTextAsync(Path.Combine(testDir, "README.md"), "This is a README").GetAwaiter().GetResult();

                foreach (var file in files)
                {
                    var filePath = Path.Combine(testDir, file);
                    File.WriteAllTextAsync(filePath, $"Content of {file}").GetAwaiter().GetResult();
                }

                // Act
                var result = _reader.ReadDirectoryAsync(testDir).GetAwaiter().GetResult();

                // Assert - README.md should not be in the result
                var readmeNotPresent = !result.Contains("README.md");

                // Cleanup
                Directory.Delete(testDir, true);

                return readmeNotPresent;
            });
    }

    /// <summary>
    /// **Feature: context-management, Property 3: Context loading determinism**
    /// For any set of context files in a directory, loading the context multiple times 
    /// should produce identical concatenated output (assuming files haven't changed)
    /// **Validates: Requirements 4.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ContextLoadingDeterminism_MultipleLoadsProduceSameResult()
    {
        var fileCountGen = Gen.Choose(1, 5);
        var contentGen = Gen.Elements("Content A", "Content B", "Content C", "Content D");
        
        var filesGen = from count in fileCountGen
                       from contents in Gen.ArrayOf(count, contentGen)
                       select contents.Select((content, i) => (name: $"file{i}.txt", content)).ToArray();

        return Prop.ForAll(
            filesGen.ToArbitrary(),
            files =>
            {
                // Arrange - Create test directory with files
                var testDir = Path.Combine(_testDirectory, Guid.NewGuid().ToString());
                Directory.CreateDirectory(testDir);

                foreach (var (name, content) in files)
                {
                    var filePath = Path.Combine(testDir, name);
                    File.WriteAllTextAsync(filePath, content).GetAwaiter().GetResult();
                }

                // Act - Load context multiple times
                var result1 = _reader.ReadDirectoryAsync(testDir).GetAwaiter().GetResult();
                var result2 = _reader.ReadDirectoryAsync(testDir).GetAwaiter().GetResult();
                var result3 = _reader.ReadDirectoryAsync(testDir).GetAwaiter().GetResult();

                // Assert - All results should be identical
                var allResultsIdentical = result1 == result2 && result2 == result3;

                // Cleanup
                Directory.Delete(testDir, true);

                return allResultsIdentical;
            });
    }

    /// <summary>
    /// **Feature: context-management, Property 7: Error resilience**
    /// For any context loading operation where some files fail to read, 
    /// the operation should continue and return content from successfully read files
    /// **Validates: Requirements 4.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ErrorResilience_ContinuesWithSuccessfullyReadFiles()
    {
        var readableFileCountGen = Gen.Choose(1, 5);
        var unreadableFileCountGen = Gen.Choose(1, 3);
        
        var testDataGen = from readableCount in readableFileCountGen
                          from unreadableCount in unreadableFileCountGen
                          select (readableCount, unreadableCount);

        return Prop.ForAll(
            testDataGen.ToArbitrary(),
            data =>
            {
                var (readableCount, unreadableCount) = data;
                
                // Arrange - Create test directory with readable and unreadable files
                var testDir = Path.Combine(_testDirectory, Guid.NewGuid().ToString());
                Directory.CreateDirectory(testDir);

                var readableFiles = new List<string>();
                var unreadableFiles = new List<FileStream>();

                try
                {
                    // Create readable files
                    for (int i = 0; i < readableCount; i++)
                    {
                        var fileName = $"readable{i}.txt";
                        var filePath = Path.Combine(testDir, fileName);
                        File.WriteAllText(filePath, $"Content of readable file {i}");
                        readableFiles.Add(fileName);
                    }

                    // Create unreadable files by locking them
                    for (int i = 0; i < unreadableCount; i++)
                    {
                        var fileName = $"unreadable{i}.txt";
                        var filePath = Path.Combine(testDir, fileName);
                        
                        // Create and lock the file
                        var fileStream = new FileStream(
                            filePath, 
                            FileMode.Create, 
                            FileAccess.ReadWrite, 
                            FileShare.None); // No sharing - this locks the file
                        
                        // Write some content
                        var writer = new StreamWriter(fileStream);
                        writer.WriteLine($"Locked content {i}");
                        writer.Flush();
                        
                        unreadableFiles.Add(fileStream);
                    }

                    // Act - Try to read the directory
                    var result = _reader.ReadDirectoryAsync(testDir).GetAwaiter().GetResult();

                    // Assert - All readable files should be in the result
                    var allReadableFilesPresent = readableFiles.All(f => result.Contains(f));
                    
                    // Assert - Result should not be empty (we have readable files)
                    var resultNotEmpty = !string.IsNullOrEmpty(result);
                    
                    // Assert - All readable file contents should be present
                    var allReadableContentsPresent = Enumerable.Range(0, readableCount)
                        .All(i => result.Contains($"Content of readable file {i}"));

                    return allReadableFilesPresent && resultNotEmpty && allReadableContentsPresent;
                }
                finally
                {
                    // Cleanup - Close and dispose locked files
                    foreach (var stream in unreadableFiles)
                    {
                        stream.Dispose();
                    }

                    // Cleanup - Delete test directory
                    if (Directory.Exists(testDir))
                    {
                        Directory.Delete(testDir, true);
                    }
                }
            });
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
