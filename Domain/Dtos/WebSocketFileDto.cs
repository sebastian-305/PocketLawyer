using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

/// <summary>
/// DTO for receiving file data via WebSocket.
/// ContractType is optional since the endpoint determines which agent to use.
/// </summary>
public class WebSocketFileDto
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Optional contract type for validation purposes.
    /// The actual agent is determined by the endpoint URL.
    /// </summary>
    [JsonPropertyName("contractType")]
    public string? ContractType { get; set; }
}
