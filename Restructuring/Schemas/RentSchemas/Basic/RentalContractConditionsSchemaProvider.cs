namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Basic;

using System.Text.Json.Nodes;

public sealed class RentalContractConditionsSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["kuendigungsfrist"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Vereinbarte Kündigungsfrist, z. B. '3 Monate'."
            },
            ["tierhaltung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zur Tierhaltung, z. B. 'Erlaubt nach Zustimmung'."
            },
            ["untervermietung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zur Untervermietung, z. B. 'Nur mit Genehmigung'."
            },
            ["besonderheiten"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Besondere Klauseln, z. B. 'Enthält Indexmietklausel (Mietanpassung an Verbraucherpreisindex)'."
            }
        },
        ["required"] = new JsonArray(
            "kuendigungsfrist",
            "tierhaltung",
            "untervermietung",
            "besonderheiten")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
