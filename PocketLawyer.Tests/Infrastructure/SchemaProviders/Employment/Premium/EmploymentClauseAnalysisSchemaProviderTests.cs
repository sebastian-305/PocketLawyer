using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Infrastructure.SchemaProviders.Employment.Premium;
using System.Text.Json.Nodes;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders.Employment.Premium;

/// <summary>
/// Tests for EmploymentClauseAnalysisSchemaProvider migration to SchemaGenerator.
/// Verifies that the generated schema matches the original manual schema with dynamic section titles.
/// </summary>
public class EmploymentClauseAnalysisSchemaProviderTests
{
    private readonly EmploymentClauseAnalysisSchemaProvider _provider = new();
    private readonly SchemaComparator _comparator = new();

    /// <summary>
    /// The original manual base schema for comparison (without section title).
    /// This is what the generated schema should match exactly.
    /// </summary>
    private readonly JsonObject _manualBaseSchema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["zusammenfassung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zusammenfassung der relevanten Klauseln in diesem Bereich.",
                ["x-displayName"] = "Zusammenfassung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-base",
                    ["importance"] = "high"
                }
            },
            ["bewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Rechtliche Bewertung der Klauseln mit Hinweisen auf Risiken oder Vorteile unter Berücksichtigung des Kollektivvertrags.",
                ["x-displayName"] = "Bewertung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-base font-semibold",
                    ["importance"] = "high"
                }
            },
            ["rechtsgrundlage"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Relevante Rechtsgrundlagen und Paragraphen (z.B. AngG, AVRAG, AZG, Kollektivvertrag).",
                ["x-displayName"] = "Rechtsgrundlage",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-sm text-secondary",
                    ["importance"] = "medium",
                    ["icon"] = "law"
                }
            },
            ["risikoScore"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "Vorteilhaft", "Neutral", "Nachteilhaft" },
                ["description"] = "Risikobewertung: Vorteilhaft (günstig für Arbeitnehmer), Neutral (ausgeglichen), Nachteilhaft (nachteilig für Arbeitnehmer).",
                ["x-displayName"] = "Risiko-Score",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge",
                    ["importance"] = "high",
                    ["icon"] = "shield"
                }
            },
            ["riskLevel"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" },
                ["description"] = "Risikostufe: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
                ["x-displayName"] = "Risikostufe",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "risk-indicator",
                    ["importance"] = "high",
                    ["icon"] = "alert"
                }
            }
        },
        ["required"] = new JsonArray { "zusammenfassung", "bewertung", "rechtsgrundlage", "risikoScore", "riskLevel" }
    };

    [Fact]
    public void GetSchema_WithoutStepName_ReturnsValidJsonObject()
    {
        // Act
        var schema = _provider.GetSchema();

        // Assert
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_WithStepName_ReturnsValidJsonObject()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");

        // Assert
        Assert.NotNull(schema);
        Assert.Equal("object", schema["type"]?.ToString());
    }

    [Fact]
    public void GetSchema_WithCompensationBenefits_HasCorrectSectionTitle()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");

        // Assert
        Assert.Equal("Vergütungs- und Zusatzleistungsklauseln", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_WithWorkingTimeVacation_HasCorrectSectionTitle()
    {
        // Act
        var schema = _provider.GetSchema("WorkingTimeVacation");

        // Assert
        Assert.Equal("Arbeitszeit- und Urlaubsklauseln", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_WithTerminationNotice_HasCorrectSectionTitle()
    {
        // Act
        var schema = _provider.GetSchema("TerminationNotice");

        // Assert
        Assert.Equal("Kündigungs- und Kündigungsfristklauseln", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_WithNonCompeteConfidentiality_HasCorrectSectionTitle()
    {
        // Act
        var schema = _provider.GetSchema("NonCompeteConfidentiality");

        // Assert
        Assert.Equal("Wettbewerbs- und Vertraulichkeitsklauseln", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_WithRightsDuties_HasCorrectSectionTitle()
    {
        // Act
        var schema = _provider.GetSchema("RightsDuties");

        // Assert
        Assert.Equal("Rechte und Pflichten", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_WithUnknownStepName_UsesFallbackSectionTitle()
    {
        // Act
        var schema = _provider.GetSchema("UnknownStep");

        // Assert
        Assert.Equal("UnknownStep", schema["x-sectionTitle"]?.ToString());
    }

    [Fact]
    public void GetSchema_GeneratedSchemaMatchesManualSchema()
    {
        // **Property 1: Schema Equivalence After Migration**
        // **Validates: Requirements 2.2, 3.1-3.11**
        
        // Act
        var generatedSchema = _provider.GetSchema("CompensationBenefits");
        
        // Create expected schema with section title
        var expectedSchema = (JsonObject)_manualBaseSchema.DeepClone();
        expectedSchema["x-sectionTitle"] = "Vergütungs- und Zusatzleistungsklauseln";

        // Assert - Compare with manual schema
        var result = _comparator.Compare(expectedSchema, generatedSchema);
        
        Assert.True(result.IsIdentical, 
            $"Generated schema does not match manual schema. Differences:\n{string.Join("\n", result.Differences)}");
    }

    [Fact]
    public void GetSchema_HasAllRequiredProperties()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
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
    public void GetSchema_AllPropertiesAreRequired()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
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
    public void GetSchema_AdditionalPropertiesIsFalse()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");

        // Assert
        Assert.False(schema["additionalProperties"]?.GetValue<bool>());
    }

    [Fact]
    public void GetSchema_BewertungHasEmploymentSpecificDescription()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
        var properties = schema["properties"]?.AsObject();
        var bewertung = properties?["bewertung"]?.AsObject();

        // Assert
        Assert.NotNull(bewertung);
        var description = bewertung["description"]?.ToString();
        Assert.NotNull(description);
        Assert.Contains("Kollektivvertrag", description);
    }

    [Fact]
    public void GetSchema_RechtsgrundlageHasEmploymentSpecificDescription()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
        var properties = schema["properties"]?.AsObject();
        var rechtsgrundlage = properties?["rechtsgrundlage"]?.AsObject();

        // Assert
        Assert.NotNull(rechtsgrundlage);
        var description = rechtsgrundlage["description"]?.ToString();
        Assert.NotNull(description);
        Assert.Contains("AngG", description);
        Assert.Contains("AVRAG", description);
        Assert.Contains("AZG", description);
        Assert.Contains("Kollektivvertrag", description);
    }

    [Fact]
    public void GetSchema_RisikoScoreHasEmploymentSpecificEnum()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
        var properties = schema["properties"]?.AsObject();
        var risikoScore = properties?["risikoScore"]?.AsObject();
        var enumValues = risikoScore?["enum"]?.AsArray();

        // Assert
        Assert.NotNull(enumValues);
        Assert.Equal(3, enumValues.Count);
        Assert.Contains(enumValues, v => v?.ToString() == "Vorteilhaft");
        Assert.Contains(enumValues, v => v?.ToString() == "Neutral");
        Assert.Contains(enumValues, v => v?.ToString() == "Nachteilhaft");
    }

    [Fact]
    public void GetSchema_RisikoScoreHasEmploymentSpecificDescription()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
        var properties = schema["properties"]?.AsObject();
        var risikoScore = properties?["risikoScore"]?.AsObject();

        // Assert
        Assert.NotNull(risikoScore);
        var description = risikoScore["description"]?.ToString();
        Assert.NotNull(description);
        Assert.Contains("Vorteilhaft", description);
        Assert.Contains("günstig für Arbeitnehmer", description);
        Assert.Contains("Neutral", description);
        Assert.Contains("Nachteilhaft", description);
        Assert.Contains("nachteilig für Arbeitnehmer", description);
    }

    [Fact]
    public void GetSchema_RiskLevelHasCorrectEnum()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
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
    public void GetSchema_AllPropertiesHaveCorrectStyling()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        
        // Zusammenfassung styling
        var zusammenfassung = properties["zusammenfassung"]?.AsObject();
        var zusammenfassungStyling = zusammenfassung?["x-styling"]?.AsObject();
        Assert.NotNull(zusammenfassungStyling);
        Assert.Equal("text-base", zusammenfassungStyling["displayClass"]?.ToString());
        Assert.Equal("high", zusammenfassungStyling["importance"]?.ToString());
        
        // Bewertung styling
        var bewertung = properties["bewertung"]?.AsObject();
        var bewertungStyling = bewertung?["x-styling"]?.AsObject();
        Assert.NotNull(bewertungStyling);
        Assert.Equal("text-base font-semibold", bewertungStyling["displayClass"]?.ToString());
        Assert.Equal("high", bewertungStyling["importance"]?.ToString());
        
        // Rechtsgrundlage styling
        var rechtsgrundlage = properties["rechtsgrundlage"]?.AsObject();
        var rechtsgrundlageStyling = rechtsgrundlage?["x-styling"]?.AsObject();
        Assert.NotNull(rechtsgrundlageStyling);
        Assert.Equal("text-sm text-secondary", rechtsgrundlageStyling["displayClass"]?.ToString());
        Assert.Equal("medium", rechtsgrundlageStyling["importance"]?.ToString());
        Assert.Equal("law", rechtsgrundlageStyling["icon"]?.ToString());
        
        // RisikoScore styling
        var risikoScore = properties["risikoScore"]?.AsObject();
        var risikoScoreStyling = risikoScore?["x-styling"]?.AsObject();
        Assert.NotNull(risikoScoreStyling);
        Assert.Equal("badge", risikoScoreStyling["displayClass"]?.ToString());
        Assert.Equal("high", risikoScoreStyling["importance"]?.ToString());
        Assert.Equal("shield", risikoScoreStyling["icon"]?.ToString());
        
        // RiskLevel styling
        var riskLevel = properties["riskLevel"]?.AsObject();
        var riskLevelStyling = riskLevel?["x-styling"]?.AsObject();
        Assert.NotNull(riskLevelStyling);
        Assert.Equal("risk-indicator", riskLevelStyling["displayClass"]?.ToString());
        Assert.Equal("high", riskLevelStyling["importance"]?.ToString());
        Assert.Equal("alert", riskLevelStyling["icon"]?.ToString());
    }

    [Fact]
    public void GetSchema_AllPropertiesHaveDisplayNames()
    {
        // Act
        var schema = _provider.GetSchema("CompensationBenefits");
        var properties = schema["properties"]?.AsObject();

        // Assert
        Assert.NotNull(properties);
        
        var zusammenfassung = properties["zusammenfassung"]?.AsObject();
        Assert.Equal("Zusammenfassung", zusammenfassung?["x-displayName"]?.ToString());
        
        var bewertung = properties["bewertung"]?.AsObject();
        Assert.Equal("Bewertung", bewertung?["x-displayName"]?.ToString());
        
        var rechtsgrundlage = properties["rechtsgrundlage"]?.AsObject();
        Assert.Equal("Rechtsgrundlage", rechtsgrundlage?["x-displayName"]?.ToString());
        
        var risikoScore = properties["risikoScore"]?.AsObject();
        Assert.Equal("Risiko-Score", risikoScore?["x-displayName"]?.ToString());
        
        var riskLevel = properties["riskLevel"]?.AsObject();
        Assert.Equal("Risikostufe", riskLevel?["x-displayName"]?.ToString());
    }

    [Fact]
    public void GetSchema_AllStepNames_ProduceValidSchemas()
    {
        // Arrange
        var stepNames = new[] 
        { 
            "CompensationBenefits", 
            "WorkingTimeVacation", 
            "TerminationNotice", 
            "NonCompeteConfidentiality", 
            "RightsDuties" 
        };

        // Act & Assert
        foreach (var stepName in stepNames)
        {
            var schema = _provider.GetSchema(stepName);
            
            Assert.NotNull(schema);
            Assert.Equal("object", schema["type"]?.ToString());
            Assert.NotNull(schema["x-sectionTitle"]);
            Assert.False(schema["additionalProperties"]?.GetValue<bool>());
            
            var properties = schema["properties"]?.AsObject();
            Assert.NotNull(properties);
            Assert.Equal(5, properties.Count);
            
            var required = schema["required"]?.AsArray();
            Assert.NotNull(required);
            Assert.Equal(5, required.Count);
        }
    }
}
