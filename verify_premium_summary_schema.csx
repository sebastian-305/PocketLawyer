#!/usr/bin/env dotnet-script
#r "nuget: System.Text.Json, 8.0.0"

using System.Text.Json;
using System.Text.Json.Nodes;

// Load the assembly
var assembly = System.Reflection.Assembly.LoadFrom("bin/Debug/net8.0/PocketLawyer.dll");

// Get the SchemaProvider type
var providerType = assembly.GetType("PocketLawyer.Infrastructure.SchemaProviders.Employment.Premium.EmploymentPremiumSummarySchemaProvider");
if (providerType == null)
{
    Console.WriteLine("ERROR: Could not find EmploymentPremiumSummarySchemaProvider type");
    return;
}

// Create an instance
var provider = Activator.CreateInstance(providerType);
if (provider == null)
{
    Console.WriteLine("ERROR: Could not create provider instance");
    return;
}

// Get the schema
var getSchemaMethod = providerType.GetMethod("GetSchema");
if (getSchemaMethod == null)
{
    Console.WriteLine("ERROR: Could not find GetSchema method");
    return;
}

var schema = getSchemaMethod.Invoke(provider, null) as JsonObject;
if (schema == null)
{
    Console.WriteLine("ERROR: GetSchema returned null");
    return;
}

// Pretty print the schema
var options = new JsonSerializerOptions 
{ 
    WriteIndented = true 
};
Console.WriteLine("Generated Schema:");
Console.WriteLine(schema.ToJsonString(options));

// Verify key properties
Console.WriteLine("\n=== Verification ===");
Console.WriteLine($"Type: {schema["type"]}");
Console.WriteLine($"Section Title: {schema["x-sectionTitle"]}");
Console.WriteLine($"Additional Properties: {schema["additionalProperties"]}");

var properties = schema["properties"]?.AsObject();
if (properties != null)
{
    Console.WriteLine($"\nProperties count: {properties.Count}");
    foreach (var prop in properties)
    {
        Console.WriteLine($"  - {prop.Key}");
        var propObj = prop.Value?.AsObject();
        if (propObj != null)
        {
            Console.WriteLine($"    Type: {propObj["type"]}");
            Console.WriteLine($"    Display Name: {propObj["x-displayName"]}");
            if (propObj.ContainsKey("x-styling"))
            {
                var styling = propObj["x-styling"]?.AsObject();
                if (styling != null)
                {
                    Console.WriteLine($"    Styling: {styling.ToJsonString()}");
                }
            }
        }
    }
}

var required = schema["required"]?.AsArray();
if (required != null)
{
    Console.WriteLine($"\nRequired fields: {string.Join(", ", required.Select(r => r?.ToString()))}");
}

Console.WriteLine("\n✓ Schema verification complete");
