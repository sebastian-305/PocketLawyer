using PocketLawyer.Service;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
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

    public async Task<string> GetFileContentAsync(string filename, byte[] content)
    {
        _logger.LogDebug("Entering GetFileContentAsync");

        string extension = filename.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();        
 
        if (!AllowedExtensions.Contains(extension))
            throw new NotSupportedException("Unknown file extension: " + extension);

        await LocalFileHandling.Save(filename, content);

        switch (extension)
        {
            case "txt":
                return await LocalFileHandling.LoadAndDeleteTextFile(filename);
            case "pdf":
                return LocalFileHandling.LoadAndDeletePdfFile(filename);
            default:
                throw new NotSupportedException($"File extension {extension} is not supported.");
        }
    }

    static class LocalFileHandling
    {
        public async static Task Save(string filename, byte[] binary)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
            await File.WriteAllBytesAsync(filePath, binary);
        }

        public async static Task<string> LoadAndDeleteTextFile(string filename)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
            string data = await File.ReadAllTextAsync(filePath);

            File.Delete(filePath);

            return data;
        }

        public static string LoadAndDeletePdfFile(string filename)
        {
            StringBuilder contentBuilder = new StringBuilder();
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), filename);

            using (PdfDocument document = PdfDocument.Open(filePath))
                foreach (Page page in document.GetPages())
                    contentBuilder.Append(page.Text);

            File.Delete(filePath);
            return contentBuilder.ToString();
        }

    }
}
