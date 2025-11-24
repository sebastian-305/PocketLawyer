using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Premium;

/// <summary>
/// Provides JSON schema for Premium clause analysis sections.
/// Used for all premium analysis sections (Costs, Terms, Use, Maintenance, Duties, Privacy).
/// </summary>
public sealed class ClauseAnalysisSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["zusammenfassung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zusammenfassung der relevanten Klauseln in diesem Bereich.",
                ["x-title"] = "Zusammenfassung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-base",
                    ["importance"] = "high"
                }
            },
            ["bewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Rechtliche Bewertung der Klauseln mit Hinweisen auf Risiken oder Vorteile.",
                ["x-title"] = "Bewertung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-base font-semibold",
                    ["importance"] = "high"
                }
            },
            ["rechtsgrundlage"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Relevante Rechtsgrundlagen und Paragraphen (z.B. MRG, ABGB).",
                ["x-title"] = "Rechtsgrundlage",
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
                ["enum"] = new JsonArray { "Zulaessig", "Nachteilhaft", "Unzulaessig" },
                ["description"] = "Risikobewertung: Zulaessig (rechtlich unbedenklich), Nachteilhaft (nachteilig für Mieter), Unzulaessig (rechtlich problematisch).",
                ["x-title"] = "Risiko-Score",
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
                ["x-title"] = "Risikostufe",
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

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
