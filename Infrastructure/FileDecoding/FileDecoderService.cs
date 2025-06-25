using PocketLawyer.Service;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PocketLawyer.Infrastructure.FileDecoding;

public class FileDecoderService : IFileDecoderService
{
    private static ILogger<FileDecoderService> _logger = new LoggerFactory().CreateLogger<FileDecoderService>();

    public string[] AllowedExtensions => ["txt", "pdf"];
    public string AllowedExtensionsString => string.Join(", ", AllowedExtensions);

    public async Task<string> GetFileContentAsync(string filename, byte[] content)
    {
        _logger.LogDebug("Entering GetFileContentAsync");

        string extension = filename.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();        
 
        if (!AllowedExtensions.Contains(extension))
            throw new NotSupportedException("Unkown file extension: " + extension);

        await LocalFileHandling.Save(filename, content);

        switch (extension)
        {
            case "txt":
                return await LocalFileHandling.LoadAndDeleteTextFile(filename);
            case "pdf":
                return LocalFileHandling.LoadAndDeletePdfFile(filename);
            default:
                throw new Exception("Programmer fucked up and forgot the extension: " + extension);
        }
    }

    static class LocalFileHandling
    {
        public async static Task Save(string filename, byte[] binary)
        {
            await File.WriteAllBytesAsync(Directory.GetCurrentDirectory() + @"\" + filename, binary);
        }

        public async static Task<string> LoadAndDeleteTextFile(string filename)
        {
            string data = await File.ReadAllTextAsync(Directory.GetCurrentDirectory() + @"\" + filename);

            File.Delete(Directory.GetCurrentDirectory() + @"\" + filename);

            return data;
        }

        public static string LoadAndDeletePdfFile(string filename)
        {
            StringBuilder contentBuilder = new StringBuilder();

            using (PdfDocument document = PdfDocument.Open(Directory.GetCurrentDirectory() + @"\" + filename))
                foreach (Page page in document.GetPages())
                    contentBuilder.Append(page.Text);

            File.Delete(Directory.GetCurrentDirectory() + @"\" + filename);
            return contentBuilder.ToString();
        }        

    }
}
