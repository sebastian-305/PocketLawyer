using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure.FileDecoding;

// Sucht einen passenden Dateidecoder und liefert den Textinhalt zurueck.
public class FileDecoderService : IFileDecoderService
{
    private readonly ILogger<FileDecoderService> _logger;
    private readonly IEnumerable<IFileDecoder> _decoders;

    // Alle konkreten Decoder werden injiziert und in einer Liste gehalten
    public FileDecoderService(
        ILogger<FileDecoderService> logger,
        IEnumerable<IFileDecoder> decoders)
    {
        _logger = logger;
        _decoders = decoders;
    }

    // Welche Dateitypen akzeptiert werden
    public string[] AllowedExtensions => ["txt", "pdf", "jpg", "jpeg", "png"];
    public string AllowedExtensionsString => string.Join(", ", AllowedExtensions);

    // Waehlt anhand der Dateiendung den passenden Decoder aus
    public async Task<string> GetFileContentAsync(string filename, Stream content)
    {
        _logger.LogDebug("Entering {Method}", nameof(GetFileContentAsync));

        // Dateiendung ermitteln
        string extension = Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
        _logger.LogDebug("Detected extension {Extension}", extension);

        // Passende Strategie fuer die Dateiendung suchen
        var decoder = _decoders.FirstOrDefault(d => d.CanDecode(extension));
        if (decoder == null)
            throw new NotSupportedException("Unknown file extension: " + extension);

        return await decoder.DecodeAsync(content);
    }
}
