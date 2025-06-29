using PocketLawyer.Service;
using Tesseract;

namespace PocketLawyer.Infrastructure.FileDecoding.Strategies;

public class ImageFileDecoder : IFileDecoder
{
    private readonly ILogger<ImageFileDecoder> _logger;
    private readonly string _tesseractDataPath;

    public ImageFileDecoder(
        ILogger<ImageFileDecoder> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _tesseractDataPath = configuration["TesseractDataPath"] ?? "./tessdata";
    }

    public bool CanDecode(string ext)
    {
        return string.Equals(ext, "jpg", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(ext, "jpeg", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(ext, "png", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> DecodeAsync(Stream input)
    {
        _logger.LogDebug("Decoding image stream via tesseract");
        using var ms = new MemoryStream();
        await input.CopyToAsync(ms);
        ms.Position = 0;

        using var engine = new TesseractEngine(_tesseractDataPath, "deu+eng", EngineMode.Default);
        using var img = Pix.LoadFromMemory(ms.ToArray());
        using var page = engine.Process(img);
        return page.GetText();
    }
}
