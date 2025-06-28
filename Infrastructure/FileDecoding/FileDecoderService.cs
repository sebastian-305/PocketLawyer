using System.Text;
using PocketLawyer.Service;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Tesseract;
using Microsoft.Extensions.Configuration;

namespace PocketLawyer.Infrastructure.FileDecoding;

// Dienst, der hochgeladene Dateien in reinen Text umwandelt,
// damit die KI den Inhalt unabhängig vom Ursprungsformat analysieren kann.

public class FileDecoderService : IFileDecoderService
{
    // Für Debug-Ausgaben und Fehlermeldungen
    private readonly ILogger<FileDecoderService> _logger;
    // Pfad zu den Sprachdaten für Tesseract-OCR
    private readonly string _tesseractDataPath;

    public FileDecoderService(ILogger<FileDecoderService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _tesseractDataPath = configuration["TesseractDataPath"] ?? "./tessdata";
    }

    // Unterstützte Dateitypen
    public string[] AllowedExtensions => ["txt", "pdf", "jpg", "jpeg", "png"];
    // Gleiche Liste als Zeichenkette für Fehlermeldungen
    public string AllowedExtensionsString => string.Join(", ", AllowedExtensions);

    // Erkennt den Dateityp und extrahiert den lesbaren Text über
    // spezialisierte Bibliotheken. Bilder werden per OCR verarbeitet.
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
                    foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
                        contentBuilder.Append(page.Text);
                    return contentBuilder.ToString();
                }
            case "jpg":
            case "jpeg":
            case "png":
                _logger.LogDebug("Decoding image file {FileName}", filename);
                return await DecodeImageAsync(content);
            default:
                throw new NotSupportedException($"File extension {extension} is not supported.");
        }
    }

    // Nutzt Tesseract-OCR, um Text aus Bilddateien zu gewinnen
    private async Task<string> DecodeImageAsync(Stream content)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms);
        ms.Position = 0;

        using var engine = new TesseractEngine(_tesseractDataPath, "deu+eng", EngineMode.Default);
        using var img = Pix.LoadFromMemory(ms.ToArray());
        using var page = engine.Process(img);
        return page.GetText();
    }

}
