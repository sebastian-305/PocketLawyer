using Microsoft.AspNetCore.Http;
namespace PocketLawyer.Service;

public interface IFileValidatorService
{
    Task<FileContentDto> ValidateAndDecodeAsync(IFormFile file);
}
