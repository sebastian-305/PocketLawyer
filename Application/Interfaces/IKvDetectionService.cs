using PocketLawyer.Domain.Dtos.Shared;

namespace PocketLawyer.Application.Interfaces;

public interface IKvDetectionService
{
    Task<KvDetectionResult> DetectAsync(string contractContent);
}
