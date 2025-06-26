using System.Text;
using PocketLawyer.Service;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PocketLawyer.Infrastructure.FileDecoding;

// Service zum Auslesen von Text aus hochgeladenen Dateien.
// Unterstützt aktuell nur .txt und .pdf.

public class FileDecoderService : IFileDecoderService
{
    private readonly ILogger<FileDecoderService> _logger;

    public FileDecoderService(ILogger<FileDecoderService> logger)
    {
        _logger = logger;
    }

    // Welche Dateitypen akzeptiert werden.
    public string[] AllowedExtensions => ["txt", "pdf"];
    public string AllowedExtensionsString => string.Join(", ", AllowedExtensions);

    public async Task<string> GetFileContentAsync(string filename, Stream content)
    {
        _logger.LogDebug("Entering {Method}", nameof(GetFileContentAsync));

        // Dateiendung bestimmen, um das passende Leseverfahren zu wählen.
        string extension = filename.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();
        _logger.LogDebug("Detected extension {Extension}", extension);
 
        if (!AllowedExtensions.Contains(extension))
            throw new NotSupportedException("Unknown file extension: " + extension);

        switch (extension)
        {
            case "txt":
                // Einfaches Einlesen von Textdateien.
                _logger.LogDebug("Decoding text file {FileName}", filename);
                using (var reader = new StreamReader(content, Encoding.UTF8, leaveOpen: true))
                {
                    // Kompletten Textinhalt aus dem Stream lesen
                    string text = await reader.ReadToEndAsync();
                    return text;
                }
            case "pdf":
                // PDF wird mit PdfPig Seite für Seite ausgelesen.
                _logger.LogDebug("Decoding pdf file {FileName}", filename);
                using (PdfDocument document = PdfDocument.Open(content))
                {
                    StringBuilder contentBuilder = new StringBuilder();
                    foreach (Page page in document.GetPages())
                        // Text jeder Seite anhängen
                        contentBuilder.Append(page.Text);
                    return contentBuilder.ToString();
                }
            default:
                // Sollte eigentlich nicht auftreten, da oben bereits geprüft.
                throw new NotSupportedException($"File extension {extension} is not supported.");
        }
    }

}
