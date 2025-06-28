namespace PocketLawyer.Service;

public interface IKollektivvertragService
{
    Task<string> GetSampleContractAsync();
    Task<string[]> GetKvsDirectoriesAsync();
}
