using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Provides JSON schema for Rental contract general data analysis.
/// </summary>
public sealed class RentalGeneralSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["vertragstyp"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Typ des Mietvertrags, z. B. 'I', 'II', 'III', 'WG'.",
                ["x-title"] = "Vertragstyp",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "font-bold",
                    ["importance"] = "high",
                    ["icon"] = "document"
                }
            },
            ["mieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Mieter:in.",
                ["x-title"] = "Mieter",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "user"
                }
            },
            ["vermieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Vermieter:in.",
                ["x-title"] = "Vermieter",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "user"
                }
            },
            ["mietobjektAdresse"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Vollständige Adresse des Mietobjekts.",
                ["x-title"] = "Mietobjekt Adresse",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "location"
                }
            },
            ["vertragsbeginn"] = new JsonObject
            {
                ["type"] = "string",
                ["format"] = "date",
                ["description"] = "Datum des Vertragsbeginns im Format YYYY-MM-DD.",
                ["x-title"] = "Vertragsbeginn",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "calendar"
                }
            },
            ["vertragsart"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray { "befristet", "unbefristet" },
                ["description"] = "Art des Vertrags: befristet oder unbefristet.",
                ["x-title"] = "Vertragsart",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge",
                    ["importance"] = "medium"
                }
            },
            ["vertragsende"] = new JsonObject
            {
                ["type"] = new JsonArray { "string", "null" },
                ["format"] = "date",
                ["description"] = "Datum des Vertragsendes im Format YYYY-MM-DD (nur bei befristeten Verträgen).",
                ["x-title"] = "Vertragsende",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "calendar"
                }
            }
        },
        ["required"] = new JsonArray { "vertragstyp", "mieter", "vermieter", "mietobjektAdresse", "vertragsbeginn", "vertragsart" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
