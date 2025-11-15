namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Basic;

using System.Text.Json.Nodes;

public sealed class RentalBasicSummarySchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["kurzfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurze, leicht verständliche Zusammenfassung der wichtigsten Eckdaten, z. B. Vertragsart und Miethöhe in 1–3 Sätzen."
            },
            ["detailfazit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Bewertung des Vertrags aus rechtlicher und inhaltlicher Sicht in mehreren Sätzen."
            },
            ["gesamtbewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurzbewertung mit Emoji und Einstufung, z. B. '🟡 teilweise unzulässig (Einzelklausel problematisch)'."
            }
        },
        ["required"] = new JsonArray(
            "kurzfazit",
            "detailfazit",
            "gesamtbewertung")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
