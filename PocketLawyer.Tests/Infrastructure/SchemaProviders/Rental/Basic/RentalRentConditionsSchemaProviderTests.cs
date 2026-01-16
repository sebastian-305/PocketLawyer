using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Tests for RentalRentConditionsSchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class RentalRentConditionsSchemaProviderTests
{
    private readonly RentalRentConditionsSchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Mietkonditionen",
        ["properties"] = new JsonObject
        {
            ["kaltmiete"] = new JsonObject
            {
                ["type"] = "number",
                ["description"] = "Monatliche Kaltmiete in Euro.",
                ["x-displayName"] = "Kaltmiete",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-bold text-lg",
                    ["importance"] = "high",
                    ["icon"] = "currency",
                    ["format"] = "currency"
                }
            },
            ["nebenkosten"] = new JsonObject
            {
                ["type"] = "number",
                ["description"] = "Monatliche Nebenkosten in Euro.",
                ["x-displayName"] = "Nebenkosten",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "currency",
                    ["format"] = "currency"
                }
            },
            ["warmmiete"] = new JsonObject
            {
                ["type"] = "number",
                ["description"] = "Monatliche Warmmiete (Kaltmiete + Nebenkosten) in Euro.",
                ["x-displayName"] = "Warmmiete",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-bold text-primary",
                    ["importance"] = "high",
                    ["icon"] = "currency",
                    ["format"] = "currency"
                }
            },
            ["kaution"] = new JsonObject
            {
                ["type"] = "number",
                ["description"] = "Höhe der Kaution in Euro.",
                ["x-displayName"] = "Kaution",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-warning",
                    ["importance"] = "medium",
                    ["icon"] = "currency",
                    ["format"] = "currency"
                }
            },
            ["zahlungsweise"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zahlungsweise der Miete (z.B. monatlich, vierteljährlich).",
                ["x-displayName"] = "Zahlungsweise",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "calendar"
                }
            },
            ["nebenkostenregelung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Beschreibung der Nebenkostenregelung (Pauschale, Vorauszahlung mit Abrechnung, etc.).",
                ["x-displayName"] = "Nebenkostenregelung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["importance"] = "medium"
                }
            }
        },
        ["required"] = new JsonArray { "kaltmiete", "nebenkosten", "warmmiete", "kaution", "zahlungsweise", "nebenkostenregelung" }
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
        Assert.Equal("Mietkonditionen", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("kaltmiete"));
        Assert.True(properties.ContainsKey("nebenkosten"));
        Assert.True(properties.ContainsKey("warmmiete"));
        Assert.True(properties.ContainsKey("kaution"));
        Assert.True(properties.ContainsKey("zahlungsweise"));
        Assert.True(properties.ContainsKey("nebenkostenregelung"));
    }

    [Fact]
    public void GetSchema_NumericPropertiesHaveCorrectType()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.Equal("number", properties?["kaltmiete"]?.AsObject()?["type"]?.ToString());
        Assert.Equal("number", properties?["nebenkosten"]?.AsObject()?["type"]?.ToString());
        Assert.Equal("number", properties?["warmmiete"]?.AsObject()?["type"]?.ToString());
        Assert.Equal("number", properties?["kaution"]?.AsObject()?["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_StringPropertiesHaveCorrectType()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.Equal("string", properties?["zahlungsweise"]?.AsObject()?["type"]?.ToString());
        Assert.Equal("string", properties?["nebenkostenregelung"]?.AsObject()?["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_CurrencyPropertiesHaveStylingFormat()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert - Check that currency properties have format in x-styling
        var kaltmieteStyling = properties?["kaltmiete"]?.AsObject()?["x-styling"]?.AsObject();
        Assert.Equal("currency", kaltmieteStyling?["format"]?.ToString());

        var nebenkostenStyling = properties?["nebenkosten"]?.AsObject()?["x-styling"]?.AsObject();
        Assert.Equal("currency", nebenkostenStyling?["format"]?.ToString());

        var warmmieteStyling = properties?["warmmiete"]?.AsObject()?["x-styling"]?.AsObject();
        Assert.Equal("currency", warmmieteStyling?["format"]?.ToString());

        var kautionStyling = properties?["kaution"]?.AsObject()?["x-styling"]?.AsObject();
        Assert.Equal("currency", kautionStyling?["format"]?.ToString());
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
