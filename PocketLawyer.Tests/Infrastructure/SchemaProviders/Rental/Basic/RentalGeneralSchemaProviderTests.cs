using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Tests for RentalGeneralSchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class RentalGeneralSchemaProviderTests
{
    private readonly RentalGeneralSchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Allgemeine Vertragsdaten",
        ["properties"] = new JsonObject
        {
            ["vertragstyp"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Typ des Mietvertrags, z. B. 'I', 'II', 'III', 'WG'.",
                ["x-displayName"] = "Vertragstyp"
            },
            ["mieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Mieter:in.",
                ["x-displayName"] = "Mieter",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "user"
                }
            },
            ["vermieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Vermieter:in.",
                ["x-displayName"] = "Vermieter",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "user"
                }
            },
            ["mietobjektAdresse"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Vollständige Adresse des Mietobjekts.",
                ["x-displayName"] = "Mietobjekt Adresse",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "location"
                }
            },
            ["vertragsbeginn"] = new JsonObject
            {
                ["type"] = "string",
                ["format"] = "date",
                ["description"] = "Datum des Vertragsbeginns im Format YYYY-MM-DD.",
                ["x-displayName"] = "Vertragsbeginn",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "calendar"
                }
            },
            ["vertragsart"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "befristet", "unbefristet" },
                ["description"] = "Art des Vertrags: befristet oder unbefristet.",
                ["x-displayName"] = "Vertragsart",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge",
                    ["importance"] = "medium"
                }
            },
            ["vertragsende"] = new JsonObject
            {
                ["type"] = new JsonArray { "string", "null" },
                ["format"] = "date",
                ["description"] = "Datum des Vertragsendes im Format YYYY-MM-DD (nur bei befristeten Verträgen).",
                ["x-displayName"] = "Vertragsende",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "calendar"
                }
            }
        },
        ["required"] = new JsonArray { "vertragstyp", "mieter", "vermieter", "mietobjektAdresse", "vertragsbeginn", "vertragsart" }
    };

    [Fact]
    public void GetSchema_ReturnsValidJsonObject()
    {
        // Act
        var schema = _provider.GetSchema();

        // Assert
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_GeneratedSchemaMatchesManualSchema()
    {
        // Act
        var generatedSchema = _provider.GetSchema();

        // Assert - Compare with manual schema
        var result = _comparator.Compare(_manualSchema, generatedSchema);
        
        Assert.True(result.IsIdentical, 
            $"Generated schema does not match manual schema. Differences:\n{string.Join("\n", result.Differences)}");
    }

    [Fact]
    public void GetSchema_HasCorrectSectionTitle()
    {
        // Act
        var schema = _provider.GetSchema();

        // Assert
        Assert.Equal("Allgemeine Vertragsdaten", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("vertragstyp"));
        Assert.True(properties.ContainsKey("mieter"));
        Assert.True(properties.ContainsKey("vermieter"));
        Assert.True(properties.ContainsKey("mietobjektAdresse"));
        Assert.True(properties.ContainsKey("vertragsbeginn"));
        Assert.True(properties.ContainsKey("vertragsart"));
        Assert.True(properties.ContainsKey("vertragsende"));
    }

    [Fact]
    public void GetSchema_VertragsartHasEnumConstraint()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var vertragsart = properties?["vertragsart"]?.AsObject();
        var enumValues = vertragsart?["enum"]?.AsArray();

        // Assert
        Assert.NotNull(enumValues);
        Assert.Equal(2, enumValues.Count);
        Assert.Contains(enumValues, v => v?.ToString() == "befristet");
        Assert.Contains(enumValues, v => v?.ToString() == "unbefristet");
    }

    [Fact]
    public void GetSchema_VertragsendeIsNullable()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var vertragsende = properties?["vertragsende"]?.AsObject();
        var type = vertragsende?["type"]?.AsArray();

        // Assert
        Assert.NotNull(type);
        Assert.Equal(2, type.Count);
        Assert.Contains(type, v => v?.ToString() == "string");
        Assert.Contains(type, v => v?.ToString() == "null");
    }

    [Fact]
    public void GetSchema_AdditionalPropertiesIsFalse()
    {
        // Act
        var schema = _provider.GetSchema();

        // Assert
        Assert.False(schema["additionalProperties"]?.GetValue<bool>());
    }
}
