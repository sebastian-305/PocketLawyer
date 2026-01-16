using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Tests for EmploymentCompensationSchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema.
/// </summary>
public class EmploymentCompensationSchemaProviderTests
{
    private readonly EmploymentCompensationSchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual schema for comparison.
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Vergütung und Zusatzleistungen",
        ["properties"] = new JsonObject
        {
            ["gehaltBrutto"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Bruttogehalt (z.B. '3.500 EUR monatlich').",
                ["x-displayName"] = "Bruttogehalt",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary font-semibold",
                    ["icon"] = "currency",
                    ["importance"] = "high"
                }
            },
            ["gehaltNettoCa"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Geschätztes Nettogehalt (z.B. '2.450 EUR monatlich ca.').",
                ["x-displayName"] = "Nettogehalt (ca.)",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "currency"
                }
            },
            ["zahlungsweise"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zahlungsweise des Gehalts (z.B. 'monatlich', '14x jährlich').",
                ["x-displayName"] = "Zahlungsweise",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "calendar"
                }
            },
            ["sonderzahlungen"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Sonderzahlungen wie 13./14. Gehalt, Weihnachtsgeld, Urlaubsgeld.",
                ["x-displayName"] = "Sonderzahlungen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "gift",
                    ["importance"] = "medium"
                }
            },
            ["zuschlaege"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zuschläge für Überstunden, Nachtarbeit, Wochenendarbeit etc.",
                ["x-displayName"] = "Zuschläge",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "plus"
                }
            },
            ["zusatzleistungen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "string"
                },
                ["description"] = "Zusatzleistungen wie Firmenwagen, Handy, Essenszuschuss, Versicherungen etc.",
                ["x-displayName"] = "Zusatzleistungen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge-list",
                    ["icon"] = "star"
                }
            }
        },
        ["required"] = new JsonArray { "gehaltBrutto", "gehaltNettoCa", "zahlungsweise", "sonderzahlungen", "zuschlaege", "zusatzleistungen" }
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
        Assert.Equal("Vergütung und Zusatzleistungen", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("gehaltBrutto"));
        Assert.True(properties.ContainsKey("gehaltNettoCa"));
        Assert.True(properties.ContainsKey("zahlungsweise"));
        Assert.True(properties.ContainsKey("sonderzahlungen"));
        Assert.True(properties.ContainsKey("zuschlaege"));
        Assert.True(properties.ContainsKey("zusatzleistungen"));
    }

    [Fact]
    public void GetSchema_ZusatzleistungenIsArrayType()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var zusatzleistungen = properties?["zusatzleistungen"]?.AsObject();

        // Assert
        Assert.NotNull(zusatzleistungen);
        Assert.Equal("array", zusatzleistungen["type"]?.ToString());
        
        var items = zusatzleistungen["items"]?.AsObject();
        Assert.NotNull(items);
        Assert.Equal("string", items["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_GehaltBruttoHasHighImportance()
    {
        // Act
        var schema = _provider.GetSchema();
        var properties = schema["properties"]?.AsObject();
        var gehaltBrutto = properties?["gehaltBrutto"]?.AsObject();
        var styling = gehaltBrutto?["x-styling"]?.AsObject();

        // Assert
        Assert.NotNull(styling);
        Assert.Equal("high", styling["importance"]?.ToString());
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
