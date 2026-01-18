using System.Text.Json;
using System.Text.Json.Serialization;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Rental;
using Xunit;
using Xunit.Abstractions;

namespace PocketLawyer.Tests.Infrastructure;

public class SerializationTest
{
    private readonly ITestOutputHelper _output;

    public SerializationTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestAnalysisDtoSerialization()
    {
        // Arrange
        var basicDto = new RentalBasicAnalysisDto
        {
            General = new RentalBasicGeneralDto(
                Vertragstyp: "Wohnraum",
                Mieter: "Max Mustermann",
                Vermieter: "Erika Musterfrau",
                MietobjektAdresse: "Musterstraße 1",
                Vertragsbeginn: "2025-01-01",
                Vertragsart: "Unbefristet"
            ),
            Rent = new RentalBasicRentDto(
                Kaltmiete: 1000m,
                Nebenkosten: 200m,
                Warmmiete: 1200m,
                Kaution: 3000m,
                Zahlungsweise: "Monatlich",
                Nebenkostenregelung: "Vorauszahlung"
            ),
            Conditions = new RentalBasicConditionsDto(
                Kuendigungsfrist: "3 Monate",
                Tierhaltung: "Erlaubt",
                Untervermietung: "Mit Zustimmung",
                Besonderheiten: "Keine"
            ),
            Summary = new RentalBasicSummaryDto(
                Kurzfazit: "Guter Vertrag",
                Gesamtfazit: "Empfehlenswert",
                Gesamtbewertung: "Gut",
                RiskLevel: "Gruen"
            )
        };

        var metadata = new AnalysisMetadataDto(
            ContractId: "test-123",
            CountryCode: "AT",
            Language: "de",
            AnalyzedAt: DateTimeOffset.UtcNow,
            IsPreview: false,
            ModelVersion: "1.0"
        );

        var wrapper = new AnalysisDto<IAnalysisDto>(basicDto, metadata);

        // Act - Test 1: Default serialization
        _output.WriteLine("Test 1 - Default serialization:");
        var json1 = JsonSerializer.Serialize(wrapper);
        _output.WriteLine(json1);
        _output.WriteLine("");

        // Act - Test 2: With GetType()
        _output.WriteLine("Test 2 - With GetType():");
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
        var json2 = JsonSerializer.Serialize(wrapper, wrapper.GetType(), options);
        _output.WriteLine(json2);
        _output.WriteLine("");

        // Act - Test 3: Serialize Result directly with its runtime type
        _output.WriteLine("Test 3 - Serialize Result directly with runtime type:");
        var json3 = JsonSerializer.Serialize(wrapper.Result, wrapper.Result.GetType(), options);
        _output.WriteLine(json3);
        _output.WriteLine("");

        // Assert
        Assert.Contains("General", json3);
        Assert.Contains("Rent", json3);
    }
}
