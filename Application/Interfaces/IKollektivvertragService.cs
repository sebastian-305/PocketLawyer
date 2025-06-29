namespace PocketLawyer.Application.Interfaces;

public interface IKollektivvertragService
{
    string[] GetKvsDirectories();
    Task<string[]> GetKvFileContentsAsync(string kvName);
}
