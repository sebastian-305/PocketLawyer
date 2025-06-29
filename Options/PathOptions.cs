using PocketLawyer.Infrastructure;

// Enthält Dateipfade zu Ressourcen und Datenordnern.
namespace PocketLawyer.Options;

// Pfadangaben für Ressourcen, werden aus der Konfiguration geladen.
public class PathOptions
{
    public string ResponsesPath { get; set; } = DefaultPaths.Responses;
    public string TesseractDataPath { get; set; } = DefaultPaths.Tessdata;
    public string KVSPath { get; set; } = DefaultPaths.Kvs;
}
