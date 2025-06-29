// Ergebnisobjekt der KV-Erkennung, enthält den ermittelten KV-Namen
// und ob alle Quellen übereinstimmten.
namespace PocketLawyer.Domain.Dtos;

public class KvDetectionResult
{
    public string KvName { get; set; } = string.Empty;
    public bool AllEqual { get; set; }
}
