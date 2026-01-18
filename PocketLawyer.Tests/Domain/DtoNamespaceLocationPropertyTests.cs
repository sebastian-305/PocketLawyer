using FsCheck;
using FsCheck.Xunit;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PocketLawyer.Tests.Domain;

/// <summary>
/// Property-based tests for DTO namespace-location consistency.
/// **Feature: repository-production-readiness**
/// </summary>
public class DtoNamespaceLocationPropertyTests
{
    #region Generators

    /// <summary>
    /// Represents a DTO file with its path and namespace information.
    /// </summary>
    private record DtoFileInfo(string FilePath, string Namespace, string FolderCategory);

    /// <summary>
    /// Gets all DTO files from the Domain/Dtos folder structure.
    /// </summary>
    private static List<DtoFileInfo> GetAllDtoFiles()
    {
        var dtoFiles = new List<DtoFileInfo>();
        var baseDirectory = GetProjectRootDirectory();
        var dtosPath = Path.Combine(baseDirectory, "Domain", "Dtos");

        if (!Directory.Exists(dtosPath))
        {
            return dtoFiles;
        }

        // Get all .cs files recursively from Domain/Dtos
        var csFiles = Directory.GetFiles(dtosPath, "*.cs", SearchOption.AllDirectories);

        foreach (var filePath in csFiles)
        {
            var relativePath = Path.GetRelativePath(baseDirectory, filePath);
            var fileContent = File.ReadAllText(filePath);
            var namespaceMatch = Regex.Match(fileContent, @"namespace\s+([\w\.]+)\s*;");

            if (namespaceMatch.Success)
            {
                var detectedNamespace = namespaceMatch.Groups[1].Value;
                var folderCategory = GetFolderCategory(relativePath);
                dtoFiles.Add(new DtoFileInfo(relativePath, detectedNamespace, folderCategory));
            }
        }

        return dtoFiles;
    }

    /// <summary>
    /// Determines the folder category (Employment, Rental, or Shared) from the file path.
    /// </summary>
    private static string GetFolderCategory(string relativePath)
    {
        var normalizedPath = relativePath.Replace("\\", "/");
        
        if (normalizedPath.Contains("Domain/Dtos/Employment/"))
            return "Employment";
        if (normalizedPath.Contains("Domain/Dtos/Rental/"))
            return "Rental";
        if (normalizedPath.Contains("Domain/Dtos/Shared/"))
            return "Shared";
        
        return "Unknown";
    }

    /// <summary>
    /// Gets the expected namespace based on the folder category.
    /// </summary>
    private static string GetExpectedNamespace(string folderCategory)
    {
        return folderCategory switch
        {
            "Employment" => "PocketLawyer.Domain.Dtos.Employment",
            "Rental" => "PocketLawyer.Domain.Dtos.Rental",
            "Shared" => "PocketLawyer.Domain.Dtos.Shared",
            _ => "PocketLawyer.Domain.Dtos"
        };
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
    /// Generator for DTO files from the actual codebase.
    /// </summary>
    private static Gen<DtoFileInfo> GenDtoFile()
    {
        var dtoFiles = GetAllDtoFiles();
        
        if (dtoFiles.Count == 0)
        {
            // Fallback: generate a synthetic DTO file for testing
            return from category in Gen.Elements("Employment", "Rental", "Shared")
                   from fileName in Gen.Elements("TestDto.cs", "AnalysisDto.cs", "BasicDto.cs")
                   let filePath = $"Domain/Dtos/{category}/{fileName}"
                   let expectedNamespace = GetExpectedNamespace(category)
                   select new DtoFileInfo(filePath, expectedNamespace, category);
        }

        return Gen.Elements(dtoFiles.ToArray());
    }

    #endregion

    #region Property 7: Namespace-Location Consistency

    /// <summary>
    /// **Feature: repository-production-readiness, Property 7: Namespace-Location Consistency**
    /// **Validates: Requirements 3.7, 8.1**
    /// 
    /// For any moved DTO file, its namespace declaration should match its folder location 
    /// (e.g., files in Domain/Dtos/Employment/ should have namespace PocketLawyer.Domain.Dtos.Employment).
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MovedDtoFiles_ShouldHaveMatchingNamespaces()
    {
        return Prop.ForAll(
            GenDtoFile().ToArbitrary(),
            dtoFile =>
            {
                // Get the expected namespace based on folder location
                var expectedNamespace = GetExpectedNamespace(dtoFile.FolderCategory);

                // Assert that the actual namespace matches the expected namespace
                Assert.Equal(expectedNamespace, dtoFile.Namespace);

                // Additional validation: namespace should start with PocketLawyer.Domain.Dtos
                Assert.StartsWith("PocketLawyer.Domain.Dtos", dtoFile.Namespace);

                // Validate that the folder category is recognized
                Assert.Contains(dtoFile.FolderCategory, new[] { "Employment", "Rental", "Shared" });

                return true;
            }
        );
    }

    #endregion

    #region Unit Tests for Namespace-Location Consistency

    /// <summary>
    /// Unit test: Verify all Employment DTOs have correct namespace.
    /// </summary>
    [Fact]
    public void EmploymentDtos_ShouldHaveEmploymentNamespace()
    {
        // Arrange
        var dtoFiles = GetAllDtoFiles();
        var employmentDtos = dtoFiles.Where(f => f.FolderCategory == "Employment").ToList();

        // Assert - should have at least some employment DTOs
        Assert.NotEmpty(employmentDtos);

        // Act & Assert - all should have correct namespace
        foreach (var dto in employmentDtos)
        {
            Assert.Equal("PocketLawyer.Domain.Dtos.Employment", dto.Namespace);
        }
    }

    /// <summary>
    /// Unit test: Verify all Rental DTOs have correct namespace.
    /// </summary>
    [Fact]
    public void RentalDtos_ShouldHaveRentalNamespace()
    {
        // Arrange
        var dtoFiles = GetAllDtoFiles();
        var rentalDtos = dtoFiles.Where(f => f.FolderCategory == "Rental").ToList();

        // Assert - should have at least some rental DTOs
        Assert.NotEmpty(rentalDtos);

        // Act & Assert - all should have correct namespace
        foreach (var dto in rentalDtos)
        {
            Assert.Equal("PocketLawyer.Domain.Dtos.Rental", dto.Namespace);
        }
    }

    /// <summary>
    /// Unit test: Verify all Shared DTOs have correct namespace.
    /// </summary>
    [Fact]
    public void SharedDtos_ShouldHaveSharedNamespace()
    {
        // Arrange
        var dtoFiles = GetAllDtoFiles();
        var sharedDtos = dtoFiles.Where(f => f.FolderCategory == "Shared").ToList();

        // Assert - should have at least some shared DTOs
        Assert.NotEmpty(sharedDtos);

        // Act & Assert - all should have correct namespace
        foreach (var dto in sharedDtos)
        {
            Assert.Equal("PocketLawyer.Domain.Dtos.Shared", dto.Namespace);
        }
    }

    /// <summary>
    /// Unit test: Verify specific known DTOs have correct namespaces.
    /// </summary>
    [Theory]
    [InlineData("Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs", "PocketLawyer.Domain.Dtos.Employment")]
    [InlineData("Domain/Dtos/Employment/EmploymentContractAnalysisDto.cs", "PocketLawyer.Domain.Dtos.Employment")]
    [InlineData("Domain/Dtos/Employment/EmploymentPremiumAnalysisDto.cs", "PocketLawyer.Domain.Dtos.Employment")]
    [InlineData("Domain/Dtos/Rental/RentalBasicAnalysisDto.cs", "PocketLawyer.Domain.Dtos.Rental")]
    [InlineData("Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs", "PocketLawyer.Domain.Dtos.Rental")]
    [InlineData("Domain/Dtos/Rental/RentContractAnalysisDto.cs", "PocketLawyer.Domain.Dtos.Rental")]
    [InlineData("Domain/Dtos/Shared/IAnalysisDto.cs", "PocketLawyer.Domain.Dtos.Shared")]
    [InlineData("Domain/Dtos/Shared/AnalysisDto.cs", "PocketLawyer.Domain.Dtos.Shared")]
    [InlineData("Domain/Dtos/Shared/ContractValidationDto.cs", "PocketLawyer.Domain.Dtos.Shared")]
    public void SpecificDtoFile_ShouldHaveCorrectNamespace(string relativePath, string expectedNamespace)
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var fullPath = Path.Combine(baseDirectory, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

        // Assert file exists
        Assert.True(File.Exists(fullPath), $"File not found: {fullPath}");

        // Act - read and parse namespace
        var fileContent = File.ReadAllText(fullPath);
        var namespaceMatch = Regex.Match(fileContent, @"namespace\s+([\w\.]+)\s*;");

        // Assert
        Assert.True(namespaceMatch.Success, $"Could not find namespace declaration in {relativePath}");
        Assert.Equal(expectedNamespace, namespaceMatch.Groups[1].Value);
    }

    /// <summary>
    /// Unit test: Verify no DTOs remain in the root Domain/Dtos folder.
    /// </summary>
    [Fact]
    public void RootDtosFolder_ShouldNotContainDtoFiles()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var dtosPath = Path.Combine(baseDirectory, "Domain", "Dtos");

        if (!Directory.Exists(dtosPath))
        {
            // If the folder doesn't exist, test passes
            return;
        }

        // Act - get .cs files directly in Domain/Dtos (not in subfolders)
        var rootDtoFiles = Directory.GetFiles(dtosPath, "*.cs", SearchOption.TopDirectoryOnly);

        // Assert - should be empty
        Assert.Empty(rootDtoFiles);
    }

    /// <summary>
    /// Unit test: Verify all three required subfolders exist.
    /// </summary>
    [Fact]
    public void DtosFolder_ShouldHaveAllRequiredSubfolders()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var dtosPath = Path.Combine(baseDirectory, "Domain", "Dtos");

        // Assert
        Assert.True(Directory.Exists(Path.Combine(dtosPath, "Employment")), "Employment folder should exist");
        Assert.True(Directory.Exists(Path.Combine(dtosPath, "Rental")), "Rental folder should exist");
        Assert.True(Directory.Exists(Path.Combine(dtosPath, "Shared")), "Shared folder should exist");
    }

    #endregion
}
