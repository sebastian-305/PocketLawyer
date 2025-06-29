using System.Text.Json.Serialization;

// Dieses DTO beschreibt die hochgeladene Datei im WebSocket-Verkehr.
// "FileName" enthält den Namen der Datei, "Data" die kodierten Inhalte.

namespace PocketLawyer.Domain.Dtos;

public class WebSocketFileDto
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}
