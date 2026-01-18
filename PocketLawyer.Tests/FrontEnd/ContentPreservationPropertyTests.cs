using FsCheck;
using FsCheck.Xunit;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PocketLawyer.Tests.FrontEnd;

/// <summary>
/// Property-based tests for content preservation from original index.html.
/// **Feature: repository-production-readiness**
/// </summary>
public class ContentPreservationPropertyTests
{
    #region Generators

    /// <summary>
    /// Represents a content section from the original index.html that should be preserved.
    /// </summary>
    private record ContentSection(
        string SectionName,
        string ContentPattern,
        bool IsReusableContent);

    /// <summary>
    /// Represents an HTML page with its path and content.
    /// </summary>
    private record HtmlPageInfo(string FilePath, string PageType, string Content);

    /// <summary>
    /// Gets all reusable content sections from the original index.html.
    /// These are content sections that should appear in at least one of the dedicated pages.
    /// </summary>
    private static List<ContentSection> GetReusableContentSections()
    {
        var sections = new List<ContentSection>();

        // Key content sections that should be preserved in dedicated pages
        // These are the main informational and branding elements

        // Header/Branding content
        sections.Add(new ContentSection(
            "BrandName",
            @"KLAUSEL\s+KLARA",
            true));

        // Mission statement
        sections.Add(new ContentSection(
            "MissionStatement",
            @"KLARA\s+macht\s+Recht\s+für\s+alle\s+zugänglich",
            true));

        // Key features - these should be preserved (adapted for specific contract types)
        sections.Add(new ContentSection(
            "FeatureQuickAnalysis",
            @"SEKUNDENSCHNELL\s+ANALYSIEREN",
            true));

        sections.Add(new ContentSection(
            "FeatureRedFlags",
            @"PROBLEMSTELLEN\s+UND\s+RED\s+FLAGS",
            true));

        sections.Add(new ContentSection(
            "FeatureChat",
            @"MIT\s+KLARA\s+ÜBER\s+DEINEN\s+VERTRAG\s+CHATTEN",
            true));

        // Why Klausel Klara section
        sections.Add(new ContentSection(
            "WhySpecialization",
            @"Spezialisiert\s+auf\s+österreichisches",
            true));

        sections.Add(new ContentSection(
            "WhyRedFlagDetection",
            @"Red-Flag-Erkennung\s+&\s+Bewertung",
            true));

        sections.Add(new ContentSection(
            "WhyDataPrivacy",
            @"Datenschutz\s+made\s+in\s+Europe",
            true));

        // Privacy statement
        sections.Add(new ContentSection(
            "PrivacyStatement",
            @"100\s*%\s+datenschutzkonform",
            true));

        // Footer copyright (using HTML entity &copy;)
        sections.Add(new ContentSection(
            "Copyright",
            @"&copy;\s*2025\s+KLAUSEL\s+KLARA",
            true));

        return sections;
    }

    /// <summary>
    /// Gets all dedicated contract pages (employment and rental).
    /// </summary>
    private static List<HtmlPageInfo> GetDedicatedPages()
    {
        var pages = new List<HtmlPageInfo>();
        var baseDirectory = GetProjectRootDirectory();
        var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");

        if (!Directory.Exists(wwwrootPath))
        {
            return pages;
        }

        // Define the dedicated contract pages
        var contractPages = new Dictionary<string, string>
        {
            { "employment.html", "employment" },
            { "rental.html", "rental" }
        };

        foreach (var (fileName, pageType) in contractPages)
        {
            var filePath = Path.Combine(wwwrootPath, fileName);
            if (File.Exists(filePath))
            {
                var relativePath = Path.GetRelativePath(baseDirectory, filePath);
                var content = File.ReadAllText(filePath);
                pages.Add(new HtmlPageInfo(relativePath, pageType, content));
            }
        }

        return pages;
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
    /// Generator for reusable content sections from the original index.html.
    /// </summary>
    private static Gen<ContentSection> GenContentSection()
    {
        var sections = GetReusableContentSections();

        if (sections.Count == 0)
        {
            // Fallback: generate a synthetic content section for testing
            return Gen.Constant(new ContentSection(
                "TestSection",
                "KLAUSEL KLARA",
                true));
        }

        return Gen.Elements(sections.ToArray());
    }

    /// <summary>
    /// Checks if content appears in at least one of the dedicated pages.
    /// </summary>
    private static bool ContentAppearsInDedicatedPages(string contentPattern, List<HtmlPageInfo> pages)
    {
        foreach (var page in pages)
        {
            if (Regex.IsMatch(page.Content, contentPattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Normalizes HTML content by removing extra whitespace for comparison.
    /// </summary>
    private static string NormalizeContent(string content)
    {
        // Remove HTML tags for text comparison
        var withoutTags = Regex.Replace(content, @"<[^>]+>", " ");
        
        // Normalize whitespace
        var normalized = Regex.Replace(withoutTags, @"\s+", " ");
        
        return normalized.Trim();
    }

    #endregion

    #region Property 2: Content Preservation from Original

    /// <summary>
    /// **Feature: repository-production-readiness, Property 2: Content Preservation from Original**
    /// **Validates: Requirements 1.3**
    /// 
    /// For any content section in the original index.html that is marked for reuse, 
    /// that content should appear in at least one of the new dedicated pages.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ReusableContent_ShouldAppearInDedicatedPages()
    {
        return Prop.ForAll(
            GenContentSection().ToArbitrary(),
            contentSection =>
            {
                // Only test reusable content sections
                if (!contentSection.IsReusableContent)
                {
                    return true;
                }

                // Get all dedicated pages
                var dedicatedPages = GetDedicatedPages();

                // Assert that we have dedicated pages
                Assert.NotEmpty(dedicatedPages);

                // Check if the content appears in at least one dedicated page
                var appearsInPages = ContentAppearsInDedicatedPages(
                    contentSection.ContentPattern,
                    dedicatedPages);

                Assert.True(
                    appearsInPages,
                    $"Content section '{contentSection.SectionName}' with pattern '{contentSection.ContentPattern}' " +
                    $"should appear in at least one dedicated page (employment.html or rental.html), " +
                    $"but was not found in any of them.");

                return true;
            }
        );
    }

    #endregion

    #region Unit Tests for Content Preservation

    /// <summary>
    /// Unit test: Verify brand name appears in employment page.
    /// </summary>
    [Fact]
    public void EmploymentPage_ShouldContainBrandName()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var employmentPath = Path.Combine(baseDirectory, "wwwroot", "employment.html");
        
        Assert.True(File.Exists(employmentPath), "employment.html should exist");
        
        var content = File.ReadAllText(employmentPath);

        // Assert - should contain brand name
        Assert.Matches(@"KLAUSEL\s+KLARA", content);
    }

    /// <summary>
    /// Unit test: Verify brand name appears in rental page.
    /// </summary>
    [Fact]
    public void RentalPage_ShouldContainBrandName()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var rentalPath = Path.Combine(baseDirectory, "wwwroot", "rental.html");
        
        Assert.True(File.Exists(rentalPath), "rental.html should exist");
        
        var content = File.ReadAllText(rentalPath);

        // Assert - should contain brand name
        Assert.Matches(@"KLAUSEL\s+KLARA", content);
    }

    /// <summary>
    /// Unit test: Verify mission statement appears in at least one dedicated page.
    /// </summary>
    [Fact]
    public void DedicatedPages_ShouldContainMissionStatement()
    {
        // Arrange
        var pages = GetDedicatedPages();
        Assert.NotEmpty(pages);

        // Act - check if mission statement appears in any page
        var missionPattern = @"KLARA\s+macht\s+Recht\s+für\s+alle\s+zugänglich";
        var appearsInPages = ContentAppearsInDedicatedPages(missionPattern, pages);

        // Assert
        Assert.True(appearsInPages, 
            "Mission statement should appear in at least one dedicated page");
    }

    /// <summary>
    /// Unit test: Verify key features appear in at least one dedicated page.
    /// Note: The generic "VERTRAG" text is adapted to specific contract types in dedicated pages.
    /// </summary>
    [Theory]
    [InlineData("SEKUNDENSCHNELL ANALYSIEREN", "Quick analysis feature")]
    [InlineData("PROBLEMSTELLEN UND RED FLAGS", "Red flags feature")]
    [InlineData("MIT KLARA ÜBER DEINEN VERTRAG CHATTEN", "Chat feature")]
    public void DedicatedPages_ShouldContainKeyFeature(string featureText, string featureName)
    {
        // Arrange
        var pages = GetDedicatedPages();
        Assert.NotEmpty(pages);

        // Act - check if feature appears in any page (case-insensitive, flexible whitespace)
        var pattern = Regex.Escape(featureText).Replace(@"\ ", @"\s+");
        var appearsInPages = ContentAppearsInDedicatedPages(pattern, pages);

        // Assert
        Assert.True(appearsInPages, 
            $"{featureName} should appear in at least one dedicated page");
    }

    /// <summary>
    /// Unit test: Verify "Why Klausel Klara" sections appear in at least one dedicated page.
    /// </summary>
    [Theory]
    [InlineData("Spezialisiert auf österreichisches", "Specialization section")]
    [InlineData("Red-Flag-Erkennung", "Red flag detection section")]
    [InlineData("Datenschutz made in Europe", "Data privacy section")]
    public void DedicatedPages_ShouldContainWhySection(string sectionText, string sectionName)
    {
        // Arrange
        var pages = GetDedicatedPages();
        Assert.NotEmpty(pages);

        // Act - check if section appears in any page
        var pattern = Regex.Escape(sectionText).Replace(@"\ ", @"\s+");
        var appearsInPages = ContentAppearsInDedicatedPages(pattern, pages);

        // Assert
        Assert.True(appearsInPages, 
            $"{sectionName} should appear in at least one dedicated page");
    }

    /// <summary>
    /// Unit test: Verify privacy statement appears in at least one dedicated page.
    /// </summary>
    [Fact]
    public void DedicatedPages_ShouldContainPrivacyStatement()
    {
        // Arrange
        var pages = GetDedicatedPages();
        Assert.NotEmpty(pages);

        // Act - check if privacy statement appears in any page
        var privacyPattern = @"100\s*%\s+datenschutzkonform";
        var appearsInPages = ContentAppearsInDedicatedPages(privacyPattern, pages);

        // Assert
        Assert.True(appearsInPages, 
            "Privacy statement should appear in at least one dedicated page");
    }

    /// <summary>
    /// Unit test: Verify copyright appears in at least one dedicated page.
    /// </summary>
    [Fact]
    public void DedicatedPages_ShouldContainCopyright()
    {
        // Arrange
        var pages = GetDedicatedPages();
        Assert.NotEmpty(pages);

        // Act - check if copyright appears in any page (using HTML entity)
        var copyrightPattern = @"&copy;\s*2025\s+KLAUSEL\s+KLARA";
        var appearsInPages = ContentAppearsInDedicatedPages(copyrightPattern, pages);

        // Assert
        Assert.True(appearsInPages, 
            "Copyright notice should appear in at least one dedicated page");
    }

    /// <summary>
    /// Unit test: Verify all reusable content sections appear in dedicated pages.
    /// </summary>
    [Fact]
    public void AllReusableContentSections_ShouldAppearInDedicatedPages()
    {
        // Arrange
        var reusableSections = GetReusableContentSections();
        var dedicatedPages = GetDedicatedPages();

        Assert.NotEmpty(reusableSections);
        Assert.NotEmpty(dedicatedPages);

        // Act & Assert - check each reusable section
        var missingContent = new List<string>();

        foreach (var section in reusableSections)
        {
            if (section.IsReusableContent)
            {
                var appearsInPages = ContentAppearsInDedicatedPages(
                    section.ContentPattern,
                    dedicatedPages);

                if (!appearsInPages)
                {
                    missingContent.Add(section.SectionName);
                }
            }
        }

        Assert.Empty(missingContent);
    }

    /// <summary>
    /// Unit test: Verify employment page has substantial content (not just a skeleton).
    /// </summary>
    [Fact]
    public void EmploymentPage_ShouldHaveSubstantialContent()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var employmentPath = Path.Combine(baseDirectory, "wwwroot", "employment.html");
        
        Assert.True(File.Exists(employmentPath), "employment.html should exist");
        
        var content = File.ReadAllText(employmentPath);
        var normalizedContent = NormalizeContent(content);

        // Assert - should have substantial text content (at least 500 characters of actual text)
        Assert.True(normalizedContent.Length > 500, 
            $"Employment page should have substantial content (found {normalizedContent.Length} characters)");
    }

    /// <summary>
    /// Unit test: Verify rental page has substantial content (not just a skeleton).
    /// </summary>
    [Fact]
    public void RentalPage_ShouldHaveSubstantialContent()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var rentalPath = Path.Combine(baseDirectory, "wwwroot", "rental.html");
        
        Assert.True(File.Exists(rentalPath), "rental.html should exist");
        
        var content = File.ReadAllText(rentalPath);
        var normalizedContent = NormalizeContent(content);

        // Assert - should have substantial text content (at least 500 characters of actual text)
        Assert.True(normalizedContent.Length > 500, 
            $"Rental page should have substantial content (found {normalizedContent.Length} characters)");
    }

    /// <summary>
    /// Unit test: Verify both dedicated pages contain Bootstrap styling.
    /// </summary>
    [Theory]
    [InlineData("employment.html")]
    [InlineData("rental.html")]
    public void DedicatedPage_ShouldContainBootstrapStyling(string fileName)
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var filePath = Path.Combine(baseDirectory, "wwwroot", fileName);
        
        Assert.True(File.Exists(filePath), $"{fileName} should exist");
        
        var content = File.ReadAllText(filePath);

        // Assert - should reference Bootstrap
        Assert.Contains("bootstrap", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Unit test: Verify both dedicated pages have proper HTML structure.
    /// </summary>
    [Theory]
    [InlineData("employment.html")]
    [InlineData("rental.html")]
    public void DedicatedPage_ShouldHaveProperHtmlStructure(string fileName)
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var filePath = Path.Combine(baseDirectory, "wwwroot", fileName);
        
        Assert.True(File.Exists(filePath), $"{fileName} should exist");
        
        var content = File.ReadAllText(filePath);

        // Assert - should have basic HTML structure
        Assert.Contains("<!DOCTYPE html>", content);
        Assert.Contains("<html", content);
        Assert.Contains("<head>", content);
        Assert.Contains("<body>", content);
        Assert.Contains("</html>", content);
    }

    #endregion
}
