namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Premium;

using System.Text.Json.Nodes;

public sealed class ClauseAnalysisSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["titel"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Überschrift der Klausel bzw. des Themenblocks, z. B. 'Mietzins (Hauptmietzins)'."
            },
            ["zusammenfassung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Zusammenfassung der Regelung in einfachen Worten (für Laien verständlich)."
            },
            ["bewertung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Juristische Bewertung der Klausel. Erläutere, ob sie üblich, zulässig, problematisch oder unzulässig ist und warum."
            },
            ["rechtsgrundlage"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Relevante gesetzliche Grundlagen, z. B. 'MRG, ABGB' oder konkrete Paragraphen."
            },
            ["risikoScore"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Risiko- bzw. Bewertungsstufe der Klausel. Erlaubte Werte: 'Zulaessig' (🟢), 'Nachteilhaft' (🟡), 'Unzulaessig' (🔴)."
            }
        },
        ["required"] = new JsonArray(
            "titel",
            "zusammenfassung",
            "bewertung",
            "rechtsgrundlage",
            "risikoScore")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}

