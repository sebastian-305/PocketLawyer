using System.Text.Json.Serialization;

namespace PocketLawyer;

public class WebSocketFileDto
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}
