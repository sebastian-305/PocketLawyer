using System.Text;

namespace PocketLawyer.Documentation.Verification;

/// <summary>
/// Formats and reports verification errors with context.
/// Generates comprehensive verification reports.
/// </summary>
/// <remarks>
/// Validates Requirements: 6.6
/// </remarks>
public class ErrorReporter
{
    /// <summary>
    /// Generates a comprehensive verification report from verification results.
    /// </summary>
    /// <param name="results">List of verification results</param>
    /// <returns>Formatted verification report</returns>
    public string GenerateReport(List<VerificationResult> results)
    {
        var report = new StringBuilder();
        
        report.AppendLine("# Documentation Verification Report");
        report.AppendLine();
        report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine();

        // Summary
        var totalDocuments = results.Count;
        var passedDocuments = results.Count(r => r.Passed);
        var failedDocuments = totalDocuments - passedDocuments;
        var totalErrors = results.Sum(r => r.Errors.Count);

        report.AppendLine("## Summary");
        report.AppendLine();
        report.AppendLine($"- **Total Documents**: {totalDocuments}");
        report.AppendLine($"- **Passed**: {passedDocuments}");
        report.AppendLine($"- **Failed**: {failedDocuments}");
        report.AppendLine($"- **Total Errors**: {totalErrors}");
        report.AppendLine();

        // Overall status
        if (failedDocuments == 0)
        {
            report.AppendLine("✅ **All documentation verified successfully!**");
            report.AppendLine();
        }
        else
        {
            report.AppendLine("❌ **Verification failed. Please review errors below.**");
            report.AppendLine();
        }

        // Detailed results
        report.AppendLine("## Detailed Results");
        report.AppendLine();

        foreach (var result in results)
        {
            report.AppendLine($"### {result.DocumentPath}");
            report.AppendLine();

            if (result.Passed)
            {
                report.AppendLine("✅ **Status**: Passed");
                report.AppendLine();
            }
            else
            {
                report.AppendLine($"❌ **Status**: Failed ({result.Errors.Count} error(s))");
                report.AppendLine();

                if (result.Errors.Any())
                {
                    report.AppendLine("**Errors:**");
                    report.AppendLine();

                    foreach (var error in result.Errors)
                    {
                        report.AppendLine(FormatError(error));
                        report.AppendLine();
                    }
                }
            }
        }

        // Error summary by type
        if (totalErrors > 0)
        {
            report.AppendLine("## Error Summary by Type");
            report.AppendLine();

            var errorsByType = results
                .SelectMany(r => r.Errors)
                .GroupBy(e => e.Type)
                .OrderByDescending(g => g.Count());

            foreach (var group in errorsByType)
            {
                report.AppendLine($"- **{group.Key}**: {group.Count()} error(s)");
            }
            report.AppendLine();
        }

        return report.ToString();
    }

    /// <summary>
    /// Formats a single verification error with context.
    /// </summary>
    /// <param name="error">Verification error to format</param>
    /// <returns>Formatted error message</returns>
    public string FormatError(VerificationError error)
    {
        var formatted = new StringBuilder();

        formatted.AppendLine($"**Error Type**: {error.Type}");
        formatted.AppendLine($"**Claim**: {error.Claim}");
        formatted.AppendLine($"**Expected Source**: {error.ExpectedSource}");
        formatted.AppendLine($"**Message**: {error.Message}");

        return formatted.ToString().TrimEnd();
    }

    /// <summary>
    /// Generates a summary report of verification results.
    /// </summary>
    /// <param name="results">List of verification results</param>
    /// <returns>Summary report string</returns>
    public string GenerateSummary(List<VerificationResult> results)
    {
        var totalDocuments = results.Count;
        var passedDocuments = results.Count(r => r.Passed);
        var failedDocuments = totalDocuments - passedDocuments;
        var totalErrors = results.Sum(r => r.Errors.Count);

        var summary = new StringBuilder();
        summary.AppendLine($"Verification Summary: {passedDocuments}/{totalDocuments} documents passed");
        
        if (failedDocuments > 0)
        {
            summary.AppendLine($"Failed documents: {failedDocuments}");
            summary.AppendLine($"Total errors: {totalErrors}");
        }

        return summary.ToString();
    }

    /// <summary>
    /// Formats errors for console output with color coding.
    /// </summary>
    /// <param name="error">Verification error to format</param>
    /// <returns>Console-formatted error message</returns>
    public string FormatErrorForConsole(VerificationError error)
    {
        return $"[{error.Type}] {error.Claim}\n" +
               $"  Expected: {error.ExpectedSource}\n" +
               $"  Message: {error.Message}";
    }

    /// <summary>
    /// Groups errors by type and generates a categorized report.
    /// </summary>
    /// <param name="errors">List of verification errors</param>
    /// <returns>Categorized error report</returns>
    public string GenerateCategorizedErrorReport(List<VerificationError> errors)
    {
        var report = new StringBuilder();
        
        var errorsByType = errors.GroupBy(e => e.Type).OrderBy(g => g.Key);

        foreach (var group in errorsByType)
        {
            report.AppendLine($"## {group.Key} ({group.Count()})");
            report.AppendLine();

            foreach (var error in group)
            {
                report.AppendLine($"- **{error.Claim}**");
                report.AppendLine($"  - Expected: `{error.ExpectedSource}`");
                report.AppendLine($"  - {error.Message}");
                report.AppendLine();
            }
        }

        return report.ToString();
    }

    /// <summary>
    /// Generates a JSON report of verification results.
    /// </summary>
    /// <param name="results">List of verification results</param>
    /// <returns>JSON-formatted report</returns>
    public string GenerateJsonReport(List<VerificationResult> results)
    {
        var report = new
        {
            timestamp = DateTime.UtcNow,
            summary = new
            {
                totalDocuments = results.Count,
                passed = results.Count(r => r.Passed),
                failed = results.Count(r => !r.Passed),
                totalErrors = results.Sum(r => r.Errors.Count)
            },
            results = results.Select(r => new
            {
                documentPath = r.DocumentPath,
                passed = r.Passed,
                errorCount = r.Errors.Count,
                errors = r.Errors.Select(e => new
                {
                    type = e.Type,
                    claim = e.Claim,
                    expectedSource = e.ExpectedSource,
                    message = e.Message
                })
            })
        };

        return System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
