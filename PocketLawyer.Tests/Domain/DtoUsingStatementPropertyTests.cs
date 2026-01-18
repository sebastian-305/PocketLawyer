using FsCheck;
using FsCheck.Xunit;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PocketLawyer.Tests.Domain;

/// <summary>
/// Property-based tests for DTO using statement correctness.
/// **Feature: repository-production-readiness**
/// </summary>
public class DtoUsingStatementPropertyTests
{
    #region Generators

    /// <summary>
    /// Represents a C# file with its path, using statements, and referenced DTO types.
    /// </summary>
    private record CSharpFileInfo(
        string FilePath,
        List<string> UsingStatements,
        List<string> ReferencedDtoTypes,
        List<string> RequiredNamespaces);

    /// <summary>
    /// Gets all C# files from the codebase that might reference DTOs.
    /// Excludes DTO files themselves since they don't need using statements for their own namespace.
    /// </summary>
    private static List<CSharpFileInfo> GetAllCSharpFiles()
    {
        var csFiles = new List<CSharpFileInfo>();
        var baseDirectory = GetProjectRootDirectory();

        // Search in key directories that might reference DTOs
        var searchDirectories = new[]
        {
            Path.Combine(baseDirectory, "Application"),
            Path.Combine(baseDirectory, "Infrastructure"),
            Path.Combine(baseDirectory, "Web"),
            Path.Combine(baseDirectory, "PocketLawyer.Tests")
        };

        foreach (var searchDir in searchDirectories)
        {
            if (!Directory.Exists(searchDir))
                continue;

            var files = Directory.GetFiles(searchDir, "*.cs", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                var relativePath = Path.GetRelativePath(baseDirectory, filePath);
                var fileContent = File.ReadAllText(filePath);

                // Extract using statements
                var usingStatements = ExtractUsingStatements(fileContent);

                // Extract referenced DTO types
                var referencedDtos = ExtractReferencedDtoTypes(fileContent);

                // Determine required namespaces based on referenced DTOs
                var requiredNamespaces = DetermineRequiredNamespaces(referencedDtos);

                // Get the file's own namespace to exclude it from required namespaces
                var fileNamespace = ExtractFileNamespace(fileContent);
                
                // Remove the file's own namespace from required namespaces
                // (files don't need using statements for their own namespace)
                requiredNamespaces = requiredNamespaces
                    .Where(ns => ns != fileNamespace)
                    .ToList();

                if (referencedDtos.Count > 0 && requiredNamespaces.Count > 0)
                {
                    csFiles.Add(new CSharpFileInfo(
                        relativePath,
                        usingStatements,
                        referencedDtos,
                        requiredNamespaces));
                }
            }
        }

        return csFiles;
    }

    /// <summary>
    /// Extracts using statements from C# file content.
    /// </summary>
    private static List<string> ExtractUsingStatements(string fileContent)
    {
        var usingStatements = new List<string>();
        var usingPattern = @"using\s+([\w\.]+)\s*;";
        var matches = Regex.Matches(fileContent, usingPattern);

        foreach (Match match in matches)
        {
            if (match.Success)
            {
                usingStatements.Add(match.Groups[1].Value);
            }
        }

        return usingStatements;
    }

    /// <summary>
    /// Extracts the namespace declaration from C# file content.
    /// </summary>
    private static string? ExtractFileNamespace(string fileContent)
    {
        var namespacePattern = @"namespace\s+([\w\.]+)\s*;";
        var match = Regex.Match(fileContent, namespacePattern);
        
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        
        return null;
    }

    /// <summary>
    /// Extracts referenced DTO types from C# file content.
    /// Excludes references in string literals and comments.
    /// </summary>
    private static List<string> ExtractReferencedDtoTypes(string fileContent)
    {
        var referencedDtos = new List<string>();

        // Remove string literals and comments to avoid false positives
        var cleanedContent = RemoveStringsAndComments(fileContent);

        // Known DTO types organized by namespace
        var employmentDtos = new[]
        {
            "EmploymentBasicAnalysisDto",
            "EmploymentContractAnalysisDto",
            "EmploymentPremiumAnalysisDto",
            "EmploymentBasicGeneralDto",
            "EmploymentBasicCompensationDto",
            "EmploymentBasicWorkingConditionsDto",
            "EmploymentBasicSummaryDto",
            "EmploymentPremiumSummaryDto"
        };

        var rentalDtos = new[]
        {
            "RentalBasicAnalysisDto",
            "RentalPremiumAnalysisDto",
            "RentContractAnalysisDto",
            "RentalBasicGeneralDto",
            "RentalBasicRentDto",
            "RentalBasicConditionsDto",
            "RentalBasicSummaryDto",
            "RentalPremiumSummaryDto",
            "PremiumSectionDto"
        };

        var sharedDtos = new[]
        {
            "IAnalysisDto",
            "AnalysisDto",
            "ContractValidationDto",
            "FileContentDto",
            "WebSocketFileDto",
            "KvDetectionAnswer",
            "KvDetectionResult",
            "IValidationDto",
            "AnalysisMetadataDto"
        };

        // Check for each DTO type in the cleaned file content
        foreach (var dto in employmentDtos.Concat(rentalDtos).Concat(sharedDtos))
        {
            // Use word boundary to avoid partial matches
            var pattern = $@"\b{Regex.Escape(dto)}\b";
            if (Regex.IsMatch(cleanedContent, pattern))
            {
                referencedDtos.Add(dto);
            }
        }

        return referencedDtos.Distinct().ToList();
    }

    /// <summary>
    /// Removes string literals and comments from C# code to avoid false positives.
    /// </summary>
    private static string RemoveStringsAndComments(string code)
    {
        // Remove single-line comments
        code = Regex.Replace(code, @"//.*?$", "", RegexOptions.Multiline);
        
        // Remove multi-line comments
        code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
        
        // Remove string literals (both regular and verbatim strings)
        code = Regex.Replace(code, @"@""(?:""""|[^""])*""", "");  // Verbatim strings
        code = Regex.Replace(code, @"""(?:\\.|[^""\\])*""", "");  // Regular strings
        
        return code;
    }

    /// <summary>
    /// Determines required namespaces based on referenced DTO types.
    /// </summary>
    private static List<string> DetermineRequiredNamespaces(List<string> referencedDtos)
    {
        var requiredNamespaces = new HashSet<string>();

        // Employment DTOs
        var employmentDtos = new[]
        {
            "EmploymentBasicAnalysisDto",
            "EmploymentContractAnalysisDto",
            "EmploymentPremiumAnalysisDto",
            "EmploymentBasicGeneralDto",
            "EmploymentBasicCompensationDto",
            "EmploymentBasicWorkingConditionsDto",
            "EmploymentBasicSummaryDto",
            "EmploymentPremiumSummaryDto"
        };

        // Rental DTOs
        var rentalDtos = new[]
        {
            "RentalBasicAnalysisDto",
            "RentalPremiumAnalysisDto",
            "RentContractAnalysisDto",
            "RentalBasicGeneralDto",
            "RentalBasicRentDto",
            "RentalBasicConditionsDto",
            "RentalBasicSummaryDto",
            "RentalPremiumSummaryDto",
            "PremiumSectionDto"
        };

        // Shared DTOs
        var sharedDtos = new[]
        {
            "IAnalysisDto",
            "AnalysisDto",
            "ContractValidationDto",
            "FileContentDto",
            "WebSocketFileDto",
            "KvDetectionAnswer",
            "KvDetectionResult",
            "IValidationDto",
            "AnalysisMetadataDto"
        };

        foreach (var dto in referencedDtos)
        {
            if (employmentDtos.Contains(dto))
            {
                requiredNamespaces.Add("PocketLawyer.Domain.Dtos.Employment");
            }
            else if (rentalDtos.Contains(dto))
            {
                requiredNamespaces.Add("PocketLawyer.Domain.Dtos.Rental");
            }
            else if (sharedDtos.Contains(dto))
            {
                requiredNamespaces.Add("PocketLawyer.Domain.Dtos.Shared");
            }
        }

        return requiredNamespaces.ToList();
    }

    /// <summary>
    /// Gets the project root directory by walking up from the test assembly location.
    /// </summary>
    private static string GetProjectRootDirectory()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(assemblyLocation);

        // Walk up until we find the solution file or reach a reasonable limit
        while (directory != null && !File.Exists(Path.Combine(directory, "PocketLawyer.sln")))
        {
            directory = Directory.GetParent(directory)?.FullName;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("Could not find project root directory");
        }

        return directory;
    }

    /// <summary>
    /// Generator for C# files from the actual codebase.
    /// </summary>
    private static Gen<CSharpFileInfo> GenCSharpFile()
    {
        var csFiles = GetAllCSharpFiles();

        if (csFiles.Count == 0)
        {
            // Fallback: generate a synthetic file for testing
            return from dtoType in Gen.Elements("EmploymentBasicAnalysisDto", "RentalBasicAnalysisDto", "IAnalysisDto")
                   let fileName = "TestFile.cs"
                   let usingStatements = new List<string> { "PocketLawyer.Domain.Dtos.Shared" }
                   let referencedDtos = new List<string> { dtoType }
                   let requiredNamespaces = DetermineRequiredNamespaces(referencedDtos)
                   select new CSharpFileInfo(fileName, usingStatements, referencedDtos, requiredNamespaces);
        }

        return Gen.Elements(csFiles.ToArray());
    }

    #endregion

    #region Property 8: Using Statement Correctness

    /// <summary>
    /// **Feature: repository-production-readiness, Property 8: Using Statement Correctness**
    /// **Validates: Requirements 3.8, 8.2**
    /// 
    /// For any C# file that references DTOs, its using statements should include 
    /// the correct namespaces for all referenced DTO types.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property CSharpFiles_ShouldHaveCorrectUsingStatements()
    {
        return Prop.ForAll(
            GenCSharpFile().ToArbitrary(),
            csFile =>
            {
                // For each required namespace, verify it exists in the using statements
                foreach (var requiredNamespace in csFile.RequiredNamespaces)
                {
                    var hasRequiredUsing = csFile.UsingStatements.Contains(requiredNamespace);

                    // Assert that the required namespace is present
                    Assert.True(
                        hasRequiredUsing,
                        $"File '{csFile.FilePath}' references DTOs from namespace '{requiredNamespace}' " +
                        $"but does not have a using statement for it. " +
                        $"Referenced DTOs: [{string.Join(", ", csFile.ReferencedDtoTypes)}]. " +
                        $"Current using statements: [{string.Join(", ", csFile.UsingStatements)}]");
                }

                // Additional validation: files should not use the old generic namespace
                var hasOldNamespace = csFile.UsingStatements.Contains("PocketLawyer.Domain.Dtos") &&
                                     !csFile.UsingStatements.Any(u => u.StartsWith("PocketLawyer.Domain.Dtos."));

                Assert.False(
                    hasOldNamespace,
                    $"File '{csFile.FilePath}' uses the old generic namespace 'PocketLawyer.Domain.Dtos' " +
                    $"without any specific subnamespaces. It should use specific namespaces like " +
                    $"'PocketLawyer.Domain.Dtos.Employment', 'PocketLawyer.Domain.Dtos.Rental', or " +
                    $"'PocketLawyer.Domain.Dtos.Shared'.");

                return true;
            }
        );
    }

    #endregion

    #region Unit Tests for Using Statement Correctness

    /// <summary>
    /// Unit test: Verify all files that reference Employment DTOs have the correct using statement.
    /// </summary>
    [Fact]
    public void FilesReferencingEmploymentDtos_ShouldHaveEmploymentUsingStatement()
    {
        // Arrange
        var csFiles = GetAllCSharpFiles();
        var filesWithEmploymentDtos = csFiles.Where(f =>
            f.RequiredNamespaces.Contains("PocketLawyer.Domain.Dtos.Employment")).ToList();

        // Assert - should have at least some files
        Assert.NotEmpty(filesWithEmploymentDtos);

        // Act & Assert - all should have correct using statement
        foreach (var file in filesWithEmploymentDtos)
        {
            Assert.Contains("PocketLawyer.Domain.Dtos.Employment", file.UsingStatements);
        }
    }

    /// <summary>
    /// Unit test: Verify all files that reference Rental DTOs have the correct using statement.
    /// </summary>
    [Fact]
    public void FilesReferencingRentalDtos_ShouldHaveRentalUsingStatement()
    {
        // Arrange
        var csFiles = GetAllCSharpFiles();
        var filesWithRentalDtos = csFiles.Where(f =>
            f.RequiredNamespaces.Contains("PocketLawyer.Domain.Dtos.Rental")).ToList();

        // Assert - should have at least some files
        Assert.NotEmpty(filesWithRentalDtos);

        // Act & Assert - all should have correct using statement
        foreach (var file in filesWithRentalDtos)
        {
            Assert.Contains("PocketLawyer.Domain.Dtos.Rental", file.UsingStatements);
        }
    }

    /// <summary>
    /// Unit test: Verify all files that reference Shared DTOs have the correct using statement.
    /// </summary>
    [Fact]
    public void FilesReferencingSharedDtos_ShouldHaveSharedUsingStatement()
    {
        // Arrange
        var csFiles = GetAllCSharpFiles();
        var filesWithSharedDtos = csFiles.Where(f =>
            f.RequiredNamespaces.Contains("PocketLawyer.Domain.Dtos.Shared")).ToList();

        // Assert - should have at least some files
        Assert.NotEmpty(filesWithSharedDtos);

        // Act & Assert - all should have correct using statement
        foreach (var file in filesWithSharedDtos)
        {
            Assert.Contains("PocketLawyer.Domain.Dtos.Shared", file.UsingStatements);
        }
    }

    /// <summary>
    /// Unit test: Verify specific known files have correct using statements.
    /// </summary>
    [Theory]
    [InlineData("Infrastructure/WebSocketAnalysisHandler.cs", "PocketLawyer.Domain.Dtos.Shared")]
    [InlineData("Application/Agents/EmploymentContractAgent.cs", "PocketLawyer.Domain.Dtos.Employment")]
    [InlineData("Application/Agents/EmploymentContractAgent.cs", "PocketLawyer.Domain.Dtos.Shared")]
    [InlineData("Application/Agents/RentalContractAgent.cs", "PocketLawyer.Domain.Dtos.Rental")]
    [InlineData("Application/Agents/RentalContractAgent.cs", "PocketLawyer.Domain.Dtos.Shared")]
    public void SpecificFile_ShouldHaveCorrectUsingStatement(string relativePath, string expectedNamespace)
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var fullPath = Path.Combine(baseDirectory, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

        // Assert file exists
        Assert.True(File.Exists(fullPath), $"File not found: {fullPath}");

        // Act - read and parse using statements
        var fileContent = File.ReadAllText(fullPath);
        var usingStatements = ExtractUsingStatements(fileContent);

        // Assert
        Assert.Contains(expectedNamespace, usingStatements);
    }

    /// <summary>
    /// Unit test: Verify no files use the old generic namespace without specific subnamespaces.
    /// </summary>
    [Fact]
    public void NoFiles_ShouldUseOldGenericNamespaceAlone()
    {
        // Arrange
        var csFiles = GetAllCSharpFiles();

        // Act - find files that use old namespace without specific subnamespaces
        var filesWithOldNamespace = csFiles.Where(f =>
        {
            var hasOldNamespace = f.UsingStatements.Contains("PocketLawyer.Domain.Dtos");
            var hasSpecificNamespace = f.UsingStatements.Any(u => 
                u.StartsWith("PocketLawyer.Domain.Dtos.") && u != "PocketLawyer.Domain.Dtos");
            return hasOldNamespace && !hasSpecificNamespace;
        }).ToList();

        // Assert - should be empty
        Assert.Empty(filesWithOldNamespace);
    }

    /// <summary>
    /// Unit test: Verify files that reference multiple DTO categories have all required using statements.
    /// </summary>
    [Fact]
    public void FilesReferencingMultipleDtoCategories_ShouldHaveAllRequiredUsingStatements()
    {
        // Arrange
        var csFiles = GetAllCSharpFiles();
        var filesWithMultipleCategories = csFiles.Where(f => f.RequiredNamespaces.Count > 1).ToList();

        // Act & Assert - all should have all required using statements
        foreach (var file in filesWithMultipleCategories)
        {
            foreach (var requiredNamespace in file.RequiredNamespaces)
            {
                Assert.True(
                    file.UsingStatements.Contains(requiredNamespace),
                    $"File '{file.FilePath}' references DTOs from multiple categories but is missing using statement for '{requiredNamespace}'");
            }
        }
    }

    /// <summary>
    /// Unit test: Verify EmploymentContractAgent has both Employment and Shared using statements.
    /// </summary>
    [Fact]
    public void EmploymentContractAgent_ShouldHaveBothEmploymentAndSharedUsingStatements()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var filePath = Path.Combine(baseDirectory, "Application", "Agents", "EmploymentContractAgent.cs");

        // Assert file exists
        Assert.True(File.Exists(filePath), $"File not found: {filePath}");

        // Act - read and parse using statements
        var fileContent = File.ReadAllText(filePath);
        var usingStatements = ExtractUsingStatements(fileContent);

        // Assert - should have both namespaces
        Assert.Contains("PocketLawyer.Domain.Dtos.Employment", usingStatements);
        Assert.Contains("PocketLawyer.Domain.Dtos.Shared", usingStatements);
    }

    /// <summary>
    /// Unit test: Verify RentalContractAgent has both Rental and Shared using statements.
    /// </summary>
    [Fact]
    public void RentalContractAgent_ShouldHaveBothRentalAndSharedUsingStatements()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var filePath = Path.Combine(baseDirectory, "Application", "Agents", "RentalContractAgent.cs");

        // Assert file exists
        Assert.True(File.Exists(filePath), $"File not found: {filePath}");

        // Act - read and parse using statements
        var fileContent = File.ReadAllText(filePath);
        var usingStatements = ExtractUsingStatements(fileContent);

        // Assert - should have both namespaces
        Assert.Contains("PocketLawyer.Domain.Dtos.Rental", usingStatements);
        Assert.Contains("PocketLawyer.Domain.Dtos.Shared", usingStatements);
    }

    #endregion
}
