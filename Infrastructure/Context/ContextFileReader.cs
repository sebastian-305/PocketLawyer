using PocketLawyer.Application.Interfaces;

namespace PocketLawyer.Infrastructure.Context;

public class ContextFileReader : IContextFileReader
{
    private readonly ILogger<ContextFileReader> _logger;
    private static readonly string[] SupportedExtensions = { ".txt", ".md" };
    private const string ReadmeFileName = "README.md";

    public ContextFileReader(ILogger<ContextFileReader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ReadDirectoryAsync(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            _logger.LogDebug("Directory path is null or empty, returning empty string");
            return string.Empty;
        }

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogDebug("Directory does not exist: {DirectoryPath}", directoryPath);
            return string.Empty;
        }

        var files = Directory.GetFiles(directoryPath)
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Where(file => !Path.GetFileName(file).Equals(ReadmeFileName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
        {
            _logger.LogDebug("No context files found in directory: {DirectoryPath}", directoryPath);
            return string.Empty;
        }

        var contentBuilder = new System.Text.StringBuilder();
        var successfullyReadFiles = 0;

        foreach (var file in files)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var fileName = Path.GetFileName(file);

                if (contentBuilder.Length > 0)
                {
                    contentBuilder.AppendLine();
                }

                contentBuilder.AppendLine($"### {fileName}");
                contentBuilder.AppendLine();
                contentBuilder.AppendLine(content);

                successfullyReadFiles++;
                _logger.LogDebug("Successfully read context file: {FilePath}", file);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read context file: {FilePath}", file);
                // Continue with remaining files
            }
        }

        var result = contentBuilder.ToString();
        _logger.LogDebug("Loaded context from {DirectoryPath}: {FileCount} files, {CharCount} characters",
            directoryPath, successfullyReadFiles, result.Length);

        return result;
    }
}
