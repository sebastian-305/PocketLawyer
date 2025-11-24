using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Provides JSON schema for Rental contract basic analysis summary.
/// </summary>
public sealed class RentalBasicSummarySchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["kurzfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurze Zusammenfassung der wichtigsten Vertragspunkte (2-3 Sätze).",
                ["x-title"] = "Kurzfazit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-lg font-semibold",
                    ["importance"] = "high"
                }
            },
            ["gesamtfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Ausführliches Fazit zur Vertragsgestaltung und wichtigen Punkten.",
                ["x-title"] = "Gesamtfazit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-base",
                    ["importance"] = "high"
                }
            },
            ["gesamtbewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Gesamtbewertung des Vertrags (z.B. 'Gut', 'Akzeptabel', 'Problematisch').",
                ["x-title"] = "Gesamtbewertung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge badge-lg",
                    ["importance"] = "high"
                }
            },
            ["riskLevel"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" },
                ["description"] = "Risikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
                ["x-title"] = "Risikostufe",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "risk-indicator",
                    ["importance"] = "high",
                    ["icon"] = "alert"
                }
            }
        },
        ["required"] = new JsonArray { "kurzfazit", "gesamtfazit", "gesamtbewertung", "riskLevel" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
