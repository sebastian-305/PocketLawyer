using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Provides JSON schema for Employment contract compensation analysis.
/// </summary>
public sealed class EmploymentCompensationSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Vergütung und Zusatzleistungen",
        ["properties"] = new JsonObject
        {
            ["gehaltBrutto"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Bruttogehalt (z.B. '3.500 EUR monatlich').",
                ["x-displayName"] = "Bruttogehalt",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary font-semibold",
                    ["icon"] = "currency",
                    ["importance"] = "high"
                }
            },
            ["gehaltNettoCa"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Geschätztes Nettogehalt (z.B. '2.450 EUR monatlich ca.').",
                ["x-displayName"] = "Nettogehalt (ca.)",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "currency"
                }
            },
            ["zahlungsweise"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zahlungsweise des Gehalts (z.B. 'monatlich', '14x jährlich').",
                ["x-displayName"] = "Zahlungsweise",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "calendar"
                }
            },
            ["sonderzahlungen"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Sonderzahlungen wie 13./14. Gehalt, Weihnachtsgeld, Urlaubsgeld.",
                ["x-displayName"] = "Sonderzahlungen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "gift",
                    ["importance"] = "medium"
                }
            },
            ["zuschlaege"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zuschläge für Überstunden, Nachtarbeit, Wochenendarbeit etc.",
                ["x-displayName"] = "Zuschläge",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "plus"
                }
            },
            ["zusatzleistungen"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "string"
                },
                ["description"] = "Zusatzleistungen wie Firmenwagen, Handy, Essenszuschuss, Versicherungen etc.",
                ["x-displayName"] = "Zusatzleistungen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge-list",
                    ["icon"] = "star"
                }
            }
        },
        ["required"] = new JsonArray { "gehaltBrutto", "gehaltNettoCa", "zahlungsweise", "sonderzahlungen", "zuschlaege", "zusatzleistungen" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
