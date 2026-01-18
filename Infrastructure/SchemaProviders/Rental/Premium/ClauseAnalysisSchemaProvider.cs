using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Rental;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Premium;

/// <summary>
/// Provides JSON schema for Premium rental clause analysis sections.
/// Used for all premium analysis sections (Costs, Terms, Use, Maintenance, Duties, Privacy).
/// Each section gets its own x-sectionTitle based on the step name.
/// Schema is generated from PremiumSectionDto with Rental-specific customizations.
/// </summary>
public sealed class ClauseAnalysisSchemaProvider
{
    private readonly SchemaGenerator _generator = new();
    
    /// <summary>
    /// Mapping of step names to German section titles.
    /// </summary>
    private static readonly Dictionary<string, string> SectionTitles = new()
    {
        ["Costs"] = "Mietzahlungs- und Nebenkostenklauseln",
        ["Terms"] = "Mietdauer und Kündigungsklauseln",
        ["Use"] = "Nutzungsklauseln",
        ["Maintenance"] = "Instandhaltungs- und Reparaturklauseln",
        ["Duties"] = "Pflichten und Obliegenheiten",
        ["Privacy"] = "Datenschutz und Privatsphäre"
    };

    /// <summary>
    /// Gets the schema for a specific step with the appropriate section title.
    /// </summary>
    /// <param name="stepName">The step name (e.g., "Costs", "Terms", "Use")</param>
    /// <returns>A JSON schema with x-sectionTitle set for the step</returns>
    public JsonObject GetSchema(string stepName)
    {
        // Generate base schema from DTO
        var schema = _generator.GenerateSchema<PremiumSectionDto>();
        
        // Add dynamic section title based on step name
        if (SectionTitles.TryGetValue(stepName, out var title))
        {
            schema["x-sectionTitle"] = title;
        }
        else
        {
            // Fallback to step name if no mapping exists
            schema["x-sectionTitle"] = stepName;
        }
        
        // Customize descriptions and add enum constraints for Rental context
        if (schema["properties"] is JsonObject properties)
        {
            // Update rechtsgrundlage description with Rental-specific legal frameworks
            if (properties["rechtsgrundlage"] is JsonObject rechtsgrundlage)
            {
                rechtsgrundlage["description"] = "Relevante Rechtsgrundlagen und Paragraphen (z.B. MRG, ABGB).";
            }
            
            // Add Rental-specific enum and description for risikoScore
            if (properties["risikoScore"] is JsonObject risikoScore)
            {
                risikoScore["enum"] = new JsonArray { "Zulaessig", "Nachteilhaft", "Unzulaessig" };
                risikoScore["description"] = "Risikobewertung: Zulaessig (rechtlich unbedenklich), Nachteilhaft (nachteilig für Mieter), Unzulaessig (rechtlich problematisch).";
            }
            
            // Add enum for riskLevel (same for both Employment and Rental)
            if (properties["riskLevel"] is JsonObject riskLevel)
            {
                riskLevel["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" };
            }
        }
        
        return schema;
    }

    /// <summary>
    /// Gets the schema without a step-specific title (for backward compatibility).
    /// </summary>
    /// <returns>A JSON schema without x-sectionTitle</returns>
    public JsonObject GetSchema() => _generator.GenerateSchema<PremiumSectionDto>();
}
