namespace PocketLawyer.Service;

public interface IKollektivvertragService
{
    Task<string> GetSampleContractAsync();
    Task<string[]> GetKvsDirectoriesAsync();
    Task<string[]> GetKvFileContentsAsync(string folderName);
}
