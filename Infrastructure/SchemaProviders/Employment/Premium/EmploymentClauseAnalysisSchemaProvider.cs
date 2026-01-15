using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Premium;

/// <summary>
/// Provides JSON schema for Premium employment clause analysis sections.
/// Used for all premium analysis sections (CompensationBenefits, WorkingTimeVacation, TerminationNotice, NonCompeteConfidentiality, RightsDuties).
/// Each section gets its own x-sectionTitle based on the step name.
/// </summary>
public sealed class EmploymentClauseAnalysisSchemaProvider
{
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

    private readonly JsonObject _baseSchema = new()
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

    /// <summary>
    /// Gets the schema for a specific step with the appropriate section title.
    /// </summary>
    /// <param name="stepName">The step name (e.g., "CompensationBenefits", "WorkingTimeVacation")</param>
    /// <returns>A JSON schema with x-sectionTitle set for the step</returns>
    public JsonObject GetSchema(string stepName)
    {
        var schema = (JsonObject)_baseSchema.DeepClone();
        
        // Add x-sectionTitle if we have a mapping for this step
        if (SectionTitles.TryGetValue(stepName, out var title))
        {
            schema["x-sectionTitle"] = title;
        }
        else
        {
            // Fallback to step name if no mapping exists
            schema["x-sectionTitle"] = stepName;
        }
        
        return schema;
    }

    /// <summary>
    /// Gets the schema without a step-specific title (for backward compatibility).
    /// </summary>
    /// <returns>A JSON schema without x-sectionTitle</returns>
    public JsonObject GetSchema() => (JsonObject)_baseSchema.DeepClone();
}
