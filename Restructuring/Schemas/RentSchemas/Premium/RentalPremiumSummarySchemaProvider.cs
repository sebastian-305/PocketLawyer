namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Premium;

using System.Text.Json.Nodes;

public sealed class RentalPremiumSummarySchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["gesamtfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Übergreifendes Fazit zur Premiumanalyse des Mietvertrags. Fasse die wichtigsten Risiken und Auffälligkeiten verständlich zusammen."
            },
            ["gesamtbewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurzbewertung mit Emoji und verbaler Einstufung, z. B. '🟡 teilweise kritisch (Einzelklausel problematisch)'."
            }
        },
        ["required"] = new JsonArray(
            "gesamtfazit",
            "gesamtbewertung")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
