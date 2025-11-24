using System.Text.Json;
using System.Text.Json.Nodes;
using PocketLawyer.Application.Interfaces;

namespace PocketLawyer.Tests.Integration;

/// <summary>
/// Integration tests for x-displayName functionality in JSON schemas.
/// Verifies that schemas contain display names and that they can be extracted by the frontend.
/// </summary>
public class SchemaDisplayNameIntegrationTests
{
    private readonly ISchemaProvider _schemaProvider;

    public SchemaDisplayNameIntegrationTests()
    {
        _schemaProvider = new PocketLawyer.Infrastructure.SchemaProviders.SchemaProvider();
    }

    #region Rental Basic Schemas

    [Fact]
    public void RentalGeneralSchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "General");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for all properties
        AssertPropertyHasDisplayName(properties, "mieter", "Mieter");
        AssertPropertyHasDisplayName(properties, "vermieter", "Vermieter");
        AssertPropertyHasDisplayName(properties, "mietobjektAdresse", "Mietobjekt Adresse");
        AssertPropertyHasDisplayName(properties, "vertragsbeginn", "Vertragsbeginn");
        AssertPropertyHasDisplayName(properties, "vertragsart", "Vertragsart");
        AssertPropertyHasDisplayName(properties, "vertragsende", "Vertragsende");
    }

    [Fact]
    public void RentalRentSchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Rent");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for key properties
        AssertPropertyHasDisplayName(properties, "kaltmiete", "Kaltmiete");
        AssertPropertyHasDisplayName(properties, "nebenkosten", "Nebenkosten");
        AssertPropertyHasDisplayName(properties, "warmmiete", "Warmmiete");
        AssertPropertyHasDisplayName(properties, "kaution", "Kaution");
    }

    [Fact]
    public void RentalConditionsSchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Conditions");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for key properties
        AssertPropertyHasDisplayName(properties, "kuendigungsfrist", "Kündigungsfrist");
        AssertPropertyHasDisplayName(properties, "tierhaltung", "Tierhaltung");
        AssertPropertyHasDisplayName(properties, "untervermietung", "Untervermietung");
    }

    [Fact]
    public void RentalBasicSummarySchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Summary");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for summary properties
        AssertPropertyHasDisplayName(properties, "kurzfazit", "Kurzfazit");
    }

    #endregion

    #region Rental Premium Schemas

    [Fact]
    public void RentalPremiumCostsSchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Costs");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for analysis properties
        AssertPropertyHasDisplayName(properties, "zusammenfassung", "Zusammenfassung");
        AssertPropertyHasDisplayName(properties, "bewertung", "Bewertung");
        AssertPropertyHasDisplayName(properties, "rechtsgrundlage", "Rechtsgrundlage");
    }

    [Fact]
    public void RentalPremiumTermsSchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Terms");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for analysis properties
        AssertPropertyHasDisplayName(properties, "zusammenfassung", "Zusammenfassung");
        AssertPropertyHasDisplayName(properties, "bewertung", "Bewertung");
    }

    [Fact]
    public void RentalPremiumUseSchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Use");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for analysis properties
        AssertPropertyHasDisplayName(properties, "zusammenfassung", "Zusammenfassung");
        AssertPropertyHasDisplayName(properties, "bewertung", "Bewertung");
    }

    [Fact]
    public void RentalPremiumMaintenanceSchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Maintenance");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for analysis properties
        AssertPropertyHasDisplayName(properties, "zusammenfassung", "Zusammenfassung");
        AssertPropertyHasDisplayName(properties, "bewertung", "Bewertung");
    }

    [Fact]
    public void RentalPremiumDutiesSchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Duties");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for analysis properties
        AssertPropertyHasDisplayName(properties, "zusammenfassung", "Zusammenfassung");
        AssertPropertyHasDisplayName(properties, "bewertung", "Bewertung");
    }

    [Fact]
    public void RentalPremiumPrivacySchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Privacy");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for analysis properties
        AssertPropertyHasDisplayName(properties, "zusammenfassung", "Zusammenfassung");
        AssertPropertyHasDisplayName(properties, "bewertung", "Bewertung");
    }

    [Fact]
    public void RentalPremiumSummarySchema_ContainsDisplayNames()
    {
        // Act
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Summary");

        // Assert
        Assert.NotNull(schema);
        var properties = schema["properties"]?.AsObject();
        Assert.NotNull(properties);

        // Verify x-displayName exists for summary properties
        AssertPropertyHasDisplayName(properties, "gesamtfazit", "Gesamtfazit");
        AssertPropertyHasDisplayName(properties, "gesamtbewertung", "Gesamtbewertung");
    }

    #endregion

    #region Section Titles

    [Fact]
    public void RentalBasicSchemas_ContainSectionTitles()
    {
        // Act & Assert
        var generalSchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "General");
        Assert.Equal("Allgemeine Vertragsdaten", generalSchema["x-sectionTitle"]?.GetValue<string>());

        var rentSchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Rent");
        Assert.Equal("Mietkonditionen", rentSchema["x-sectionTitle"]?.GetValue<string>());

        var conditionsSchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Conditions");
        Assert.Equal("Vertragsbedingungen", conditionsSchema["x-sectionTitle"]?.GetValue<string>());

        var summarySchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "Summary");
        Assert.Equal("Zusammenfassung", summarySchema["x-sectionTitle"]?.GetValue<string>());
    }

    [Fact]
    public void RentalPremiumSchemas_ContainSectionTitles()
    {
        // Act & Assert
        var costsSchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Costs");
        Assert.Equal("Mietzahlungs- und Nebenkostenklauseln", costsSchema["x-sectionTitle"]?.GetValue<string>());

        var termsSchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Terms");
        Assert.Equal("Mietdauer und Kündigungsklauseln", termsSchema["x-sectionTitle"]?.GetValue<string>());

        var useSchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Use");
        Assert.Equal("Nutzungsklauseln", useSchema["x-sectionTitle"]?.GetValue<string>());

        var maintenanceSchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Maintenance");
        Assert.Equal("Instandhaltungs- und Reparaturklauseln", maintenanceSchema["x-sectionTitle"]?.GetValue<string>());

        var dutiesSchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Duties");
        Assert.Equal("Pflichten und Obliegenheiten", dutiesSchema["x-sectionTitle"]?.GetValue<string>());

        var privacySchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Privacy");
        Assert.Equal("Datenschutz und Privatsphäre", privacySchema["x-sectionTitle"]?.GetValue<string>());

        var summarySchema = _schemaProvider.GetSchemaForEndpoint("Rental", "Premium", "Summary");
        Assert.Equal("Gesamtzusammenfassung", summarySchema["x-sectionTitle"]?.GetValue<string>());
    }

    #endregion

    #region Frontend Extraction Tests

    [Fact]
    public void Frontend_CanExtractDisplayNameFromSchema()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "General");
        var schemaJson = schema.ToJsonString();

        // Act - Simulate frontend parsing
        var parsedSchema = JsonNode.Parse(schemaJson);
        var properties = parsedSchema?["properties"]?.AsObject();
        var mieterProperty = properties?["mieter"]?.AsObject();
        var displayName = mieterProperty?["x-displayName"]?.GetValue<string>();

        // Assert
        Assert.NotNull(displayName);
        Assert.Equal("Mieter", displayName);
    }

    [Fact]
    public void Frontend_CanExtractSectionTitleFromSchema()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "General");
        var schemaJson = schema.ToJsonString();

        // Act - Simulate frontend parsing
        var parsedSchema = JsonNode.Parse(schemaJson);
        var sectionTitle = parsedSchema?["x-sectionTitle"]?.GetValue<string>();

        // Assert
        Assert.NotNull(sectionTitle);
        Assert.Equal("Allgemeine Vertragsdaten", sectionTitle);
    }

    [Fact]
    public void Frontend_CanIterateAllPropertiesAndExtractDisplayNames()
    {
        // Arrange
        var schema = _schemaProvider.GetSchemaForEndpoint("Rental", "Basic", "General");
        var schemaJson = schema.ToJsonString();
        var parsedSchema = JsonNode.Parse(schemaJson);
        var properties = parsedSchema?["properties"]?.AsObject();

        // Act - Simulate frontend iteration
        var displayNames = new Dictionary<string, string>();
        if (properties != null)
        {
            foreach (var property in properties)
            {
                var propertyObj = property.Value?.AsObject();
                var displayName = propertyObj?["x-displayName"]?.GetValue<string>();
                if (displayName != null)
                {
                    displayNames[property.Key] = displayName;
                }
            }
        }

        // Assert
        Assert.NotEmpty(displayNames);
        Assert.True(displayNames.Count >= 5); // At least 5 properties with display names
        Assert.Contains("mieter", displayNames.Keys);
        Assert.Contains("vermieter", displayNames.Keys);
        Assert.Equal("Mieter", displayNames["mieter"]);
        Assert.Equal("Vermieter", displayNames["vermieter"]);
    }

    [Fact]
    public void AllRentalSchemas_HaveDisplayNamesForAllProperties()
    {
        // Arrange
        var schemaKeys = new[]
        {
            ("Rental", "Basic", "General"),
            ("Rental", "Basic", "Rent"),
            ("Rental", "Basic", "Conditions"),
            ("Rental", "Basic", "Summary"),
            ("Rental", "Premium", "Costs"),
            ("Rental", "Premium", "Terms"),
            ("Rental", "Premium", "Use"),
            ("Rental", "Premium", "Maintenance"),
            ("Rental", "Premium", "Duties"),
            ("Rental", "Premium", "Privacy"),
            ("Rental", "Premium", "Summary")
        };

        // Act & Assert
        foreach (var (contractType, tier, step) in schemaKeys)
        {
            var schema = _schemaProvider.GetSchemaForEndpoint(contractType, tier, step);
            Assert.NotNull(schema);

            var properties = schema["properties"]?.AsObject();
            Assert.NotNull(properties);

            // Verify all properties have x-displayName
            foreach (var property in properties)
            {
                var propertyObj = property.Value?.AsObject();
                Assert.NotNull(propertyObj);

                // Check if it's a nested object (like Summary)
                var typeNode = propertyObj["type"];
                var isObject = false;
                
                if (typeNode != null)
                {
                    // Type can be a string or an array (e.g., ["string", "null"])
                    if (typeNode is System.Text.Json.Nodes.JsonValue)
                    {
                        isObject = typeNode.GetValue<string>() == "object";
                    }
                }
                
                if (isObject)
                {
                    var nestedProperties = propertyObj["properties"]?.AsObject();
                    if (nestedProperties != null)
                    {
                        foreach (var nestedProperty in nestedProperties)
                        {
                            var nestedPropertyObj = nestedProperty.Value?.AsObject();
                            Assert.NotNull(nestedPropertyObj);
                            Assert.True(
                                nestedPropertyObj.ContainsKey("x-displayName"),
                                $"Property '{property.Key}.{nestedProperty.Key}' in schema {contractType}/{tier}/{step} is missing x-displayName"
                            );
                        }
                    }
                }
                else
                {
                    Assert.True(
                        propertyObj.ContainsKey("x-displayName"),
                        $"Property '{property.Key}' in schema {contractType}/{tier}/{step} is missing x-displayName"
                    );
                }
            }
        }
    }

    #endregion

    #region Helper Methods

    private void AssertPropertyHasDisplayName(JsonObject properties, string propertyName, string expectedDisplayName)
    {
        Assert.True(properties.ContainsKey(propertyName), $"Property '{propertyName}' not found in schema");
        
        var property = properties[propertyName]?.AsObject();
        Assert.NotNull(property);
        
        Assert.True(property.ContainsKey("x-displayName"), $"Property '{propertyName}' is missing x-displayName");
        
        var displayName = property["x-displayName"]?.GetValue<string>();
        Assert.Equal(expectedDisplayName, displayName);
    }

    #endregion
}
