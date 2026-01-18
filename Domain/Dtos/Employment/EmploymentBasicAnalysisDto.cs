using System.Text.Json.Serialization;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Infrastructure.SchemaProviders;

namespace PocketLawyer.Domain.Dtos.Employment;

/// <summary>
/// DTO for Employment contract basic analysis results.
/// Contains all basic analysis sections: General, Compensation, WorkingConditions, and Summary.
/// Property names match the step names in SchemaProvider for consistency.
/// JSON serialization uses camelCase for backward compatibility.
/// </summary>
public record EmploymentBasicAnalysisDto : IAnalysisDto
{
    [JsonPropertyName("General")]
    public required EmploymentBasicGeneralDto General { get; init; }
    
    [JsonPropertyName("Compensation")]
    public required EmploymentBasicCompensationDto Compensation { get; init; }
    
    [JsonPropertyName("WorkingConditions")]
    public required EmploymentBasicWorkingConditionsDto WorkingConditions { get; init; }
    
    [JsonPropertyName("Summary")]
    public required EmploymentBasicSummaryDto Summary { get; init; }
    
    [JsonIgnore]
    public string ContractType => "Arbeitsvertrag";
    
    [JsonIgnore]
    public string Tier => "Basic";
}

/// <summary>
/// DTO for general employment contract data.
/// </summary>
[SchemaSection("Allgemeine Arbeitsdaten")]
public record EmploymentBasicGeneralDto(
    [property: JsonPropertyName("arbeitgeber")]
    [property: SchemaDisplay("Arbeitgeber",
        Description = "Name des Arbeitgebers.",
        Icon = "building",
        DisplayClass = "text-primary")]
    string Arbeitgeber,
    
    [property: JsonPropertyName("arbeitnehmer")]
    [property: SchemaDisplay("Arbeitnehmer",
        Description = "Name des/der Arbeitnehmer:in.",
        Icon = "user",
        DisplayClass = "text-primary")]
    string Arbeitnehmer,
    
    [property: JsonPropertyName("position")]
    [property: SchemaDisplay("Position",
        Description = "Bezeichnung der Position/Tätigkeit.",
        Icon = "briefcase",
        DisplayClass = "text-secondary")]
    string Position,
    
    [property: JsonPropertyName("startdatum")]
    [property: SchemaDisplay("Startdatum",
        Description = "Datum des Arbeitsbeginns im Format YYYY-MM-DD.",
        Format = "date",
        Icon = "calendar",
        DisplayClass = "text-info")]
    string Startdatum,
    
    [property: JsonPropertyName("vertragsart")]
    [property: SchemaDisplay("Vertragsart",
        Description = "Art des Vertrags: befristet oder unbefristet.",
        DisplayClass = "badge",
        Importance = "medium",
        Enum = new[] { "befristet", "unbefristet" })]
    string Vertragsart,
    
    [property: JsonPropertyName("probezeit")]
    [property: SchemaDisplay("Probezeit",
        Description = "Dauer der Probezeit (z.B. '1 Monat', '3 Monate', 'keine').",
        Icon = "clock",
        DisplayClass = "text-secondary")]
    string Probezeit,
    
    [property: JsonPropertyName("vertragsende")]
    [property: SchemaDisplay("Vertragsende",
        Description = "Datum des Vertragsendes im Format YYYY-MM-DD (nur bei befristeten Verträgen).",
        Format = "date",
        Icon = "calendar",
        DisplayClass = "text-info")]
    string? Vertragsende = null
);

/// <summary>
/// DTO for employment compensation and benefits.
/// </summary>
[SchemaSection("Vergütung und Zusatzleistungen")]
public record EmploymentBasicCompensationDto(
    [property: JsonPropertyName("gehaltBrutto")]
    [property: SchemaDisplay("Bruttogehalt",
        Description = "Bruttogehalt (z.B. '3.500 EUR monatlich').",
        Icon = "currency",
        DisplayClass = "text-primary font-semibold",
        Importance = "high")]
    string GehaltBrutto,
    
    [property: JsonPropertyName("gehaltNettoCa")]
    [property: SchemaDisplay("Nettogehalt (ca.)",
        Description = "Geschätztes Nettogehalt (z.B. '2.450 EUR monatlich ca.').",
        Icon = "currency",
        DisplayClass = "text-secondary")]
    string GehaltNettoCa,
    
    [property: JsonPropertyName("zahlungsweise")]
    [property: SchemaDisplay("Zahlungsweise",
        Description = "Zahlungsweise des Gehalts (z.B. 'monatlich', '14x jährlich').",
        Icon = "calendar",
        DisplayClass = "text-secondary")]
    string Zahlungsweise,
    
    [property: JsonPropertyName("sonderzahlungen")]
    [property: SchemaDisplay("Sonderzahlungen",
        Description = "Sonderzahlungen wie 13./14. Gehalt, Weihnachtsgeld, Urlaubsgeld.",
        Icon = "gift",
        DisplayClass = "text-info",
        Importance = "medium")]
    string Sonderzahlungen,
    
    [property: JsonPropertyName("zuschlaege")]
    [property: SchemaDisplay("Zuschläge",
        Description = "Zuschläge für Überstunden, Nachtarbeit, Wochenendarbeit etc.",
        Icon = "plus",
        DisplayClass = "text-secondary")]
    string Zuschlaege,
    
    [property: JsonPropertyName("zusatzleistungen")]
    [property: SchemaDisplay("Zusatzleistungen",
        Description = "Zusatzleistungen wie Firmenwagen, Handy, Essenszuschuss, Versicherungen etc.",
        Icon = "star",
        DisplayClass = "badge-list")]
    string[] Zusatzleistungen
);

/// <summary>
/// DTO for employment working conditions.
/// </summary>
[SchemaSection("Arbeitsbedingungen")]
public record EmploymentBasicWorkingConditionsDto(
    [property: JsonPropertyName("wochenarbeitszeit")]
    [property: SchemaDisplay("Wochenarbeitszeit",
        Description = "Wöchentliche Arbeitszeit (z.B. '40 Stunden', '38,5 Stunden').",
        Icon = "clock",
        DisplayClass = "text-primary",
        Importance = "high")]
    string Wochenarbeitszeit,
    
    [property: JsonPropertyName("arbeitszeitmodell")]
    [property: SchemaDisplay("Arbeitszeitmodell",
        Description = "Arbeitszeitmodell (z.B. 'Vollzeit', 'Teilzeit 30h', 'Gleitzeit').",
        DisplayClass = "badge",
        Importance = "medium")]
    string Arbeitszeitmodell,
    
    [property: JsonPropertyName("ueberstundenregelung")]
    [property: SchemaDisplay("Überstundenregelung",
        Description = "Regelung zu Überstunden (z.B. 'mit Gehalt abgegolten', 'Zeitausgleich', 'Auszahlung').",
        Icon = "clock",
        DisplayClass = "text-secondary",
        Importance = "medium")]
    string Ueberstundenregelung,
    
    [property: JsonPropertyName("urlaubstage")]
    [property: SchemaDisplay("Urlaubstage",
        Description = "Anzahl der Urlaubstage pro Jahr (z.B. '25 Tage', '30 Tage').",
        Icon = "sun",
        DisplayClass = "text-info",
        Importance = "medium")]
    string Urlaubstage,
    
    [property: JsonPropertyName("arbeitsort")]
    [property: SchemaDisplay("Arbeitsort",
        Description = "Arbeitsort/Standort (z.B. 'Wien, Büro Hauptstraße 1', 'flexibel').",
        Icon = "location",
        DisplayClass = "text-secondary")]
    string Arbeitsort,
    
    [property: JsonPropertyName("remoteArbeit")]
    [property: SchemaDisplay("Remote-Arbeit",
        Description = "Regelung zu Remote-Arbeit/Homeoffice (z.B. 'bis zu 2 Tage/Woche', 'nicht vorgesehen', '100% remote').",
        Icon = "home",
        DisplayClass = "badge")]
    string RemoteArbeit
);

/// <summary>
/// DTO for basic analysis summary with KV mention.
/// </summary>
[SchemaSection("Zusammenfassung")]
public record EmploymentBasicSummaryDto(
    [property: JsonPropertyName("kurzfazit")]
    [property: SchemaDisplay("Kurzfazit",
        Description = "Kurze Zusammenfassung der wichtigsten Vertragspunkte (2-3 Sätze).",
        DisplayClass = "text-lg font-semibold",
        Importance = "high")]
    string Kurzfazit,
    
    [property: JsonPropertyName("kollektivvertrag")]
    [property: SchemaDisplay("Kollektivvertrag",
        Description = "Erwähnung des erkannten Kollektivvertrags oder 'Kein Kollektivvertrag erkannt'.",
        Icon = "document",
        DisplayClass = "badge badge-info",
        Importance = "high")]
    string Kollektivvertrag,
    
    [property: JsonPropertyName("gesamtbewertung")]
    [property: SchemaDisplay("Gesamtbewertung",
        Description = "Gesamtbewertung des Vertrags (z.B. 'Gut', 'Akzeptabel', 'Problematisch').",
        DisplayClass = "badge badge-lg",
        Importance = "high")]
    string Gesamtbewertung,
    
    [property: JsonPropertyName("riskLevel")]
    [property: SchemaDisplay("Risikostufe",
        Description = "Risikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
        Icon = "alert",
        DisplayClass = "risk-indicator",
        Importance = "high",
        Enum = new[] { "Gruen", "Gelb", "Rot" })]
    string RiskLevel
);
