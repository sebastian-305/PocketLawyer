using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos.Shared;

public class ContractValidationDto :IValidationDto
{
    [JsonPropertyName("isReadable")]
    public bool IsReadable { get; set; }

    [JsonPropertyName("isContract")]
    public bool IsContract { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("kollektivvertrag")]
    public string? Kollektivvertrag { get; set; }
}
