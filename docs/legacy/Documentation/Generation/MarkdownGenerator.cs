using System.Text;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Base class for markdown document generation providing common formatting utilities,
/// code reference formatting, and link generation.
/// </summary>
/// <remarks>
/// Validates Requirements: 8.1, 8.2, 8.3, 8.4, 8.5
/// </remarks>
public class MarkdownGenerator
{
    private readonly StringBuilder _content = new();

    /// <summary>
    /// Gets the current markdown content.
    /// </summary>
    public string Content => _content.ToString();

    /// <summary>
    /// Clears the current content.
    /// </summary>
    public void Clear()
    {
        _content.Clear();
    }

    #region Headers

    /// <summary>
    /// Adds a level 1 header.
    /// </summary>
    public MarkdownGenerator H1(string text)
    {
        _content.AppendLine($"# {text}");
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a level 2 header.
    /// </summary>
    public MarkdownGenerator H2(string text)
    {
        _content.AppendLine($"## {text}");
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a level 3 header.
    /// </summary>
    public MarkdownGenerator H3(string text)
    {
        _content.AppendLine($"### {text}");
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a level 4 header.
    /// </summary>
    public MarkdownGenerator H4(string text)
    {
        _content.AppendLine($"#### {text}");
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a level 5 header.
    /// </summary>
    public MarkdownGenerator H5(string text)
    {
        _content.AppendLine($"##### {text}");
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a level 6 header.
    /// </summary>
    public MarkdownGenerator H6(string text)
    {
        _content.AppendLine($"###### {text}");
        _content.AppendLine();
        return this;
    }

    #endregion

    #region Text Formatting

    /// <summary>
    /// Adds a paragraph of text.
    /// </summary>
    public MarkdownGenerator Paragraph(string text)
    {
        _content.AppendLine(text);
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a line of text without a trailing newline.
    /// </summary>
    public MarkdownGenerator Line(string text)
    {
        _content.AppendLine(text);
        return this;
    }

    /// <summary>
    /// Adds a blank line.
    /// </summary>
    public MarkdownGenerator BlankLine()
    {
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Formats text as bold.
    /// </summary>
    public static string Bold(string text) => $"**{text}**";

    /// <summary>
    /// Formats text as italic.
    /// </summary>
    public static string Italic(string text) => $"*{text}*";

    /// <summary>
    /// Formats text as inline code.
    /// </summary>
    public static string InlineCode(string text) => $"`{text}`";

    #endregion

    #region Lists

    /// <summary>
    /// Adds an unordered list.
    /// </summary>
    public MarkdownGenerator UnorderedList(IEnumerable<string> items, int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * 2);
        foreach (var item in items)
        {
            _content.AppendLine($"{indent}- {item}");
        }
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds an ordered list.
    /// </summary>
    public MarkdownGenerator OrderedList(IEnumerable<string> items, int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * 2);
        var index = 1;
        foreach (var item in items)
        {
            _content.AppendLine($"{indent}{index}. {item}");
            index++;
        }
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a single list item (for building lists incrementally).
    /// </summary>
    public MarkdownGenerator ListItem(string text, int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * 2);
        _content.AppendLine($"{indent}- {text}");
        return this;
    }

    #endregion

    #region Code Blocks

    /// <summary>
    /// Adds a code block with optional language specification.
    /// </summary>
    public MarkdownGenerator CodeBlock(string code, string? language = null)
    {
        _content.AppendLine($"```{language ?? ""}");
        _content.AppendLine(code);
        _content.AppendLine("```");
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a C# code block.
    /// </summary>
    public MarkdownGenerator CSharpCodeBlock(string code)
    {
        return CodeBlock(code, "csharp");
    }

    /// <summary>
    /// Adds a JSON code block.
    /// </summary>
    public MarkdownGenerator JsonCodeBlock(string code)
    {
        return CodeBlock(code, "json");
    }

    #endregion

    #region Tables

    /// <summary>
    /// Adds a markdown table.
    /// </summary>
    /// <param name="headers">Column headers</param>
    /// <param name="rows">Table rows (each row is a list of cell values)</param>
    /// <param name="alignments">Optional column alignments (left, center, right)</param>
    public MarkdownGenerator Table(
        IEnumerable<string> headers,
        IEnumerable<IEnumerable<string>> rows,
        IEnumerable<string>? alignments = null)
    {
        var headerList = headers.ToList();
        var rowList = rows.ToList();
        var alignmentList = alignments?.ToList() ?? Enumerable.Repeat("left", headerList.Count).ToList();

        // Header row
        _content.AppendLine("| " + string.Join(" | ", headerList) + " |");

        // Separator row with alignment
        var separators = alignmentList.Select(a => a.ToLower() switch
        {
            "center" => ":---:",
            "right" => "---:",
            _ => "---"
        });
        _content.AppendLine("| " + string.Join(" | ", separators) + " |");

        // Data rows
        foreach (var row in rowList)
        {
            _content.AppendLine("| " + string.Join(" | ", row) + " |");
        }

        _content.AppendLine();
        return this;
    }

    #endregion

    #region Links

    /// <summary>
    /// Creates a markdown link.
    /// </summary>
    public static string Link(string text, string url) => $"[{text}]({url})";

    /// <summary>
    /// Creates an anchor link (for internal document navigation).
    /// </summary>
    public static string AnchorLink(string text, string anchor) => $"[{text}](#{anchor})";

    /// <summary>
    /// Creates an anchor ID from text (converts to lowercase, replaces spaces with hyphens).
    /// </summary>
    public static string CreateAnchor(string text)
    {
        return text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("/", "-")
            .Replace("\\", "-");
    }

    /// <summary>
    /// Adds a link to the content.
    /// </summary>
    public MarkdownGenerator AddLink(string text, string url)
    {
        _content.Append(Link(text, url));
        return this;
    }

    #endregion

    #region Code References

    /// <summary>
    /// Formats a code reference consistently using the pattern: `ClassName` in `Path/To/File.cs`
    /// Requirement 8.5: THE System SHALL format code references consistently using the pattern: `ClassName` in `Path/To/File.cs`
    /// </summary>
    public static string CodeReference(string className, string filePath)
    {
        return $"`{className}` in `{filePath}`";
    }

    /// <summary>
    /// Formats a class reference with namespace and file location.
    /// Requirement 8.2: WHEN documenting a class or interface, THE System SHALL include the namespace and file location
    /// </summary>
    public static string ClassReference(string className, string namespaceName, string filePath)
    {
        return $"`{className}` (namespace: `{namespaceName}`) in `{filePath}`";
    }

    /// <summary>
    /// Formats a method reference with class and file location.
    /// Requirement 8.4: WHEN documenting an API endpoint, THE System SHALL reference the controller class and method name
    /// </summary>
    public static string MethodReference(string methodName, string className, string filePath)
    {
        return $"`{methodName}()` in `{className}` (`{filePath}`)";
    }

    /// <summary>
    /// Formats a property reference with class and file location.
    /// </summary>
    public static string PropertyReference(string propertyName, string className, string filePath)
    {
        return $"`{propertyName}` property of `{className}` (`{filePath}`)";
    }

    /// <summary>
    /// Formats a configuration reference.
    /// Requirement 8.3: WHEN documenting configuration, THE System SHALL reference the specific configuration file and key path
    /// </summary>
    public static string ConfigReference(string keyPath, string configFile)
    {
        return $"`{keyPath}` in `{configFile}`";
    }

    /// <summary>
    /// Formats a configuration option class reference.
    /// </summary>
    public static string OptionReference(string propertyName, string optionClassName, string filePath)
    {
        return $"`{propertyName}` in `{optionClassName}` (`{filePath}`)";
    }

    /// <summary>
    /// Formats a file path reference.
    /// Requirement 8.1: WHEN documenting a technical feature, THE System SHALL include the file path where it is implemented
    /// </summary>
    public static string FileReference(string filePath)
    {
        return $"`{filePath}`";
    }

    /// <summary>
    /// Formats a directory reference.
    /// </summary>
    public static string DirectoryReference(string directoryPath)
    {
        return $"`{directoryPath}/`";
    }

    #endregion

    #region Blockquotes and Callouts

    /// <summary>
    /// Adds a blockquote.
    /// </summary>
    public MarkdownGenerator Blockquote(string text)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            _content.AppendLine($"> {line}");
        }
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a note callout.
    /// </summary>
    public MarkdownGenerator Note(string text)
    {
        _content.AppendLine($"> **Note:** {text}");
        _content.AppendLine();
        return this;
    }

    /// <summary>
    /// Adds a warning callout.
    /// </summary>
    public MarkdownGenerator Warning(string text)
    {
        _content.AppendLine($"> **Warning:** {text}");
        _content.AppendLine();
        return this;
    }

    #endregion

    #region Horizontal Rules

    /// <summary>
    /// Adds a horizontal rule.
    /// </summary>
    public MarkdownGenerator HorizontalRule()
    {
        _content.AppendLine("---");
        _content.AppendLine();
        return this;
    }

    #endregion

    #region Raw Content

    /// <summary>
    /// Adds raw content without any formatting.
    /// </summary>
    public MarkdownGenerator Raw(string content)
    {
        _content.Append(content);
        return this;
    }

    /// <summary>
    /// Adds raw content with a newline.
    /// </summary>
    public MarkdownGenerator RawLine(string content)
    {
        _content.AppendLine(content);
        return this;
    }

    #endregion

    #region Output

    /// <summary>
    /// Writes the markdown content to a file.
    /// </summary>
    public void WriteToFile(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, _content.ToString());
    }

    /// <summary>
    /// Returns the markdown content as a string.
    /// </summary>
    public override string ToString() => _content.ToString();

    #endregion
}
