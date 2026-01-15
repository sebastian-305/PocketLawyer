using System.Text.Json;
using PocketLawyer.Domain.Dtos;
using Xunit;

namespace PocketLawyer.Tests.Domain;

/// <summary>
/// Unit tests for Employment DTO serialization/deserialization.
/// Tests that DTOs can be serialized to JSON and deserialized back correctly.
/// </summary>
public class EmploymentDtoTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    #region Basic Analysis DTO Tests

    [Fact]
    public void EmploymentBasicGeneralDto_SerializesAndDeserializes_Successfully()
    {
        // Arrange
        var dto = new EmploymentBasicGeneralDto(
            Arbeitgeber: "Test GmbH",
            Arbeitnehmer: "Max Mustermann",
            Position: "Software Developer",
            Startdatum: "01.01.2024",
            Vertragsart: "Unbefristet",
            Probezeit: "3 Monate",
            Vertragsende: null
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentBasicGeneralDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(dto.Arbeitgeber, deserialized.Arbeitgeber);
        Assert.Equal(dto.Arbeitnehmer, deserialized.Arbeitnehmer);
        Assert.Equal(dto.Position, deserialized.Position);
        Assert.Equal(dto.Startdatum, deserialized.Startdatum);
        Assert.Equal(dto.Vertragsart, deserialized.Vertragsart);
        Assert.Equal(dto.Probezeit, deserialized.Probezeit);
        Assert.Equal(dto.Vertragsende, deserialized.Vertragsende);
    }

    [Fact]
    public void EmploymentBasicGeneralDto_WithVertragsende_SerializesCorrectly()
    {
        // Arrange
        var dto = new EmploymentBasicGeneralDto(
            Arbeitgeber: "Test GmbH",
            Arbeitnehmer: "Max Mustermann",
            Position: "Software Developer",
            Startdatum: "01.01.2024",
            Vertragsart: "Befristet",
            Probezeit: "3 Monate",
            Vertragsende: "31.12.2024"
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentBasicGeneralDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("31.12.2024", deserialized.Vertragsende);
    }

    [Fact]
    public void EmploymentBasicCompensationDto_SerializesAndDeserializes_Successfully()
    {
        // Arrange
        var dto = new EmploymentBasicCompensationDto(
            GehaltBrutto: "3500 EUR",
            GehaltNettoCa: "2400 EUR",
            Zahlungsweise: "Monatlich",
            Sonderzahlungen: "13. und 14. Gehalt",
            Zuschlaege: "Überstundenzuschläge",
            Zusatzleistungen: new[] { "Firmenwagen", "Laptop", "Handy" }
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentBasicCompensationDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(dto.GehaltBrutto, deserialized.GehaltBrutto);
        Assert.Equal(dto.GehaltNettoCa, deserialized.GehaltNettoCa);
        Assert.Equal(dto.Zahlungsweise, deserialized.Zahlungsweise);
        Assert.Equal(dto.Sonderzahlungen, deserialized.Sonderzahlungen);
        Assert.Equal(dto.Zuschlaege, deserialized.Zuschlaege);
        Assert.Equal(dto.Zusatzleistungen, deserialized.Zusatzleistungen);
    }

    [Fact]
    public void EmploymentBasicCompensationDto_WithEmptyZusatzleistungen_SerializesCorrectly()
    {
        // Arrange
        var dto = new EmploymentBasicCompensationDto(
            GehaltBrutto: "3500 EUR",
            GehaltNettoCa: "2400 EUR",
            Zahlungsweise: "Monatlich",
            Sonderzahlungen: "Keine",
            Zuschlaege: "Keine",
            Zusatzleistungen: Array.Empty<string>()
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentBasicCompensationDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Empty(deserialized.Zusatzleistungen);
    }

    [Fact]
    public void EmploymentBasicWorkingConditionsDto_SerializesAndDeserializes_Successfully()
    {
        // Arrange
        var dto = new EmploymentBasicWorkingConditionsDto(
            Wochenarbeitszeit: "40 Stunden",
            Arbeitszeitmodell: "Vollzeit",
            Ueberstundenregelung: "Abgeltung durch Zeitausgleich",
            Urlaubstage: "25 Tage",
            Arbeitsort: "Wien",
            RemoteArbeit: "2 Tage pro Woche"
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentBasicWorkingConditionsDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(dto.Wochenarbeitszeit, deserialized.Wochenarbeitszeit);
        Assert.Equal(dto.Arbeitszeitmodell, deserialized.Arbeitszeitmodell);
        Assert.Equal(dto.Ueberstundenregelung, deserialized.Ueberstundenregelung);
        Assert.Equal(dto.Urlaubstage, deserialized.Urlaubstage);
        Assert.Equal(dto.Arbeitsort, deserialized.Arbeitsort);
        Assert.Equal(dto.RemoteArbeit, deserialized.RemoteArbeit);
    }

    [Fact]
    public void EmploymentBasicSummaryDto_SerializesAndDeserializes_Successfully()
    {
        // Arrange
        var dto = new EmploymentBasicSummaryDto(
            Kurzfazit: "Standardvertrag mit fairen Bedingungen",
            Kollektivvertrag: "IT-Kollektivvertrag",
            Gesamtbewertung: "Positiv",
            RiskLevel: "Gruen"
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentBasicSummaryDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(dto.Kurzfazit, deserialized.Kurzfazit);
        Assert.Equal(dto.Kollektivvertrag, deserialized.Kollektivvertrag);
        Assert.Equal(dto.Gesamtbewertung, deserialized.Gesamtbewertung);
        Assert.Equal(dto.RiskLevel, deserialized.RiskLevel);
    }

    [Fact]
    public void EmploymentBasicAnalysisDto_SerializesAndDeserializes_Successfully()
    {
        // Arrange
        var dto = new EmploymentBasicAnalysisDto
        {
            General = new EmploymentBasicGeneralDto(
                Arbeitgeber: "Test GmbH",
                Arbeitnehmer: "Max Mustermann",
                Position: "Software Developer",
                Startdatum: "01.01.2024",
                Vertragsart: "Unbefristet",
                Probezeit: "3 Monate"
            ),
            Compensation = new EmploymentBasicCompensationDto(
                GehaltBrutto: "3500 EUR",
                GehaltNettoCa: "2400 EUR",
                Zahlungsweise: "Monatlich",
                Sonderzahlungen: "13. und 14. Gehalt",
                Zuschlaege: "Überstundenzuschläge",
                Zusatzleistungen: new[] { "Firmenwagen" }
            ),
            WorkingConditions = new EmploymentBasicWorkingConditionsDto(
                Wochenarbeitszeit: "40 Stunden",
                Arbeitszeitmodell: "Vollzeit",
                Ueberstundenregelung: "Zeitausgleich",
                Urlaubstage: "25 Tage",
                Arbeitsort: "Wien",
                RemoteArbeit: "2 Tage pro Woche"
            ),
            Summary = new EmploymentBasicSummaryDto(
                Kurzfazit: "Standardvertrag",
                Kollektivvertrag: "IT-KV",
                Gesamtbewertung: "Positiv",
                RiskLevel: "Gruen"
            )
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentBasicAnalysisDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.General);
        Assert.NotNull(deserialized.Compensation);
        Assert.NotNull(deserialized.WorkingConditions);
        Assert.NotNull(deserialized.Summary);
        Assert.Equal("Arbeitsvertrag", deserialized.ContractType);
        Assert.Equal("Basic", deserialized.Tier);
    }

    #endregion

    #region Premium Analysis DTO Tests

    [Fact]
    public void EmploymentPremiumSummaryDto_SerializesAndDeserializes_Successfully()
    {
        // Arrange
        var dto = new EmploymentPremiumSummaryDto(
            Gesamtfazit: "Der Vertrag ist insgesamt fair gestaltet",
            Gesamtbewertung: "Gruen",
            Handlungsempfehlungen: new[] { "Keine Änderungen erforderlich", "Vertrag kann unterzeichnet werden" }
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentPremiumSummaryDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(dto.Gesamtfazit, deserialized.Gesamtfazit);
        Assert.Equal(dto.Gesamtbewertung, deserialized.Gesamtbewertung);
        Assert.Equal(dto.Handlungsempfehlungen, deserialized.Handlungsempfehlungen);
    }

    [Fact]
    public void EmploymentPremiumSummaryDto_WithEmptyHandlungsempfehlungen_SerializesCorrectly()
    {
        // Arrange
        var dto = new EmploymentPremiumSummaryDto(
            Gesamtfazit: "Vertrag ist akzeptabel",
            Gesamtbewertung: "Gelb",
            Handlungsempfehlungen: Array.Empty<string>()
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentPremiumSummaryDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Empty(deserialized.Handlungsempfehlungen);
    }

    [Fact]
    public void EmploymentPremiumAnalysisDto_SerializesAndDeserializes_Successfully()
    {
        // Arrange
        var section = new PremiumSectionDto(
            Zusammenfassung: "Test Zusammenfassung",
            Bewertung: "Test Bewertung",
            Rechtsgrundlage: "Test Rechtsgrundlage",
            RisikoScore: "Zulaessig",
            RiskLevel: "Gruen"
        );

        var dto = new EmploymentPremiumAnalysisDto
        {
            CompensationBenefits = section,
            WorkingTimeVacation = section,
            TerminationNotice = section,
            NonCompeteConfidentiality = section,
            RightsDuties = section,
            Summary = new EmploymentPremiumSummaryDto(
                Gesamtfazit: "Gesamtfazit",
                Gesamtbewertung: "Gruen",
                Handlungsempfehlungen: new[] { "Empfehlung 1" }
            )
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<EmploymentPremiumAnalysisDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.CompensationBenefits);
        Assert.NotNull(deserialized.WorkingTimeVacation);
        Assert.NotNull(deserialized.TerminationNotice);
        Assert.NotNull(deserialized.NonCompeteConfidentiality);
        Assert.NotNull(deserialized.RightsDuties);
        Assert.NotNull(deserialized.Summary);
        Assert.Equal("Arbeitsvertrag", deserialized.ContractType);
        Assert.Equal("Premium", deserialized.Tier);
    }

    #endregion

    #region JSON Property Name Tests

    [Fact]
    public void EmploymentBasicGeneralDto_UsesCamelCaseJsonPropertyNames()
    {
        // Arrange
        var dto = new EmploymentBasicGeneralDto(
            Arbeitgeber: "Test",
            Arbeitnehmer: "Test",
            Position: "Test",
            Startdatum: "Test",
            Vertragsart: "Test",
            Probezeit: "Test"
        );

        // Act
        var json = JsonSerializer.Serialize(dto);

        // Assert
        Assert.Contains("\"arbeitgeber\"", json);
        Assert.Contains("\"arbeitnehmer\"", json);
        Assert.Contains("\"position\"", json);
        Assert.Contains("\"startdatum\"", json);
        Assert.Contains("\"vertragsart\"", json);
        Assert.Contains("\"probezeit\"", json);
    }

    [Fact]
    public void EmploymentBasicCompensationDto_UsesCamelCaseJsonPropertyNames()
    {
        // Arrange
        var dto = new EmploymentBasicCompensationDto(
            GehaltBrutto: "Test",
            GehaltNettoCa: "Test",
            Zahlungsweise: "Test",
            Sonderzahlungen: "Test",
            Zuschlaege: "Test",
            Zusatzleistungen: Array.Empty<string>()
        );

        // Act
        var json = JsonSerializer.Serialize(dto);

        // Assert
        Assert.Contains("\"gehaltBrutto\"", json);
        Assert.Contains("\"gehaltNettoCa\"", json);
        Assert.Contains("\"zahlungsweise\"", json);
        Assert.Contains("\"sonderzahlungen\"", json);
        Assert.Contains("\"zuschlaege\"", json);
        Assert.Contains("\"zusatzleistungen\"", json);
    }

    [Fact]
    public void EmploymentBasicSummaryDto_UsesCamelCaseJsonPropertyNames()
    {
        // Arrange
        var dto = new EmploymentBasicSummaryDto(
            Kurzfazit: "Test",
            Kollektivvertrag: "Test",
            Gesamtbewertung: "Test",
            RiskLevel: "Test"
        );

        // Act
        var json = JsonSerializer.Serialize(dto);

        // Assert
        Assert.Contains("\"kurzfazit\"", json);
        Assert.Contains("\"kollektivvertrag\"", json);
        Assert.Contains("\"gesamtbewertung\"", json);
        Assert.Contains("\"riskLevel\"", json);
    }

    [Fact]
    public void EmploymentPremiumSummaryDto_UsesCamelCaseJsonPropertyNames()
    {
        // Arrange
        var dto = new EmploymentPremiumSummaryDto(
            Gesamtfazit: "Test",
            Gesamtbewertung: "Test",
            Handlungsempfehlungen: Array.Empty<string>()
        );

        // Act
        var json = JsonSerializer.Serialize(dto);

        // Assert
        Assert.Contains("\"gesamtfazit\"", json);
        Assert.Contains("\"gesamtbewertung\"", json);
        Assert.Contains("\"handlungsempfehlungen\"", json);
    }

    #endregion
}
