using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Tests for EmploymentGeneralSchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class EmploymentGeneralSchemaProviderTests
{
    private readonly EmploymentGeneralSchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Allgemeine Arbeitsdaten",
        ["properties"] = new JsonObject
        {
            ["arbeitgeber"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des Arbeitgebers.",
                ["x-displayName"] = "Arbeitgeber",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "building"
                }
            },
            ["arbeitnehmer"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Arbeitnehmer:in.",
                ["x-displayName"] = "Arbeitnehmer",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "user"
                }
            },
            ["position"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Bezeichnung der Position/Tätigkeit.",
                ["x-displayName"] = "Position",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "briefcase"
                }
            },
            ["startdatum"] = new JsonObject
            {
                ["type"] = "string",
                ["format"] = "date",
                ["description"] = "Datum des Arbeitsbeginns im Format YYYY-MM-DD.",
                ["x-displayName"] = "Startdatum",
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
            ["probezeit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Dauer der Probezeit (z.B. '1 Monat', '3 Monate', 'keine').",
                ["x-displayName"] = "Probezeit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "clock"
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
        ["required"] = new JsonArray { "arbeitgeber", "arbeitnehmer", "position", "startdatum", "vertragsart", "probezeit" }
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
        Assert.Equal("Allgemeine Arbeitsdaten", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("arbeitgeber"));
        Assert.True(properties.ContainsKey("arbeitnehmer"));
        Assert.True(properties.ContainsKey("position"));
        Assert.True(properties.ContainsKey("startdatum"));
        Assert.True(properties.ContainsKey("vertragsart"));
        Assert.True(properties.ContainsKey("probezeit"));
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
