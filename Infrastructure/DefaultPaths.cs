using System.IO;

namespace PocketLawyer.Infrastructure;

public static class DefaultPaths
{
    public const string BaseData = "data";
    public static string Responses => Path.Combine(BaseData, "Responses");
    public static string Kvs => Path.Combine(BaseData, "KVS");
    public static string Tessdata => Path.Combine(BaseData, "tessdata");
    public static string Context => Path.Combine(BaseData, "Context");
    public static string RentContext => Path.Combine(BaseData, "Context", "Rent");
    public static string EmploymentContext => Path.Combine(BaseData, "Context", "Employment");

}
