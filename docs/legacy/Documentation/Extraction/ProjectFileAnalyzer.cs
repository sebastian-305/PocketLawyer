using System.Xml.Linq;

namespace PocketLawyer.Documentation.Extraction;

/// <summary>
/// Analyzes .csproj project files using System.Xml.Linq to extract .NET version
/// (TargetFramework) and NuGet package dependencies (PackageReference elements).
/// </summary>
/// <remarks>
/// Validates Requirements: 5.1, 5.2
/// </remarks>
public class ProjectFileAnalyzer
{
    /// <summary>
    /// Analyzes a .csproj project file and extracts all relevant information.
    /// </summary>
    /// <param name="filePath">Path to the .csproj file</param>
    /// <returns>Information about the project file</returns>
    public ProjectFileInfo AnalyzeProjectFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Project file not found: {filePath}");
        }

        var xmlContent = File.ReadAllText(filePath);
        return AnalyzeProjectXml(xmlContent, filePath);
    }

    /// <summary>
    /// Analyzes .csproj XML content and extracts all relevant information.
    /// </summary>
    /// <param name="xmlContent">The XML content to analyze</param>
    /// <param name="filePath">Optional file path for reference</param>
    /// <returns>Information about the project file</returns>
    public ProjectFileInfo AnalyzeProjectXml(string xmlContent, string? filePath = null)
    {
        var projectInfo = new ProjectFileInfo
        {
            Path = filePath ?? string.Empty
        };

        try
        {
            var document = XDocument.Parse(xmlContent);
            var root = document.Root;

            if (root == null)
            {
                throw new InvalidOperationException("Project file has no root element");
            }

            // Extract TargetFramework from PropertyGroup
            var targetFramework = root.Descendants("TargetFramework").FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(targetFramework))
            {
                projectInfo.TargetFramework = targetFramework;
            }

            // Extract TargetFrameworks (plural) if present
            var targetFrameworks = root.Descendants("TargetFrameworks").FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(targetFrameworks))
            {
                projectInfo.TargetFrameworks = targetFrameworks.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            // Extract SDK attribute from Project element
            var sdkAttribute = root.Attribute("Sdk");
            if (sdkAttribute != null)
            {
                projectInfo.Sdk = sdkAttribute.Value;
            }

            // Extract Nullable setting
            var nullable = root.Descendants("Nullable").FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(nullable))
            {
                projectInfo.Nullable = nullable;
            }

            // Extract ImplicitUsings setting
            var implicitUsings = root.Descendants("ImplicitUsings").FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(implicitUsings))
            {
                projectInfo.ImplicitUsings = implicitUsings;
            }

            // Extract PackageReference elements
            var packageReferences = root.Descendants("PackageReference")
                .Select(pr => new PackageReferenceInfo
                {
                    Name = pr.Attribute("Include")?.Value ?? string.Empty,
                    Version = pr.Attribute("Version")?.Value ?? string.Empty
                })
                .Where(pr => !string.IsNullOrEmpty(pr.Name))
                .ToList();

            projectInfo.PackageReferences = packageReferences;

            // Extract ProjectReference elements
            var projectReferences = root.Descendants("ProjectReference")
                .Select(pr => pr.Attribute("Include")?.Value ?? string.Empty)
                .Where(pr => !string.IsNullOrEmpty(pr))
                .ToList();

            projectInfo.ProjectReferences = projectReferences;
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidOperationException($"Failed to parse project file XML: {ex.Message}", ex);
        }

        return projectInfo;
    }

    /// <summary>
    /// Extracts the .NET version from the TargetFramework string.
    /// For example, "net8.0" returns "8.0", "net6.0" returns "6.0".
    /// </summary>
    /// <param name="targetFramework">The TargetFramework value (e.g., "net8.0")</param>
    /// <returns>The .NET version number</returns>
    public string ExtractDotNetVersion(string targetFramework)
    {
        if (string.IsNullOrEmpty(targetFramework))
        {
            return string.Empty;
        }

        // Handle formats like "net8.0", "net6.0", "netcoreapp3.1", "net48"
        if (targetFramework.StartsWith("net"))
        {
            var versionPart = targetFramework.Substring(3);
            
            // Remove "coreapp" if present (e.g., "netcoreapp3.1")
            if (versionPart.StartsWith("coreapp"))
            {
                versionPart = versionPart.Substring(7);
            }
            
            // Handle "net48" style (no dot) - insert dot after first digit
            if (!versionPart.Contains('.') && versionPart.Length >= 2)
            {
                versionPart = versionPart.Insert(1, ".");
            }

            return versionPart;
        }

        return targetFramework;
    }
}

/// <summary>
/// Information about a .csproj project file.
/// </summary>
public class ProjectFileInfo
{
    public string Path { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
    public List<string> TargetFrameworks { get; set; } = new();
    public string Sdk { get; set; } = string.Empty;
    public string Nullable { get; set; } = string.Empty;
    public string ImplicitUsings { get; set; } = string.Empty;
    public List<PackageReferenceInfo> PackageReferences { get; set; } = new();
    public List<string> ProjectReferences { get; set; } = new();
}

/// <summary>
/// Information about a NuGet package reference.
/// </summary>
public class PackageReferenceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
