#!/usr/bin/env dotnet-script
#r "nuget: System.Text.Json, 8.0.0"

using System.Text.Json.Nodes;
using System.Reflection;

// Load the assemblies
var domainAssembly = Assembly.LoadFrom("Domain/bin/Debug/net8.0/PocketLawyer.Domain.dll");
var infraAssembly = Assembly.LoadFrom("Infrastructure/bin/Debug/net8.0/PocketLawyer.Infrastructure.dll");

// Get the types
var dtoType = domainAssembly.GetType("PocketLawyer.Domain.Dtos.RentalPremiumSummaryDto");
var generatorType = infraAssembly.GetType("PocketLawyer.Infrastructure.SchemaProviders.SchemaGenerator");

if (dtoType == null)
{
    Console.WriteLine("ERROR: Could not find RentalPremiumSummaryDto type");
    return 1;
}

if (generatorType == null)
{
    Console.WriteLine("ERROR: Could not find SchemaGenerator type");
    return 1;
}

// Create generator instance
var generator = Activator.CreateInstance(generatorType);
var generateMethod = generatorType.GetMethod("GenerateSchema", new[] { typeof(Type) });

if (generateMethod == null)
{
    Console.WriteLine("ERROR: Could not find GenerateSchema method");
    return 1;
}

// Generate schema
var generatedSchema = (JsonObject)generateMethod.Invoke(generator, new object[] { dtoType });

Console.WriteLine("Generated Schema:");
Console.WriteLine(generatedSchema.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

// Manual schema for comparison
var manualSchema = new JsonObject
{
    ["type"] = "object",
    ["additionalProperties"] = false,
    ["x-sectionTitle"] = "Gesamtzusammenfassung",
    ["properties"] = new JsonObject
    {
        ["gesamtfazit"] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = "Gesamtfazit der Premium-Analyse mit Zusammenfassung aller Risiken und Empfehlungen.",
            ["x-displayName"] = "Gesamtfazit",
            ["x-styling"] = new JsonObject
            {
                ["displayClass"] = "text-lg font-semibold",
                ["importance"] = "high"
            }
        },
        ["gesamtbewertung"] = new JsonObject
        {
            ["type"] = "string",
            ["description"] = "Gesamtrisikostufe des Vertrags: Gruen (unkritisch), Gelb (beachtenswert), Rot (problematisch).",
            ["x-displayName"] = "Gesamtbewertung",
            ["x-styling"] = new JsonObject
            {
                ["displayClass"] = "risk-indicator risk-indicator-lg",
                ["importance"] = "high",
                ["icon"] = "alert"
            }
        },
        ["handlungsempfehlungen"] = new JsonObject
        {
            ["type"] = "array",
            ["items"] = new JsonObject
            {
                ["type"] = "string"
            },
            ["description"] = "Liste konkreter Handlungsempfehlungen für den Mieter.",
            ["x-displayName"] = "Handlungsempfehlungen",
            ["x-styling"] = new JsonObject
            {
                ["displayClass"] = "list-disc ml-4",
                ["importance"] = "high",
                ["icon"] = "checklist"
            }
        }
    },
    ["required"] = new JsonArray { "gesamtfazit", "gesamtbewertung", "handlungsempfehlungen" }
};

Console.WriteLine("\n\nManual Schema:");
Console.WriteLine(manualSchema.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

// Simple comparison
bool matches = generatedSchema.ToJsonString() == manualSchema.ToJsonString();
Console.WriteLine($"\n\nSchemas match: {matches}");

return matches ? 0 : 1;
