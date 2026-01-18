using System.Text.RegularExpressions;

namespace PocketLawyer.Documentation.Verification;

/// <summary>
/// Verifies that all file paths referenced in documentation point to existing files.
/// Checks code files, configuration files, and directory references.
/// </summary>
/// <remarks>
/// Validates Requirements: 6.4, 8.1, 8.2, 8.3, 8.4
/// Property 4: File references point to existing files
/// </remarks>
public class FileReferenceVerifier
{
    private readonly string _repositoryRoot;

    // Regex patterns for extracting file references from documentation
    private static readonly Regex CodeFilePattern = new Regex(
        @"`([A-Za-z0-9_/\.]+\.cs)`",
        RegexOptions.Compiled);

    private static readonly Regex JsonFilePattern = new Regex(
        @"`([A-Za-z0-9_/\.]+\.json)`",
        RegexOptions.Compiled);

    private static readonly Regex CsprojFilePattern = new Regex(
        @"`([A-Za-z0-9_/\.]+\.csproj)`",
        RegexOptions.Compiled);

    private static readonly Regex DirectoryPattern = new Regex(
        @"`([A-Za-z0-9_/]+/)`",
        RegexOptions.Compiled);

    private static readonly Regex InlineCodeReferencePattern = new Regex(
        @"in `([A-Za-z0-9_/\.]+\.[a-z]+)`",
        RegexOptions.Compiled);

    private static readonly Regex ParenthesizedFilePattern = new Regex(
        @"\(`([A-Za-z0-9_/\.]+\.[a-z]+)`\)",
        RegexOptions.Compiled);

    public FileReferenceVerifier(string repositoryRoot)
    {
        _repositoryRoot = repositoryRoot ?? throw new ArgumentNullException(nameof(repositoryRoot));
    }

    /// <summary>
    /// Verifies all file references in a documentation file.
    /// </summary>
    /// <param name="documentContent">Content of the documentation file</param>
    /// <param name="documentPath">Path to the documentation file (for error reporting)</param>
    /// <returns>List of verification errors</returns>
    public List<VerificationError> VerifyFileReferences(string documentContent, string documentPath)
    {
        var errors = new List<VerificationError>();

        // Extract all file references
        var fileReferences = ExtractFileReferences(documentContent);

        // Verify each file reference
        foreach (var fileRef in fileReferences)
        {
            if (!FileOrDirectoryExists(fileRef))
            {
                errors.Add(new VerificationError
                {
                    Type = "FileReferenceNotFound",
                    Claim = $"File reference: {fileRef}",
                    ExpectedSource = fileRef,
                    Message = $"Referenced file or directory does not exist in repository: {fileRef}"
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Extracts all file and directory references from documentation content.
    /// </summary>
    /// <param name="content">Documentation content</param>
    /// <returns>List of unique file/directory references</returns>
    private List<string> ExtractFileReferences(string content)
    {
        var references = new HashSet<string>();

        // Extract C# file references
        foreach (Match match in CodeFilePattern.Matches(content))
        {
            references.Add(match.Groups[1].Value);
        }

        // Extract JSON file references
        foreach (Match match in JsonFilePattern.Matches(content))
        {
            references.Add(match.Groups[1].Value);
        }

        // Extract .csproj file references
        foreach (Match match in CsprojFilePattern.Matches(content))
        {
            references.Add(match.Groups[1].Value);
        }

        // Extract directory references
        foreach (Match match in DirectoryPattern.Matches(content))
        {
            references.Add(match.Groups[1].Value);
        }

        // Extract inline code references (e.g., "in `Path/To/File.cs`")
        foreach (Match match in InlineCodeReferencePattern.Matches(content))
        {
            references.Add(match.Groups[1].Value);
        }

        // Extract parenthesized file references (e.g., "(`Path/To/File.cs`)")
        foreach (Match match in ParenthesizedFilePattern.Matches(content))
        {
            references.Add(match.Groups[1].Value);
        }

        return references.ToList();
    }

    /// <summary>
    /// Checks if a file or directory exists in the repository.
    /// </summary>
    /// <param name="path">Relative path from repository root</param>
    /// <returns>True if file or directory exists</returns>
    private bool FileOrDirectoryExists(string path)
    {
        // Normalize path separators
        var normalizedPath = path.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_repositoryRoot, normalizedPath);

        // Check if it's a file
        if (File.Exists(fullPath))
        {
            return true;
        }

        // Check if it's a directory (with or without trailing slash)
        if (Directory.Exists(fullPath))
        {
            return true;
        }

        // Try without trailing slash if present
        if (normalizedPath.EndsWith(Path.DirectorySeparatorChar))
        {
            var pathWithoutSlash = normalizedPath.TrimEnd(Path.DirectorySeparatorChar);
            fullPath = Path.Combine(_repositoryRoot, pathWithoutSlash);
            if (Directory.Exists(fullPath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Verifies that a specific file reference exists.
    /// </summary>
    /// <param name="fileReference">File or directory path to verify</param>
    /// <returns>True if the file or directory exists</returns>
    public bool VerifyFileReference(string fileReference)
    {
        return FileOrDirectoryExists(fileReference);
    }

    /// <summary>
    /// Gets the full path for a file reference.
    /// </summary>
    /// <param name="fileReference">Relative file path</param>
    /// <returns>Full path to the file</returns>
    public string GetFullPath(string fileReference)
    {
        var normalizedPath = fileReference.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_repositoryRoot, normalizedPath);
    }

    /// <summary>
    /// Verifies that all files in a list of file references exist.
    /// </summary>
    /// <param name="fileReferences">List of file references to verify</param>
    /// <returns>Dictionary mapping file references to existence status</returns>
    public Dictionary<string, bool> VerifyMultipleFileReferences(IEnumerable<string> fileReferences)
    {
        var results = new Dictionary<string, bool>();

        foreach (var fileRef in fileReferences)
        {
            results[fileRef] = FileOrDirectoryExists(fileRef);
        }

        return results;
    }

    /// <summary>
    /// Extracts and verifies all code file references (*.cs files).
    /// </summary>
    /// <param name="documentContent">Documentation content</param>
    /// <returns>List of verification errors for code files</returns>
    public List<VerificationError> VerifyCodeFileReferences(string documentContent)
    {
        var errors = new List<VerificationError>();
        var codeFiles = new HashSet<string>();

        // Extract C# file references
        foreach (Match match in CodeFilePattern.Matches(documentContent))
        {
            codeFiles.Add(match.Groups[1].Value);
        }

        foreach (Match match in InlineCodeReferencePattern.Matches(documentContent))
        {
            var file = match.Groups[1].Value;
            if (file.EndsWith(".cs"))
            {
                codeFiles.Add(file);
            }
        }

        // Verify each code file
        foreach (var codeFile in codeFiles)
        {
            if (!FileOrDirectoryExists(codeFile))
            {
                errors.Add(new VerificationError
                {
                    Type = "CodeFileNotFound",
                    Claim = $"Code file reference: {codeFile}",
                    ExpectedSource = codeFile,
                    Message = $"Referenced code file does not exist: {codeFile}"
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Extracts and verifies all configuration file references (*.json files).
    /// </summary>
    /// <param name="documentContent">Documentation content</param>
    /// <returns>List of verification errors for configuration files</returns>
    public List<VerificationError> VerifyConfigFileReferences(string documentContent)
    {
        var errors = new List<VerificationError>();
        var configFiles = new HashSet<string>();

        // Extract JSON file references
        foreach (Match match in JsonFilePattern.Matches(documentContent))
        {
            configFiles.Add(match.Groups[1].Value);
        }

        foreach (Match match in InlineCodeReferencePattern.Matches(documentContent))
        {
            var file = match.Groups[1].Value;
            if (file.EndsWith(".json"))
            {
                configFiles.Add(file);
            }
        }

        // Verify each config file
        foreach (var configFile in configFiles)
        {
            if (!FileOrDirectoryExists(configFile))
            {
                errors.Add(new VerificationError
                {
                    Type = "ConfigFileNotFound",
                    Claim = $"Configuration file reference: {configFile}",
                    ExpectedSource = configFile,
                    Message = $"Referenced configuration file does not exist: {configFile}"
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Extracts and verifies all directory references.
    /// </summary>
    /// <param name="documentContent">Documentation content</param>
    /// <returns>List of verification errors for directories</returns>
    public List<VerificationError> VerifyDirectoryReferences(string documentContent)
    {
        var errors = new List<VerificationError>();
        var directories = new HashSet<string>();

        // Extract directory references
        foreach (Match match in DirectoryPattern.Matches(documentContent))
        {
            directories.Add(match.Groups[1].Value);
        }

        // Verify each directory
        foreach (var directory in directories)
        {
            if (!FileOrDirectoryExists(directory))
            {
                errors.Add(new VerificationError
                {
                    Type = "DirectoryNotFound",
                    Claim = $"Directory reference: {directory}",
                    ExpectedSource = directory,
                    Message = $"Referenced directory does not exist: {directory}"
                });
            }
        }

        return errors;
    }
}
