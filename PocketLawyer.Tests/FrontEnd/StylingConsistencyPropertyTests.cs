using FsCheck;
using FsCheck.Xunit;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PocketLawyer.Tests.FrontEnd;

/// <summary>
/// Property-based tests for styling consistency across pages.
/// **Feature: repository-production-readiness**
/// </summary>
public class StylingConsistencyPropertyTests
{
    #region Generators

    /// <summary>
    /// Represents a CSS color variable or class definition.
    /// </summary>
    private record CssColorDefinition(
        string VariableName,
        string ColorValue,
        string SourceFile);

    /// <summary>
    /// Represents an HTML page with its CSS references.
    /// </summary>
    private record HtmlPageInfo(string FilePath, string Content);

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
    /// Gets all CSS files from the wwwroot/css directory.
    /// </summary>
    private static List<string> GetCssFiles()
    {
        var baseDirectory = GetProjectRootDirectory();
        var cssDirectory = Path.Combine(baseDirectory, "wwwroot", "css");

        if (!Directory.Exists(cssDirectory))
        {
            return new List<string>();
        }

        return Directory.GetFiles(cssDirectory, "*.css").ToList();
    }

    /// <summary>
    /// Gets all HTML pages from the wwwroot directory.
    /// </summary>
    private static List<HtmlPageInfo> GetHtmlPages()
    {
        var pages = new List<HtmlPageInfo>();
        var baseDirectory = GetProjectRootDirectory();
        var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");

        if (!Directory.Exists(wwwrootPath))
        {
            return pages;
        }

        var htmlFiles = new[] { "index.html", "employment.html", "rental.html" };

        foreach (var fileName in htmlFiles)
        {
            var filePath = Path.Combine(wwwrootPath, fileName);
            if (File.Exists(filePath))
            {
                var relativePath = Path.GetRelativePath(baseDirectory, filePath);
                var content = File.ReadAllText(filePath);
                pages.Add(new HtmlPageInfo(relativePath, content));
            }
        }

        return pages;
    }

    /// <summary>
    /// Extracts CSS color variables from a CSS file.
    /// </summary>
    private static List<CssColorDefinition> ExtractColorVariables(string cssFilePath)
    {
        var colorDefinitions = new List<CssColorDefinition>();
        var content = File.ReadAllText(cssFilePath);
        var fileName = Path.GetFileName(cssFilePath);

        // Match CSS variable definitions: --variable-name: #hexcolor or color-name;
        var variablePattern = @"--([a-zA-Z0-9-]+)\s*:\s*(#[0-9a-fA-F]{3,8}|[a-zA-Z]+)\s*;";
        var matches = Regex.Matches(content, variablePattern);

        foreach (Match match in matches)
        {
            var variableName = match.Groups[1].Value;
            var colorValue = match.Groups[2].Value.ToLowerInvariant();

            colorDefinitions.Add(new CssColorDefinition(
                variableName,
                colorValue,
                fileName));
        }

        return colorDefinitions;
    }

    /// <summary>
    /// Extracts hardcoded color values from CSS content.
    /// </summary>
    private static List<string> ExtractHardcodedColors(string cssContent)
    {
        var hardcodedColors = new List<string>();

        // Match color properties with hardcoded values (not using var())
        // Exclude CSS variable definitions (lines starting with --)
        var colorPattern = @"(?<!--[a-zA-Z0-9-]*\s*:\s*)(color|background-color|border-color|background)\s*:\s*(?!var\()([#a-zA-Z0-9]+)";
        var matches = Regex.Matches(cssContent, colorPattern);

        foreach (Match match in matches)
        {
            var colorValue = match.Groups[2].Value.ToLowerInvariant();
            
            // Filter out non-color values like 'none', 'inherit', 'transparent'
            if (colorValue != "none" && 
                colorValue != "inherit" && 
                colorValue != "transparent" &&
                colorValue != "auto" &&
                !colorValue.StartsWith("url("))
            {
                hardcodedColors.Add(colorValue);
            }
        }

        return hardcodedColors;
    }

    /// <summary>
    /// Gets all CSS color variables defined across all CSS files.
    /// </summary>
    private static Dictionary<string, List<CssColorDefinition>> GetAllColorVariables()
    {
        var allVariables = new Dictionary<string, List<CssColorDefinition>>();
        var cssFiles = GetCssFiles();

        foreach (var cssFile in cssFiles)
        {
            var variables = ExtractColorVariables(cssFile);
            
            foreach (var variable in variables)
            {
                if (!allVariables.ContainsKey(variable.VariableName))
                {
                    allVariables[variable.VariableName] = new List<CssColorDefinition>();
                }
                allVariables[variable.VariableName].Add(variable);
            }
        }

        return allVariables;
    }

    /// <summary>
    /// Generator for CSS color variables that appear in multiple files.
    /// </summary>
    private static Gen<KeyValuePair<string, List<CssColorDefinition>>> GenSharedColorVariable()
    {
        var allVariables = GetAllColorVariables();
        
        // Filter to only variables that appear in multiple files
        var sharedVariables = allVariables
            .Where(kvp => kvp.Value.Count > 1)
            .ToList();

        if (sharedVariables.Count == 0)
        {
            // If no shared variables, return a dummy entry
            return Gen.Constant(new KeyValuePair<string, List<CssColorDefinition>>(
                "dummy",
                new List<CssColorDefinition>()));
        }

        return Gen.Elements(sharedVariables.ToArray());
    }

    #endregion

    #region Property 3: Styling Consistency Across Pages

    /// <summary>
    /// **Feature: repository-production-readiness, Property 3: Styling Consistency Across Pages**
    /// **Validates: Requirements 2.2, 2.3**
    /// 
    /// For any CSS color variable or class used across multiple pages, 
    /// the color values should be identical across all pages.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SharedColorVariables_ShouldHaveIdenticalValues()
    {
        return Prop.ForAll(
            GenSharedColorVariable().ToArbitrary(),
            sharedVariable =>
            {
                // Skip dummy entries
                if (sharedVariable.Key == "dummy")
                {
                    return true;
                }

                var variableName = sharedVariable.Key;
                var definitions = sharedVariable.Value;

                // Get all unique color values for this variable
                var uniqueColors = definitions
                    .Select(d => d.ColorValue)
                    .Distinct()
                    .ToList();

                // Assert that all definitions use the same color value
                Assert.True(
                    uniqueColors.Count == 1,
                    $"CSS variable '--{variableName}' has inconsistent color values across files:\n" +
                    string.Join("\n", definitions.Select(d => 
                        $"  {d.SourceFile}: {d.ColorValue}")));

                return true;
            }
        );
    }

    #endregion

    #region Unit Tests for Styling Consistency

    /// <summary>
    /// Unit test: Verify styles.css exists in the css folder.
    /// </summary>
    [Fact]
    public void StylesCss_ShouldExistInCssFolder()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var stylesPath = Path.Combine(baseDirectory, "wwwroot", "css", "styles.css");

        // Assert
        Assert.True(File.Exists(stylesPath), "styles.css should exist in wwwroot/css/");
    }

    /// <summary>
    /// Unit test: Verify all HTML pages reference CSS files from the css folder.
    /// </summary>
    [Theory]
    [InlineData("index.html")]
    [InlineData("employment.html")]
    [InlineData("rental.html")]
    public void HtmlPage_ShouldReferenceCssFromCssFolder(string fileName)
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var filePath = Path.Combine(baseDirectory, "wwwroot", fileName);
        
        Assert.True(File.Exists(filePath), $"{fileName} should exist");
        
        var content = File.ReadAllText(filePath);

        // Assert - should reference CSS from css/ folder
        Assert.Matches(@"href=""css/[^""]+\.css""", content);
    }

    /// <summary>
    /// Unit test: Verify CSS color variables are defined in styles.css.
    /// </summary>
    [Theory]
    [InlineData("blue-bg")]
    [InlineData("paper-bg")]
    [InlineData("highlight")]
    [InlineData("neutral-bg")]
    public void StylesCss_ShouldDefineColorVariable(string variableName)
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var stylesPath = Path.Combine(baseDirectory, "wwwroot", "css", "styles.css");
        
        Assert.True(File.Exists(stylesPath), "styles.css should exist");
        
        var content = File.ReadAllText(stylesPath);

        // Assert - should define the color variable
        Assert.Matches($@"--{variableName}\s*:\s*#[0-9a-fA-F]+", content);
    }

    /// <summary>
    /// Unit test: Verify no duplicate color variable definitions with different values.
    /// </summary>
    [Fact]
    public void AllCssFiles_ShouldNotHaveDuplicateVariablesWithDifferentValues()
    {
        // Arrange
        var allVariables = GetAllColorVariables();

        // Act & Assert - check each variable
        var inconsistentVariables = new List<string>();

        foreach (var (variableName, definitions) in allVariables)
        {
            var uniqueColors = definitions
                .Select(d => d.ColorValue)
                .Distinct()
                .ToList();

            if (uniqueColors.Count > 1)
            {
                inconsistentVariables.Add(
                    $"--{variableName}: " + 
                    string.Join(", ", definitions.Select(d => $"{d.SourceFile}={d.ColorValue}")));
            }
        }

        Assert.Empty(inconsistentVariables);
    }

    /// <summary>
    /// Unit test: Verify minimal use of hardcoded colors in styles.css.
    /// Hardcoded colors should be replaced with CSS variables for consistency.
    /// </summary>
    [Fact]
    public void StylesCss_ShouldMinimizeHardcodedColors()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var stylesPath = Path.Combine(baseDirectory, "wwwroot", "css", "styles.css");
        
        Assert.True(File.Exists(stylesPath), "styles.css should exist");
        
        var content = File.ReadAllText(stylesPath);

        // Extract hardcoded colors (excluding variable definitions)
        var hardcodedColors = ExtractHardcodedColors(content);

        // Filter out acceptable hardcoded values
        var problematicColors = hardcodedColors
            .Where(c => c != "black" && c != "white" && !c.StartsWith("rgba"))
            .Distinct()
            .ToList();

        // Assert - should have minimal hardcoded colors
        // We allow some hardcoded colors for specific cases, but not many
        Assert.True(
            problematicColors.Count <= 5,
            $"styles.css should minimize hardcoded colors. Found: {string.Join(", ", problematicColors)}");
    }

    /// <summary>
    /// Unit test: Verify all pages use the same CSS files.
    /// </summary>
    [Fact]
    public void AllPages_ShouldUseConsistentCssFiles()
    {
        // Arrange
        var pages = GetHtmlPages();
        Assert.NotEmpty(pages);

        // Extract CSS references from each page
        var cssReferences = new Dictionary<string, List<string>>();
        var cssPattern = @"href=""([^""]+\.css)""";

        foreach (var page in pages)
        {
            var matches = Regex.Matches(page.Content, cssPattern);
            var refs = matches.Select(m => m.Groups[1].Value).ToList();
            cssReferences[page.FilePath] = refs;
        }

        // Assert - all pages should reference styles.css
        foreach (var (filePath, refs) in cssReferences)
        {
            Assert.Contains("css/styles.css", refs);
        }
    }

    /// <summary>
    /// Unit test: Verify color scheme uses CSS variables consistently.
    /// </summary>
    [Fact]
    public void StylesCss_ShouldUseVariablesForColors()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var stylesPath = Path.Combine(baseDirectory, "wwwroot", "css", "styles.css");
        
        Assert.True(File.Exists(stylesPath), "styles.css should exist");
        
        var content = File.ReadAllText(stylesPath);

        // Count var() usages vs hardcoded color values
        var varUsages = Regex.Matches(content, @"var\(--[a-zA-Z0-9-]+\)").Count;
        var hardcodedColors = ExtractHardcodedColors(content)
            .Where(c => c != "black" && c != "white")
            .Count();

        // Assert - should use variables more than hardcoded colors
        Assert.True(
            varUsages > hardcodedColors,
            $"styles.css should use CSS variables more than hardcoded colors. " +
            $"Found {varUsages} var() usages and {hardcodedColors} hardcoded colors.");
    }

    /// <summary>
    /// Unit test: Verify new color variables for consistency were added.
    /// </summary>
    [Theory]
    [InlineData("blue-bg-dark", "Dark blue variant")]
    [InlineData("highlight-hover", "Highlight hover state")]
    [InlineData("accent-orange", "Orange accent color")]
    [InlineData("error-bg", "Error background")]
    [InlineData("error-text", "Error text")]
    public void StylesCss_ShouldDefineConsistencyVariables(string variableName, string description)
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var stylesPath = Path.Combine(baseDirectory, "wwwroot", "css", "styles.css");
        
        Assert.True(File.Exists(stylesPath), "styles.css should exist");
        
        var content = File.ReadAllText(stylesPath);

        // Assert - should define the consistency variable
        Assert.Matches($@"--{variableName}\s*:\s*#[0-9a-fA-F]+", content);
    }

    /// <summary>
    /// Unit test: Verify CSS files are properly organized in css folder.
    /// </summary>
    [Fact]
    public void CssFolder_ShouldContainAllCssFiles()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var cssDirectory = Path.Combine(baseDirectory, "wwwroot", "css");

        // Assert - css folder should exist
        Assert.True(Directory.Exists(cssDirectory), "wwwroot/css/ folder should exist");

        // Assert - should contain expected CSS files
        var expectedFiles = new[] { "styles.css", "praes.css", "print.css" };
        
        foreach (var fileName in expectedFiles)
        {
            var filePath = Path.Combine(cssDirectory, fileName);
            Assert.True(File.Exists(filePath), $"{fileName} should exist in wwwroot/css/");
        }
    }

    /// <summary>
    /// Unit test: Verify no CSS files remain in wwwroot root.
    /// </summary>
    [Fact]
    public void WwwrootRoot_ShouldNotContainCssFiles()
    {
        // Arrange
        var baseDirectory = GetProjectRootDirectory();
        var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");

        // Get CSS files directly in wwwroot (not in subdirectories)
        var cssFiles = Directory.GetFiles(wwwrootPath, "*.css", SearchOption.TopDirectoryOnly);

        // Assert - should not have CSS files in root
        Assert.Empty(cssFiles);
    }

    #endregion
}
