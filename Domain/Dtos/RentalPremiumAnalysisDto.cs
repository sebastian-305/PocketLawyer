using System.Text.Json.Serialization;
using PocketLawyer.Infrastructure.SchemaProviders;

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
[SchemaSection("Gesamtzusammenfassung")]
public record RentalPremiumSummaryDto(
    [property: JsonPropertyName("gesamtfazit")]
    [property: SchemaDisplay("Gesamtfazit",
        Description = "Gesamtfazit der Premium-Analyse mit Zusammenfassung aller Risiken und Empfehlungen.",
        DisplayClass = "text-lg font-semibold",
        Importance = "high")]
    string Gesamtfazit,
    
    [property: JsonPropertyName("gesamtbewertung")]
    [property: SchemaDisplay("Gesamtbewertung",
        Description = "Gesamtrisikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
        Icon = "alert",
        DisplayClass = "risk-indicator risk-indicator-lg",
        Importance = "high",
        Enum = new[] { "Gruen", "Gelb", "Rot" })]
    string Gesamtbewertung,
    
    [property: JsonPropertyName("handlungsempfehlungen")]
    [property: SchemaDisplay("Handlungsempfehlungen",
        Description = "Liste konkreter Handlungsempfehlungen für den Mieter.",
        Icon = "checklist",
        DisplayClass = "list-disc ml-4",
        Importance = "high")]
    string[] Handlungsempfehlungen
);

/// <summary>
/// DTO for a premium analysis section.
/// Contains legal evaluation of a specific contract area.
/// </summary>
public record PremiumSectionDto(
    [property: JsonPropertyName("zusammenfassung")]
    [property: SchemaDisplay("Zusammenfassung",
        Description = "Zusammenfassung der relevanten Klauseln in diesem Bereich.",
        DisplayClass = "text-base",
        Importance = "high")]
    string Zusammenfassung,
    
    [property: JsonPropertyName("bewertung")]
    [property: SchemaDisplay("Bewertung",
        Description = "Rechtliche Bewertung der Klauseln mit Hinweisen auf Risiken oder Vorteile.",
        DisplayClass = "text-base font-semibold",
        Importance = "high")]
    string Bewertung,
    
    [property: JsonPropertyName("rechtsgrundlage")]
    [property: SchemaDisplay("Rechtsgrundlage",
        Description = "Relevante Rechtsgrundlagen und Paragraphen.",
        Icon = "law",
        DisplayClass = "text-sm text-secondary",
        Importance = "medium")]
    string Rechtsgrundlage,
    
    [property: JsonPropertyName("risikoScore")]
    [property: SchemaDisplay("Risiko-Score",
        Description = "Risikobewertung der Klauseln.",
        Icon = "shield",
        DisplayClass = "badge",
        Importance = "high")]
    string RisikoScore,
    
    [property: JsonPropertyName("riskLevel")]
    [property: SchemaDisplay("Risikostufe",
        Description = "Risikostufe: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
        Icon = "alert",
        DisplayClass = "risk-indicator",
        Importance = "high")]
    string RiskLevel
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
