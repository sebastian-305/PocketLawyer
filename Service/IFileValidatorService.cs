using Microsoft.AspNetCore.Http;
using PocketLawyer;
namespace PocketLawyer.Service;

public interface IFileValidatorService
{
    Task<FileContentDto> ValidateAndDecodeAsync(IFormFile file);
}
