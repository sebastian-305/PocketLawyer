using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Restructuring;


public sealed record RentalBasicGeneralDto(
    string Vertragstyp,          // I, II, III, WG
    string Mieter,
    string Vermieter,
    string MietobjektAdresse,
    DateOnly Vertragsbeginn,
    string Vertragsart           // Befristet / Unbefristet
);

public sealed record RentalBasicRentDto(
    decimal Kaltmiete,
    decimal Nebenkosten,
    decimal Warmmiete,
    decimal Kaution,
    string Zahlungsweise,
    string Nebenkostenregelung
);

public sealed record RentalBasicConditionsDto(
    string Kuendigungsfrist,
    string Tierhaltung,
    string Untervermietung,
    string Besonderheiten
);

public enum OverallRiskLevel
{
    Gruen,
    Gelb,
    Rot
}

public sealed record RentalBasicSummaryDto(
    string Kurzfazit,       // „Ihr Mietvertrag ist unbefristet…“
    string Gesamtfazit,     // längerer Text
    string Gesamtbewertung, // z.B. "🟡 teilweise unzulässig (Einzelklausel problematisch)"
    OverallRiskLevel RiskLevel
);

public sealed record RentalBasicAnalysisDto(
    RentalBasicGeneralDto AllgemeineVertragsdaten,
    RentalBasicRentDto Mietkonditionen,
    RentalBasicConditionsDto Vertragsbedingungen,
    RentalBasicSummaryDto Summary
) : IAnalysisDto
{
    public string ContractType => "Mietvertrag";
    public string Tier => "Basic";
}
