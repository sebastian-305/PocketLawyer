using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Premium;

/// <summary>
/// Provides JSON schema for Rental contract premium analysis summary.
/// </summary>
public sealed class RentalPremiumSummarySchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["gesamtfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Gesamtfazit der Premium-Analyse mit Zusammenfassung aller Risiken und Empfehlungen.",
                ["x-title"] = "Gesamtfazit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-lg font-semibold",
                    ["importance"] = "high"
                }
            },
            ["gesamtbewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" },
                ["description"] = "Gesamtrisikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
                ["x-title"] = "Gesamtbewertung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "risk-indicator risk-indicator-lg",
                    ["importance"] = "high",
                    ["icon"] = "alert"
                }
            },
            ["handlungsempfehlungen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "string"
                },
                ["description"] = "Liste konkreter Handlungsempfehlungen für den Mieter.",
                ["x-title"] = "Handlungsempfehlungen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "list-disc ml-4",
                    ["importance"] = "high",
                    ["icon"] = "checklist"
                }
            }
        },
        ["required"] = new JsonArray { "gesamtfazit", "gesamtbewertung", "handlungsempfehlungen" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
