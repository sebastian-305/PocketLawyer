namespace PocketLawyer.Service;

public interface IKollektivvertragService
{
    Task<string> GetSampleContractAsync();
    string[] GetKvsDirectories();
    Task<string[]> GetKvFileContentsAsync(string kvName);
}
