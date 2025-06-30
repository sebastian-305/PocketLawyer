using PocketLawyer.Application.Interfaces;

namespace PocketLawyer.Infrastructure.FileDecoding;

public class FileDecoderService : IFileDecoderService
{
    private static readonly string[] _allowedExtensions = ["txt", "pdf", "jpg", "jpeg", "png"];
    private readonly ILogger<FileDecoderService> _logger;
    private readonly IEnumerable<IFileDecoder> _decoders;

    public FileDecoderService(
        ILogger<FileDecoderService> logger,
        IEnumerable<IFileDecoder> decoders)
    {
        _logger = logger;
        _decoders = decoders;
    }

    public string[] AllowedExtensions => _allowedExtensions;
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
