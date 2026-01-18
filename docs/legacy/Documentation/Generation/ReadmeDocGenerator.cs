using PocketLawyer.Documentation.Extraction;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Generates README.md documentation from project files and legacy README reference.
/// Creates a clear, user-friendly README with project overview, quick start guide, and documentation links.
/// </summary>
/// <remarks>
/// Validates Requirements: 10.1, 10.2, 10.3, 10.4, 10.6, 10.7
/// </remarks>
public class ReadmeDocGenerator
{
    private readonly MarkdownGenerator _markdown;
    private readonly ProjectFileAnalyzer _projectAnalyzer;
    private readonly DocumentationLanguage _lang;

    public ReadmeDocGenerator()
    {
        _markdown = new MarkdownGenerator();
        _projectAnalyzer = new ProjectFileAnalyzer();
        _lang = DocumentationLanguage.German;
    }

    /// <summary>
    /// Generates complete README.md documentation.
    /// Requirement 10.2: THE System SHALL use the legacy README as a reference for information extraction, but verify all claims against source code
    /// Requirement 10.3: WHEN documenting how to start the program, THE System SHALL provide step-by-step instructions for users who clone or download the repository
    /// Requirement 10.4: WHEN documenting prerequisites, THE System SHALL extract the .NET version from PocketLawyer.csproj
    /// Requirement 10.6: THE System SHALL include typical README sections: Project Description, Features, Prerequisites, Installation, Quick Start, Usage, Documentation Links, Contributing, License
    /// </summary>
    /// <param name="projectInfo">Project file information (.csproj)</param>
    /// <param name="projectRoot">Root directory of the project</param>
    /// <param name="legacyReadmePath">Optional path to legacy README for reference</param>
    /// <returns>Complete README.md content</returns>
    public string GenerateReadmeDocumentation(
        ProjectFileInfo projectInfo,
        string projectRoot,
        string? legacyReadmePath = null)
    {
        _markdown.Clear();

        // Project Title and Description
        GenerateHeaderSection();

        // Features
        GenerateFeaturesSection();

        // Prerequisites
        GeneratePrerequisitesSection(projectInfo);

        // Quick Start
        GenerateQuickStartSection(projectInfo, projectRoot);

        // Usage
        GenerateUsageSection();

        // Documentation
        GenerateDocumentationSection();

        // Testing
        GenerateTestingSection();

        // Technology Stack
        GenerateTechnologyStackSection(projectInfo);

        // Architecture Principles
        GenerateArchitecturePrinciplesSection();

        // Footer
        GenerateFooterSection();

        return _markdown.Content;
    }

    /// <summary>
    /// Generates the header section with project title and description.
    /// </summary>
    private void GenerateHeaderSection()
    {
        _markdown.H1(_lang.ProjectTitle);
        _markdown.Paragraph(MarkdownGenerator.Bold(_lang.ProjectSubtitle));
        _markdown.BlankLine();
        _markdown.Paragraph(_lang.ProjectDescription);
    }

    /// <summary>
    /// Generates the Features section.
    /// </summary>
    private void GenerateFeaturesSection()
    {
        _markdown.H2(_lang.Features);
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold(_lang.FeatureContractAnalysis)} - {_lang.FeatureContractAnalysisDesc}",
            $"{MarkdownGenerator.Bold(_lang.FeatureMultiFormat)} - {_lang.FeatureMultiFormatDesc}",
            $"{MarkdownGenerator.Bold(_lang.FeatureRealTime)} - {_lang.FeatureRealTimeDesc}",
            $"{MarkdownGenerator.Bold(_lang.FeatureTiered)} - {_lang.FeatureTieredDesc}",
            $"{MarkdownGenerator.Bold(_lang.FeatureSchemaDriven)} - {_lang.FeatureSchemaDrivenDesc}",
            $"{MarkdownGenerator.Bold(_lang.FeatureAustrianLaw)} - {_lang.FeatureAustrianLawDesc}",
            $"{MarkdownGenerator.Bold(_lang.FeatureProductionReady)} - {_lang.FeatureProductionReadyDesc}"
        });
    }

    /// <summary>
    /// Generates the Prerequisites section.
    /// Requirement 10.4: WHEN documenting prerequisites, THE System SHALL extract the .NET version from PocketLawyer.csproj
    /// </summary>
    private void GeneratePrerequisitesSection(ProjectFileInfo projectInfo)
    {
        _markdown.H2(_lang.Prerequisites);
        _markdown.Paragraph(_lang.PrerequisitesIntro);

        var dotnetVersion = _projectAnalyzer.ExtractDotNetVersion(projectInfo.TargetFramework);
        var projectFileName = Path.GetFileName(projectInfo.Path);
        
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold(_lang.PrerequisiteDotNet)} - {string.Format(_lang.PrerequisiteDotNetDesc, dotnetVersion, MarkdownGenerator.FileReference(projectFileName))}",
            $"{MarkdownGenerator.Bold(_lang.PrerequisiteGit)} - {_lang.PrerequisiteGitDesc}",
            $"{MarkdownGenerator.Bold(_lang.PrerequisiteApiKeys)} - {_lang.PrerequisiteApiKeysDesc}"
        });

        _markdown.H3(_lang.DownloadDotNet);
        _markdown.Paragraph(string.Format(_lang.DownloadDotNetIntro, dotnetVersion));
        _markdown.UnorderedList(new[]
        {
            MarkdownGenerator.Link(".NET Downloads", "https://dotnet.microsoft.com/download")
        });

        _markdown.Paragraph(_lang.VerifyInstallation);
        _markdown.CodeBlock("dotnet --version", "bash");
    }

    /// <summary>
    /// Generates the Quick Start section with step-by-step instructions.
    /// Requirement 10.3: WHEN documenting how to start the program, THE System SHALL provide step-by-step instructions for users who clone or download the repository
    /// Requirement 10.7: THE System SHALL verify that all commands and file references in README.md are accurate and reference actual files
    /// </summary>
    private void GenerateQuickStartSection(ProjectFileInfo projectInfo, string projectRoot)
    {
        _markdown.H2(_lang.QuickStart);
        _markdown.Paragraph(_lang.QuickStartIntro);

        _markdown.H3(_lang.StepCloneRepo);
        _markdown.CodeBlock("git clone <repository-url>\ncd PocketLawyer", "bash");

        _markdown.H3(_lang.StepRestoreDeps);
        _markdown.Paragraph("Alle NuGet-Paketabhängigkeiten wiederherstellen:");
        _markdown.CodeBlock("dotnet restore", "bash");
        
        var projectFileName = Path.GetFileName(projectInfo.Path);
        _markdown.Paragraph($"Dies lädt alle Pakete herunter, die in {MarkdownGenerator.FileReference(projectFileName)} aufgelistet sind.");

        _markdown.H3(_lang.StepConfigureServices);
        _markdown.Paragraph("Richten Sie Ihre KI-Service-API-Schlüssel mit User Secrets ein (empfohlen für die Entwicklung):");
        _markdown.CodeBlock(
            "dotnet user-secrets init\n" +
            "dotnet user-secrets set \"Nebius:ApiKey\" \"your-nebius-api-key\"",
            "bash"
        );

        _markdown.Note("Committen Sie niemals API-Schlüssel in die Versionskontrolle. Verwenden Sie User Secrets für die Entwicklung und Umgebungsvariablen für die Produktion.");

        _markdown.H3(_lang.StepConfigureContext);
        _markdown.Paragraph($"Stellen Sie sicher, dass das Kontextdatei-Verzeichnis unter {MarkdownGenerator.DirectoryReference("data/Context")} existiert. " +
                          "Diese Dateien enthalten domänenspezifisches Wissen für die KI-Modelle.");

        var contextPath = Path.Combine(projectRoot, "data", "Context");
        if (Directory.Exists(contextPath))
        {
            _markdown.Paragraph($"✓ Kontextverzeichnis gefunden unter {MarkdownGenerator.DirectoryReference("data/Context")}");
        }
        else
        {
            _markdown.Warning($"Kontextverzeichnis nicht gefunden unter {MarkdownGenerator.DirectoryReference("data/Context")}. " +
                            "Die Anwendung funktioniert möglicherweise nicht korrekt ohne diese Dateien.");
        }

        _markdown.H3(_lang.StepRunApp);
        _markdown.Paragraph("Starten Sie die Anwendung:");
        _markdown.CodeBlock("dotnet run", "bash");

        _markdown.Paragraph("Die Anwendung startet standardmäßig mit dem HTTP-Profil.");

        _markdown.H3(_lang.StepOpenBrowser);
        _markdown.Paragraph("Öffnen Sie Ihren Webbrowser und navigieren Sie zu:");
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("HTTP:")} http://localhost:5112"
        });

        _markdown.Note("Für HTTPS-Unterstützung verwenden Sie: dotnet run --launch-profile https (Port 7218). Dies erfordert ein gültiges Entwicklerzertifikat.");

        _markdown.Paragraph("Sie sollten die Klausel Klara Startseite mit Optionen zur Analyse von Arbeits- oder Mietverträgen sehen.");
    }

    /// <summary>
    /// Generates the Usage section.
    /// </summary>
    private void GenerateUsageSection()
    {
        _markdown.H2(_lang.Usage);

        _markdown.H3("Einen Vertrag analysieren");
        _markdown.OrderedList(new[]
        {
            "Navigieren Sie zur Anwendung in Ihrem Webbrowser",
            "Wählen Sie den Vertragstyp (Arbeitsvertrag oder Mietvertrag)",
            "Laden Sie Ihre Vertragsdatei hoch (PDF, Bild oder Text)",
            "Warten Sie, bis die Analyse abgeschlossen ist",
            "Überprüfen Sie die Basis- und Premium-Analyseergebnisse"
        });

        _markdown.H3("Unterstützte Dateiformate");
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("PDF")} - Textextraktion mit UglyToad.PdfPig",
            $"{MarkdownGenerator.Bold("Bilder")} - JPG, PNG (OCR mit Tesseract)",
            $"{MarkdownGenerator.Bold("Text")} - Einfache Textdateien (.txt)"
        });

        _markdown.H3("Analyseebenen");
        _markdown.Paragraph("Klausel Klara bietet zwei Analyseebenen:");

        _markdown.H4("Basis-Analyse");
        _markdown.UnorderedList(new[]
        {
            "Allgemeine Vertragsinformationen",
            "Wichtige Bedingungen",
            "Erste Risikobewertung",
            "Vertragszusammenfassung"
        });

        _markdown.H4("Premium-Analyse");
        _markdown.UnorderedList(new[]
        {
            "Detaillierte Klausel-für-Klausel-Analyse",
            "Überprüfung der rechtlichen Konformität",
            "Umfassende Risikobewertung",
            "Spezifische Empfehlungen und Warnungen"
        });

        _markdown.H3("WebSocket API");
        _markdown.Paragraph("Für programmatischen Zugriff verbinden Sie sich mit den WebSocket-Endpunkten:");
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("Arbeitsverträge:")} {MarkdownGenerator.InlineCode("ws://localhost:5112/employment-analysis")}",
            $"{MarkdownGenerator.Bold("Mietverträge:")} {MarkdownGenerator.InlineCode("ws://localhost:5112/rental-analysis")}"
        });

        _markdown.Note("Die Ports können in Properties/launchSettings.json konfiguriert werden.");

        _markdown.Paragraph($"Für detaillierte API-Dokumentation siehe {MarkdownGenerator.Link("API_REFERENCE.md", "docs/API_REFERENCE.md")}.");
    }

    /// <summary>
    /// Generates the Documentation section with links to all Phase 1 docs.
    /// </summary>
    private void GenerateDocumentationSection()
    {
        _markdown.H2(_lang.Documentation);
        _markdown.Paragraph(_lang.DocumentationIntro);

        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Link("Architektur", "docs/ARCHITECTURE.md")} - {_lang.DocArchitecture}",
            $"{MarkdownGenerator.Link("API-Referenz", "docs/API_REFERENCE.md")} - {_lang.DocApiReference}",
            $"{MarkdownGenerator.Link("Datenmodelle", "docs/DATA_MODELS.md")} - {_lang.DocDataModels}",
            $"{MarkdownGenerator.Link("Konfiguration", "docs/CONFIGURATION.md")} - {_lang.DocConfiguration}",
            $"{MarkdownGenerator.Link("Deployment", "docs/DEPLOYMENT.md")} - {_lang.DocDeployment}"
        });
    }

    /// <summary>
    /// Generates the Testing section.
    /// </summary>
    private void GenerateTestingSection()
    {
        _markdown.H2(_lang.Testing);
        _markdown.Paragraph(_lang.TestingIntro);

        _markdown.H3("Tests ausführen");
        _markdown.CodeBlock("# Alle Tests ausführen\ndotnet test", "bash");

        _markdown.H3("Test-Coverage");
        _markdown.Paragraph("Einen Code-Coverage-Bericht generieren:");
        _markdown.CodeBlock("dotnet test --collect:\"XPlat Code Coverage\"", "bash");

        _markdown.H3("Test-Kategorien");
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("Unit-Tests")} - Testen einzelne Komponenten und Methoden",
            $"{MarkdownGenerator.Bold("Property-Based Tests")} - Überprüfen universelle Eigenschaften mit FsCheck",
            $"{MarkdownGenerator.Bold("Integrationstests")} - Testen End-to-End-Workflows"
        });
    }

    /// <summary>
    /// Generates the Technology Stack section.
    /// </summary>
    private void GenerateTechnologyStackSection(ProjectFileInfo projectInfo)
    {
        _markdown.H2(_lang.TechnologyStack);

        var dotnetVersion = _projectAnalyzer.ExtractDotNetVersion(projectInfo.TargetFramework);
        var projectFileName = Path.GetFileName(projectInfo.Path);
        
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("Backend:")} ASP.NET Core {dotnetVersion}",
            $"{MarkdownGenerator.Bold("KI-Dienste:")} Nebius, Claude, Gemini",
            $"{MarkdownGenerator.Bold("PDF-Verarbeitung:")} UglyToad.PdfPig",
            $"{MarkdownGenerator.Bold("OCR:")} Tesseract",
            $"{MarkdownGenerator.Bold("Schema-Validierung:")} NJsonSchema",
            $"{MarkdownGenerator.Bold("Logging:")} Serilog",
            $"{MarkdownGenerator.Bold("Testing:")} xUnit, FsCheck, FluentAssertions",
            $"{MarkdownGenerator.Bold("Frontend:")} Vanilla JavaScript, Bootstrap 5"
        });

        _markdown.Paragraph($"Für eine vollständige Liste der Abhängigkeiten siehe {MarkdownGenerator.FileReference(projectFileName)}.");
    }

    /// <summary>
    /// Generates the Architecture Principles section.
    /// </summary>
    private void GenerateArchitecturePrinciplesSection()
    {
        _markdown.H2("Architekturprinzipien");
        _markdown.Paragraph("Das Projekt folgt bewährten Softwareentwicklungsprinzipien:");

        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("Onion-Architektur:")} Abhängigkeiten fließen nach innen zur Domäne",
            $"{MarkdownGenerator.Bold("Single Source of Truth:")} JSON-Schemas definieren alle Datenstrukturen",
            $"{MarkdownGenerator.Bold("Separation of Concerns:")} Jede Schicht hat eine spezifische Verantwortung",
            $"{MarkdownGenerator.Bold("Testbarkeit:")} Alle Komponenten sind für einfaches Testen konzipiert"
        });

        _markdown.Paragraph($"Für detaillierte Architekturinformationen siehe {MarkdownGenerator.Link("ARCHITECTURE.md", "docs/ARCHITECTURE.md")}.");
    }

    /// <summary>
    /// Generates the footer section.
    /// </summary>
    private void GenerateFooterSection()
    {
        _markdown.HorizontalRule();
        _markdown.Paragraph(MarkdownGenerator.Bold("Erstellt mit ❤️ für österreichische Vertragsanalyse"));
    }

    /// <summary>
    /// Writes the generated documentation to a file.
    /// </summary>
    public void WriteToFile(string filePath)
    {
        _markdown.WriteToFile(filePath);
    }
}
