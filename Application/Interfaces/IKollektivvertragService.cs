namespace PocketLawyer.Application.Interfaces;

// Zugriffsschicht für Kollektivvertragsdateien.

// Liefert verfügbare KV-Ordner und deren Inhalte.
public interface IKollektivvertragService
{
    string[] GetKvsDirectories();
    Task<string[]> GetKvFileContentsAsync(string kvName);
}
