using PocketLawyer.Application.Interfaces;

// Wählt anhand der Dateiendung den passenden Decoder aus
// und liefert den dekodierten Text zurück.

namespace PocketLawyer.Infrastructure.FileDecoding;

// Service der alle Decoder bündelt und die Dateiinhalte liest.
public class FileDecoderService : IFileDecoderService
{
    private readonly ILogger<FileDecoderService> _logger;
    private readonly IEnumerable<IFileDecoder> _decoders;

    public FileDecoderService(
        ILogger<FileDecoderService> logger,
        IEnumerable<IFileDecoder> decoders)
    {
        _logger = logger;
        _decoders = decoders;
    }

    public string[] AllowedExtensions => ["txt", "pdf", "jpg", "jpeg", "png"];
    public string AllowedExtensionsString => string.Join(", ", AllowedExtensions);

    public async Task<string> GetFileContentAsync(string filename, Stream content)
    {
        _logger.LogDebug("Entering {Method}", nameof(GetFileContentAsync));

        string extension = Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
        _logger.LogDebug("Detected extension {Extension}", extension);

        var decoder = _decoders.FirstOrDefault(d => d.CanDecode(extension));
        if (decoder == null)
            throw new NotSupportedException("Unknown file extension: " + extension);

        return await decoder.DecodeAsync(content);
    }
}
