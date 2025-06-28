namespace PocketLawyer;

// Ergebnis, das nach der KV-Erkennung an das Frontend zurückgegeben wird.

/// <summary>
/// Aggregiertes Ergebnis der KV-Erkennung einschließlich eines Flags,
/// das angibt, ob alle KI-Antworten übereinstimmten.
/// </summary>
public class KvDetectionResult
{
    public string KvName { get; set; } = string.Empty;
    public bool AllEqual { get; set; }
}
