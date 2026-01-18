using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Rental;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders;

/// <summary>
/// Tests for PremiumSectionDto schema generation.
/// Verifies that SchemaDisplay attributes are correctly applied.
/// </summary>
public class PremiumSectionDtoSchemaTests
{
    private readonly SchemaGenerator _generator = new();

    [Fact]
    public void GenerateSchema_PremiumSectionDto_ReturnsValidSchema()
    {
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();

        // Assert
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
        Assert.False(schema["additionalProperties"]?.GetValue<bool>());
    }

    [Fact]
    public void GenerateSchema_PremiumSectionDto_HasAllProperties()
    {
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.ContainsKey("zusammenfassung"));
        Assert.True(properties.ContainsKey("bewertung"));
        Assert.True(properties.ContainsKey("rechtsgrundlage"));
        Assert.True(properties.ContainsKey("risikoScore"));
        Assert.True(properties.ContainsKey("riskLevel"));
    }

    [Fact]
    public void GenerateSchema_PremiumSectionDto_AllPropertiesAreRequired()
    {
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        var required = schema["required"]?.AsArray();

        // Assert
        Assert.NotNull(required);
        Assert.Equal(5, required.Count);
        Assert.Contains(required, v => v?.ToString() == "zusammenfassung");
        Assert.Contains(required, v => v?.ToString() == "bewertung");
        Assert.Contains(required, v => v?.ToString() == "rechtsgrundlage");
        Assert.Contains(required, v => v?.ToString() == "risikoScore");
        Assert.Contains(required, v => v?.ToString() == "riskLevel");
    }

    [Fact]
    public void GenerateSchema_PremiumSectionDto_ZusammenfassungHasCorrectMetadata()
    {
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        var properties = schema["properties"]?.AsObject();
        var zusammenfassung = properties?["zusammenfassung"]?.AsObject();

        // Assert
        Assert.NotNull(zusammenfassung);
        Assert.Equal("string", zusammenfassung["type"]?.ToString());
        Assert.Equal("Zusammenfassung", zusammenfassung["x-displayName"]?.ToString());
        Assert.Equal("Zusammenfassung der relevanten Klauseln in diesem Bereich.", zusammenfassung["description"]?.ToString());
        
        var styling = zusammenfassung["x-styling"]?.AsObject();
        Assert.NotNull(styling);
        Assert.Equal("text-base", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GenerateSchema_PremiumSectionDto_BewertungHasCorrectMetadata()
    {
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        var properties = schema["properties"]?.AsObject();
        var bewertung = properties?["bewertung"]?.AsObject();

        // Assert
        Assert.NotNull(bewertung);
        Assert.Equal("string", bewertung["type"]?.ToString());
        Assert.Equal("Bewertung", bewertung["x-displayName"]?.ToString());
        Assert.Equal("Rechtliche Bewertung der Klauseln mit Hinweisen auf Risiken oder Vorteile.", bewertung["description"]?.ToString());
        
        var styling = bewertung["x-styling"]?.AsObject();
        Assert.NotNull(styling);
        Assert.Equal("text-base font-semibold", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GenerateSchema_PremiumSectionDto_RechtsgrundlageHasCorrectMetadata()
    {
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        var properties = schema["properties"]?.AsObject();
        var rechtsgrundlage = properties?["rechtsgrundlage"]?.AsObject();

        // Assert
        Assert.NotNull(rechtsgrundlage);
        Assert.Equal("string", rechtsgrundlage["type"]?.ToString());
        Assert.Equal("Rechtsgrundlage", rechtsgrundlage["x-displayName"]?.ToString());
        Assert.Equal("Relevante Rechtsgrundlagen und Paragraphen.", rechtsgrundlage["description"]?.ToString());
        
        var styling = rechtsgrundlage["x-styling"]?.AsObject();
        Assert.NotNull(styling);
        Assert.Equal("law", styling["icon"]?.ToString());
        Assert.Equal("text-sm text-secondary", styling["displayClass"]?.ToString());
        Assert.Equal("medium", styling["importance"]?.ToString());
    }

    [Fact]
    public void GenerateSchema_PremiumSectionDto_RisikoScoreHasCorrectMetadata()
    {
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        var properties = schema["properties"]?.AsObject();
        var risikoScore = properties?["risikoScore"]?.AsObject();

        // Assert
        Assert.NotNull(risikoScore);
        Assert.Equal("string", risikoScore["type"]?.ToString());
        Assert.Equal("Risiko-Score", risikoScore["x-displayName"]?.ToString());
        Assert.Equal("Risikobewertung der Klauseln.", risikoScore["description"]?.ToString());
        
        var styling = risikoScore["x-styling"]?.AsObject();
        Assert.NotNull(styling);
        Assert.Equal("shield", styling["icon"]?.ToString());
        Assert.Equal("badge", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GenerateSchema_PremiumSectionDto_RiskLevelHasCorrectMetadata()
    {
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        var properties = schema["properties"]?.AsObject();
        var riskLevel = properties?["riskLevel"]?.AsObject();

        // Assert
        Assert.NotNull(riskLevel);
        Assert.Equal("string", riskLevel["type"]?.ToString());
        Assert.Equal("Risikostufe", riskLevel["x-displayName"]?.ToString());
        Assert.Equal("Risikostufe: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).", riskLevel["description"]?.ToString());
        
        var styling = riskLevel["x-styling"]?.AsObject();
        Assert.NotNull(styling);
        Assert.Equal("alert", styling["icon"]?.ToString());
        Assert.Equal("risk-indicator", styling["displayClass"]?.ToString());
        Assert.Equal("high", styling["importance"]?.ToString());
    }

    [Fact]
    public void GenerateSchema_PremiumSectionDto_NoEnumConstraintsByDefault()
    {
        // The base schema should not have enum constraints
        // These will be added dynamically by each SchemaProvider based on context
        
        // Act
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        var properties = schema["properties"]?.AsObject();
        var risikoScore = properties?["risikoScore"]?.AsObject();
        var riskLevel = properties?["riskLevel"]?.AsObject();

        // Assert
        Assert.NotNull(risikoScore);
        Assert.NotNull(riskLevel);
        
        // Enum constraints should NOT be present in the base schema
        Assert.Null(risikoScore["enum"]);
        Assert.Null(riskLevel["enum"]);
    }
}
