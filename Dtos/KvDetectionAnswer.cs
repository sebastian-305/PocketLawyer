using System.Text.Json.Serialization;

namespace PocketLawyer;

// Repräsentation des kleinen JSON-Objekts,
// das bei der KV-Erkennung vom KI-Dienst zurückgegeben wird.

/// <summary>
/// Einfaches DTO, das das Ergebnis der KI-Erkennung enthält.
/// </summary>
public class KvDetectionAnswer
{
    [JsonPropertyName("kollektivvertrag")]
    public string Kollektivvertrag { get; set; } = string.Empty;
}
