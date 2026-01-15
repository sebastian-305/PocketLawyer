using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Unit tests for Employment schema validation.
/// Tests that schemas validate correct JSON structures and reject invalid ones.
/// </summary>
public class EmploymentSchemaValidationTests
{
    private readonly ISchemaProvider _schemaProvider;
    private readonly ISchemaValidator _schemaValidator;

    public EmploymentSchemaValidationTests()
    {
        _schemaProvider = new SchemaProvider();
        _schemaValidator = new SchemaValidator();
    }

    #region Basic Schema Validation Tests

    [Fact]
    public void EmploymentBasicGeneral_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "General");
        var validJson = new JsonObject
        {
            ["arbeitgeber"] = "Musterfirma GmbH",
            ["arbeitnehmer"] = "Max Mustermann",
            ["position"] = "Software Developer",
            ["startdatum"] = "2024-01-15",
            ["vertragsart"] = "unbefristet",
            ["probezeit"] = "3 Monate"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Basic General JSON should pass validation");
    }

    [Fact]
    public void EmploymentBasicGeneral_MissingRequiredField_ShouldFailValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "General");
        var invalidJson = new JsonObject
        {
            ["arbeitgeber"] = "Musterfirma GmbH",
            ["arbeitnehmer"] = "Max Mustermann",
            // Missing required "position" field
            ["startdatum"] = "2024-01-15",
            ["vertragsart"] = "unbefristet",
            ["probezeit"] = "3 Monate"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, invalidJson);

        // Assert
        Assert.False(isValid, "JSON missing required field should fail validation");
    }

    [Fact]
    public void EmploymentBasicCompensation_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "Compensation");
        var validJson = new JsonObject
        {
            ["gehaltBrutto"] = "3500 EUR monatlich",
            ["gehaltNettoCa"] = "2450 EUR monatlich ca.",
            ["zahlungsweise"] = "monatlich",
            ["sonderzahlungen"] = "13. und 14. Gehalt",
            ["zuschlaege"] = "Überstundenzuschlag 50%",
            ["zusatzleistungen"] = new JsonArray("Firmenwagen", "Handy", "Essenszuschuss")
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Basic Compensation JSON should pass validation");
    }

    [Fact]
    public void EmploymentBasicWorkingConditions_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "WorkingConditions");
        var validJson = new JsonObject
        {
            ["wochenarbeitszeit"] = "40 Stunden",
            ["arbeitszeitmodell"] = "Vollzeit",
            ["ueberstundenregelung"] = "Zeitausgleich",
            ["urlaubstage"] = "25 Tage",
            ["arbeitsort"] = "Wien, Büro Hauptstraße 1",
            ["remoteArbeit"] = "bis zu 2 Tage/Woche"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Basic WorkingConditions JSON should pass validation");
    }

    [Fact]
    public void EmploymentBasicSummary_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "Summary");
        var validJson = new JsonObject
        {
            ["kurzfazit"] = "Standardarbeitsvertrag mit fairen Konditionen",
            ["kollektivvertrag"] = "IT-Kollektivvertrag",
            ["gesamtbewertung"] = "Gut",
            ["riskLevel"] = "Gruen"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Basic Summary JSON should pass validation");
    }

    [Fact]
    public void EmploymentBasicSummary_InvalidRiskLevel_ShouldFailValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Basic", "Summary");
        var invalidJson = new JsonObject
        {
            ["kurzfazit"] = "Standardarbeitsvertrag mit fairen Konditionen",
            ["kollektivvertrag"] = "IT-Kollektivvertrag",
            ["gesamtbewertung"] = "Gut",
            ["riskLevel"] = "Blue" // Invalid enum value
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, invalidJson);

        // Assert
        Assert.False(isValid, "JSON with invalid enum value should fail validation");
    }

    #endregion

    #region Premium Schema Validation Tests

    [Fact]
    public void EmploymentPremiumCompensationBenefits_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "CompensationBenefits");
        var validJson = new JsonObject
        {
            ["zusammenfassung"] = "Vergütung entspricht dem Kollektivvertrag",
            ["bewertung"] = "Die Vergütungsklauseln sind fair und transparent",
            ["rechtsgrundlage"] = "§ 1152 ABGB, IT-Kollektivvertrag",
            ["risikoScore"] = "Neutral",
            ["riskLevel"] = "Gruen"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Premium CompensationBenefits JSON should pass validation");
    }

    [Fact]
    public void EmploymentPremiumWorkingTimeVacation_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "WorkingTimeVacation");
        var validJson = new JsonObject
        {
            ["zusammenfassung"] = "Arbeitszeit und Urlaub entsprechen gesetzlichen Vorgaben",
            ["bewertung"] = "Keine problematischen Klauseln erkannt",
            ["rechtsgrundlage"] = "AZG, UrlG",
            ["risikoScore"] = "Vorteilhaft",
            ["riskLevel"] = "Gruen"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Premium WorkingTimeVacation JSON should pass validation");
    }

    [Fact]
    public void EmploymentPremiumTerminationNotice_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "TerminationNotice");
        var validJson = new JsonObject
        {
            ["zusammenfassung"] = "Kündigungsfristen entsprechen dem Gesetz",
            ["bewertung"] = "Standardklauseln ohne Auffälligkeiten",
            ["rechtsgrundlage"] = "§ 1159 ABGB, AngG",
            ["risikoScore"] = "Neutral",
            ["riskLevel"] = "Gruen"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Premium TerminationNotice JSON should pass validation");
    }

    [Fact]
    public void EmploymentPremiumNonCompeteConfidentiality_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "NonCompeteConfidentiality");
        var validJson = new JsonObject
        {
            ["zusammenfassung"] = "Wettbewerbsverbot zeitlich und räumlich angemessen",
            ["bewertung"] = "Vertraulichkeitsklauseln sind rechtlich zulässig",
            ["rechtsgrundlage"] = "§ 36 AngG",
            ["risikoScore"] = "Neutral",
            ["riskLevel"] = "Gelb"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Premium NonCompeteConfidentiality JSON should pass validation");
    }

    [Fact]
    public void EmploymentPremiumRightsDuties_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "RightsDuties");
        var validJson = new JsonObject
        {
            ["zusammenfassung"] = "Rechte und Pflichten sind ausgewogen",
            ["bewertung"] = "Keine einseitigen Benachteiligungen erkennbar",
            ["rechtsgrundlage"] = "§ 1151 ABGB",
            ["risikoScore"] = "Vorteilhaft",
            ["riskLevel"] = "Gruen"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Premium RightsDuties JSON should pass validation");
    }

    [Fact]
    public void EmploymentPremiumSummary_ValidJson_ShouldPassValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "Summary");
        var validJson = new JsonObject
        {
            ["gesamtfazit"] = "Der Arbeitsvertrag ist insgesamt fair gestaltet",
            ["gesamtbewertung"] = "Gut",
            ["handlungsempfehlungen"] = new JsonArray(
                "Keine dringenden Maßnahmen erforderlich",
                "Wettbewerbsverbot vor Unterzeichnung prüfen lassen"
            )
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, validJson);

        // Assert
        Assert.True(isValid, "Valid Employment Premium Summary JSON should pass validation");
    }

    [Fact]
    public void EmploymentPremiumClause_InvalidRisikoScore_ShouldFailValidation()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Employment", "Premium", "CompensationBenefits");
        var invalidJson = new JsonObject
        {
            ["zusammenfassung"] = "Vergütung entspricht dem Kollektivvertrag",
            ["bewertung"] = "Die Vergütungsklauseln sind fair und transparent",
            ["rechtsgrundlage"] = "§ 1152 ABGB, IT-Kollektivvertrag",
            ["risikoScore"] = "InvalidValue", // Invalid enum value
            ["riskLevel"] = "Gruen"
        };

        // Act
        var isValid = _schemaValidator.Validate(schema, invalidJson);

        // Assert
        Assert.False(isValid, "JSON with invalid risikoScore enum should fail validation");
    }

    #endregion

    #region Schema Structure Tests

    [Theory]
    [InlineData("Employment", "Basic", "General")]
    [InlineData("Employment", "Basic", "Compensation")]
    [InlineData("Employment", "Basic", "WorkingConditions")]
    [InlineData("Employment", "Basic", "Summary")]
    public void EmploymentBasicSchemas_ShouldDisallowAdditionalProperties(string contractType, string tier, string step)
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint(contractType, tier, step);

        // Assert
        Assert.True(schema.TryGetPropertyValue("additionalProperties", out var additionalPropsNode),
            "Schema should have additionalProperties property");
        Assert.False(additionalPropsNode?.GetValue<bool>() ?? true,
            "Schema should set additionalProperties to false");
    }

    [Theory]
    [InlineData("Employment", "Premium", "CompensationBenefits")]
    [InlineData("Employment", "Premium", "WorkingTimeVacation")]
    [InlineData("Employment", "Premium", "TerminationNotice")]
    [InlineData("Employment", "Premium", "NonCompeteConfidentiality")]
    [InlineData("Employment", "Premium", "RightsDuties")]
    [InlineData("Employment", "Premium", "Summary")]
    public void EmploymentPremiumSchemas_ShouldDisallowAdditionalProperties(string contractType, string tier, string step)
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint(contractType, tier, step);

        // Assert
        Assert.True(schema.TryGetPropertyValue("additionalProperties", out var additionalPropsNode),
            "Schema should have additionalProperties property");
        Assert.False(additionalPropsNode?.GetValue<bool>() ?? true,
            "Schema should set additionalProperties to false");
    }

    #endregion
}
