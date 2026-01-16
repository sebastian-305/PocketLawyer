using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Tests for EmploymentBasicSummarySchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class EmploymentBasicSummarySchemaProviderTests
{
    private readonly EmploymentBasicSummarySchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Zusammenfassung",
        ["properties"] = new JsonObject
        {
            ["kurzfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurze Zusammenfassung der wichtigsten Vertragspunkte (2-3 Sätze).",
                ["x-displayName"] = "Kurzfazit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-lg font-semibold",
                    ["importance"] = "high"
                }
            },
            ["kollektivvertrag"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Erwähnung des erkannten Kollektivvertrags oder 'Kein Kollektivvertrag erkannt'.",
                ["x-displayName"] = "Kollektivvertrag",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge badge-info",
                    ["icon"] = "document",
                    ["importance"] = "high"
                }
            },
            ["gesamtbewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Gesamtbewertung des Vertrags (z.B. 'Gut', 'Akzeptabel', 'Problematisch').",
                ["x-displayName"] = "Gesamtbewertung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge badge-lg",
                    ["importance"] = "high"
                }
            },
            ["riskLevel"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" },
                ["description"] = "Risikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
                ["x-displayName"] = "Risikostufe",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "risk-indicator",
                    ["importance"] = "high",
                    ["icon"] = "alert"
                }
            }
        },
        ["required"] = new JsonArray { "kurzfazit", "kollektivvertrag", "gesamtbewertung", "riskLevel" }
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
        Assert.Equal("Zusammenfassung", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("kurzfazit"));
        Assert.True(properties.ContainsKey("kollektivvertrag"));
        Assert.True(properties.ContainsKey("gesamtbewertung"));
        Assert.True(properties.ContainsKey("riskLevel"));
    }

    [Fact]
    public void GetSchema_RiskLevelHasEnumConstraint()
    {
        // Verify enum handling for riskLevel property
        
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var riskLevel = properties?["riskLevel"]?.AsObject();
        var enumValues = riskLevel?["enum"]?.AsArray();

        // Assert
        Assert.NotNull(enumValues);
        Assert.Equal(3, enumValues.Count);
        Assert.Contains(enumValues, v => v?.ToString() == "Gruen");
        Assert.Contains(enumValues, v => v?.ToString() == "Gelb");
        Assert.Contains(enumValues, v => v?.ToString() == "Rot");
    }

    [Fact]
    public void GetSchema_AllPropertiesAreRequired()
    {
        // Act
        var schema = _provider.GetSchema();
        var required = schema["required"]?.AsArray();

        // Assert
        Assert.NotNull(required);
        Assert.Equal(4, required.Count);
        Assert.Contains(required, v => v?.ToString() == "kurzfazit");
        Assert.Contains(required, v => v?.ToString() == "kollektivvertrag");
        Assert.Contains(required, v => v?.ToString() == "gesamtbewertung");
        Assert.Contains(required, v => v?.ToString() == "riskLevel");
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
    public void GetSchema_KurzfazitHasCorrectStyling()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var kurzfazit = properties?["kurzfazit"]?.AsObject();
        var styling = kurzfazit?["x-styling"]?.AsObject();

        // Assert
        Assert.NotNull(styling);
        Assert.Equal("text-lg font-semibold", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GetSchema_KollektivvertragHasIconAndStyling()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var kollektivvertrag = properties?["kollektivvertrag"]?.AsObject();
        var styling = kollektivvertrag?["x-styling"]?.AsObject();

        // Assert
        Assert.NotNull(styling);
        Assert.Equal("document", styling["icon"]?.ToString());
        Assert.Equal("badge badge-info", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GetSchema_RiskLevelHasAlertIconAndStyling()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var riskLevel = properties?["riskLevel"]?.AsObject();
        var styling = riskLevel?["x-styling"]?.AsObject();

        // Assert
        Assert.NotNull(styling);
        Assert.Equal("alert", styling["icon"]?.ToString());
        Assert.Equal("risk-indicator", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }
}
