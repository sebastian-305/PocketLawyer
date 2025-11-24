using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

/// <summary>
/// DTO for Rental contract premium analysis results.
/// Contains detailed legal analysis of all contract sections.
/// </summary>
public record RentalPremiumAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("mietzahlungUndNebenkosten")]
    public required PremiumSectionDto MietzahlungUndNebenkosten { get; init; }
    
    [JsonPropertyName("mietdauerUndKuendigung")]
    public required PremiumSectionDto MietdauerUndKuendigung { get; init; }
    
    [JsonPropertyName("gebrauchDerMietsache")]
    public required PremiumSectionDto GebrauchDerMietsache { get; init; }
    
    [JsonPropertyName("instandhaltungUndReparaturen")]
    public required PremiumSectionDto InstandhaltungUndReparaturen { get; init; }
    
    [JsonPropertyName("rechteUndPflichten")]
    public required PremiumSectionDto RechteUndPflichten { get; init; }
    
    [JsonPropertyName("datenschutzUndKommunikation")]
    public required PremiumSectionDto DatenschutzUndKommunikation { get; init; }
    
    [JsonPropertyName("gesamtfazit")]
    public required string Gesamtfazit { get; init; }
    
    [JsonPropertyName("gesamtbewertung")]
    public required string Gesamtbewertung { get; init; }
    
    [JsonIgnore]
    public string ContractType => "Mietvertrag";
    
    [JsonIgnore]
    public string Tier => "Premium";
}

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
