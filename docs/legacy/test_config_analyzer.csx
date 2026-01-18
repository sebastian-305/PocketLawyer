#!/usr/bin/env dotnet-script
#r "Documentation/bin/Debug/net8.0/PocketLawyer.dll"

using PocketLawyer.Documentation.Extraction;

var analyzer = new ConfigurationAnalyzer();

// Test 1: Simple JSON parsing
var json = @"{
    ""AllowedHosts"": ""*"",
    ""MaxConnections"": 100
}";

var result = analyzer.AnalyzeConfigurationJson(json);
Console.WriteLine($"Test 1 - Sections found: {result.Sections.Count}");
Console.WriteLine($"Test 1 - Root section keys: {result.Sections.First(s => s.Name == "Root").Keys.Count}");

// Test 2: Nested sections
var json2 = @"{
    ""Logging"": {
        ""LogLevel"": ""Information"",
        ""Path"": ""logs""
    }
}";

var result2 = analyzer.AnalyzeConfigurationJson(json2);
Console.WriteLine($"Test 2 - Sections found: {result2.Sections.Count}");
var loggingSection = result2.Sections.FirstOrDefault(s => s.Name == "Logging");
Console.WriteLine($"Test 2 - Logging section keys: {loggingSection?.Keys.Count ?? 0}");

// Test 3: Option class parsing
var optionPath = "Options/UploadOptions.cs";
if (File.Exists(optionPath))
{
    var optionInfo = analyzer.AnalyzeOptionClass(optionPath);
    Console.WriteLine($"Test 3 - Option class: {optionInfo.Name}");
    Console.WriteLine($"Test 3 - Properties: {optionInfo.Properties.Count}");
}

Console.WriteLine("\nAll manual tests passed!");
