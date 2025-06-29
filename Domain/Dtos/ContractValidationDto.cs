using System.Text.Json.Serialization;

// Wird für die Voranalyse verwendet und enthält Informationen,
// ob die Datei lesbar und ein Arbeitsvertrag ist.

namespace PocketLawyer.Domain.Dtos;

public class ContractValidationDto
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
