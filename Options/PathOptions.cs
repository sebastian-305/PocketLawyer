using System.IO;

namespace PocketLawyer.Options;

public class PathOptions
{
    public string ResponsesPath { get; set; } = Path.Combine("data", "Responses");
    public string TesseractDataPath { get; set; } = Path.Combine("data", "tessdata");
    public string KVSPath { get; set; } = Path.Combine("data", "KVS");
}
