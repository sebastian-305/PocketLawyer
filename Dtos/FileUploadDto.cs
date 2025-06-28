using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PocketLawyer;

// Repräsentiert die Formdaten beim Hochladen einer Vertragsdatei.

/// <summary>
/// DTO, das von den Upload-Endpunkten verwendet wird.
/// </summary>
public class FileUploadDto
{
    [FromForm(Name = "myfile")]
    public IFormFile? UploadedFile { get; set; }
    public string? Kollektivvertrag { get; set; }
}
