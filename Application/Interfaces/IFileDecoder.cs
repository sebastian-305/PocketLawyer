namespace PocketLawyer.Application.Interfaces;

// Stellt Funktionen zum Dekodieren verschiedener Dateitypen bereit.

// Jeder Decoder kann prüfen, ob er eine bestimmte Endung unterstützt.
public interface IFileDecoder
{
    bool CanDecode(string ext);
    Task<string> DecodeAsync(Stream input);
}
