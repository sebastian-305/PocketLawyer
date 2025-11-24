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
        ["x-sectionTitle"] = "Allgemeine Vertragsdaten",
        ["properties"] = new JsonObject
        {
            ["mieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Mieter:in.",
                ["x-displayName"] = "Mieter",
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
                ["x-displayName"] = "Vermieter",
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
                ["x-displayName"] = "Mietobjekt Adresse",
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
                ["x-displayName"] = "Vertragsbeginn",
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
                ["x-displayName"] = "Vertragsart",
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
                ["x-displayName"] = "Vertragsende",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "calendar"
                }
            }
        },
        ["required"] = new JsonArray { "mieter", "vermieter", "mietobjektAdresse", "vertragsbeginn", "vertragsart" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
