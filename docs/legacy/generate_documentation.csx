#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.CodeAnalysis.CSharp, 4.8.0"
#r "nuget: System.Text.Json, 8.0.0"

using System;
using System.IO;
using PocketLawyer.Documentation.Generation;

// Get repository root (current directory)
var repositoryRoot = Directory.GetCurrentDirectory();
var outputDirectory = Path.Combine(repositoryRoot, "docs");

Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("POCKETLAWYER DOCUMENTATION GENERATION");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine();
Console.WriteLine($"Repository Root: {repositoryRoot}");
Console.WriteLine($"Output Directory: {outputDirectory}");
Console.WriteLine();

// Create orchestrator
var orchestrator = new DocumentationOrchestrator(repositoryRoot, outputDirectory);

// Generate all documentation
Console.WriteLine("Starting documentation generation...");
Console.WriteLine();

var result = orchestrator.GenerateAllDocumentation();

// Generate and display report
var report = orchestrator.GenerateReport(result);
Console.WriteLine(report);

// Exit with appropriate code
Environment.Exit(result.Success ? 0 : 1);
