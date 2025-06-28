using PocketLawyer;

namespace PocketLawyer.Service;

public interface IKvDetectionService
{
    Task<KvDetectionResult> DetectAsync(string contractContent);
}
