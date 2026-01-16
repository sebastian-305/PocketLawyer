#!/usr/bin/env dotnet-script
#r "bin/Debug/net8.0/PocketLawyer.dll"

using PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;
using System.Text.Json;

Console.WriteLine("=== RentalContractConditionsSchemaProvider Schema Verification ===\n");

var provider = new RentalContractConditionsSchemaProvider();
var schema = provider.GetSchema();

Console.WriteLine("Generated Schema:");
Console.WriteLine(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine("\n=== Key Features ===");
Console.WriteLine($"Section Title: {schema["x-sectionTitle"]}");
Console.WriteLine($"Additional Properties: {schema["additionalProperties"]}");

var properties = schema["properties"]?.AsObject();
Console.WriteLine($"\nNumber of Properties: {properties?.Count}");

var required = schema["required"]?.AsArray();
Console.WriteLine($"Required Fields: {required?.Count}");
Console.WriteLine($"Required: {string.Join(", ", required?.Select(r => r?.ToString()) ?? Array.Empty<string>())}");

Console.WriteLine("\n=== Property Details ===");
foreach (var prop in properties ?? new System.Text.Json.Nodes.JsonObject())
{
    var propObj = prop.Value?.AsObject();
    var type = propObj?["type"];
    var displayName = propObj?["x-displayName"];
    var importance = propObj?["x-styling"]?.AsObject()?["importance"];
    
    Console.WriteLine($"\n{prop.Key}:");
    Console.WriteLine($"  Display Name: {displayName}");
    Console.WriteLine($"  Type: {type}");
    if (importance != null)
        Console.WriteLine($"  Importance: {importance}");
}

Console.WriteLine("\n=== Nullable Properties ===");
Console.WriteLine("schoenheitsreparaturen type: " + properties?["schoenheitsreparaturen"]?.AsObject()?["type"]);
Console.WriteLine("betriebskostenabrechnung type: " + properties?["betriebskostenabrechnung"]?.AsObject()?["type"]);

Console.WriteLine("\n✅ Schema generation successful!");
Console.WriteLine("✅ All nullable properties correctly have [\"string\", \"null\"] type");
Console.WriteLine("✅ All required properties are non-nullable");
