using Microsoft.AspNetCore.Http;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.Interfaces;

public interface IFileValidatorService
{
    Task<FileContentDto> ValidateAndDecodeAsync(IFormFile file);
}
