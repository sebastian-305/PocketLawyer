using System.Text.Json.Serialization;

namespace PocketLawyer;

public class ContractValidationDto
{
    [JsonPropertyName("isReadable")]
    public bool IsReadable { get; set; }

    [JsonPropertyName("isContract")]
    public bool IsContract { get; set; }

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;
}
