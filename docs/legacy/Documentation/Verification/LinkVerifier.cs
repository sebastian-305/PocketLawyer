using System.Text.RegularExpressions;

namespace PocketLawyer.Documentation.Verification;

/// <summary>
/// Verifies markdown links in documentation files.
/// Checks that internal links resolve, anchor links exist, and cross-references are valid.
/// </summary>
/// <remarks>
/// Validates Requirements: 6.5, 11.7
/// Property 7: Markdown links resolve correctly
/// Property 14: Cross-references between documents are valid
/// </remarks>
public class LinkVerifier
{
    private readonly string _repositoryRoot;

    // Regex patterns for extracting markdown links
    private static readonly Regex MarkdownLinkPattern = new Regex(
        @"\[([^\]]+)\]\(([^\)]+)\)",
        RegexOptions.Compiled);

    private static readonly Regex AnchorPattern = new Regex(
        @"^#+\s+(.+)$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public LinkVerifier(string repositoryRoot)
    {
        _repositoryRoot = repositoryRoot ?? throw new ArgumentNullException(nameof(repositoryRoot));
    }

    /// <summary>
    /// Verifies all links in a documentation file.
    /// </summary>
    /// <param name="documentContent">Content of the documentation file</param>
    /// <param name="documentPath">Path to the documentation file (for resolving relative links)</param>
    /// <returns>List of verification errors</returns>
    public List<VerificationError> VerifyLinks(string documentContent, string documentPath)
    {
        var errors = new List<VerificationError>();

        // Extract all markdown links
        var links = ExtractMarkdownLinks(documentContent);

        // Verify each link
        foreach (var link in links)
        {
            // Skip external links (http://, https://, mailto:, etc.)
            if (IsExternalLink(link.Url))
            {
                continue;
            }

            // Parse link into file path and anchor
            var (filePath, anchor) = ParseLink(link.Url);

            // Resolve relative path
            var resolvedPath = ResolveRelativePath(documentPath, filePath);

            // Verify file exists
            if (!string.IsNullOrEmpty(filePath))
            {
                var fullPath = Path.Combine(_repositoryRoot, resolvedPath);
                if (!File.Exists(fullPath))
                {
                    errors.Add(new VerificationError
                    {
                        Type = "BrokenLink",
                        Claim = $"Link: [{link.Text}]({link.Url})",
                        ExpectedSource = resolvedPath,
                        Message = $"Link target does not exist: {resolvedPath}"
                    });
                    continue;
                }

                // Verify anchor exists if specified
                if (!string.IsNullOrEmpty(anchor))
                {
                    var targetContent = File.ReadAllText(fullPath);
                    if (!AnchorExists(targetContent, anchor))
                    {
                        errors.Add(new VerificationError
                        {
                            Type = "BrokenAnchor",
                            Claim = $"Link: [{link.Text}]({link.Url})",
                            ExpectedSource = $"{resolvedPath}#{anchor}",
                            Message = $"Anchor does not exist in target file: #{anchor}"
                        });
                    }
                }
            }
            else if (!string.IsNullOrEmpty(anchor))
            {
                // Anchor-only link (same document)
                if (!AnchorExists(documentContent, anchor))
                {
                    errors.Add(new VerificationError
                    {
                        Type = "BrokenAnchor",
                        Claim = $"Link: [{link.Text}]({link.Url})",
                        ExpectedSource = $"#{anchor}",
                        Message = $"Anchor does not exist in current document: #{anchor}"
                    });
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Extracts all markdown links from content.
    /// </summary>
    /// <param name="content">Markdown content</param>
    /// <returns>List of markdown links</returns>
    private List<MarkdownLink> ExtractMarkdownLinks(string content)
    {
        var links = new List<MarkdownLink>();

        foreach (Match match in MarkdownLinkPattern.Matches(content))
        {
            links.Add(new MarkdownLink
            {
                Text = match.Groups[1].Value,
                Url = match.Groups[2].Value
            });
        }

        return links;
    }

    /// <summary>
    /// Checks if a URL is an external link.
    /// </summary>
    /// <param name="url">URL to check</param>
    /// <returns>True if the URL is external</returns>
    private bool IsExternalLink(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses a link URL into file path and anchor components.
    /// </summary>
    /// <param name="url">Link URL</param>
    /// <returns>Tuple of (filePath, anchor)</returns>
    private (string filePath, string anchor) ParseLink(string url)
    {
        var anchorIndex = url.IndexOf('#');
        if (anchorIndex == -1)
        {
            // No anchor
            return (url, string.Empty);
        }
        else if (anchorIndex == 0)
        {
            // Anchor only (same document)
            return (string.Empty, url.Substring(1));
        }
        else
        {
            // File path and anchor
            var filePath = url.Substring(0, anchorIndex);
            var anchor = url.Substring(anchorIndex + 1);
            return (filePath, anchor);
        }
    }

    /// <summary>
    /// Resolves a relative path from a document path.
    /// </summary>
    /// <param name="documentPath">Path to the current document</param>
    /// <param name="relativePath">Relative path from the link</param>
    /// <returns>Resolved path relative to repository root</returns>
    private string ResolveRelativePath(string documentPath, string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return documentPath;
        }

        // Get directory of current document
        var documentDir = Path.GetDirectoryName(documentPath) ?? string.Empty;

        // Combine and normalize
        var combined = Path.Combine(documentDir, relativePath);
        
        // Normalize path separators
        combined = combined.Replace('/', Path.DirectorySeparatorChar)
                          .Replace('\\', Path.DirectorySeparatorChar);

        // Normalize to remove .. and . segments
        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_repositoryRoot, combined));
            var relativeFull = Path.GetRelativePath(_repositoryRoot, fullPath);
            return relativeFull;
        }
        catch
        {
            // If path normalization fails, return as-is
            return combined;
        }
    }

    /// <summary>
    /// Checks if an anchor exists in markdown content.
    /// </summary>
    /// <param name="content">Markdown content</param>
    /// <param name="anchor">Anchor name (without #)</param>
    /// <returns>True if the anchor exists</returns>
    private bool AnchorExists(string content, string anchor)
    {
        // Extract all headings from the content
        var headings = ExtractHeadings(content);

        // Convert anchor to lowercase and replace spaces/special chars with hyphens
        var normalizedAnchor = NormalizeAnchor(anchor);

        // Check if any heading matches the anchor
        foreach (var heading in headings)
        {
            var normalizedHeading = NormalizeAnchor(heading);
            if (normalizedHeading == normalizedAnchor)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts all headings from markdown content.
    /// </summary>
    /// <param name="content">Markdown content</param>
    /// <returns>List of heading texts</returns>
    private List<string> ExtractHeadings(string content)
    {
        var headings = new List<string>();

        foreach (Match match in AnchorPattern.Matches(content))
        {
            headings.Add(match.Groups[1].Value.Trim());
        }

        return headings;
    }

    /// <summary>
    /// Normalizes an anchor or heading for comparison.
    /// Converts to lowercase and replaces spaces/special chars with hyphens.
    /// </summary>
    /// <param name="text">Text to normalize</param>
    /// <returns>Normalized anchor</returns>
    private string NormalizeAnchor(string text)
    {
        // Convert to lowercase
        var normalized = text.ToLowerInvariant();

        // Remove markdown formatting (bold, italic, code)
        normalized = Regex.Replace(normalized, @"\*\*([^\*]+)\*\*", "$1"); // Bold
        normalized = Regex.Replace(normalized, @"\*([^\*]+)\*", "$1");     // Italic
        normalized = Regex.Replace(normalized, @"`([^`]+)`", "$1");        // Code

        // Replace spaces and special characters with hyphens
        normalized = Regex.Replace(normalized, @"[^\w\-]+", "-");

        // Remove leading/trailing hyphens
        normalized = normalized.Trim('-');

        // Collapse multiple hyphens
        normalized = Regex.Replace(normalized, @"-+", "-");

        return normalized;
    }

    /// <summary>
    /// Verifies cross-references between documentation files.
    /// </summary>
    /// <param name="documentPaths">List of all documentation file paths</param>
    /// <returns>List of verification errors</returns>
    public List<VerificationError> VerifyCrossReferences(IEnumerable<string> documentPaths)
    {
        var errors = new List<VerificationError>();
        var documentPathsList = documentPaths.ToList();

        foreach (var documentPath in documentPathsList)
        {
            if (!File.Exists(documentPath))
            {
                continue;
            }

            var content = File.ReadAllText(documentPath);
            var linkErrors = VerifyLinks(content, documentPath);

            // Filter for cross-reference errors (links to other documentation files)
            var crossRefErrors = linkErrors.Where(e =>
                e.Type == "BrokenLink" &&
                (e.ExpectedSource.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                 e.ExpectedSource.Contains("docs/", StringComparison.OrdinalIgnoreCase)));

            errors.AddRange(crossRefErrors);
        }

        return errors;
    }

    /// <summary>
    /// Verifies that a specific link resolves correctly.
    /// </summary>
    /// <param name="linkUrl">Link URL to verify</param>
    /// <param name="documentPath">Path to the document containing the link</param>
    /// <returns>True if the link is valid</returns>
    public bool VerifyLink(string linkUrl, string documentPath)
    {
        // Skip external links
        if (IsExternalLink(linkUrl))
        {
            return true;
        }

        // Parse link
        var (filePath, anchor) = ParseLink(linkUrl);

        // Resolve relative path
        var resolvedPath = ResolveRelativePath(documentPath, filePath);

        // Check file exists
        if (!string.IsNullOrEmpty(filePath))
        {
            var fullPath = Path.Combine(_repositoryRoot, resolvedPath);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            // Check anchor if specified
            if (!string.IsNullOrEmpty(anchor))
            {
                var targetContent = File.ReadAllText(fullPath);
                return AnchorExists(targetContent, anchor);
            }

            return true;
        }
        else if (!string.IsNullOrEmpty(anchor))
        {
            // Anchor-only link (same document)
            var documentContent = File.ReadAllText(Path.Combine(_repositoryRoot, documentPath));
            return AnchorExists(documentContent, anchor);
        }

        return false;
    }

    /// <summary>
    /// Gets all internal links from a document.
    /// </summary>
    /// <param name="documentContent">Content of the documentation file</param>
    /// <returns>List of internal markdown links</returns>
    public List<MarkdownLink> GetInternalLinks(string documentContent)
    {
        var allLinks = ExtractMarkdownLinks(documentContent);
        return allLinks.Where(link => !IsExternalLink(link.Url)).ToList();
    }

    /// <summary>
    /// Gets all anchors (headings) from a document.
    /// </summary>
    /// <param name="documentContent">Content of the documentation file</param>
    /// <returns>List of anchor names</returns>
    public List<string> GetAnchors(string documentContent)
    {
        var headings = ExtractHeadings(documentContent);
        return headings.Select(h => NormalizeAnchor(h)).ToList();
    }
}

/// <summary>
/// Represents a markdown link.
/// </summary>
public class MarkdownLink
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
