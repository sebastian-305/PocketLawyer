using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Rental.Premium;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Rental.Premium;

/// <summary>
/// Tests for RentalPremiumSummarySchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class RentalPremiumSummarySchemaProviderTests
{
    private readonly RentalPremiumSummarySchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Gesamtzusammenfassung",
        ["properties"] = new JsonObject
        {
            ["gesamtfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Gesamtfazit der Premium-Analyse mit Zusammenfassung aller Risiken und Empfehlungen.",
                ["x-displayName"] = "Gesamtfazit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-lg font-semibold",
                    ["importance"] = "high"
                }
            },
            ["gesamtbewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" },
                ["description"] = "Gesamtrisikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
                ["x-displayName"] = "Gesamtbewertung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "risk-indicator risk-indicator-lg",
                    ["importance"] = "high",
                    ["icon"] = "alert"
                }
            },
            ["handlungsempfehlungen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "string"
                },
                ["description"] = "Liste konkreter Handlungsempfehlungen für den Mieter.",
                ["x-displayName"] = "Handlungsempfehlungen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "list-disc ml-4",
                    ["importance"] = "high",
                    ["icon"] = "checklist"
                }
            }
        },
        ["required"] = new JsonArray { "gesamtfazit", "gesamtbewertung", "handlungsempfehlungen" }
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
        // **Property 1: Schema Equivalence After Migration**
        // **Validates: Requirements 2.2, 3.1-3.11**
        
        // Act
        var generatedSchema = _provider.GetSchema();
        var result = _comparator.Compare(_manualSchema, generatedSchema);

        // Assert
        Assert.True(result.IsIdentical, 
            $"Generated schema does not match manual schema. Differences:\n{string.Join("\n", result.Differences)}");
    }

    [Fact]
    public void GetSchema_HasCorrectSectionTitle()
    {
        // Act
        var schema = _provider.GetSchema();

        // Assert
        Assert.Equal("Gesamtzusammenfassung", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"] as JsonObject;

        // Assert
        Assert.NotNull(properties);
        Assert.Contains("gesamtfazit", properties.Select(p => p.Key));
        Assert.Contains("gesamtbewertung", properties.Select(p => p.Key));
        Assert.Contains("handlungsempfehlungen", properties.Select(p => p.Key));
    }

    [Fact]
    public void GetSchema_HasCorrectRequiredArray()
    {
        // Act
        var schema = _provider.GetSchema();
        var required = schema["required"] as JsonArray;

        // Assert
        Assert.NotNull(required);
        Assert.Equal(3, required.Count);
        Assert.Contains(required, r => r?.ToString() == "gesamtfazit");
        Assert.Contains(required, r => r?.ToString() == "gesamtbewertung");
        Assert.Contains(required, r => r?.ToString() == "handlungsempfehlungen");
    }

    [Fact]
    public void GetSchema_GesamtbewertungHasEnumValues()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"] as JsonObject;
        var gesamtbewertung = properties?["gesamtbewertung"] as JsonObject;
        var enumValues = gesamtbewertung?["enum"] as JsonArray;

        // Assert
        Assert.NotNull(enumValues);
        Assert.Equal(3, enumValues.Count);
        Assert.Contains(enumValues, e => e?.ToString() == "Gruen");
        Assert.Contains(enumValues, e => e?.ToString() == "Gelb");
        Assert.Contains(enumValues, e => e?.ToString() == "Rot");
    }

    [Fact]
    public void GetSchema_HandlungsempfehlungenIsArrayOfStrings()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"] as JsonObject;
        var handlungsempfehlungen = properties?["handlungsempfehlungen"] as JsonObject;
        var items = handlungsempfehlungen?["items"] as JsonObject;

        // Assert
        Assert.Equal("array", handlungsempfehlungen?["type"]?.ToString());
        Assert.NotNull(items);
        Assert.Equal("string", items["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_AllPropertiesHaveDisplayNames()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"] as JsonObject;

        // Assert
        Assert.NotNull(properties);
        foreach (var prop in properties)
        {
            var propSchema = prop.Value as JsonObject;
            Assert.NotNull(propSchema);
            Assert.NotNull(propSchema["x-displayName"]);
            Assert.False(string.IsNullOrEmpty(propSchema["x-displayName"]?.ToString()));
        }
    }

    [Fact]
    public void GetSchema_AllPropertiesHaveDescriptions()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"] as JsonObject;

        // Assert
        Assert.NotNull(properties);
        foreach (var prop in properties)
        {
            var propSchema = prop.Value as JsonObject;
            Assert.NotNull(propSchema);
            Assert.NotNull(propSchema["description"]);
            Assert.False(string.IsNullOrEmpty(propSchema["description"]?.ToString()));
        }
    }

    [Fact]
    public void GetSchema_AllPropertiesHaveStyling()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"] as JsonObject;

        // Assert
        Assert.NotNull(properties);
        foreach (var prop in properties)
        {
            var propSchema = prop.Value as JsonObject;
            Assert.NotNull(propSchema);
            var styling = propSchema["x-styling"] as JsonObject;
            Assert.NotNull(styling);
            Assert.NotNull(styling["displayClass"]);
            Assert.NotNull(styling["importance"]);
        }
    }

    [Fact]
    public void GetSchema_AdditionalPropertiesIsFalse()
    {
        // Act
        var schema = _provider.GetSchema();

        // Assert
        Assert.Equal(false, schema["additionalProperties"]?.GetValue<bool>());
    }

    [Fact]
    public void GetSchema_ReturnsNewInstanceEachTime()
    {
        // Act
        var schema1 = _provider.GetSchema();
        var schema2 = _provider.GetSchema();

        // Assert - Modifying one should not affect the other
        schema1["test"] = "value";
        Assert.Null(schema2["test"]);
    }
}
