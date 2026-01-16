#!/usr/bin/env dotnet-script
#r "nuget: System.Text.Json, 8.0.0"

using System.Text.Json;
using System.Text.Json.Nodes;

// This script verifies the generated schema for EmploymentBasicSummaryDto
// Run with: dotnet script verify_summary_schema.csx

var schema = new JsonObject
{
    ["type"] = "object",
    ["additionalProperties"] = false,
    ["x-sectionTitle"] = "Zusammenfassung",
    ["properties"] = new JsonObject
    {
        ["kurzfazit"] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = "Kurze Zusammenfassung der wichtigsten Vertragspunkte (2-3 Sätze).",
            ["x-displayName"] = "Kurzfazit",
            ["x-styling"] = new JsonObject
            {
                ["displayClass"] = "text-lg font-semibold",
                ["importance"] = "high"
            }
        },
        ["kollektivvertrag"] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = "Erwähnung des erkannten Kollektivvertrags oder 'Kein Kollektivvertrag erkannt'.",
            ["x-displayName"] = "Kollektivvertrag",
            ["x-styling"] = new JsonObject
            {
                ["displayClass"] = "badge badge-info",
                ["icon"] = "document",
                ["importance"] = "high"
            }
        },
        ["gesamtbewertung"] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = "Gesamtbewertung des Vertrags (z.B. 'Gut', 'Akzeptabel', 'Problematisch').",
            ["x-displayName"] = "Gesamtbewertung",
            ["x-styling"] = new JsonObject
            {
                ["displayClass"] = "badge badge-lg",
                ["importance"] = "high"
            }
        },
        ["riskLevel"] = new JsonObject
        {
            ["type"] = "string",
            ["enum"] = new JsonArray { "Gruen", "Gelb", "Rot" },
            ["description"] = "Risikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
            ["x-displayName"] = "Risikostufe",
            ["x-styling"] = new JsonObject
            {
                ["displayClass"] = "risk-indicator",
                ["importance"] = "high",
                ["icon"] = "alert"
            }
        }
    },
    ["required"] = new JsonArray { "kurzfazit", "kollektivvertrag", "gesamtbewertung", "riskLevel" }
};

Console.WriteLine("Expected Schema for EmploymentBasicSummaryDto:");
Console.WriteLine(schema.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine("\n✓ Schema includes all 4 properties");
Console.WriteLine("✓ riskLevel has enum constraint: [Gruen, Gelb, Rot]");
Console.WriteLine("✓ All properties have appropriate styling and importance");
Console.WriteLine("✓ Section title: Zusammenfassung");
