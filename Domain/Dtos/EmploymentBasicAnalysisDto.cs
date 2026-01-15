using System.Text.Json.Serialization;

namespace PocketLawyer.Domain.Dtos;

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
public record EmploymentBasicGeneralDto(
    [property: JsonPropertyName("arbeitgeber")] string Arbeitgeber,
    [property: JsonPropertyName("arbeitnehmer")] string Arbeitnehmer,
    [property: JsonPropertyName("position")] string Position,
    [property: JsonPropertyName("startdatum")] string Startdatum,
    [property: JsonPropertyName("vertragsart")] string Vertragsart,
    [property: JsonPropertyName("probezeit")] string Probezeit,
    [property: JsonPropertyName("vertragsende")] string? Vertragsende = null
);

/// <summary>
/// DTO for employment compensation and benefits.
/// </summary>
public record EmploymentBasicCompensationDto(
    [property: JsonPropertyName("gehaltBrutto")] string GehaltBrutto,
    [property: JsonPropertyName("gehaltNettoCa")] string GehaltNettoCa,
    [property: JsonPropertyName("zahlungsweise")] string Zahlungsweise,
    [property: JsonPropertyName("sonderzahlungen")] string Sonderzahlungen,
    [property: JsonPropertyName("zuschlaege")] string Zuschlaege,
    [property: JsonPropertyName("zusatzleistungen")] string[] Zusatzleistungen
);

/// <summary>
/// DTO for employment working conditions.
/// </summary>
public record EmploymentBasicWorkingConditionsDto(
    [property: JsonPropertyName("wochenarbeitszeit")] string Wochenarbeitszeit,
    [property: JsonPropertyName("arbeitszeitmodell")] string Arbeitszeitmodell,
    [property: JsonPropertyName("ueberstundenregelung")] string Ueberstundenregelung,
    [property: JsonPropertyName("urlaubstage")] string Urlaubstage,
    [property: JsonPropertyName("arbeitsort")] string Arbeitsort,
    [property: JsonPropertyName("remoteArbeit")] string RemoteArbeit
);

/// <summary>
/// DTO for basic analysis summary with KV mention.
/// </summary>
public record EmploymentBasicSummaryDto(
    [property: JsonPropertyName("kurzfazit")] string Kurzfazit,
    [property: JsonPropertyName("kollektivvertrag")] string Kollektivvertrag,
    [property: JsonPropertyName("gesamtbewertung")] string Gesamtbewertung,
    [property: JsonPropertyName("riskLevel")] string RiskLevel
);
