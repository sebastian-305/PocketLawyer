using PocketLawyer.Infrastructure;
namespace PocketLawyer.Options;

public class PathOptions
{
    public string ResponsesPath { get; set; } = DefaultPaths.Responses;
    public string TesseractDataPath { get; set; } = DefaultPaths.Tessdata;
    public string KVSPath { get; set; } = DefaultPaths.Kvs;
}
