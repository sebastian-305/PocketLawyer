using System.Text.Json.Serialization;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Attributes;

namespace PocketLawyer.Domain.Dtos.Rental;

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
[SchemaSection("Allgemeine Vertragsdaten")]
public record RentalBasicGeneralDto(
    [property: JsonPropertyName("vertragstyp")]
    [property: SchemaDisplay("Vertragstyp",
        Description = "Typ des Mietvertrags, z. B. 'I', 'II', 'III', 'WG'.")]
    string Vertragstyp,
    
    [property: JsonPropertyName("mieter")]
    [property: SchemaDisplay("Mieter",
        Description = "Name des/der Mieter:in.",
        Icon = "user",
        DisplayClass = "text-primary")]
    string Mieter,
    
    [property: JsonPropertyName("vermieter")]
    [property: SchemaDisplay("Vermieter",
        Description = "Name des/der Vermieter:in.",
        Icon = "user",
        DisplayClass = "text-primary")]
    string Vermieter,
    
    [property: JsonPropertyName("mietobjektAdresse")]
    [property: SchemaDisplay("Mietobjekt Adresse",
        Description = "Vollständige Adresse des Mietobjekts.",
        Icon = "location",
        DisplayClass = "text-secondary")]
    string MietobjektAdresse,
    
    [property: JsonPropertyName("vertragsbeginn")]
    [property: SchemaDisplay("Vertragsbeginn",
        Description = "Datum des Vertragsbeginns im Format YYYY-MM-DD.",
        Format = "date",
        Icon = "calendar",
        DisplayClass = "text-info")]
    string Vertragsbeginn,
    
    [property: JsonPropertyName("vertragsart")]
    [property: SchemaDisplay("Vertragsart",
        Description = "Art des Vertrags: befristet oder unbefristet.",
        DisplayClass = "badge",
        Importance = "medium",
        Enum = new[] { "befristet", "unbefristet" })]
    string Vertragsart,
    
    [property: JsonPropertyName("vertragsende")]
    [property: SchemaDisplay("Vertragsende",
        Description = "Datum des Vertragsendes im Format YYYY-MM-DD (nur bei befristeten Verträgen).",
        Format = "date",
        Icon = "calendar",
        DisplayClass = "text-info")]
    string? Vertragsende = null
);

/// <summary>
/// DTO for rental conditions (rent, costs, deposit).
/// </summary>
[SchemaSection("Mietkonditionen")]
public record RentalBasicRentDto(
    [property: JsonPropertyName("kaltmiete")]
    [property: SchemaDisplay("Kaltmiete",
        Description = "Monatliche Kaltmiete in Euro.",
        DisplayClass = "font-bold text-lg",
        Importance = "high",
        Icon = "currency",
        StylingFormat = "currency")]
    decimal Kaltmiete,
    
    [property: JsonPropertyName("nebenkosten")]
    [property: SchemaDisplay("Nebenkosten",
        Description = "Monatliche Nebenkosten in Euro.",
        DisplayClass = "text-secondary",
        Icon = "currency",
        StylingFormat = "currency")]
    decimal Nebenkosten,
    
    [property: JsonPropertyName("warmmiete")]
    [property: SchemaDisplay("Warmmiete",
        Description = "Monatliche Warmmiete (Kaltmiete + Nebenkosten) in Euro.",
        DisplayClass = "font-bold text-primary",
        Importance = "high",
        Icon = "currency",
        StylingFormat = "currency")]
    decimal Warmmiete,
    
    [property: JsonPropertyName("kaution")]
    [property: SchemaDisplay("Kaution",
        Description = "Höhe der Kaution in Euro.",
        DisplayClass = "text-warning",
        Importance = "medium",
        Icon = "currency",
        StylingFormat = "currency")]
    decimal Kaution,
    
    [property: JsonPropertyName("zahlungsweise")]
    [property: SchemaDisplay("Zahlungsweise",
        Description = "Zahlungsweise der Miete (z.B. monatlich, vierteljährlich).",
        DisplayClass = "text-info",
        Icon = "calendar")]
    string Zahlungsweise,
    
    [property: JsonPropertyName("nebenkostenregelung")]
    [property: SchemaDisplay("Nebenkostenregelung",
        Description = "Beschreibung der Nebenkostenregelung (Pauschale, Vorauszahlung mit Abrechnung, etc.).",
        DisplayClass = "text-secondary",
        Importance = "medium")]
    string Nebenkostenregelung
);

/// <summary>
/// DTO for rental contract conditions (termination, pets, subletting).
/// </summary>
[SchemaSection("Vertragsbedingungen")]
public record RentalBasicConditionsDto(
    [property: JsonPropertyName("kuendigungsfrist")]
    [property: SchemaDisplay("Kündigungsfrist",
        Description = "Kündigungsfrist für den Mietvertrag (z.B. '3 Monate zum Monatsende').",
        Icon = "clock",
        DisplayClass = "font-semibold",
        Importance = "high")]
    string Kuendigungsfrist,
    
    [property: JsonPropertyName("tierhaltung")]
    [property: SchemaDisplay("Tierhaltung",
        Description = "Regelung zur Tierhaltung (erlaubt, verboten, mit Zustimmung).",
        Icon = "pet",
        DisplayClass = "text-secondary")]
    string Tierhaltung,
    
    [property: JsonPropertyName("untervermietung")]
    [property: SchemaDisplay("Untervermietung",
        Description = "Regelung zur Untervermietung (erlaubt, verboten, mit Zustimmung).",
        Icon = "users",
        DisplayClass = "text-secondary")]
    string Untervermietung,
    
    [property: JsonPropertyName("besonderheiten")]
    [property: SchemaDisplay("Besonderheiten",
        Description = "Weitere besondere Vertragsbedingungen oder Klauseln.",
        Icon = "info",
        DisplayClass = "text-info")]
    string Besonderheiten,
    
    [property: JsonPropertyName("schoenheitsreparaturen")]
    [property: SchemaDisplay("Schönheitsreparaturen",
        Description = "Regelung zu Schönheitsreparaturen (Mieter, Vermieter, geteilt).",
        Icon = "tool",
        DisplayClass = "text-secondary",
        Importance = "medium")]
    string? Schoenheitsreparaturen = null,
    
    [property: JsonPropertyName("betriebskostenabrechnung")]
    [property: SchemaDisplay("Betriebskostenabrechnung",
        Description = "Regelung zur Betriebskostenabrechnung (Frist, Umfang).",
        Icon = "document",
        DisplayClass = "text-secondary")]
    string? Betriebskostenabrechnung = null
);

/// <summary>
/// DTO for basic analysis summary.
/// </summary>
[SchemaSection("Zusammenfassung")]
public record RentalBasicSummaryDto(
    [property: JsonPropertyName("kurzfazit")]
    [property: SchemaDisplay("Kurzfazit",
        Description = "Kurze Zusammenfassung der wichtigsten Vertragspunkte (2-3 Sätze).",
        DisplayClass = "text-lg font-semibold",
        Importance = "high")]
    string Kurzfazit,
    
    [property: JsonPropertyName("gesamtfazit")]
    [property: SchemaDisplay("Gesamtfazit",
        Description = "Ausführliches Fazit zur Vertragsgestaltung und wichtigen Punkten.",
        DisplayClass = "text-base",
        Importance = "high")]
    string Gesamtfazit,
    
    [property: JsonPropertyName("gesamtbewertung")]
    [property: SchemaDisplay("Gesamtbewertung",
        Description = "Gesamtbewertung des Vertrags (z.B. 'Gut', 'Akzeptabel', 'Problematisch').",
        DisplayClass = "badge badge-lg",
        Importance = "high")]
    string Gesamtbewertung,
    
    [property: JsonPropertyName("riskLevel")]
    [property: SchemaDisplay("Risikostufe",
        Description = "Risikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
        DisplayClass = "risk-indicator",
        Importance = "high",
        Icon = "alert",
        Enum = new[] { "Gruen", "Gelb", "Rot" })]
    string RiskLevel
);

