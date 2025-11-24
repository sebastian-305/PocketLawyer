using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Provides JSON schema for Rental contract rent conditions analysis.
/// </summary>
public sealed class RentalRentConditionsSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Mietkonditionen",
        ["properties"] = new JsonObject
        {
            ["kaltmiete"] = new JsonObject
            {
                ["type"] = "number",
                ["description"] = "Monatliche Kaltmiete in Euro.",
                ["x-displayName"] = "Kaltmiete",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-bold text-lg",
                    ["importance"] = "high",
                    ["icon"] = "currency",
                    ["format"] = "currency"
                }
            },
            ["nebenkosten"] = new JsonObject
            {
                ["type"] = "number",
                ["description"] = "Monatliche Nebenkosten in Euro.",
                ["x-displayName"] = "Nebenkosten",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "currency",
                    ["format"] = "currency"
                }
            },
            ["warmmiete"] = new JsonObject
            {
                ["type"] = "number",
                ["description"] = "Monatliche Warmmiete (Kaltmiete + Nebenkosten) in Euro.",
                ["x-displayName"] = "Warmmiete",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-bold text-primary",
                    ["importance"] = "high",
                    ["icon"] = "currency",
                    ["format"] = "currency"
                }
            },
            ["kaution"] = new JsonObject
            {
                ["type"] = "number",
                ["description"] = "Höhe der Kaution in Euro.",
                ["x-displayName"] = "Kaution",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-warning",
                    ["importance"] = "medium",
                    ["icon"] = "currency",
                    ["format"] = "currency"
                }
            },
            ["zahlungsweise"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zahlungsweise der Miete (z.B. monatlich, vierteljährlich).",
                ["x-displayName"] = "Zahlungsweise",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "calendar"
                }
            },
            ["nebenkostenregelung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Beschreibung der Nebenkostenregelung (Pauschale, Vorauszahlung mit Abrechnung, etc.).",
                ["x-displayName"] = "Nebenkostenregelung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["importance"] = "medium"
                }
            }
        },
        ["required"] = new JsonArray { "kaltmiete", "nebenkosten", "warmmiete", "kaution", "zahlungsweise", "nebenkostenregelung" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
