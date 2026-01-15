using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Premium;

/// <summary>
/// Provides JSON schema for Employment contract premium analysis summary.
/// </summary>
public sealed class EmploymentPremiumSummarySchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Gesamtfazit",
        ["properties"] = new JsonObject
        {
            ["gesamtfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Ausführliches Gesamtfazit zur rechtlichen Bewertung des Arbeitsvertrags unter Berücksichtigung aller analysierten Bereiche und des Kollektivvertrags.",
                ["x-displayName"] = "Gesamtfazit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-lg font-semibold",
                    ["importance"] = "high"
                }
            },
            ["gesamtbewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Gesamtbewertung des Vertrags (z.B. 'Sehr gut', 'Gut', 'Akzeptabel', 'Problematisch', 'Kritisch').",
                ["x-displayName"] = "Gesamtbewertung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge badge-lg",
                    ["importance"] = "high"
                }
            },
            ["handlungsempfehlungen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "string"
                },
                ["description"] = "Liste konkreter Handlungsempfehlungen für den Arbeitnehmer (z.B. 'Nachverhandlung der Kündigungsfrist empfohlen', 'Wettbewerbsverbot prüfen lassen').",
                ["x-displayName"] = "Handlungsempfehlungen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "list-disc",
                    ["importance"] = "high",
                    ["icon"] = "lightbulb"
                }
            }
        },
        ["required"] = new JsonArray { "gesamtfazit", "gesamtbewertung", "handlungsempfehlungen" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
