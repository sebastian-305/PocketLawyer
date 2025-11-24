using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

/// <summary>
/// DTO for Rental contract basic analysis results.
/// Contains all basic analysis sections: General, Rent, Conditions, and Summary.
/// Property names match the step names in SchemaProvider for consistency.
/// JSON serialization uses technical keys for backward compatibility.
/// </summary>
public record RentalBasicAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("General")]
    public required RentalBasicGeneralDto General { get; init; }
    
    [JsonPropertyName("Rent")]
    public required RentalBasicRentDto Rent { get; init; }
    
    [JsonPropertyName("Conditions")]
    public required RentalBasicConditionsDto Conditions { get; init; }
    
    [JsonPropertyName("Summary")]
    public required RentalBasicSummaryDto Summary { get; init; }
    
    [JsonIgnore]
    public string ContractType => "Mietvertrag";
    
    [JsonIgnore]
    public string Tier => "Basic";
}

/// <summary>
/// DTO for general rental contract data.
/// </summary>
public record RentalBasicGeneralDto(
    [property: JsonPropertyName("vertragstyp")] string Vertragstyp,
    [property: JsonPropertyName("mieter")] string Mieter,
    [property: JsonPropertyName("vermieter")] string Vermieter,
    [property: JsonPropertyName("mietobjektAdresse")] string MietobjektAdresse,
    [property: JsonPropertyName("vertragsbeginn")] string Vertragsbeginn,
    [property: JsonPropertyName("vertragsart")] string Vertragsart,
    [property: JsonPropertyName("vertragsende")] string? Vertragsende = null
);

/// <summary>
/// DTO for rental conditions (rent, costs, deposit).
/// </summary>
public record RentalBasicRentDto(
    [property: JsonPropertyName("kaltmiete")] decimal Kaltmiete,
    [property: JsonPropertyName("nebenkosten")] decimal Nebenkosten,
    [property: JsonPropertyName("warmmiete")] decimal Warmmiete,
    [property: JsonPropertyName("kaution")] decimal Kaution,
    [property: JsonPropertyName("zahlungsweise")] string Zahlungsweise,
    [property: JsonPropertyName("nebenkostenregelung")] string Nebenkostenregelung
);

/// <summary>
/// DTO for rental contract conditions (termination, pets, subletting).
/// </summary>
public record RentalBasicConditionsDto(
    [property: JsonPropertyName("kuendigungsfrist")] string Kuendigungsfrist,
    [property: JsonPropertyName("tierhaltung")] string Tierhaltung,
    [property: JsonPropertyName("untervermietung")] string Untervermietung,
    [property: JsonPropertyName("besonderheiten")] string Besonderheiten,
    [property: JsonPropertyName("schoenheitsreparaturen")] string? Schoenheitsreparaturen = null,
    [property: JsonPropertyName("betriebskostenabrechnung")] string? Betriebskostenabrechnung = null
);

/// <summary>
/// DTO for basic analysis summary.
/// </summary>
public record RentalBasicSummaryDto(
    [property: JsonPropertyName("kurzfazit")] string Kurzfazit,
    [property: JsonPropertyName("gesamtfazit")] string Gesamtfazit,
    [property: JsonPropertyName("gesamtbewertung")] string Gesamtbewertung,
    [property: JsonPropertyName("riskLevel")] string RiskLevel
);
