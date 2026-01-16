#!/usr/bin/env dotnet-script
#r "nuget: System.Text.Json, 8.0.0"

using System.Text.Json;
using System.Text.Json.Nodes;

// Load the assemblies
#r "./bin/Debug/net8.0/PocketLawyer.dll"

using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Domain.Dtos;

Console.WriteLine("=== Verifying PremiumSectionDto Schema Generation ===\n");

// Create SchemaGenerator
var generator = new SchemaGenerator();

// Generate schema from PremiumSectionDto
var schema = generator.GenerateSchema<PremiumSectionDto>();

Console.WriteLine("Generated Schema:");
Console.WriteLine(schema.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine();

// Verify key properties
var properties = schema["properties"] as JsonObject;
if (properties == null)
{
    Console.WriteLine("❌ ERROR: No properties found in schema");
    return 1;
}

Console.WriteLine("=== Verification Results ===\n");

// Check each property
var expectedProperties = new[] { "zusammenfassung", "bewertung", "rechtsgrundlage", "risikoScore", "riskLevel" };
var allPropertiesPresent = true;

foreach (var propName in expectedProperties)
{
    if (properties.ContainsKey(propName))
    {
        var prop = properties[propName] as JsonObject;
        var displayName = prop?["x-displayName"]?.ToString();
        var description = prop?["description"]?.ToString();
        var styling = prop?["x-styling"] as JsonObject;
        
        Console.WriteLine($"✓ Property '{propName}' found");
        Console.WriteLine($"  - Display Name: {displayName}");
        Console.WriteLine($"  - Description: {description}");
        if (styling != null)
        {
            Console.WriteLine($"  - Styling: {styling.ToJsonString()}");
        }
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine($"❌ Property '{propName}' NOT found");
        allPropertiesPresent = false;
    }
}

// Check required array
var required = schema["required"] as JsonArray;
if (required != null)
{
    Console.WriteLine($"✓ Required fields: {string.Join(", ", required.Select(r => r?.ToString()))}");
}
else
{
    Console.WriteLine("❌ No required array found");
}

Console.WriteLine();

// Check additionalProperties
var additionalProperties = schema["additionalProperties"];
if (additionalProperties != null && additionalProperties.ToString() == "False")
{
    Console.WriteLine("✓ additionalProperties is set to false");
}
else
{
    Console.WriteLine($"❌ additionalProperties is not false: {additionalProperties}");
}

Console.WriteLine();

if (allPropertiesPresent)
{
    Console.WriteLine("✅ All properties are present with SchemaDisplay attributes!");
    return 0;
}
else
{
    Console.WriteLine("❌ Some properties are missing");
    return 1;
}
