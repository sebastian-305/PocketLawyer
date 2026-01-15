using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Provides JSON schema for Employment contract basic analysis summary.
/// </summary>
public sealed class EmploymentBasicSummarySchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Zusammenfassung",
        ["properties"] = new JsonObject
        {
            ["kurzfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurze Zusammenfassung der wichtigsten Vertragspunkte (2-3 Sätze).",
                ["x-displayName"] = "Kurzfazit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-lg font-semibold",
                    ["importance"] = "high"
                }
            },
            ["kollektivvertrag"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Erwähnung des erkannten Kollektivvertrags oder 'Kein Kollektivvertrag erkannt'.",
                ["x-displayName"] = "Kollektivvertrag",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge badge-info",
                    ["icon"] = "document",
                    ["importance"] = "high"
                }
            },
            ["gesamtbewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Gesamtbewertung des Vertrags (z.B. 'Gut', 'Akzeptabel', 'Problematisch').",
                ["x-displayName"] = "Gesamtbewertung",
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
                ["x-displayName"] = "Risikostufe",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "risk-indicator",
                    ["importance"] = "high",
                    ["icon"] = "alert"
                }
            }
        },
        ["required"] = new JsonArray { "kurzfazit", "kollektivvertrag", "gesamtbewertung", "riskLevel" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
