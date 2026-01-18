using System.Text.RegularExpressions;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Generates consistent Markdown anchors from text.
/// Follows GitHub-style anchor generation rules with support for German umlauts.
/// </summary>
public static class AnchorGenerator
{
    /// <summary>
    /// Generates a Markdown anchor from the given text.
    /// </summary>
    /// <param name="text">The text to convert to an anchor.</param>
    /// <returns>A lowercase anchor string with spaces replaced by hyphens and special characters removed.</returns>
    /// <remarks>
    /// The generation process:
    /// 1. Converts to lowercase
    /// 2. Replaces spaces with hyphens
    /// 3. Replaces German umlauts (ä→ae, ö→oe, ü→ue, ß→ss)
    /// 4. Removes special characters (except hyphens)
    /// 5. Reduces multiple consecutive hyphens to a single hyphen
    /// 6. Trims leading and trailing hyphens
    /// </remarks>
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // 1. Convert to lowercase
        var result = text.ToLowerInvariant();

        // 2. Replace spaces with hyphens
        result = result.Replace(" ", "-");

        // 3. Replace German umlauts
        result = result.Replace("ä", "ae");
        result = result.Replace("ö", "oe");
        result = result.Replace("ü", "ue");
        result = result.Replace("ß", "ss");

        // 4. Remove special characters (keep only alphanumeric and hyphens)
        result = Regex.Replace(result, @"[^a-z0-9\-]", "");

        // 5. Reduce multiple consecutive hyphens to a single hyphen
        result = Regex.Replace(result, @"-+", "-");

        // 6. Trim leading and trailing hyphens
        result = result.Trim('-');

        return result;
    }
}
