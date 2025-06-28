namespace PocketLawyer;

// Vereinfachtes DTO für den /test-Endpunkt.
// Zeigt an, ob alle KV-Erkennungsaufrufe das gleiche Ergebnis lieferten.

/// <summary>
/// DTO für den Testendpunkt.
/// </summary>
public class KvDetectionTestResultDto
{
    public bool AllEqual { get; set; }
}
