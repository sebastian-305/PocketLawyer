namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Language configuration for documentation generation.
/// Provides localized strings for German documentation.
/// </summary>
public class DocumentationLanguage
{
    public static DocumentationLanguage German => new()
    {
        // README sections
        ProjectTitle = "Klausel Klara",
        ProjectSubtitle = "KI-gestützte Vertragsanalyse für österreichische Arbeits- und Mietverträge",
        ProjectDescription = "Klausel Klara ist eine produktionsreife Webanwendung, die österreichische Arbeits- und Mietverträge mithilfe von KI analysiert. " +
                           "Entwickelt mit ASP.NET Core und unterstützt durch KI-Dienste, bietet sie umfassende Vertragsanalysen mit Echtzeit-Ergebnissen über WebSocket-Verbindungen.",
        
        // Section headers
        Features = "Funktionen",
        Prerequisites = "Voraussetzungen",
        QuickStart = "Schnellstart",
        Usage = "Verwendung",
        Documentation = "Dokumentation",
        Testing = "Tests",
        TechnologyStack = "Technologie-Stack",
        Contributing = "Mitwirken",
        License = "Lizenz",
        
        // Features list
        FeatureContractAnalysis = "Vertragsanalyse",
        FeatureContractAnalysisDesc = "Umfassende Analyse von Arbeits- und Mietverträgen",
        FeatureMultiFormat = "Multi-Format-Unterstützung",
        FeatureMultiFormatDesc = "Unterstützt PDF, Bilder (via OCR) und Textdateien",
        FeatureRealTime = "Echtzeit-Ergebnisse",
        FeatureRealTimeDesc = "WebSocket-basiertes Streaming der Analyseergebnisse",
        FeatureTiered = "Gestaffelte Analyse",
        FeatureTieredDesc = "Basis- und Premium-Analyseebenen für unterschiedliche Detailstufen",
        FeatureSchemaDriven = "Schema-gesteuertes UI",
        FeatureSchemaDrivenDesc = "Dynamisches Frontend-Rendering basierend auf JSON-Schemas",
        FeatureAustrianLaw = "Österreichischer Rechtskontext",
        FeatureAustrianLawDesc = "Spezialisiert auf österreichisches Arbeits- und Mietrecht",
        FeatureProductionReady = "Produktionsbereit",
        FeatureProductionReadyDesc = "Umfassende Tests, Logging und Fehlerbehandlung",
        
        // Prerequisites
        PrerequisitesIntro = "Bevor Sie beginnen, stellen Sie sicher, dass Folgendes installiert ist:",
        PrerequisiteDotNet = ".NET SDK",
        PrerequisiteDotNetDesc = "Version {0} oder höher (extrahiert aus {1})",
        PrerequisiteGit = "Git",
        PrerequisiteGitDesc = "Zum Klonen des Repositorys",
        PrerequisiteApiKeys = "KI-Service API-Schlüssel",
        PrerequisiteApiKeysDesc = "API-Schlüssel für Nebius, Claude oder Gemini KI-Dienste",
        DownloadDotNet = ".NET SDK herunterladen",
        DownloadDotNetIntro = "Falls Sie .NET {0} SDK nicht installiert haben, laden Sie es hier herunter:",
        VerifyInstallation = "Überprüfen Sie Ihre Installation:",
        
        // Quick Start
        QuickStartIntro = "Folgen Sie diesen Schritten, um Klausel Klara auf Ihrem lokalen Rechner zu starten:",
        StepCloneRepo = "1. Repository klonen",
        StepRestoreDeps = "2. Abhängigkeiten wiederherstellen",
        StepConfigureServices = "3. KI-Dienste konfigurieren",
        StepConfigureContext = "4. Kontextdateien einrichten",
        StepRunApp = "5. Anwendung starten",
        StepOpenBrowser = "6. Browser öffnen",
        
        // Configuration
        ConfigureServicesIntro = "Erstellen Sie eine {0} Datei im Projektverzeichnis:",
        ConfigureContextIntro = "Die Anwendung benötigt Kontextdateien für die KI-Analyse. Diese befinden sich im {0} Verzeichnis:",
        
        // Usage
        UsageIntro = "Nach dem Start der Anwendung können Sie:",
        UsageUploadContract = "Einen Vertrag hochladen (PDF, Bild oder Text)",
        UsageSelectType = "Den Vertragstyp auswählen (Arbeitsvertrag oder Mietvertrag)",
        UsageSelectTier = "Die Analyseebene wählen (Basis oder Premium)",
        UsageViewResults = "Die Analyseergebnisse in Echtzeit anzeigen",
        
        // Documentation
        DocumentationIntro = "Ausführliche Dokumentation finden Sie in den folgenden Dateien:",
        DocArchitecture = "Systemarchitektur und Design-Prinzipien",
        DocApiReference = "API-Endpunkte und WebSocket-Protokoll",
        DocDataModels = "DTO-Strukturen und Datenmodelle",
        DocConfiguration = "Konfigurationsoptionen und Einstellungen",
        DocDeployment = "Deployment-Anleitung und Systemanforderungen",
        
        // Testing
        TestingIntro = "Das Projekt enthält umfassende Tests:",
        RunAllTests = "Alle Tests ausführen:",
        RunUnitTests = "Nur Unit-Tests ausführen:",
        RunIntegrationTests = "Nur Integrationstests ausführen:",
        RunPropertyTests = "Property-Based Tests ausführen:",
        ViewCoverage = "Test-Coverage anzeigen:",
        
        // Technology Stack
        TechStackIntro = "Klausel Klara verwendet folgende Technologien:",
        TechBackend = "Backend",
        TechFrontend = "Frontend",
        TechAi = "KI-Dienste",
        TechTesting = "Testing",
        TechOther = "Weitere",
        
        // Contributing
        ContributingIntro = "Beiträge sind willkommen! Bitte beachten Sie:",
        ContributingFork = "Forken Sie das Repository",
        ContributingBranch = "Erstellen Sie einen Feature-Branch",
        ContributingCommit = "Committen Sie Ihre Änderungen",
        ContributingPush = "Pushen Sie zum Branch",
        ContributingPr = "Öffnen Sie einen Pull Request",
        
        // License
        LicenseIntro = "Dieses Projekt ist lizenziert unter der MIT-Lizenz. Siehe LICENSE-Datei für Details.",
        
        // Architecture
        ArchitectureTitle = "Architektur",
        ArchitectureOverview = "Übersicht",
        ArchitectureLayers = "Schichten",
        ArchitectureDependencies = "Abhängigkeiten",
        ArchitecturePrinciples = "Design-Prinzipien",
        ArchitectureDiagrams = "Diagramme",
        
        // API Reference
        ApiTitle = "API-Referenz",
        ApiWebSocketEndpoints = "WebSocket-Endpunkte",
        ApiHttpEndpoints = "HTTP-Endpunkte",
        ApiMessageFormats = "Nachrichtenformate",
        ApiErrorHandling = "Fehlerbehandlung",
        
        // Data Models
        DataModelsTitle = "Datenmodelle",
        DataModelsHierarchy = "DTO-Hierarchie",
        DataModelsEmployment = "Arbeitsvertrags-DTOs",
        DataModelsRental = "Mietvertrags-DTOs",
        DataModelsShared = "Gemeinsame DTOs",
        DataModelsEnums = "Enumerationen",
        
        // Configuration
        ConfigurationTitle = "Konfiguration",
        ConfigurationStructure = "appsettings.json Struktur",
        ConfigurationLogging = "Logging-Konfiguration",
        ConfigurationAiServices = "KI-Dienste-Konfiguration",
        ConfigurationPaths = "Pfad-Konfiguration",
        ConfigurationUpload = "Upload-Konfiguration",
        ConfigurationContext = "Kontext-Konfiguration",
        ConfigurationSecrets = "User Secrets",
        ConfigurationEnvVars = "Umgebungsvariablen",
        
        // Deployment
        DeploymentTitle = "Deployment",
        DeploymentRequirements = "Systemanforderungen",
        DeploymentInstallation = "Installation",
        DeploymentConfiguration = "Konfiguration",
        DeploymentContextFiles = "Kontextdateien",
        DeploymentRunning = "Anwendung ausführen",
        DeploymentHealthChecks = "Health Checks",
        
        // Common terms
        Example = "Beispiel",
        Note = "Hinweis",
        Warning = "Warnung",
        Important = "Wichtig",
        SeeAlso = "Siehe auch",
        MoreInfo = "Weitere Informationen",
        Required = "Erforderlich",
        Optional = "Optional",
        Default = "Standard",
        Type = "Typ",
        Description = "Beschreibung",
        Properties = "Eigenschaften",
        Methods = "Methoden",
        Parameters = "Parameter",
        Returns = "Rückgabewert",
        Throws = "Wirft",
        Inherits = "Erbt von",
        Implements = "Implementiert",
        Namespace = "Namespace",
        Assembly = "Assembly",
        Version = "Version",
        Author = "Autor",
        Date = "Datum",
        Status = "Status",
        Path = "Pfad",
        File = "Datei",
        Directory = "Verzeichnis",
        Command = "Befehl",
        Output = "Ausgabe",
        Input = "Eingabe",
        Request = "Anfrage",
        Response = "Antwort",
        Error = "Fehler",
        Success = "Erfolg",
        Failed = "Fehlgeschlagen",
        Passed = "Bestanden",
        Skipped = "Übersprungen",
        Total = "Gesamt",
        
        // API Reference specific terms
        Endpoints = "Endpunkte",
        HttpMethod = "HTTP-Methode",
        Route = "Route",
        RequestBody = "Request-Body",
        ResponseBody = "Response-Body",
        Source = "Quelle",
        Accessors = "Zugriffsmethoden",
        Attributes = "Attribute",
        Modifiers = "Modifikatoren",
        InheritsFrom = "Erbt von",
        
        // Data Models specific terms
        Name = "Name",
        Values = "Werte",
        Value = "Wert",
        Class = "Klasse",
        Interface = "Schnittstelle",
        Record = "Record",
        Enum = "Enumeration",
        
        // Additional common terms
        Overview = "Übersicht",
        TableOfContents = "Inhaltsverzeichnis",
        BaseUrl = "Basis-URL",
        Authentication = "Authentifizierung",
        MessageFormats = "Nachrichtenformate",
        ErrorHandling = "Fehlerbehandlung",
        Diagrams = "Diagramme"
    };

    // Properties for all localized strings
    public string ProjectTitle { get; init; } = string.Empty;
    public string ProjectSubtitle { get; init; } = string.Empty;
    public string ProjectDescription { get; init; } = string.Empty;
    public string Features { get; init; } = string.Empty;
    public string Prerequisites { get; init; } = string.Empty;
    public string QuickStart { get; init; } = string.Empty;
    public string Usage { get; init; } = string.Empty;
    public string Documentation { get; init; } = string.Empty;
    public string Testing { get; init; } = string.Empty;
    public string TechnologyStack { get; init; } = string.Empty;
    public string Contributing { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    
    public string FeatureContractAnalysis { get; init; } = string.Empty;
    public string FeatureContractAnalysisDesc { get; init; } = string.Empty;
    public string FeatureMultiFormat { get; init; } = string.Empty;
    public string FeatureMultiFormatDesc { get; init; } = string.Empty;
    public string FeatureRealTime { get; init; } = string.Empty;
    public string FeatureRealTimeDesc { get; init; } = string.Empty;
    public string FeatureTiered { get; init; } = string.Empty;
    public string FeatureTieredDesc { get; init; } = string.Empty;
    public string FeatureSchemaDriven { get; init; } = string.Empty;
    public string FeatureSchemaDrivenDesc { get; init; } = string.Empty;
    public string FeatureAustrianLaw { get; init; } = string.Empty;
    public string FeatureAustrianLawDesc { get; init; } = string.Empty;
    public string FeatureProductionReady { get; init; } = string.Empty;
    public string FeatureProductionReadyDesc { get; init; } = string.Empty;
    
    public string PrerequisitesIntro { get; init; } = string.Empty;
    public string PrerequisiteDotNet { get; init; } = string.Empty;
    public string PrerequisiteDotNetDesc { get; init; } = string.Empty;
    public string PrerequisiteGit { get; init; } = string.Empty;
    public string PrerequisiteGitDesc { get; init; } = string.Empty;
    public string PrerequisiteApiKeys { get; init; } = string.Empty;
    public string PrerequisiteApiKeysDesc { get; init; } = string.Empty;
    public string DownloadDotNet { get; init; } = string.Empty;
    public string DownloadDotNetIntro { get; init; } = string.Empty;
    public string VerifyInstallation { get; init; } = string.Empty;
    
    public string QuickStartIntro { get; init; } = string.Empty;
    public string StepCloneRepo { get; init; } = string.Empty;
    public string StepRestoreDeps { get; init; } = string.Empty;
    public string StepConfigureServices { get; init; } = string.Empty;
    public string StepConfigureContext { get; init; } = string.Empty;
    public string StepRunApp { get; init; } = string.Empty;
    public string StepOpenBrowser { get; init; } = string.Empty;
    
    public string ConfigureServicesIntro { get; init; } = string.Empty;
    public string ConfigureContextIntro { get; init; } = string.Empty;
    
    public string UsageIntro { get; init; } = string.Empty;
    public string UsageUploadContract { get; init; } = string.Empty;
    public string UsageSelectType { get; init; } = string.Empty;
    public string UsageSelectTier { get; init; } = string.Empty;
    public string UsageViewResults { get; init; } = string.Empty;
    
    public string DocumentationIntro { get; init; } = string.Empty;
    public string DocArchitecture { get; init; } = string.Empty;
    public string DocApiReference { get; init; } = string.Empty;
    public string DocDataModels { get; init; } = string.Empty;
    public string DocConfiguration { get; init; } = string.Empty;
    public string DocDeployment { get; init; } = string.Empty;
    
    public string TestingIntro { get; init; } = string.Empty;
    public string RunAllTests { get; init; } = string.Empty;
    public string RunUnitTests { get; init; } = string.Empty;
    public string RunIntegrationTests { get; init; } = string.Empty;
    public string RunPropertyTests { get; init; } = string.Empty;
    public string ViewCoverage { get; init; } = string.Empty;
    
    public string TechStackIntro { get; init; } = string.Empty;
    public string TechBackend { get; init; } = string.Empty;
    public string TechFrontend { get; init; } = string.Empty;
    public string TechAi { get; init; } = string.Empty;
    public string TechTesting { get; init; } = string.Empty;
    public string TechOther { get; init; } = string.Empty;
    
    public string ContributingIntro { get; init; } = string.Empty;
    public string ContributingFork { get; init; } = string.Empty;
    public string ContributingBranch { get; init; } = string.Empty;
    public string ContributingCommit { get; init; } = string.Empty;
    public string ContributingPush { get; init; } = string.Empty;
    public string ContributingPr { get; init; } = string.Empty;
    
    public string LicenseIntro { get; init; } = string.Empty;
    
    // Architecture
    public string ArchitectureTitle { get; init; } = string.Empty;
    public string ArchitectureOverview { get; init; } = string.Empty;
    public string ArchitectureLayers { get; init; } = string.Empty;
    public string ArchitectureDependencies { get; init; } = string.Empty;
    public string ArchitecturePrinciples { get; init; } = string.Empty;
    public string ArchitectureDiagrams { get; init; } = string.Empty;
    
    // API Reference
    public string ApiTitle { get; init; } = string.Empty;
    public string ApiWebSocketEndpoints { get; init; } = string.Empty;
    public string ApiHttpEndpoints { get; init; } = string.Empty;
    public string ApiMessageFormats { get; init; } = string.Empty;
    public string ApiErrorHandling { get; init; } = string.Empty;
    
    // Data Models
    public string DataModelsTitle { get; init; } = string.Empty;
    public string DataModelsHierarchy { get; init; } = string.Empty;
    public string DataModelsEmployment { get; init; } = string.Empty;
    public string DataModelsRental { get; init; } = string.Empty;
    public string DataModelsShared { get; init; } = string.Empty;
    public string DataModelsEnums { get; init; } = string.Empty;
    
    // Configuration
    public string ConfigurationTitle { get; init; } = string.Empty;
    public string ConfigurationStructure { get; init; } = string.Empty;
    public string ConfigurationLogging { get; init; } = string.Empty;
    public string ConfigurationAiServices { get; init; } = string.Empty;
    public string ConfigurationPaths { get; init; } = string.Empty;
    public string ConfigurationUpload { get; init; } = string.Empty;
    public string ConfigurationContext { get; init; } = string.Empty;
    public string ConfigurationSecrets { get; init; } = string.Empty;
    public string ConfigurationEnvVars { get; init; } = string.Empty;
    
    // Deployment
    public string DeploymentTitle { get; init; } = string.Empty;
    public string DeploymentRequirements { get; init; } = string.Empty;
    public string DeploymentInstallation { get; init; } = string.Empty;
    public string DeploymentConfiguration { get; init; } = string.Empty;
    public string DeploymentContextFiles { get; init; } = string.Empty;
    public string DeploymentRunning { get; init; } = string.Empty;
    public string DeploymentHealthChecks { get; init; } = string.Empty;
    
    // Common terms
    public string Example { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public string Warning { get; init; } = string.Empty;
    public string Important { get; init; } = string.Empty;
    public string SeeAlso { get; init; } = string.Empty;
    public string MoreInfo { get; init; } = string.Empty;
    public string Required { get; init; } = string.Empty;
    public string Optional { get; init; } = string.Empty;
    public string Default { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Properties { get; init; } = string.Empty;
    public string Methods { get; init; } = string.Empty;
    public string Parameters { get; init; } = string.Empty;
    public string Returns { get; init; } = string.Empty;
    public string Throws { get; init; } = string.Empty;
    public string Inherits { get; init; } = string.Empty;
    public string Implements { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public string Assembly { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string File { get; init; } = string.Empty;
    public string Directory { get; init; } = string.Empty;
    public string Command { get; init; } = string.Empty;
    public string Output { get; init; } = string.Empty;
    public string Input { get; init; } = string.Empty;
    public string Request { get; init; } = string.Empty;
    public string Response { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
    public string Success { get; init; } = string.Empty;
    public string Failed { get; init; } = string.Empty;
    public string Passed { get; init; } = string.Empty;
    public string Skipped { get; init; } = string.Empty;
    public string Total { get; init; } = string.Empty;
    
    // API Reference specific terms
    public string Endpoints { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string RequestBody { get; init; } = string.Empty;
    public string ResponseBody { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Accessors { get; init; } = string.Empty;
    public string Attributes { get; init; } = string.Empty;
    public string Modifiers { get; init; } = string.Empty;
    public string InheritsFrom { get; init; } = string.Empty;
    
    // Data Models specific terms
    public string Name { get; init; } = string.Empty;
    public string Values { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Class { get; init; } = string.Empty;
    public string Interface { get; init; } = string.Empty;
    public string Record { get; init; } = string.Empty;
    public string Enum { get; init; } = string.Empty;
    
    // Additional common terms
    public string Overview { get; init; } = string.Empty;
    public string TableOfContents { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string Authentication { get; init; } = string.Empty;
    public string MessageFormats { get; init; } = string.Empty;
    public string ErrorHandling { get; init; } = string.Empty;
    public string Diagrams { get; init; } = string.Empty;
}
