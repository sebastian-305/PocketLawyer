using FsCheck;
using FsCheck.Xunit;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PocketLawyer.Tests.FrontEnd;

/// <summary>
/// Property-based tests for navigation link bidirectionality between contract type pages.
/// **Feature: repository-production-readiness**
/// </summary>
public class NavigationLinkBidirectionalityPropertyTests
{
    #region Generators

    /// <summary>
    /// Represents an HTML page with its path and navigation links.
    /// </summary>
    private record HtmlPageInfo(string FilePath, string PageType, List<string> NavigationLinks);

    /// <summary>
    /// Represents a pair of contract type pages that should have bidirectional links.
    /// </summary>
    private record PagePair(string Page1Type, string Page2Type, string Page1Path, string Page2Path);

    /// <summary>
    /// Gets all contract type HTML pages from the wwwroot folder.
    /// </summary>
    private static List<HtmlPageInfo> GetAllContractPages()
    {
        var pages = new List<HtmlPageInfo>();
        var baseDirectory = GetProjectRootDirectory();
        var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");

        if (!Directory.Exists(wwwrootPath))
        {
            return pages;
        }

        // Define the contract type pages we're interested in
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
                var fileContent = File.ReadAllText(filePath);
                var navigationLinks = ExtractNavigationLinks(fileContent);
                pages.Add(new HtmlPageInfo(relativePath, pageType, navigationLinks));
            }
        }

        return pages;
    }

    /// <summary>
    /// Extracts navigation links from HTML content.
    /// Looks for href attributes in anchor tags.
    /// </summary>
    private static List<string> ExtractNavigationLinks(string htmlContent)
    {
        var links = new List<string>();
        
        // Match href attributes in anchor tags
        var hrefPattern = @"href\s*=\s*[""']([^""']+)[""']";
        var matches = Regex.Matches(htmlContent, hrefPattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var href = match.Groups[1].Value;
                // Only include relative HTML links (not external URLs or anchors)
                if (href.EndsWith(".html") && !href.StartsWith("http"))
                {
                    links.Add(href);
                }
            }
        }

        return links;
    }

    /// <summary>
    /// Gets all pairs of contract type pages that should have bidirectional links.
    /// </summary>
    private static List<PagePair> GetContractPagePairs()
    {
        var pages = GetAllContractPages();
        var pairs = new List<PagePair>();

        // Create pairs of pages (employment-rental)
        var employmentPage = pages.FirstOrDefault(p => p.PageType == "employment");
        var rentalPage = pages.FirstOrDefault(p => p.PageType == "rental");

        if (employmentPage != null && rentalPage != null)
        {
            pairs.Add(new PagePair(
                employmentPage.PageType,
                rentalPage.PageType,
                employmentPage.FilePath,
                rentalPage.FilePath
            ));
        }

        return pairs;
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
    /// Generator for page pairs from the actual codebase.
    /// </summary>
    private static Gen<PagePair> GenPagePair()
    {
        var pagePairs = GetContractPagePairs();
        
        if (pagePairs.Count == 0)
        {
            // Fallback: generate a synthetic page pair for testing
            return Gen.Constant(new PagePair(
                "employment",
                "rental",
                "wwwroot/employment.html",
                "wwwroot/rental.html"
            ));
        }

        return Gen.Elements(pagePairs.ToArray());
    }

    /// <summary>
    /// Checks if a page contains a link to another page.
    /// </summary>
    private static bool PageContainsLinkTo(HtmlPageInfo sourcePage, string targetPageFileName)
    {
        return sourcePage.NavigationLinks.Any(link => 
            link.Equals(targetPageFileName, StringComparison.OrdinalIgnoreCase) ||
            link.EndsWith("/" + targetPageFileName, StringComparison.OrdinalIgnoreCase)
        );
    }

    #endregion

    #region Property 1: Navigation Link Bidirectionality

    /// <summary>
    /// **Feature: repository-production-readiness, Property 1: Navigation Link Bidirectionality**
    /// **Validates: Requirements 1.2**
    /// 
    /// For any pair of contract type pages (employment and rental), each page should contain 
    /// a navigation link to the other page.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ContractTypePages_ShouldHaveBidirectionalNavigationLinks()
    {
        return Prop.ForAll(
            GenPagePair().ToArbitrary(),
            pagePair =>
            {
                // Get the actual page information
                var pages = GetAllContractPages();
                var page1 = pages.FirstOrDefault(p => p.PageType == pagePair.Page1Type);
                var page2 = pages.FirstOrDefault(p => p.PageType == pagePair.Page2Type);

                // Both pages should exist
                Assert.NotNull(page1);
                Assert.NotNull(page2);

                // Determine the expected link file names
                var page1FileName = Path.GetFileName(page1.FilePath);
                var page2FileName = Path.GetFileName(page2.FilePath);

                // Page 1 should contain a link to Page 2
                var page1HasLinkToPage2 = PageContainsLinkTo(page1, page2FileName);
                Assert.True(page1HasLinkToPage2, 
                    $"{page1FileName} should contain a navigation link to {page2FileName}");

                // Page 2 should contain a link to Page 1
                var page2HasLinkToPage1 = PageContainsLinkTo(page2, page1FileName);
                Assert.True(page2HasLinkToPage1, 
                    $"{page2FileName} should contain a navigation link to {page1FileName}");

                // Both pages should have at least one navigation link
                Assert.NotEmpty(page1.NavigationLinks);
                Assert.NotEmpty(page2.NavigationLinks);

                return true;
            }
        );
    }

    #endregion

    #region Unit Tests for Navigation Link Bidirectionality

    /// <summary>
    /// Unit test: Verify employment page contains link to rental page.
    /// </summary>
    [Fact]
    public void EmploymentPage_ShouldContainLinkToRentalPage()
    {
        // Arrange
        var pages = GetAllContractPages();
        var employmentPage = pages.FirstOrDefault(p => p.PageType == "employment");

        // Assert page exists
        Assert.NotNull(employmentPage);

        // Act & Assert - should contain link to rental.html
        var hasLinkToRental = PageContainsLinkTo(employmentPage, "rental.html");
        Assert.True(hasLinkToRental, "employment.html should contain a navigation link to rental.html");
    }

    /// <summary>
    /// Unit test: Verify rental page contains link to employment page.
    /// </summary>
    [Fact]
    public void RentalPage_ShouldContainLinkToEmploymentPage()
    {
        // Arrange
        var pages = GetAllContractPages();
        var rentalPage = pages.FirstOrDefault(p => p.PageType == "rental");

        // Assert page exists
        Assert.NotNull(rentalPage);

        // Act & Assert - should contain link to employment.html
        var hasLinkToEmployment = PageContainsLinkTo(rentalPage, "employment.html");
        Assert.True(hasLinkToEmployment, "rental.html should contain a navigation link to employment.html");
    }

    /// <summary>
    /// Unit test: Verify both contract pages exist.
    /// </summary>
    [Fact]
    public void ContractPages_ShouldExist()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");

        // Assert
        Assert.True(File.Exists(Path.Combine(wwwrootPath, "employment.html")), 
            "employment.html should exist in wwwroot");
        Assert.True(File.Exists(Path.Combine(wwwrootPath, "rental.html")), 
            "rental.html should exist in wwwroot");
    }

    /// <summary>
    /// Unit test: Verify employment page has navigation links.
    /// </summary>
    [Fact]
    public void EmploymentPage_ShouldHaveNavigationLinks()
    {
        // Arrange
        var pages = GetAllContractPages();
        var employmentPage = pages.FirstOrDefault(p => p.PageType == "employment");

        // Assert
        Assert.NotNull(employmentPage);
        Assert.NotEmpty(employmentPage.NavigationLinks);
    }

    /// <summary>
    /// Unit test: Verify rental page has navigation links.
    /// </summary>
    [Fact]
    public void RentalPage_ShouldHaveNavigationLinks()
    {
        // Arrange
        var pages = GetAllContractPages();
        var rentalPage = pages.FirstOrDefault(p => p.PageType == "rental");

        // Assert
        Assert.NotNull(rentalPage);
        Assert.NotEmpty(rentalPage.NavigationLinks);
    }

    /// <summary>
    /// Unit test: Verify specific navigation link structure in employment page.
    /// </summary>
    [Fact]
    public void EmploymentPage_ShouldHaveCorrectNavigationStructure()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var employmentPath = Path.Combine(baseDirectory, "wwwroot", "employment.html");
        var content = File.ReadAllText(employmentPath);

        // Assert - should have a nav element with link to rental
        Assert.Contains("rental.html", content);
        
        // Should have navigation in header or nav element
        var hasNavElement = content.Contains("<nav") || content.Contains("<header");
        Assert.True(hasNavElement, "employment.html should have navigation structure");
    }

    /// <summary>
    /// Unit test: Verify specific navigation link structure in rental page.
    /// </summary>
    [Fact]
    public void RentalPage_ShouldHaveCorrectNavigationStructure()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var rentalPath = Path.Combine(baseDirectory, "wwwroot", "rental.html");
        var content = File.ReadAllText(rentalPath);

        // Assert - should have a nav element with link to employment
        Assert.Contains("employment.html", content);
        
        // Should have navigation in header or nav element
        var hasNavElement = content.Contains("<nav") || content.Contains("<header");
        Assert.True(hasNavElement, "rental.html should have navigation structure");
    }

    /// <summary>
    /// Unit test: Verify bidirectionality for all page pairs.
    /// </summary>
    [Fact]
    public void AllContractPagePairs_ShouldHaveBidirectionalLinks()
    {
        // Arrange
        var pages = GetAllContractPages();
        
        // Assert - should have at least 2 pages
        Assert.True(pages.Count >= 2, "Should have at least 2 contract type pages");

        // Act & Assert - check bidirectionality for each pair
        var employmentPage = pages.FirstOrDefault(p => p.PageType == "employment");
        var rentalPage = pages.FirstOrDefault(p => p.PageType == "rental");

        Assert.NotNull(employmentPage);
        Assert.NotNull(rentalPage);

        // Employment should link to rental
        Assert.True(PageContainsLinkTo(employmentPage, "rental.html"),
            "employment.html should link to rental.html");

        // Rental should link to employment
        Assert.True(PageContainsLinkTo(rentalPage, "employment.html"),
            "rental.html should link to employment.html");
    }

    #endregion
}
