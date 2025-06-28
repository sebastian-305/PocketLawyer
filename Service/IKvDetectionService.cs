namespace PocketLawyer.Service;

public interface IKvDetectionService
{
    Task<string> DetectKvAsync(string contractText);
}
