using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PocketLawyer.Service;
using PocketLawyer;
using System.IO;

namespace PocketLawyer.Infrastructure;

public class FileValidatorService : IFileValidatorService
{
    private readonly ILogger<FileValidatorService> _logger;
    private readonly IFileDecoderService _decoderService;
    private readonly UploadOptions _options;

    public FileValidatorService(
        ILogger<FileValidatorService> logger,
        IFileDecoderService decoderService,
        IOptions<UploadOptions> uploadOptions)
    {
        _logger = logger;
        _decoderService = decoderService;
        _options = uploadOptions.Value;
    }

    public async Task<FileContentDto> ValidateAndDecodeAsync(IFormFile file)
    {
        _logger.LogDebug("Entering {Method}", nameof(ValidateAndDecodeAsync));

        string filename = file.FileName;
        string extension = Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
        if (!_decoderService.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Fileextension not allowed: {FileName}", filename);
            throw new FileValidationException(
                $"Fileextension not recognised! Recognised extensions are {_decoderService.AllowedExtensionsString}.",
                filename);
        }

        long maxUploadSizeMb = _options.MaxUploadSizeMB;
        long maxBytes = maxUploadSizeMb * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new FileValidationException(
                $"File too large. Maximum {maxUploadSizeMb} MB allowed.",
                filename);
        }

        await using var stream = file.OpenReadStream();

        string fileContent;
        try
        {
            fileContent = await _decoderService.GetFileContentAsync(filename, stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding file {FileName}", filename);
            throw new FileValidationException("Error decoding file", filename, ex);
        }

        return new FileContentDto(filename, fileContent);
    }
}
