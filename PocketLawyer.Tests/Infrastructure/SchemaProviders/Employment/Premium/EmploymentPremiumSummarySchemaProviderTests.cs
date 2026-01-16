using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Employment.Premium;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Employment.Premium;

/// <summary>
/// Tests for EmploymentPremiumSummarySchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class EmploymentPremiumSummarySchemaProviderTests
{
    private readonly EmploymentPremiumSummarySchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Gesamtfazit",
        ["properties"] = new JsonObject
        {
            ["gesamtfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Ausführliches Gesamtfazit zur rechtlichen Bewertung des Arbeitsvertrags unter Berücksichtigung aller analysierten Bereiche und des Kollektivvertrags.",
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
                ["description"] = "Gesamtbewertung des Vertrags (z.B. 'Sehr gut', 'Gut', 'Akzeptabel', 'Problematisch', 'Kritisch').",
                ["x-displayName"] = "Gesamtbewertung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge badge-lg",
                    ["importance"] = "high"
                }
            },
            ["handlungsempfehlungen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "string"
                },
                ["description"] = "Liste konkreter Handlungsempfehlungen für den Arbeitnehmer (z.B. 'Nachverhandlung der Kündigungsfrist empfohlen', 'Wettbewerbsverbot prüfen lassen').",
                ["x-displayName"] = "Handlungsempfehlungen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "list-disc",
                    ["importance"] = "high",
                    ["icon"] = "lightbulb"
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
        Assert.Equal("Gesamtfazit", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("gesamtfazit"));
        Assert.True(properties.ContainsKey("gesamtbewertung"));
        Assert.True(properties.ContainsKey("handlungsempfehlungen"));
    }

    [Fact]
    public void GetSchema_AllPropertiesAreRequired()
    {
        // Act
        var schema = _provider.GetSchema();
        var required = schema["required"]?.AsArray();

        // Assert
        Assert.NotNull(required);
        Assert.Equal(3, required.Count);
        Assert.Contains(required, v => v?.ToString() == "gesamtfazit");
        Assert.Contains(required, v => v?.ToString() == "gesamtbewertung");
        Assert.Contains(required, v => v?.ToString() == "handlungsempfehlungen");
    }

    [Fact]
    public void GetSchema_AdditionalPropertiesIsFalse()
    {
        // Act
        var schema = _provider.GetSchema();

        // Assert
        Assert.False(schema["additionalProperties"]?.GetValue<bool>());
    }

    [Fact]
    public void GetSchema_GesamtfazitHasCorrectStyling()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var gesamtfazit = properties?["gesamtfazit"]?.AsObject();
        var styling = gesamtfazit?["x-styling"]?.AsObject();

        // Assert
        Assert.NotNull(styling);
        Assert.Equal("text-lg font-semibold", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GetSchema_GesamtbewertungHasCorrectStyling()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var gesamtbewertung = properties?["gesamtbewertung"]?.AsObject();
        var styling = gesamtbewertung?["x-styling"]?.AsObject();

        // Assert
        Assert.NotNull(styling);
        Assert.Equal("badge badge-lg", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GetSchema_HandlungsempfehlungenIsArrayType()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var handlungsempfehlungen = properties?["handlungsempfehlungen"]?.AsObject();

        // Assert
        Assert.NotNull(handlungsempfehlungen);
        Assert.Equal("array", handlungsempfehlungen["type"]?.ToString());
        
        var items = handlungsempfehlungen["items"]?.AsObject();
        Assert.NotNull(items);
        Assert.Equal("string", items["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_HandlungsempfehlungenHasIconAndStyling()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var handlungsempfehlungen = properties?["handlungsempfehlungen"]?.AsObject();
        var styling = handlungsempfehlungen?["x-styling"]?.AsObject();

        // Assert
        Assert.NotNull(styling);
        Assert.Equal("lightbulb", styling["icon"]?.ToString());
        Assert.Equal("list-disc", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GetSchema_AllPropertiesHaveDescriptions()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        
        var gesamtfazit = properties["gesamtfazit"]?.AsObject();
        Assert.NotNull(gesamtfazit?["description"]);
        Assert.Contains("rechtlichen Bewertung", gesamtfazit["description"]?.ToString());
        
        var gesamtbewertung = properties["gesamtbewertung"]?.AsObject();
        Assert.NotNull(gesamtbewertung?["description"]);
        Assert.Contains("Gesamtbewertung des Vertrags", gesamtbewertung["description"]?.ToString());
        
        var handlungsempfehlungen = properties["handlungsempfehlungen"]?.AsObject();
        Assert.NotNull(handlungsempfehlungen?["description"]);
        Assert.Contains("konkreter Handlungsempfehlungen", handlungsempfehlungen["description"]?.ToString());
    }

    [Fact]
    public void GetSchema_AllPropertiesHaveDisplayNames()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        
        var gesamtfazit = properties["gesamtfazit"]?.AsObject();
        Assert.Equal("Gesamtfazit", gesamtfazit?["x-displayName"]?.ToString());
        
        var gesamtbewertung = properties["gesamtbewertung"]?.AsObject();
        Assert.Equal("Gesamtbewertung", gesamtbewertung?["x-displayName"]?.ToString());
        
        var handlungsempfehlungen = properties["handlungsempfehlungen"]?.AsObject();
        Assert.Equal("Handlungsempfehlungen", handlungsempfehlungen?["x-displayName"]?.ToString());
    }
}
