using System.Text.Json.Serialization;

// Repräsentiert die Antwort der KV-Erkennung
// und enthält den ermittelten Kollektivvertrag.

namespace PocketLawyer.Domain.Dtos;

public class KvDetectionAnswer
{
    [JsonPropertyName("kollektivvertrag")]
    public string Kollektivvertrag { get; set; } = string.Empty;
}
