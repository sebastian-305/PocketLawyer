using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

/// <summary>
/// DTO for Rental contract premium analysis results.
/// Contains detailed legal analysis of all contract sections.
/// Property names match the step names in SchemaProvider for consistency.
/// JSON serialization uses technical keys for backward compatibility.
/// </summary>
public record RentalPremiumAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("Costs")]
    public required PremiumSectionDto Costs { get; init; }
    
    [JsonPropertyName("Terms")]
    public required PremiumSectionDto Terms { get; init; }
    
    [JsonPropertyName("Use")]
    public required PremiumSectionDto Use { get; init; }
    
    [JsonPropertyName("Maintenance")]
    public required PremiumSectionDto Maintenance { get; init; }
    
    [JsonPropertyName("Duties")]
    public required PremiumSectionDto Duties { get; init; }
    
    [JsonPropertyName("Privacy")]
    public required PremiumSectionDto Privacy { get; init; }
    
    [JsonPropertyName("Summary")]
    public required RentalPremiumSummaryDto Summary { get; init; }
    
    [JsonIgnore]
    public string ContractType => "Mietvertrag";
    
    [JsonIgnore]
    public string Tier => "Premium";
}

/// <summary>
/// DTO for premium analysis summary.
/// </summary>
public record RentalPremiumSummaryDto(
    [property: JsonPropertyName("gesamtfazit")] string Gesamtfazit,
    [property: JsonPropertyName("gesamtbewertung")] string Gesamtbewertung,
    [property: JsonPropertyName("handlungsempfehlungen")] string[] Handlungsempfehlungen
);

/// <summary>
/// DTO for a premium analysis section.
/// Contains legal evaluation of a specific contract area.
/// </summary>
public record PremiumSectionDto(
    [property: JsonPropertyName("zusammenfassung")] string Zusammenfassung,
    [property: JsonPropertyName("bewertung")] string Bewertung,
    [property: JsonPropertyName("rechtsgrundlage")] string Rechtsgrundlage,
    [property: JsonPropertyName("risikoScore")] string RisikoScore,
    [property: JsonPropertyName("riskLevel")] string RiskLevel
);

/// <summary>
/// Risk score for individual clauses.
/// </summary>
public enum ClauseRiskScore
{
    Zulaessig,
    Nachteilhaft,
    Unzulaessig
}

/// <summary>
/// Overall risk level for contract or section.
/// </summary>
public enum OverallRiskLevel
{
    Gruen,
    Gelb,
    Rot
}
