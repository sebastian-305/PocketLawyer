using Microsoft.AspNetCore.Http;

// Prüft hochgeladene Dateien auf Größe und erlaubte Erweiterungen
// und gibt den dekodierten Text zurück.
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using PocketLawyer.Domain.Dtos;
using System.IO;

namespace PocketLawyer.Infrastructure;

// Kombiniert Validierung und Decodierung des Uploads.
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

        // Prüfen der Dateiendung gegen die Liste erlaubter Formate
        if (!_decoderService.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Fileextension not allowed: {FileName}", filename);
            throw new FileValidationException(
                $"Fileextension not recognised! Recognised extensions are {_decoderService.AllowedExtensionsString}.",
                filename);
        }

        // Größenlimit berechnen
        long maxUploadSizeMb = _options.MaxUploadSizeMB;
        long maxBytes = maxUploadSizeMb * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new FileValidationException(
                $"File too large. Maximum {maxUploadSizeMb} MB allowed.",
                filename);
        }

        // Öffnen und Dekodieren des Streams
        await using var stream = file.OpenReadStream();

        string fileContent;
        try
        {
            fileContent = await _decoderService.GetFileContentAsync(filename, stream);
        }
        catch (Exception ex)
        {
            // Umgang mit Fehlersituationen: Fehler loggen und aussagekräftige Exception werfen
            _logger.LogError(ex, "Error decoding file {FileName}", filename);
            throw new FileValidationException("Error decoding file", filename, ex);
        }

        return new FileContentDto(filename, fileContent);
    }
}
