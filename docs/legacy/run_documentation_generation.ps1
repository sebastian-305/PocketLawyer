# PowerShell script to run documentation generation
Write-Host "=".PadRight(80, '=')
Write-Host "POCKETLAWYER DOCUMENTATION GENERATION"
Write-Host "=".PadRight(80, '=')
Write-Host ""

# Get repository root
$repositoryRoot = Get-Location
$outputDirectory = Join-Path $repositoryRoot "docs"

Write-Host "Repository Root: $repositoryRoot"
Write-Host "Output Directory: $outputDirectory"
Write-Host ""

# Compile and run a simple C# script to generate documentation
$csharpCode = @"
using System;
using System.IO;
using PocketLawyer.Documentation.Generation;

var repositoryRoot = @"$repositoryRoot";
var outputDirectory = @"$outputDirectory";

Console.WriteLine("Starting documentation generation...");
Console.WriteLine();

var orchestrator = new DocumentationOrchestrator(repositoryRoot, outputDirectory);
var result = orchestrator.GenerateAllDocumentation();

var report = orchestrator.GenerateReport(result);
Console.WriteLine(report);

var reportPath = Path.Combine(outputDirectory, "GENERATION_REPORT.txt");
File.WriteAllText(reportPath, report);
Console.WriteLine("Report saved to: " + reportPath);

Environment.Exit(result.Success ? 0 : 1);
"@

# Save to temp file
$tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
$csharpCode | Out-File -FilePath $tempFile -Encoding UTF8

# Run with dotnet-script or compile and run
Write-Host "Compiling and running documentation generator..."
Write-Host ""

# Use dotnet run with inline code
dotnet run --project PocketLawyer.csproj -- generate-docs

# Clean up
Remove-Item $tempFile -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Documentation generation complete!"
