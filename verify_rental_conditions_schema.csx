#!/usr/bin/env dotnet-script
#r "nuget: System.Text.Json, 8.0.0"

using System.Text.Json.Nodes;
using System.Reflection;

// Load the assembly
var assembly = Assembly.LoadFrom("./bin/Debug/net8.0/PocketLawyer.dll");

// Get the SchemaGenerator type
var generatorType = assembly.GetType("PocketLawyer.Infrastructure.SchemaProviders.SchemaGenerator");
if (generatorType == null)
{
    Console.WriteLine("ERROR: Could not find SchemaGenerator type");
    return 1;
}

// Get the DTO type
var dtoType = assembly.GetType("PocketLawyer.Domain.Dtos.RentalBasicConditionsDto");
if (dtoType == null)
{
    Console.WriteLine("ERROR: Could not find RentalBasicConditionsDto type");
    return 1;
}

// Get the SchemaProvider type
var providerType = assembly.GetType("PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic.RentalContractConditionsSchemaProvider");
if (providerType == null)
{
    Console.WriteLine("ERROR: Could not find RentalContractConditionsSchemaProvider type");
    return 1;
}

// Create instances
var generator = Activator.CreateInstance(generatorType);
var provider = Activator.CreateInstance(providerType);

// Generate schema using SchemaGenerator
var generateMethod = generatorType.GetMethod("GenerateSchema", new Type[] { typeof(Type) });
if (generateMethod == null)
{
    Console.WriteLine("ERROR: Could not find GenerateSchema method");
    return 1;
}

var generatedSchema = generateMethod.Invoke(generator, new object[] { dtoType }) as JsonObject;

// Get manual schema from provider
var getSchemaMethod = providerType.GetMethod("GetSchema");
if (getSchemaMethod == null)
{
    Console.WriteLine("ERROR: Could not find GetSchema method");
    return 1;
}

var manualSchema = getSchemaMethod.Invoke(provider, null) as JsonObject;

// Compare schemas
Console.WriteLine("=== MANUAL SCHEMA ===");
Console.WriteLine(manualSchema?.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine();
Console.WriteLine("=== GENERATED SCHEMA ===");
Console.WriteLine(generatedSchema?.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine();

// Basic comparison
if (manualSchema?.ToJsonString() == generatedSchema?.ToJsonString())
{
    Console.WriteLine("✓ Schemas are IDENTICAL");
    return 0;
}
else
{
    Console.WriteLine("✗ Schemas are DIFFERENT");
    return 1;
}
