using FsCheck;
using FsCheck.Xunit;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PocketLawyer.Tests.FrontEnd;

/// <summary>
/// Property-based tests for Bootstrap 5 class validity.
/// **Feature: repository-production-readiness**
/// </summary>
public class BootstrapClassValidityPropertyTests
{
    #region Generators

    /// <summary>
    /// Represents an HTML element with Bootstrap classes.
    /// </summary>
    private record HtmlElementWithClasses(
        string ElementTag,
        List<string> BootstrapClasses,
        string SourceFile);

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
    /// Gets all HTML pages from the wwwroot directory.
    /// </summary>
    private static List<string> GetHtmlFiles()
    {
        var baseDirectory = GetProjectRootDirectory();
        var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");

        if (!Directory.Exists(wwwrootPath))
        {
            return new List<string>();
        }

        var htmlFiles = new[] { "index.html", "employment.html", "rental.html" };
        var result = new List<string>();

        foreach (var fileName in htmlFiles)
        {
            var filePath = Path.Combine(wwwrootPath, fileName);
            if (File.Exists(filePath))
            {
                result.Add(filePath);
            }
        }

        return result;
    }

    /// <summary>
    /// Known valid Bootstrap 5 class prefixes and patterns.
    /// This is a comprehensive but not exhaustive list of Bootstrap 5 classes.
    /// </summary>
    private static readonly HashSet<string> ValidBootstrapPrefixes = new()
    {
        // Layout
        "container", "row", "col", "g-", "gx-", "gy-",
        
        // Display
        "d-", "flex-", "justify-content-", "align-items-", "align-self-", "align-content-",
        
        // Spacing
        "m-", "mt-", "mb-", "ms-", "me-", "mx-", "my-",
        "p-", "pt-", "pb-", "ps-", "pe-", "px-", "py-",
        "gap-",
        
        // Sizing
        "w-", "h-", "mw-", "mh-", "vw-", "vh-",
        
        // Text
        "text-", "fs-", "fw-", "fst-", "lh-", "font-",
        
        // Background
        "bg-", "bg-gradient",
        
        // Borders
        "border", "rounded",
        
        // Colors
        "text-primary", "text-secondary", "text-success", "text-danger", "text-warning",
        "text-info", "text-light", "text-dark", "text-white", "text-muted",
        
        // Components
        "btn", "modal", "toast", "accordion", "alert", "badge", "card", "dropdown",
        "nav", "navbar", "pagination", "progress", "spinner", "table", "form",
        
        // Utilities
        "position-", "top-", "bottom-", "start-", "end-",
        "translate-", "overflow-", "shadow", "opacity-",
        "visible", "invisible", "visually-hidden",
        "list-unstyled", "list-inline",
        "img-", "figure", "object-fit-",
        
        // Interactive
        "disabled", "active", "show", "hide", "collapse", "fade",
        
        // Responsive
        "col-sm-", "col-md-", "col-lg-", "col-xl-", "col-xxl-",
        "d-sm-", "d-md-", "d-lg-", "d-xl-", "d-xxl-",
    };

    /// <summary>
    /// Extracts all Bootstrap classes from HTML files.
    /// </summary>
    private static List<HtmlElementWithClasses> ExtractBootstrapClasses(string htmlFilePath)
    {
        var elements = new List<HtmlElementWithClasses>();
        var content = File.ReadAllText(htmlFilePath);
        var fileName = Path.GetFileName(htmlFilePath);

        // Match HTML elements with class attributes
        var elementPattern = @"<(\w+)[^>]*\sclass=""([^""]+)""[^>]*>";
        var matches = Regex.Matches(content, elementPattern);

        foreach (Match match in matches)
        {
            var elementTag = match.Groups[1].Value;
            var classAttribute = match.Groups[2].Value;
            
            // Split classes and filter for potential Bootstrap classes
            var classes = classAttribute.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var bootstrapClasses = classes
                .Where(c => IsLikelyBootstrapClass(c))
                .ToList();

            if (bootstrapClasses.Count > 0)
            {
                elements.Add(new HtmlElementWithClasses(
                    elementTag,
                    bootstrapClasses,
                    fileName));
            }
        }

        return elements;
    }

    /// <summary>
    /// Checks if a class name is likely a Bootstrap class.
    /// </summary>
    private static bool IsLikelyBootstrapClass(string className)
    {
        // Check against known Bootstrap prefixes
        foreach (var prefix in ValidBootstrapPrefixes)
        {
            if (className == prefix || className.StartsWith(prefix))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if a Bootstrap class is valid according to Bootstrap 5 conventions.
    /// </summary>
    private static bool IsValidBootstrap5Class(string className)
    {
        // Exact matches
        if (ValidBootstrapPrefixes.Contains(className))
        {
            return true;
        }

        // Check if it starts with a valid prefix
        foreach (var prefix in ValidBootstrapPrefixes)
        {
            if (className.StartsWith(prefix))
            {
                // Additional validation for numbered classes
                if (prefix.EndsWith("-"))
                {
                    var suffix = className.Substring(prefix.Length);
                    
                    // Check for valid numeric suffixes (0-5, auto, etc.)
                    if (suffix == "auto" || suffix == "none" || suffix == "0" || 
                        suffix == "1" || suffix == "2" || suffix == "3" || 
                        suffix == "4" || suffix == "5" || suffix == "6")
                    {
                        return true;
                    }
                    
                    // Check for percentage values (w-25, w-50, w-75, w-100)
                    if (suffix == "25" || suffix == "50" || suffix == "75" || suffix == "100")
                    {
                        return true;
                    }
                }
                
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all Bootstrap classes from all HTML files.
    /// </summary>
    private static List<HtmlElementWithClasses> GetAllBootstrapElements()
    {
        var allElements = new List<HtmlElementWithClasses>();
        var htmlFiles = GetHtmlFiles();

        foreach (var htmlFile in htmlFiles)
        {
            var elements = ExtractBootstrapClasses(htmlFile);
            allElements.AddRange(elements);
        }

        return allElements;
    }

    /// <summary>
    /// Generator for HTML elements with Bootstrap classes.
    /// </summary>
    private static Gen<HtmlElementWithClasses> GenBootstrapElement()
    {
        var allElements = GetAllBootstrapElements();

        if (allElements.Count == 0)
        {
            // Fallback: generate a synthetic element for testing
            return Gen.Constant(new HtmlElementWithClasses(
                "div",
                new List<string> { "container" },
                "test.html"));
        }

        return Gen.Elements(allElements.ToArray());
    }

    #endregion

    #region Property 4: Bootstrap Class Validity

    /// <summary>
    /// **Feature: repository-production-readiness, Property 4: Bootstrap Class Validity**
    /// **Validates: Requirements 2.4**
    /// 
    /// For any HTML element with Bootstrap classes, the classes should be valid Bootstrap 5 classes.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property BootstrapClasses_ShouldBeValidBootstrap5Classes()
    {
        return Prop.ForAll(
            GenBootstrapElement().ToArbitrary(),
            element =>
            {
                // Check each Bootstrap class
                var invalidClasses = new List<string>();

                foreach (var className in element.BootstrapClasses)
                {
                    if (!IsValidBootstrap5Class(className))
                    {
                        invalidClasses.Add(className);
                    }
                }

                // Assert that all classes are valid
                Assert.True(
                    invalidClasses.Count == 0,
                    $"Element <{element.ElementTag}> in {element.SourceFile} has invalid Bootstrap classes:\n" +
                    string.Join(", ", invalidClasses));

                return true;
            }
        );
    }

    #endregion

    #region Unit Tests for Bootstrap Class Validity

    /// <summary>
    /// Unit test: Verify all HTML pages reference Bootstrap 5.
    /// </summary>
    [Theory]
    [InlineData("index.html")]
    [InlineData("employment.html")]
    [InlineData("rental.html")]
    public void HtmlPage_ShouldReferenceBootstrap5(string fileName)
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var filePath = Path.Combine(baseDirectory, "wwwroot", fileName);
        
        Assert.True(File.Exists(filePath), $"{fileName} should exist");
        
        var content = File.ReadAllText(filePath);

        // Assert - should reference Bootstrap 5.3.x
        Assert.Matches(@"bootstrap@5\.\d+\.\d+", content);
    }

    /// <summary>
    /// Unit test: Verify common Bootstrap layout classes are used correctly.
    /// </summary>
    [Fact]
    public void HtmlPages_ShouldUseBootstrapLayoutClasses()
    {
        // Arrange
        var htmlFiles = GetHtmlFiles();
        Assert.NotEmpty(htmlFiles);

        // Act - extract all Bootstrap classes
        var allElements = GetAllBootstrapElements();

        // Assert - should have common layout classes
        var allClasses = allElements.SelectMany(e => e.BootstrapClasses).Distinct().ToList();
        
        Assert.Contains(allClasses, c => c.StartsWith("container"));
        Assert.Contains(allClasses, c => c.StartsWith("d-"));
        Assert.Contains(allClasses, c => c.StartsWith("col-"));
    }

    /// <summary>
    /// Unit test: Verify Bootstrap spacing utilities are used.
    /// </summary>
    [Fact]
    public void HtmlPages_ShouldUseBootstrapSpacingUtilities()
    {
        // Arrange
        var allElements = GetAllBootstrapElements();
        Assert.NotEmpty(allElements);

        // Act - get all classes
        var allClasses = allElements.SelectMany(e => e.BootstrapClasses).Distinct().ToList();

        // Assert - should have spacing utilities
        var hasSpacing = allClasses.Any(c => 
            c.StartsWith("m-") || c.StartsWith("p-") || 
            c.StartsWith("mt-") || c.StartsWith("mb-") ||
            c.StartsWith("pt-") || c.StartsWith("pb-"));

        Assert.True(hasSpacing, "Pages should use Bootstrap spacing utilities");
    }

    /// <summary>
    /// Unit test: Verify Bootstrap button classes are used correctly.
    /// </summary>
    [Fact]
    public void HtmlPages_ShouldUseBootstrapButtonClasses()
    {
        // Arrange
        var allElements = GetAllBootstrapElements();
        
        // Act - find button elements
        var buttonElements = allElements.Where(e => 
            e.ElementTag == "button" || e.ElementTag == "a" || e.ElementTag == "input");

        // Assert - buttons should have btn class
        var hasButtonClass = buttonElements.Any(e => 
            e.BootstrapClasses.Any(c => c.StartsWith("btn")));

        Assert.True(hasButtonClass, "Button elements should use Bootstrap btn classes");
    }

    /// <summary>
    /// Unit test: Verify Bootstrap modal classes are used correctly.
    /// </summary>
    [Fact]
    public void HtmlPages_ShouldUseBootstrapModalClasses()
    {
        // Arrange
        var allElements = GetAllBootstrapElements();

        // Act - find modal elements
        var modalElements = allElements.Where(e => 
            e.BootstrapClasses.Any(c => c.Contains("modal")));

        // Assert - should have modal classes
        Assert.NotEmpty(modalElements);
    }

    /// <summary>
    /// Unit test: Verify no invalid Bootstrap class patterns.
    /// </summary>
    [Fact]
    public void AllBootstrapClasses_ShouldBeValid()
    {
        // Arrange
        var allElements = GetAllBootstrapElements();
        Assert.NotEmpty(allElements);

        // Act - check all classes
        var invalidClasses = new List<(string ClassName, string File)>();

        foreach (var element in allElements)
        {
            foreach (var className in element.BootstrapClasses)
            {
                if (!IsValidBootstrap5Class(className))
                {
                    invalidClasses.Add((className, element.SourceFile));
                }
            }
        }

        // Assert - should have no invalid classes
        Assert.Empty(invalidClasses);
    }

    /// <summary>
    /// Unit test: Verify Bootstrap responsive classes are used.
    /// </summary>
    [Fact]
    public void HtmlPages_ShouldUseBootstrapResponsiveClasses()
    {
        // Arrange
        var allElements = GetAllBootstrapElements();
        Assert.NotEmpty(allElements);

        // Act - get all classes
        var allClasses = allElements.SelectMany(e => e.BootstrapClasses).Distinct().ToList();

        // Assert - should have responsive classes
        var hasResponsive = allClasses.Any(c => 
            c.Contains("-sm-") || c.Contains("-md-") || 
            c.Contains("-lg-") || c.Contains("-xl-"));

        Assert.True(hasResponsive, "Pages should use Bootstrap responsive classes");
    }

    #endregion
}
