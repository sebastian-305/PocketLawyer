using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Provides JSON schema for Employment contract working conditions analysis.
/// </summary>
public sealed class EmploymentWorkingConditionsSchemaProvider
{
    private readonly JsonObject _schema = new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["x-sectionTitle"] = "Arbeitsbedingungen",
        ["properties"] = new JsonObject
        {
            ["wochenarbeitszeit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Wöchentliche Arbeitszeit (z.B. '40 Stunden', '38,5 Stunden').",
                ["x-displayName"] = "Wochenarbeitszeit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-primary",
                    ["icon"] = "clock",
                    ["importance"] = "high"
                }
            },
            ["arbeitszeitmodell"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Arbeitszeitmodell (z.B. 'Vollzeit', 'Teilzeit 30h', 'Gleitzeit').",
                ["x-displayName"] = "Arbeitszeitmodell",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge",
                    ["importance"] = "medium"
                }
            },
            ["ueberstundenregelung"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zu Überstunden (z.B. 'mit Gehalt abgegolten', 'Zeitausgleich', 'Auszahlung').",
                ["x-displayName"] = "Überstundenregelung",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "clock",
                    ["importance"] = "medium"
                }
            },
            ["urlaubstage"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Anzahl der Urlaubstage pro Jahr (z.B. '25 Tage', '30 Tage').",
                ["x-displayName"] = "Urlaubstage",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-info",
                    ["icon"] = "sun",
                    ["importance"] = "medium"
                }
            },
            ["arbeitsort"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Arbeitsort/Standort (z.B. 'Wien, Büro Hauptstraße 1', 'flexibel').",
                ["x-displayName"] = "Arbeitsort",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "text-secondary",
                    ["icon"] = "location"
                }
            },
            ["remoteArbeit"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Regelung zu Remote-Arbeit/Homeoffice (z.B. 'bis zu 2 Tage/Woche', 'nicht vorgesehen', '100% remote').",
                ["x-displayName"] = "Remote-Arbeit",
                ["x-styling"] = new JsonObject
                {
                    ["displayClass"] = "badge",
                    ["icon"] = "home"
                }
            }
        },
        ["required"] = new JsonArray { "wochenarbeitszeit", "arbeitszeitmodell", "ueberstundenregelung", "urlaubstage", "arbeitsort", "remoteArbeit" }
    };

    public JsonObject GetSchema() => (JsonObject)_schema.DeepClone();
}
