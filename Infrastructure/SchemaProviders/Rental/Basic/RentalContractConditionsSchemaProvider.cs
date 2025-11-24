using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Provides JSON schema for Rental contract conditions analysis.
/// </summary>
public sealed class RentalContractConditionsSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Vertragsbedingungen",
        ["properties"] = new JsonObject
        {
            ["kuendigungsfrist"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kündigungsfrist für den Mietvertrag (z.B. '3 Monate zum Monatsende').",
                ["x-displayName"] = "Kündigungsfrist",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-semibold",
                    ["importance"] = "high",
                    ["icon"] = "clock"
                }
            },
            ["tierhaltung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zur Tierhaltung (erlaubt, verboten, mit Zustimmung).",
                ["x-displayName"] = "Tierhaltung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "pet"
                }
            },
            ["untervermietung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zur Untervermietung (erlaubt, verboten, mit Zustimmung).",
                ["x-displayName"] = "Untervermietung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "users"
                }
            },
            ["besonderheiten"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Weitere besondere Vertragsbedingungen oder Klauseln.",
                ["x-displayName"] = "Besonderheiten",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "info"
                }
            },
            ["schoenheitsreparaturen"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zu Schönheitsreparaturen (Mieter, Vermieter, geteilt).",
                ["x-displayName"] = "Schönheitsreparaturen",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["importance"] = "medium",
                    ["icon"] = "tool"
                }
            },
            ["betriebskostenabrechnung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zur Betriebskostenabrechnung (Frist, Umfang).",
                ["x-displayName"] = "Betriebskostenabrechnung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "document"
                }
            }
        },
        ["required"] = new JsonArray { "kuendigungsfrist", "tierhaltung", "untervermietung", "besonderheiten" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
