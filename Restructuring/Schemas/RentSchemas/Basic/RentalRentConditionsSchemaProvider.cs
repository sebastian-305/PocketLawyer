namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Basic;

using System.Text.Json.Nodes;

public sealed class RentalRentConditionsSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["properties"] = new JsonObject
        {
            ["kaltmiete"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Monatliche Nettomiete (ohne Betriebskosten), z. B. '950 €'."
            },
            ["nebenkosten"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Monatliche Betriebskosten/Nebenkosten, z. B. '250 €'."
            },
            ["warmmiete"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Monatliche Gesamtmiete inkl. Nebenkosten, z. B. '1.200 €'."
            },
            ["kaution"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Höhe der Kaution, z. B. '2.850 €'."
            },
            ["zahlungsweise"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Angabe zu Zahlungsrhythmus, Fälligkeit und Zahlungsart, z. B. 'Monatlich, bis 3. Werktag, per Überweisung'."
            },
            ["nebenkostenregelung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Kurzbeschreibung der Abrechnung der Nebenkosten, z. B. 'Jährliche Abrechnung laut Betriebskostenverordnung'."
            }
        },
        ["required"] = new JsonArray(
            "kaltmiete",
            "nebenkosten",
            "warmmiete",
            "kaution",
            "zahlungsweise",
            "nebenkostenregelung")
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
