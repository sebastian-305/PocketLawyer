using System.Text.RegularExpressions;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Validates Mermaid diagram syntax to ensure diagrams are syntactically correct.
/// Checks for invalid characters, missing quotes, correct syntax, and balanced brackets.
/// </summary>
/// <remarks>
/// Validates Requirements: 6.1, 6.2
/// </remarks>
public class MermaidValidator
{
    private static readonly string[] ValidDiagramTypes = new[]
    {
        "graph",
        "flowchart",
        "sequenceDiagram",
        "classDiagram",
        "erDiagram",
        "stateDiagram",
        "gantt",
        "pie",
        "journey"
    };

    private static readonly char[] InvalidNodeNameChars = new[]
    {
        '"', '\'', '<', '>', '&', '|', '\\', '/', '?', '*', ':'
    };

    /// <summary>
    /// Validates a Mermaid diagram for syntactic correctness.
    /// Requirement 6.1: WHEN der Mermaid_Diagram_Generator ein Diagramm erstellt, THEN THE System SHALL syntaktisch valide Mermaid-Syntax generieren
    /// Requirement 6.2: WHEN ein Diagramm Knotennamen enthält, THEN THE System SHALL Sonderzeichen escapen oder entfernen
    /// </summary>
    /// <param name="mermaidCode">The Mermaid diagram code to validate</param>
    /// <returns>ValidationResult with IsValid flag and list of errors</returns>
    public ValidationResult Validate(string mermaidCode)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(mermaidCode))
        {
            errors.Add("Mermaid-Code ist leer oder null");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Remove code fence markers if present
        var code = mermaidCode.Trim();
        if (code.StartsWith("```mermaid"))
        {
            code = code.Substring("```mermaid".Length);
        }
        if (code.EndsWith("```"))
        {
            code = code.Substring(0, code.Length - 3);
        }
        code = code.Trim();

        var lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            errors.Add("Mermaid-Code enthält keine Zeilen");
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Check for valid diagram type
        var firstLine = lines[0].Trim();
        if (!ValidateDiagramType(firstLine))
        {
            errors.Add($"Ungültiger Diagrammtyp: '{firstLine}'. Erlaubte Typen: {string.Join(", ", ValidDiagramTypes)}");
        }

        // Check for invalid characters in node names
        ValidateNodeNames(lines, errors);

        // Check for balanced brackets
        ValidateBalancedBrackets(code, errors);

        // Check for missing quotes around special characters
        ValidateMissingQuotes(lines, errors);

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    /// <summary>
    /// Sanitizes a node name by removing or escaping special characters.
    /// Requirement 6.2: WHEN ein Diagramm Knotennamen enthält, THEN THE System SHALL Sonderzeichen escapen oder entfernen
    /// </summary>
    /// <param name="nodeName">The node name to sanitize</param>
    /// <returns>Sanitized node name safe for use in Mermaid diagrams</returns>
    public string SanitizeNodeName(string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return "Node";
        }

        // Remove invalid characters
        var sanitized = nodeName;
        foreach (var invalidChar in InvalidNodeNameChars)
        {
            sanitized = sanitized.Replace(invalidChar.ToString(), "");
        }

        // Remove brackets and parentheses that could interfere with Mermaid syntax
        sanitized = sanitized
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("(", "")
            .Replace(")", "");

        // Replace spaces with underscores for node IDs
        sanitized = sanitized.Replace(" ", "_");

        // Remove any remaining special characters except underscores, hyphens, and alphanumeric
        sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9_\-]", "");

        // Ensure it starts with a letter or underscore
        if (!string.IsNullOrEmpty(sanitized) && !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = "_" + sanitized;
        }

        // If empty after sanitization, return a default name
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return "Node";
        }

        return sanitized;
    }

    #region Private Validation Methods

    private bool ValidateDiagramType(string firstLine)
    {
        // Check if the first line starts with a valid diagram type
        foreach (var diagramType in ValidDiagramTypes)
        {
            if (firstLine.StartsWith(diagramType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private void ValidateNodeNames(string[] lines, List<string> errors)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Skip empty lines and diagram type declaration
            if (string.IsNullOrWhiteSpace(line) || i == 0)
            {
                continue;
            }

            // Check for invalid characters in node definitions
            // Node definitions typically follow patterns like: NodeId[Label] or NodeId(Label) or NodeId{Label}
            var nodePattern = @"(\w+)\s*[\[\(\{]([^\]\)\}]*)[\]\)\}]";
            var matches = Regex.Matches(line, nodePattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    var nodeId = match.Groups[1].Value;
                    var nodeLabel = match.Groups[2].Value;

                    // Check node ID for invalid characters
                    foreach (var invalidChar in InvalidNodeNameChars)
                    {
                        if (nodeId.Contains(invalidChar))
                        {
                            errors.Add($"Zeile {i + 1}: Knotenname '{nodeId}' enthält ungültiges Zeichen '{invalidChar}'");
                        }
                    }

                    // Node labels can contain special characters if properly quoted
                    // We check if special characters are present without quotes
                    if (ContainsSpecialCharacters(nodeLabel) && !IsProperlyQuoted(nodeLabel))
                    {
                        errors.Add($"Zeile {i + 1}: Knotenbeschriftung '{nodeLabel}' enthält Sonderzeichen ohne Anführungszeichen");
                    }
                }
            }
        }
    }

    private void ValidateBalancedBrackets(string code, List<string> errors)
    {
        var bracketPairs = new Dictionary<char, char>
        {
            { '[', ']' },
            { '(', ')' },
            { '{', '}' }
        };

        var stack = new Stack<(char bracket, int position)>();
        
        for (int i = 0; i < code.Length; i++)
        {
            var ch = code[i];
            
            if (bracketPairs.ContainsKey(ch))
            {
                stack.Push((ch, i));
            }
            else if (bracketPairs.ContainsValue(ch))
            {
                if (stack.Count == 0)
                {
                    errors.Add($"Position {i}: Schließende Klammer '{ch}' ohne passende öffnende Klammer");
                    continue;
                }

                var (openBracket, openPosition) = stack.Pop();
                var expectedClosing = bracketPairs[openBracket];
                
                if (ch != expectedClosing)
                {
                    errors.Add($"Position {i}: Erwartete '{expectedClosing}' aber fand '{ch}' (öffnende Klammer bei Position {openPosition})");
                }
            }
        }

        // Check for unclosed brackets
        while (stack.Count > 0)
        {
            var (openBracket, openPosition) = stack.Pop();
            errors.Add($"Position {openPosition}: Öffnende Klammer '{openBracket}' wurde nicht geschlossen");
        }
    }

    private void ValidateMissingQuotes(string[] lines, List<string> errors)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Skip empty lines and diagram type declaration
            if (string.IsNullOrWhiteSpace(line) || i == 0)
            {
                continue;
            }

            // Check for special characters in labels that should be quoted
            // Pattern: NodeId[Label] or NodeId(Label) or NodeId{Label}
            var labelPattern = @"[\[\(\{]([^\]\)\}""']*)[\]\)\}]";
            var matches = Regex.Matches(line, labelPattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 2)
                {
                    var label = match.Groups[1].Value;
                    
                    // Check if label contains special characters that require quoting
                    if (ContainsSpecialCharacters(label) && !label.Trim().StartsWith("\"") && !label.Trim().StartsWith("'"))
                    {
                        errors.Add($"Zeile {i + 1}: Beschriftung '{label}' enthält Sonderzeichen und sollte in Anführungszeichen stehen");
                    }
                }
            }
        }
    }

    private bool ContainsSpecialCharacters(string text)
    {
        // Check for characters that typically require quoting in Mermaid
        var specialChars = new[] { ':', ';', ',', '.', '!', '?', '-', '/', '\\', '|', '&', '<', '>', '=' };
        return specialChars.Any(ch => text.Contains(ch));
    }

    private bool IsProperlyQuoted(string text)
    {
        var trimmed = text.Trim();
        return (trimmed.StartsWith("\"") && trimmed.EndsWith("\"")) ||
               (trimmed.StartsWith("'") && trimmed.EndsWith("'"));
    }

    #endregion
}

/// <summary>
/// Represents the result of a Mermaid diagram validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation was successful.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();
}
