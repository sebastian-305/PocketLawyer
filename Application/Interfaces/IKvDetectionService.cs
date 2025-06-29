using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Application.Interfaces;

public interface IKvDetectionService
{
    Task<KvDetectionResult> DetectAsync(string contractContent);
}
