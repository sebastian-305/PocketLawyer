using System.Text.Json.Serialization;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Rental;
using PocketLawyer.Infrastructure.SchemaProviders;

namespace PocketLawyer.Domain.Dtos.Employment;

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
[SchemaSection("Gesamtfazit")]
public record EmploymentPremiumSummaryDto(
    [property: JsonPropertyName("gesamtfazit")]
    [property: SchemaDisplay("Gesamtfazit",
        Description = "Ausführliches Gesamtfazit zur rechtlichen Bewertung des Arbeitsvertrags unter Berücksichtigung aller analysierten Bereiche und des Kollektivvertrags.",
        DisplayClass = "text-lg font-semibold",
        Importance = "high")]
    string Gesamtfazit,
    
    [property: JsonPropertyName("gesamtbewertung")]
    [property: SchemaDisplay("Gesamtbewertung",
        Description = "Gesamtbewertung des Vertrags (z.B. 'Sehr gut', 'Gut', 'Akzeptabel', 'Problematisch', 'Kritisch').",
        DisplayClass = "badge badge-lg",
        Importance = "high")]
    string Gesamtbewertung,
    
    [property: JsonPropertyName("handlungsempfehlungen")]
    [property: SchemaDisplay("Handlungsempfehlungen",
        Description = "Liste konkreter Handlungsempfehlungen für den Arbeitnehmer (z.B. 'Nachverhandlung der Kündigungsfrist empfohlen', 'Wettbewerbsverbot prüfen lassen').",
        Icon = "lightbulb",
        DisplayClass = "list-disc",
        Importance = "high")]
    string[] Handlungsempfehlungen
);
