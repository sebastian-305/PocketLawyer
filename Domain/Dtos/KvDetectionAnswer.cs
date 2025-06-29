using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

public class KvDetectionAnswer
{
    [JsonPropertyName("kollektivvertrag")]
    public string Kollektivvertrag { get; set; } = string.Empty;
}
