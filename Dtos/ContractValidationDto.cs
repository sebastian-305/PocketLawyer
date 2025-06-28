using System.Text.Json.Serialization;

namespace PocketLawyer;

// Ergebnis der Voranalyse, bei der Lesbarkeit und
// Vertragscharakter der hochgeladenen Datei geprüft werden.

/// <summary>
/// DTO, das nach dem Aufruf der Vorvalidierung zurückgegeben wird.
/// </summary>
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
