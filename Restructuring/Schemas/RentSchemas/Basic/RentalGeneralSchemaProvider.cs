namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Basic;

using System.Text.Json.Nodes;

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
                ["description"] = "Typ des Mietvertrags, z. B. 'I', 'II', 'III', 'WG'."
            },
            ["mieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Mieter:in."
            },
            ["vermieter"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Vermieter:in (Person oder Unternehmen)."
            },
            ["mietobjektAdresse"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Adresse und kurze Beschreibung des Mietobjekts, z. B. Straße, PLZ, Ort, Wohnungsgröße."
            },
            ["vertragsbeginn"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Vertragsbeginn im Format YYYY-MM-DD, z. B. '2024-04-01'."
            },
            ["vertragsart"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Befristet oder Unbefristet."
            }
        },
        ["required"] = new JsonArray(
            "vertragstyp",
            "mieter",
            "vermieter",
            "mietobjektAdresse",
            "vertragsbeginn",
            "vertragsart")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}

