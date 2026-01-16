using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Premium;

/// <summary>
/// Provides JSON schema for Premium employment clause analysis sections.
/// Used for all premium analysis sections (CompensationBenefits, WorkingTimeVacation, TerminationNotice, NonCompeteConfidentiality, RightsDuties).
/// Each section gets its own x-sectionTitle based on the step name.
/// Schema is generated from PremiumSectionDto with Employment-specific customizations.
/// </summary>
public sealed class EmploymentClauseAnalysisSchemaProvider
{
    private readonly SchemaGenerator _generator = new();
    
    /// <summary>
    /// Mapping of step names to German section titles.
    /// </summary>
    private static readonly Dictionary<string, string> SectionTitles = new()
    {
        ["CompensationBenefits"] = "Vergütungs- und Zusatzleistungsklauseln",
        ["WorkingTimeVacation"] = "Arbeitszeit- und Urlaubsklauseln",
        ["TerminationNotice"] = "Kündigungs- und Kündigungsfristklauseln",
        ["NonCompeteConfidentiality"] = "Wettbewerbs- und Vertraulichkeitsklauseln",
        ["RightsDuties"] = "Rechte und Pflichten"
    };

    /// <summary>
    /// Gets the schema for a specific step with the appropriate section title.
    /// </summary>
    /// <param name="stepName">The step name (e.g., "CompensationBenefits", "WorkingTimeVacation")</param>
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
        
        // Customize descriptions and add enum constraints for Employment context
        if (schema["properties"] is JsonObject properties)
        {
            // Update bewertung description to include Kollektivvertrag reference
            if (properties["bewertung"] is JsonObject bewertung)
            {
                bewertung["description"] = "Rechtliche Bewertung der Klauseln mit Hinweisen auf Risiken oder Vorteile unter Berücksichtigung des Kollektivvertrags.";
            }
            
            // Update rechtsgrundlage description with Employment-specific legal frameworks
            if (properties["rechtsgrundlage"] is JsonObject rechtsgrundlage)
            {
                rechtsgrundlage["description"] = "Relevante Rechtsgrundlagen und Paragraphen (z.B. AngG, AVRAG, AZG, Kollektivvertrag).";
            }
            
            // Add Employment-specific enum and description for risikoScore
            if (properties["risikoScore"] is JsonObject risikoScore)
            {
                risikoScore["enum"] = new JsonArray { "Vorteilhaft", "Neutral", "Nachteilhaft" };
                risikoScore["description"] = "Risikobewertung: Vorteilhaft (günstig für Arbeitnehmer), Neutral (ausgeglichen), Nachteilhaft (nachteilig für Arbeitnehmer).";
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
