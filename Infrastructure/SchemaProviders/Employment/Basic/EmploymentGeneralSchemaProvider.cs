using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Provides JSON schema for Employment contract general data analysis.
/// </summary>
public sealed class EmploymentGeneralSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Allgemeine Arbeitsdaten",
        ["properties"] = new JsonObject
        {
            ["arbeitgeber"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des Arbeitgebers.",
                ["x-displayName"] = "Arbeitgeber",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "building"
                }
            },
            ["arbeitnehmer"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Name des/der Arbeitnehmer:in.",
                ["x-displayName"] = "Arbeitnehmer",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "user"
                }
            },
            ["position"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Bezeichnung der Position/Tätigkeit.",
                ["x-displayName"] = "Position",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "briefcase"
                }
            },
            ["startdatum"] = new JsonObject
            {
                ["type"] = "string",
                ["format"] = "date",
                ["description"] = "Datum des Arbeitsbeginns im Format YYYY-MM-DD.",
                ["x-displayName"] = "Startdatum",
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
            ["probezeit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Dauer der Probezeit (z.B. '1 Monat', '3 Monate', 'keine').",
                ["x-displayName"] = "Probezeit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "clock"
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
        ["required"] = new JsonArray { "arbeitgeber", "arbeitnehmer", "position", "startdatum", "vertragsart", "probezeit" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
