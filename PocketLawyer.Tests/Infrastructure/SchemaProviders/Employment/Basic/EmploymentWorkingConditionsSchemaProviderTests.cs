using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Tests for EmploymentWorkingConditionsSchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class EmploymentWorkingConditionsSchemaProviderTests
{
    private readonly EmploymentWorkingConditionsSchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Arbeitsbedingungen",
        ["properties"] = new JsonObject
        {
            ["wochenarbeitszeit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Wöchentliche Arbeitszeit (z.B. '40 Stunden', '38,5 Stunden').",
                ["x-displayName"] = "Wochenarbeitszeit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "clock",
                    ["importance"] = "high"
                }
            },
            ["arbeitszeitmodell"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Arbeitszeitmodell (z.B. 'Vollzeit', 'Teilzeit 30h', 'Gleitzeit').",
                ["x-displayName"] = "Arbeitszeitmodell",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge",
                    ["importance"] = "medium"
                }
            },
            ["ueberstundenregelung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zu Überstunden (z.B. 'mit Gehalt abgegolten', 'Zeitausgleich', 'Auszahlung').",
                ["x-displayName"] = "Überstundenregelung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "clock",
                    ["importance"] = "medium"
                }
            },
            ["urlaubstage"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Anzahl der Urlaubstage pro Jahr (z.B. '25 Tage', '30 Tage').",
                ["x-displayName"] = "Urlaubstage",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "sun",
                    ["importance"] = "medium"
                }
            },
            ["arbeitsort"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Arbeitsort/Standort (z.B. 'Wien, Büro Hauptstraße 1', 'flexibel').",
                ["x-displayName"] = "Arbeitsort",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "location"
                }
            },
            ["remoteArbeit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zu Remote-Arbeit/Homeoffice (z.B. 'bis zu 2 Tage/Woche', 'nicht vorgesehen', '100% remote').",
                ["x-displayName"] = "Remote-Arbeit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge",
                    ["icon"] = "home"
                }
            }
        },
        ["required"] = new JsonArray { "wochenarbeitszeit", "arbeitszeitmodell", "ueberstundenregelung", "urlaubstage", "arbeitsort", "remoteArbeit" }
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

    /// <summary>
    /// Property 1: Schema Equivalence After Migration
    /// Validates: Requirements 2.2, 3.1-3.11
    /// </summary>
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
        Assert.Equal("Arbeitsbedingungen", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("wochenarbeitszeit"));
        Assert.True(properties.ContainsKey("arbeitszeitmodell"));
        Assert.True(properties.ContainsKey("ueberstundenregelung"));
        Assert.True(properties.ContainsKey("urlaubstage"));
        Assert.True(properties.ContainsKey("arbeitsort"));
        Assert.True(properties.ContainsKey("remoteArbeit"));
    }

    [Fact]
    public void GetSchema_WochenarbeitszeitHasCorrectMetadata()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var wochenarbeitszeit = properties?["wochenarbeitszeit"]?.AsObject();
        var styling = wochenarbeitszeit?["x-styling"]?.AsObject();

        // Assert
        Assert.NotNull(wochenarbeitszeit);
        Assert.Equal("Wochenarbeitszeit", wochenarbeitszeit["x-displayName"]?.ToString());
        Assert.Equal("clock", styling?["icon"]?.ToString());
        Assert.Equal("text-primary", styling?["displayClass"]?.ToString());
        Assert.Equal("high", styling?["importance"]?.ToString());
    }

    [Fact]
    public void GetSchema_AllPropertiesHaveCorrectIcons()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert - Verify icons where specified
        var wochenarbeitszeit = properties?["wochenarbeitszeit"]?.AsObject();
        Assert.Equal("clock", wochenarbeitszeit?["x-styling"]?.AsObject()?["icon"]?.ToString());

        var ueberstundenregelung = properties?["ueberstundenregelung"]?.AsObject();
        Assert.Equal("clock", ueberstundenregelung?["x-styling"]?.AsObject()?["icon"]?.ToString());

        var urlaubstage = properties?["urlaubstage"]?.AsObject();
        Assert.Equal("sun", urlaubstage?["x-styling"]?.AsObject()?["icon"]?.ToString());

        var arbeitsort = properties?["arbeitsort"]?.AsObject();
        Assert.Equal("location", arbeitsort?["x-styling"]?.AsObject()?["icon"]?.ToString());

        var remoteArbeit = properties?["remoteArbeit"]?.AsObject();
        Assert.Equal("home", remoteArbeit?["x-styling"]?.AsObject()?["icon"]?.ToString());
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
