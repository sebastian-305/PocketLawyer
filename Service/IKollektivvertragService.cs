namespace PocketLawyer.Service;

public interface IKollektivvertragService
{
    string[] GetKvsDirectories();
    Task<string[]> GetKvFileContentsAsync(string kvName);
}
