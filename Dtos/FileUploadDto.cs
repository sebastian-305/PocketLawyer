using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PocketLawyer;

public class FileUploadDto
{
    [FromForm(Name = "myfile")]
    public IFormFile? UploadedFile { get; set; }
    public string? Kollektivvertrag { get; set; }
}
