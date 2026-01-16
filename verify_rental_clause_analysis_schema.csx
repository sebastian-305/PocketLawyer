#!/usr/bin/env dotnet-script
#r "nuget: System.Text.Json, 8.0.0"

using System.Text.Json;
using System.Text.Json.Nodes;

// Load the compiled assembly
var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net8.0", "PocketLawyer.dll");
if (!File.Exists(assemblyPath))
{
    Console.WriteLine("Assembly not found. Building project...");
    var buildResult = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = "build --configuration Debug",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    });
    buildResult?.WaitForExit();
    
    if (!File.Exists(assemblyPath))
    {
        Console.WriteLine("Build failed or assembly not found at: " + assemblyPath);
        return;
    }
}

var assembly = System.Reflection.Assembly.LoadFrom(assemblyPath);

// Get the ClauseAnalysisSchemaProvider type
var providerType = assembly.GetType("PocketLawyer.Infrastructure.SchemaProviders.Rental.Premium.ClauseAnalysisSchemaProvider");
if (providerType == null)
{
    Console.WriteLine("ClauseAnalysisSchemaProvider type not found");
    return;
}

// Create an instance
var provider = Activator.CreateInstance(providerType);
if (provider == null)
{
    Console.WriteLine("Failed to create provider instance");
    return;
}

// Get the GetSchema method
var getSchemaMethod = providerType.GetMethod("GetSchema", new[] { typeof(string) });
if (getSchemaMethod == null)
{
    Console.WriteLine("GetSchema method not found");
    return;
}

Console.WriteLine("=== Rental ClauseAnalysisSchemaProvider Schema Verification ===\n");

// Test all step names
var stepNames = new[] { "Costs", "Terms", "Use", "Maintenance", "Duties", "Privacy" };

foreach (var stepName in stepNames)
{
    Console.WriteLine($"--- Step: {stepName} ---");
    
    var schema = getSchemaMethod.Invoke(provider, new object[] { stepName }) as JsonObject;
    if (schema == null)
    {
        Console.WriteLine("Failed to get schema");
        continue;
    }
    
    // Pretty print the schema
    var options = new JsonSerializerOptions 
    { 
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    Console.WriteLine(JsonSerializer.Serialize(schema, options));
    Console.WriteLine();
}

// Verify key properties
Console.WriteLine("=== Verification Summary ===");
var costsSchema = getSchemaMethod.Invoke(provider, new object[] { "Costs" }) as JsonObject;
if (costsSchema != null)
{
    Console.WriteLine($"✓ Schema type: {costsSchema["type"]}");
    Console.WriteLine($"✓ Additional properties: {costsSchema["additionalProperties"]}");
    Console.WriteLine($"✓ Section title: {costsSchema["x-sectionTitle"]}");
    
    var properties = costsSchema["properties"] as JsonObject;
    if (properties != null)
    {
        Console.WriteLine($"✓ Number of properties: {properties.Count}");
        Console.WriteLine($"✓ Properties: {string.Join(", ", properties.Select(p => p.Key))}");
    }
    
    var required = costsSchema["required"] as JsonArray;
    if (required != null)
    {
        Console.WriteLine($"✓ Required fields: {string.Join(", ", required.Select(r => r?.ToString()))}");
    }
    
    // Check Rental-specific customizations
    var rechtsgrundlage = properties?["rechtsgrundlage"] as JsonObject;
    if (rechtsgrundlage != null)
    {
        var description = rechtsgrundlage["description"]?.ToString();
        Console.WriteLine($"✓ Rechtsgrundlage description contains MRG: {description?.Contains("MRG")}");
        Console.WriteLine($"✓ Rechtsgrundlage description contains ABGB: {description?.Contains("ABGB")}");
    }
    
    var risikoScore = properties?["risikoScore"] as JsonObject;
    if (risikoScore != null)
    {
        var enumValues = risikoScore["enum"] as JsonArray;
        Console.WriteLine($"✓ RisikoScore enum values: {string.Join(", ", enumValues?.Select(e => e?.ToString()) ?? Array.Empty<string>())}");
        var description = risikoScore["description"]?.ToString();
        Console.WriteLine($"✓ RisikoScore description contains 'Mieter': {description?.Contains("Mieter")}");
    }
    
    var riskLevel = properties?["riskLevel"] as JsonObject;
    if (riskLevel != null)
    {
        var enumValues = riskLevel["enum"] as JsonArray;
        Console.WriteLine($"✓ RiskLevel enum values: {string.Join(", ", enumValues?.Select(e => e?.ToString()) ?? Array.Empty<string>())}");
    }
}

Console.WriteLine("\n=== Schema Generation Complete ===");
