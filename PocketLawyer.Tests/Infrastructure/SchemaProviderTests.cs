using PocketLawyer.Infrastructure.SchemaProviders;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Tests for SchemaProvider to verify x-sectionTitle is set correctly.
/// </summary>
public class SchemaProviderTests
{
    [Theory]
    [InlineData("Costs", "Mietzahlungs- und Nebenkostenklauseln")]
    [InlineData("Terms", "Mietdauer und Kündigungsklauseln")]
    [InlineData("Use", "Nutzungsklauseln")]
    [InlineData("Maintenance", "Instandhaltungs- und Reparaturklauseln")]
    [InlineData("Duties", "Pflichten und Obliegenheiten")]
    [InlineData("Privacy", "Datenschutz und Privatsphäre")]
    public void GetSchemaForEndpoint_PremiumSections_ShouldHaveCorrectSectionTitle(string step, string expectedTitle)
    {
        // Arrange
        var provider = new SchemaProvider();

        // Act
        var schema = provider.GetSchemaForEndpoint("Rental", "Premium", step);

        // Assert
        Assert.True(schema.TryGetPropertyValue("x-sectionTitle", out var titleNode), 
            $"Schema for {step} should have x-sectionTitle");
        Assert.Equal(expectedTitle, titleNode?.ToString());
    }

    [Fact]
    public void GetSchemaForEndpoint_PremiumSummary_ShouldHaveCorrectSectionTitle()
    {
        // Arrange
        var provider = new SchemaProvider();

        // Act
        var schema = provider.GetSchemaForEndpoint("Rental", "Premium", "Summary");

        // Assert
        Assert.True(schema.TryGetPropertyValue("x-sectionTitle", out var titleNode), 
            "Schema for Summary should have x-sectionTitle");
        Assert.Equal("Gesamtzusammenfassung", titleNode?.ToString());
    }

    [Theory]
    [InlineData("General")]
    [InlineData("Rent")]
    [InlineData("Conditions")]
    [InlineData("Summary")]
    public void GetSchemaForEndpoint_BasicSections_ShouldHaveSectionTitle(string step)
    {
        // Arrange
        var provider = new SchemaProvider();

        // Act
        var schema = provider.GetSchemaForEndpoint("Rental", "Basic", step);

        // Assert
        Assert.True(schema.TryGetPropertyValue("x-sectionTitle", out var titleNode), 
            $"Schema for {step} should have x-sectionTitle");
        Assert.NotNull(titleNode?.ToString());
        Assert.NotEmpty(titleNode?.ToString());
    }

    #region Employment Schema Tests

    [Theory]
    [InlineData("General", "Allgemeine Arbeitsdaten")]
    [InlineData("Compensation", "Vergütung und Zusatzleistungen")]
    [InlineData("WorkingConditions", "Arbeitsbedingungen")]
    [InlineData("Summary", "Zusammenfassung")]
    public void GetSchemaForEndpoint_EmploymentBasicSections_ShouldHaveCorrectSectionTitle(string step, string expectedTitle)
    {
        // Arrange
        var provider = new SchemaProvider();

        // Act
        var schema = provider.GetSchemaForEndpoint("Employment", "Basic", step);

        // Assert
        Assert.True(schema.TryGetPropertyValue("x-sectionTitle", out var titleNode), 
            $"Schema for {step} should have x-sectionTitle");
        Assert.Equal(expectedTitle, titleNode?.ToString());
    }

    [Theory]
    [InlineData("CompensationBenefits", "Vergütungs- und Zusatzleistungsklauseln")]
    [InlineData("WorkingTimeVacation", "Arbeitszeit- und Urlaubsklauseln")]
    [InlineData("TerminationNotice", "Kündigungs- und Kündigungsfristklauseln")]
    [InlineData("NonCompeteConfidentiality", "Wettbewerbs- und Vertraulichkeitsklauseln")]
    [InlineData("RightsDuties", "Rechte und Pflichten")]
    public void GetSchemaForEndpoint_EmploymentPremiumSections_ShouldHaveCorrectSectionTitle(string step, string expectedTitle)
    {
        // Arrange
        var provider = new SchemaProvider();

        // Act
        var schema = provider.GetSchemaForEndpoint("Employment", "Premium", step);

        // Assert
        Assert.True(schema.TryGetPropertyValue("x-sectionTitle", out var titleNode), 
            $"Schema for {step} should have x-sectionTitle");
        Assert.Equal(expectedTitle, titleNode?.ToString());
    }

    [Fact]
    public void GetSchemaForEndpoint_EmploymentPremiumSummary_ShouldHaveCorrectSectionTitle()
    {
        // Arrange
        var provider = new SchemaProvider();

        // Act
        var schema = provider.GetSchemaForEndpoint("Employment", "Premium", "Summary");

        // Assert
        Assert.True(schema.TryGetPropertyValue("x-sectionTitle", out var titleNode), 
            "Schema for Summary should have x-sectionTitle");
        Assert.Equal("Gesamtfazit", titleNode?.ToString());
    }

    [Theory]
    [InlineData("Employment", "Basic", "General")]
    [InlineData("Employment", "Basic", "Compensation")]
    [InlineData("Employment", "Basic", "WorkingConditions")]
    [InlineData("Employment", "Basic", "Summary")]
    public void GetSchemaForEndpoint_EmploymentBasicSchemas_ShouldHaveRequiredProperties(string contractType, string tier, string step)
    {
        // Arrange
        var provider = new SchemaProvider();

        // Act
        var schema = provider.GetSchemaForEndpoint(contractType, tier, step);

        // Assert
        Assert.True(schema.TryGetPropertyValue("type", out var typeNode), "Schema should have 'type' property");
        Assert.Equal("object", typeNode?.ToString());
        
        Assert.True(schema.TryGetPropertyValue("properties", out var propertiesNode), "Schema should have 'properties'");
        Assert.NotNull(propertiesNode);
        
        Assert.True(schema.TryGetPropertyValue("required", out var requiredNode), "Schema should have 'required' array");
        Assert.NotNull(requiredNode);
    }

    [Theory]
    [InlineData("Employment", "Premium", "CompensationBenefits")]
    [InlineData("Employment", "Premium", "WorkingTimeVacation")]
    [InlineData("Employment", "Premium", "TerminationNotice")]
    [InlineData("Employment", "Premium", "NonCompeteConfidentiality")]
    [InlineData("Employment", "Premium", "RightsDuties")]
    [InlineData("Employment", "Premium", "Summary")]
    public void GetSchemaForEndpoint_EmploymentPremiumSchemas_ShouldHaveRequiredProperties(string contractType, string tier, string step)
    {
        // Arrange
        var provider = new SchemaProvider();

        // Act
        var schema = provider.GetSchemaForEndpoint(contractType, tier, step);

        // Assert
        Assert.True(schema.TryGetPropertyValue("type", out var typeNode), "Schema should have 'type' property");
        Assert.Equal("object", typeNode?.ToString());
        
        Assert.True(schema.TryGetPropertyValue("properties", out var propertiesNode), "Schema should have 'properties'");
        Assert.NotNull(propertiesNode);
        
        Assert.True(schema.TryGetPropertyValue("required", out var requiredNode), "Schema should have 'required' array");
        Assert.NotNull(requiredNode);
    }

    #endregion
}
