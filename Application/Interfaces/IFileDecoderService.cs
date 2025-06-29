namespace PocketLawyer.Application.Interfaces;

// Aggregiert verschiedene FileDecoder und bietet Hilfsfunktionen an.

// Liefert den Inhalt einer Datei abhängig von ihrer Endung.
public interface IFileDecoderService
{
    string[] AllowedExtensions { get; }
    string AllowedExtensionsString { get; }
    Task<string> GetFileContentAsync(string filename, Stream content);
}
