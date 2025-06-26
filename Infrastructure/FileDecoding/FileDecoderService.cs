using System.Text;
using PocketLawyer.Service;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PocketLawyer.Infrastructure.FileDecoding;

public class FileDecoderService : IFileDecoderService
{
    private readonly ILogger<FileDecoderService> _logger;

    public FileDecoderService(ILogger<FileDecoderService> logger)
    {
        _logger = logger;
    }

    public string[] AllowedExtensions => ["txt", "pdf"];
    public string AllowedExtensionsString => string.Join(", ", AllowedExtensions);

    public async Task<string> GetFileContentAsync(string filename, Stream content)
    {
        _logger.LogDebug("Entering {Method}", nameof(GetFileContentAsync));

        string extension = Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
        _logger.LogDebug("Detected extension {Extension}", extension);
 
        if (!AllowedExtensions.Contains(extension))
            throw new NotSupportedException("Unknown file extension: " + extension);

        switch (extension)
        {
            case "txt":
                _logger.LogDebug("Decoding text file {FileName}", filename);
                using (var reader = new StreamReader(content, Encoding.UTF8, leaveOpen: true))
                {
                    string text = await reader.ReadToEndAsync();
                    return text;
                }
            case "pdf":
                _logger.LogDebug("Decoding pdf file {FileName}", filename);
                using (PdfDocument document = PdfDocument.Open(content))
                {
                    StringBuilder contentBuilder = new StringBuilder();
                    foreach (Page page in document.GetPages())
                        contentBuilder.Append(page.Text);
                    return contentBuilder.ToString();
                }
            default:
                throw new NotSupportedException($"File extension {extension} is not supported.");
        }
    }

}
