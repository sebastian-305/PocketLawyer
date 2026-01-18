using Microsoft.AspNetCore.Http;
using PocketLawyer.Domain.Dtos.Shared;

namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Service for validating and decoding uploaded contract files.
/// </summary>
public interface IFileValidatorService
{
    /// <summary>
    /// Validates file size and type, then decodes the file content to text.
    /// </summary>
    /// <param name="file">The uploaded file to validate and decode</param>
    /// <returns>DTO containing file name and decoded text content</returns>
    /// <exception cref="FileValidationException">Thrown when file validation fails</exception>
    Task<FileContentDto> ValidateAndDecodeAsync(IFormFile file);
}
