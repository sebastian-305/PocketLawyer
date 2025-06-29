using Microsoft.AspNetCore.Http;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.Interfaces;

// Überprüft hochgeladene Dateien und liefert deren Textinhalt.

// Kombination aus Validierung und Decodierung von Uploads.
public interface IFileValidatorService
{
    Task<FileContentDto> ValidateAndDecodeAsync(IFormFile file);
}
