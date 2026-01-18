#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.CodeAnalysis.CSharp, 4.8.0"
#r "bin/Debug/net8.0/PocketLawyer.dll"

using PocketLawyer.Documentation.Extraction;
using PocketLawyer.Documentation.Generation;

// Create the generator
var generator = new ReadmeDocGenerator();
var projectAnalyzer = new ProjectFileAnalyzer();

// Analyze the project file
var projectInfo = projectAnalyzer.AnalyzeProjectFile("PocketLawyer.csproj");

// Generate the README
var projectRoot = Directory.GetCurrentDirectory();
var readmeContent = generator.GenerateReadmeDocumentation(projectInfo, projectRoot);

// Write to file
generator.WriteToFile("README.md");

Console.WriteLine("README.md generated successfully!");
Console.WriteLine($"File size: {new FileInfo("README.md").Length} bytes");
Console.WriteLine("\nFirst 500 characters:");
Console.WriteLine(readmeContent.Substring(0, Math.Min(500, readmeContent.Length)));
