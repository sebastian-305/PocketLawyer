using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Tests for RentalContractConditionsSchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class RentalContractConditionsSchemaProviderTests
{
    private readonly RentalContractConditionsSchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Vertragsbedingungen",
        ["properties"] = new JsonObject
        {
            ["kuendigungsfrist"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kündigungsfrist für den Mietvertrag (z.B. '3 Monate zum Monatsende').",
                ["x-displayName"] = "Kündigungsfrist",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-semibold",
                    ["importance"] = "high",
                    ["icon"] = "clock"
                }
            },
            ["tierhaltung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zur Tierhaltung (erlaubt, verboten, mit Zustimmung).",
                ["x-displayName"] = "Tierhaltung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "pet"
                }
            },
            ["untervermietung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zur Untervermietung (erlaubt, verboten, mit Zustimmung).",
                ["x-displayName"] = "Untervermietung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "users"
                }
            },
            ["besonderheiten"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Weitere besondere Vertragsbedingungen oder Klauseln.",
                ["x-displayName"] = "Besonderheiten",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "info"
                }
            },
            ["schoenheitsreparaturen"] = new JsonObject
            {
                ["type"] = new JsonArray { "string", "null" },
                ["description"] = "Regelung zu Schönheitsreparaturen (Mieter, Vermieter, geteilt).",
                ["x-displayName"] = "Schönheitsreparaturen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["importance"] = "medium",
                    ["icon"] = "tool"
                }
            },
            ["betriebskostenabrechnung"] = new JsonObject
            {
                ["type"] = new JsonArray { "string", "null" },
                ["description"] = "Regelung zur Betriebskostenabrechnung (Frist, Umfang).",
                ["x-displayName"] = "Betriebskostenabrechnung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "document"
                }
            }
        },
        ["required"] = new JsonArray { "kuendigungsfrist", "tierhaltung", "untervermietung", "besonderheiten" }
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
        Assert.Equal("Vertragsbedingungen", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("kuendigungsfrist"));
        Assert.True(properties.ContainsKey("tierhaltung"));
        Assert.True(properties.ContainsKey("untervermietung"));
        Assert.True(properties.ContainsKey("besonderheiten"));
        Assert.True(properties.ContainsKey("schoenheitsreparaturen"));
        Assert.True(properties.ContainsKey("betriebskostenabrechnung"));
    }

    [Fact]
    public void GetSchema_AllPropertiesHaveCorrectTypes()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert - Required properties should be strings
        Assert.Equal("string", properties?["kuendigungsfrist"]?.AsObject()?["type"]?.ToString());
        Assert.Equal("string", properties?["tierhaltung"]?.AsObject()?["type"]?.ToString());
        Assert.Equal("string", properties?["untervermietung"]?.AsObject()?["type"]?.ToString());
        Assert.Equal("string", properties?["besonderheiten"]?.AsObject()?["type"]?.ToString());
        
        // Assert - Nullable properties should be ["string", "null"]
        var schoenheitsreparaturenType = properties?["schoenheitsreparaturen"]?.AsObject()?["type"]?.AsArray();
        Assert.NotNull(schoenheitsreparaturenType);
        Assert.Equal(2, schoenheitsreparaturenType.Count);
        Assert.Contains(schoenheitsreparaturenType, v => v?.ToString() == "string");
        Assert.Contains(schoenheitsreparaturenType, v => v?.ToString() == "null");
        
        var betriebskostenabrechnungType = properties?["betriebskostenabrechnung"]?.AsObject()?["type"]?.AsArray();
        Assert.NotNull(betriebskostenabrechnungType);
        Assert.Equal(2, betriebskostenabrechnungType.Count);
        Assert.Contains(betriebskostenabrechnungType, v => v?.ToString() == "string");
        Assert.Contains(betriebskostenabrechnungType, v => v?.ToString() == "null");
    }

    [Fact]
    public void GetSchema_RequiredFieldsAreCorrect()
    {
        // Act
        var schema = _provider.GetSchema();
        var required = schema["required"]?.AsArray();

        // Assert
        Assert.NotNull(required);
        Assert.Equal(4, required.Count);
        Assert.Contains(required, r => r?.ToString() == "kuendigungsfrist");
        Assert.Contains(required, r => r?.ToString() == "tierhaltung");
        Assert.Contains(required, r => r?.ToString() == "untervermietung");
        Assert.Contains(required, r => r?.ToString() == "besonderheiten");
    }

    [Fact]
    public void GetSchema_OptionalFieldsAreNotRequired()
    {
        // Act
        var schema = _provider.GetSchema();
        var required = schema["required"]?.AsArray();

        // Assert - schoenheitsreparaturen and betriebskostenabrechnung should NOT be in required
        Assert.DoesNotContain(required, r => r?.ToString() == "schoenheitsreparaturen");
        Assert.DoesNotContain(required, r => r?.ToString() == "betriebskostenabrechnung");
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
    public void GetSchema_KuendigungsfristHasHighImportance()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        var styling = properties?["kuendigungsfrist"]?.AsObject()?["x-styling"]?.AsObject();
        Assert.Equal("high", styling?["importance"]?.ToString());
    }

    [Fact]
    public void GetSchema_SchoenheitsreparaturenHasMediumImportance()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        var styling = properties?["schoenheitsreparaturen"]?.AsObject()?["x-styling"]?.AsObject();
        Assert.Equal("medium", styling?["importance"]?.ToString());
    }
}
