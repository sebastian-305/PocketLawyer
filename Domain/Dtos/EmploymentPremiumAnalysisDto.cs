using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

/// <summary>
/// DTO for Employment contract premium analysis results.
/// Contains detailed legal analysis of all contract sections.
/// Property names match the step names in SchemaProvider for consistency.
/// JSON serialization uses camelCase for backward compatibility.
/// </summary>
public record EmploymentPremiumAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("CompensationBenefits")]
    public required PremiumSectionDto CompensationBenefits { get; init; }
    
    [JsonPropertyName("WorkingTimeVacation")]
    public required PremiumSectionDto WorkingTimeVacation { get; init; }
    
    [JsonPropertyName("TerminationNotice")]
    public required PremiumSectionDto TerminationNotice { get; init; }
    
    [JsonPropertyName("NonCompeteConfidentiality")]
    public required PremiumSectionDto NonCompeteConfidentiality { get; init; }
    
    [JsonPropertyName("RightsDuties")]
    public required PremiumSectionDto RightsDuties { get; init; }
    
    [JsonPropertyName("Summary")]
    public required EmploymentPremiumSummaryDto Summary { get; init; }
    
    [JsonIgnore]
    public string ContractType => "Arbeitsvertrag";
    
    [JsonIgnore]
    public string Tier => "Premium";
}

/// <summary>
/// DTO for premium analysis summary.
/// </summary>
public record EmploymentPremiumSummaryDto(
    [property: JsonPropertyName("gesamtfazit")] string Gesamtfazit,
    [property: JsonPropertyName("gesamtbewertung")] string Gesamtbewertung,
    [property: JsonPropertyName("handlungsempfehlungen")] string[] Handlungsempfehlungen
);
