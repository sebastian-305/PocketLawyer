using System.IO;

// Enthält zentrale Standardpfade innerhalb der Anwendung.

namespace PocketLawyer.Infrastructure;

// Bietet Konstanten für Ordnerstrukturen an.
public static class DefaultPaths
{
    public const string BaseData = "data";

    public static string Responses => Path.Combine(BaseData, "Responses");
    public static string Kvs => Path.Combine(BaseData, "KVS");
    public static string Tessdata => Path.Combine(BaseData, "tessdata");
    public static string Context => Path.Combine(BaseData, "Context");
}
