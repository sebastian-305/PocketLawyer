namespace PocketLawyer.Service;

public interface IKvDetectionService
{
    Task<string> DetectAsync(string contractContent);
}
