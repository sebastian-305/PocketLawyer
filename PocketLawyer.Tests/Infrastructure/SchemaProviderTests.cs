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
}
