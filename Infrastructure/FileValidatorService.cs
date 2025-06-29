using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PocketLawyer.Service;
using PocketLawyer;
using System.IO;

namespace PocketLawyer.Infrastructure;

public class FileValidatorService : IFileValidatorService
{
    private readonly ILogger<FileValidatorService> _logger;
    private readonly IFileDecoderService _decoderService;
    private readonly IConfiguration _configuration;

    public FileValidatorService(
        ILogger<FileValidatorService> logger,
        IFileDecoderService decoderService,
        IConfiguration configuration)
    {
        _logger = logger;
        _decoderService = decoderService;
        _configuration = configuration;
    }

    public async Task<FileContentDto> ValidateAndDecodeAsync(IFormFile file)
    {
        _logger.LogDebug("Entering {Method}", nameof(ValidateAndDecodeAsync));

        string filename = file.FileName;
        string extension = Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
        if (!_decoderService.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Fileextension not allowed: {FileName}", filename);
            throw new BadHttpRequestException($"Fileextension not recognised! Recognised extensions are {_decoderService.AllowedExtensionsString}.");
        }

        long maxUploadSizeMb = _configuration.GetValue<long>("MaxUploadSizeMB");
        long maxBytes = maxUploadSizeMb * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new BadHttpRequestException($"File too large. Maximum {maxUploadSizeMb} MB allowed.");
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
            throw new BadHttpRequestException(ex.Message);
        }

        return new FileContentDto(filename, fileContent);
    }
}
