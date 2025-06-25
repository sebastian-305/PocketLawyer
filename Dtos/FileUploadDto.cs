using Microsoft.AspNetCore.Http;

namespace PocketLawyer;

public class FileUploadDto
{
    public IFormFile? MyFile { get; set; }
}
