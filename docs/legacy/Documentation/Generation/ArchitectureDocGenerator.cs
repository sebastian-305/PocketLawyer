using PocketLawyer.Documentation.Extraction;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Generates ARCHITECTURE.md documentation from folder structure and source code analysis.
/// Documents layers (Web, Application, Domain, Infrastructure), design principles, and dependencies.
/// </summary>
/// <remarks>
/// Validates Requirements: 1.1, 1.2, 1.3, 1.4, 1.5
/// </remarks>
public class ArchitectureDocGenerator
{
    private readonly MarkdownGenerator _markdown;
    private readonly MermaidDiagramGenerator _diagramGenerator;
    private readonly DocumentationLanguage _lang;

    public ArchitectureDocGenerator()
    {
        _markdown = new MarkdownGenerator();
        _diagramGenerator = new MermaidDiagramGenerator();
        _lang = DocumentationLanguage.German;
    }

    /// <summary>
    /// Generates complete ARCHITECTURE.md documentation from project structure and source files.
    /// Requirement 1.1: WHEN creating ARCHITECTURE.md, THE System SHALL extract layer information from the actual folder structure
    /// </summary>
    /// <param name="projectRoot">Root directory of the project</param>
    /// <param name="sourceFiles">All source files from the project</param>
    /// <returns>Complete ARCHITECTURE.md content</returns>
    public string GenerateArchitectureDocumentation(string projectRoot, IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.Clear();

        // Header
        _markdown.H1(_lang.ArchitectureTitle);
        _markdown.Paragraph("Dieses Dokument beschreibt die Architektur des PocketLawyer-Systems. " +
                          "Alle Informationen werden aus dem tatsächlichen Quellcode und der Ordnerstruktur extrahiert.");

        // Table of Contents
        GenerateTableOfContents();

        // Overview
        GenerateOverviewSection(projectRoot);

        // Layers
        GenerateLayersSection(projectRoot, sourceFiles);

        // Dependencies
        GenerateDependenciesSection(sourceFiles);

        // Design Principles
        GenerateDesignPrinciplesSection(sourceFiles, projectRoot);

        // Diagrams
        GenerateDiagramsSection(projectRoot, sourceFiles);

        return _markdown.Content;
    }

    private void GenerateTableOfContents()
    {
        _markdown.H2(_lang.TableOfContents);
        _markdown.UnorderedList(new[]
        {
            MarkdownGenerator.AnchorLink(_lang.Overview, AnchorGenerator.Generate(_lang.Overview)),
            MarkdownGenerator.AnchorLink(_lang.ArchitectureLayers, AnchorGenerator.Generate(_lang.ArchitectureLayers)),
            MarkdownGenerator.AnchorLink(_lang.ArchitectureDependencies, AnchorGenerator.Generate(_lang.ArchitectureDependencies)),
            MarkdownGenerator.AnchorLink(_lang.ArchitecturePrinciples, AnchorGenerator.Generate(_lang.ArchitecturePrinciples)),
            MarkdownGenerator.AnchorLink(_lang.Diagrams, AnchorGenerator.Generate(_lang.Diagrams))
        });
    }

    private void GenerateOverviewSection(string projectRoot)
    {
        _markdown.H2(_lang.Overview);
        _markdown.Paragraph("PocketLawyer folgt einem Onion-Architecture-Muster (auch bekannt als Clean Architecture). " +
                          "Das System ist in verschiedene Schichten mit klarer Trennung der Verantwortlichkeiten und Abhängigkeitsregeln organisiert.");

        _markdown.H3("Architekturstil");
        _markdown.Paragraph("Die Architektur folgt diesen Schlüsselprinzipien:");
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("Onion Architecture:")} Abhängigkeiten fließen nach innen zum Domain-Kern",
            $"{MarkdownGenerator.Bold("Separation of Concerns:")} Jede Schicht hat eine spezifische Verantwortung",
            $"{MarkdownGenerator.Bold("Dependency Inversion:")} High-Level-Module hängen nicht von Low-Level-Modulen ab",
            $"{MarkdownGenerator.Bold("Domain-Centric:")} Geschäftslogik ist in der Domain-Schicht isoliert"
        });

        _markdown.H3("Schichtenstruktur");
        _markdown.Paragraph("Das System ist in vier Hauptschichten organisiert:");

        // Verify actual directories exist
        var layers = new Dictionary<string, string>
        {
            { "Domain", "Kern-Geschäftsentitäten und DTOs" },
            { "Application", "Geschäftslogik und Use Cases" },
            { "Infrastructure", "Externe Dienste und Implementierungen" },
            { "Web", "API-Controller und Präsentationsschicht" }
        };

        foreach (var layer in layers)
        {
            var layerPath = Path.Combine(projectRoot, layer.Key);
            if (Directory.Exists(layerPath))
            {
                _markdown.ListItem($"{MarkdownGenerator.Bold(layer.Key + "-Schicht:")} {layer.Value} ({MarkdownGenerator.DirectoryReference(layer.Key)})");
            }
        }
    }

    /// <summary>
    /// Generates the Layers section documenting each architectural layer.
    /// Requirement 1.1: Extract layer information from actual folder structure
    /// </summary>
    private void GenerateLayersSection(string projectRoot, IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.H2("Layers");
        _markdown.Paragraph("Each layer has specific responsibilities and dependencies. " +
                          "The following sections describe each layer in detail.");

        // Web Layer
        GenerateWebLayerSection(projectRoot, sourceFiles);

        // Application Layer
        GenerateApplicationLayerSection(projectRoot, sourceFiles);

        // Domain Layer
        GenerateDomainLayerSection(projectRoot, sourceFiles);

        // Infrastructure Layer
        GenerateInfrastructureLayerSection(projectRoot, sourceFiles);
    }

    private void GenerateWebLayerSection(string projectRoot, IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.H3("Web Layer");
        _markdown.Paragraph($"The Web layer contains controllers and presentation logic. " +
                          $"Located in {MarkdownGenerator.DirectoryReference("Web")}.");

        var webFiles = sourceFiles
            .Where(f => f.Path.Contains("Web" + Path.DirectorySeparatorChar) || f.Path.Contains("Web/"))
            .ToList();

        if (webFiles.Any())
        {
            _markdown.H4("Key Components");

            // Group by subdirectory or file type
            var controllers = webFiles.Where(f => f.Classes.Any(c => c.Name.EndsWith("Controller"))).ToList();
            
            if (controllers.Any())
            {
                _markdown.H5("Controllers");
                foreach (var file in controllers)
                {
                    foreach (var controller in file.Classes.Where(c => c.Name.EndsWith("Controller")))
                    {
                        var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                        _markdown.ListItem($"{MarkdownGenerator.CodeReference(controller.Name, relativePath)}");
                        
                        // List key methods
                        var publicMethods = controller.Methods.Where(m => m.Modifiers.Contains("public")).Take(5).ToList();
                        if (publicMethods.Any())
                        {
                            foreach (var method in publicMethods)
                            {
                                _markdown.ListItem($"{MarkdownGenerator.InlineCode(method.Name + "()")}", 1);
                            }
                        }
                    }
                }
            }

            // Program.cs
            var programFile = webFiles.FirstOrDefault(f => f.Path.EndsWith("Program.cs"));
            if (programFile != null)
            {
                var relativePath = Path.GetRelativePath(projectRoot, programFile.Path).Replace("\\", "/");
                _markdown.H5("Application Startup");
                _markdown.Paragraph($"Application configuration and dependency injection setup in {MarkdownGenerator.FileReference(relativePath)}.");
            }
        }
        else
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No Web layer components found."));
        }
    }

    private void GenerateApplicationLayerSection(string projectRoot, IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.H3("Application Layer");
        _markdown.Paragraph($"The Application layer contains business logic, use cases, and agents. " +
                          $"Located in {MarkdownGenerator.DirectoryReference("Application")}.");

        var appFiles = sourceFiles
            .Where(f => f.Path.Contains("Application" + Path.DirectorySeparatorChar) || f.Path.Contains("Application/"))
            .ToList();

        if (appFiles.Any())
        {
            _markdown.H4("Key Components");

            // Agents
            var agentFiles = appFiles.Where(f => f.Path.Contains("Agents")).ToList();
            if (agentFiles.Any())
            {
                _markdown.H5("Contract Analysis Agents");
                _markdown.Paragraph($"Agents handle contract analysis workflows. Located in {MarkdownGenerator.DirectoryReference("Application/Agents")}.");
                
                foreach (var file in agentFiles)
                {
                    foreach (var agentClass in file.Classes.Where(c => c.Name.Contains("Agent")))
                    {
                        var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                        _markdown.ListItem($"{MarkdownGenerator.CodeReference(agentClass.Name, relativePath)}");
                    }
                }
            }

            // Services
            var serviceFiles = appFiles.Where(f => f.Classes.Any(c => c.Name.EndsWith("Service"))).ToList();
            if (serviceFiles.Any())
            {
                _markdown.H5("Services");
                foreach (var file in serviceFiles)
                {
                    foreach (var service in file.Classes.Where(c => c.Name.EndsWith("Service")))
                    {
                        var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                        _markdown.ListItem($"{MarkdownGenerator.CodeReference(service.Name, relativePath)}");
                    }
                }
            }

            // Exceptions
            var exceptionFiles = appFiles.Where(f => f.Path.Contains("Exceptions")).ToList();
            if (exceptionFiles.Any())
            {
                _markdown.H5("Exceptions");
                _markdown.Paragraph($"Custom exception types in {MarkdownGenerator.DirectoryReference("Application/Exceptions")}.");
                
                var exceptionCount = exceptionFiles.SelectMany(f => f.Classes.Where(c => c.Name.EndsWith("Exception"))).Count();
                _markdown.Paragraph($"Total custom exceptions: {exceptionCount}");
            }
        }
        else
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No Application layer components found."));
        }
    }

    private void GenerateDomainLayerSection(string projectRoot, IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.H3("Domain Layer");
        _markdown.Paragraph($"The Domain layer contains core business entities and Data Transfer Objects (DTOs). " +
                          $"Located in {MarkdownGenerator.DirectoryReference("Domain")}.");

        var domainFiles = sourceFiles
            .Where(f => f.Path.Contains("Domain" + Path.DirectorySeparatorChar) || f.Path.Contains("Domain/"))
            .ToList();

        if (domainFiles.Any())
        {
            _markdown.H4("Key Components");

            // DTOs
            var dtoFiles = domainFiles.Where(f => f.Path.Contains("Dtos")).ToList();
            if (dtoFiles.Any())
            {
                _markdown.H5("Data Transfer Objects (DTOs)");
                _markdown.Paragraph($"DTOs define the data structures used throughout the system. Located in {MarkdownGenerator.DirectoryReference("Domain/Dtos")}.");

                // Count DTOs by category
                var employmentDtos = dtoFiles.Where(f => f.Namespace.Contains("Employment")).SelectMany(f => f.Classes.Concat(f.Records)).Count();
                var rentalDtos = dtoFiles.Where(f => f.Namespace.Contains("Rental")).SelectMany(f => f.Classes.Concat(f.Records)).Count();
                var sharedDtos = dtoFiles.Where(f => f.Namespace.Contains("Shared")).SelectMany(f => f.Classes.Concat(f.Records)).Count();

                _markdown.UnorderedList(new[]
                {
                    $"Employment Contract DTOs: {employmentDtos}",
                    $"Rental Contract DTOs: {rentalDtos}",
                    $"Shared DTOs: {sharedDtos}"
                });

                _markdown.Paragraph($"For complete DTO documentation, see {MarkdownGenerator.Link("DATA_MODELS.md", "DATA_MODELS.md")}.");
            }

            // Enums
            var enumCount = domainFiles.SelectMany(f => f.Enums).Count();
            if (enumCount > 0)
            {
                _markdown.H5("Enumerations");
                _markdown.Paragraph($"Total enumerations: {enumCount}");
            }
        }
        else
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No Domain layer components found."));
        }
    }

    private void GenerateInfrastructureLayerSection(string projectRoot, IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.H3("Infrastructure Layer");
        _markdown.Paragraph($"The Infrastructure layer contains implementations of external services and integrations. " +
                          $"Located in {MarkdownGenerator.DirectoryReference("Infrastructure")}.");

        var infraFiles = sourceFiles
            .Where(f => f.Path.Contains("Infrastructure" + Path.DirectorySeparatorChar) || f.Path.Contains("Infrastructure/"))
            .ToList();

        if (infraFiles.Any())
        {
            _markdown.H4("Key Components");

            // Group by functionality
            var aiServiceFiles = infraFiles.Where(f => f.Classes.Any(c => c.Name.Contains("AI") || c.Name.Contains("Service"))).ToList();
            if (aiServiceFiles.Any())
            {
                _markdown.H5("AI Services");
                foreach (var file in aiServiceFiles)
                {
                    foreach (var service in file.Classes.Where(c => c.Name.Contains("Service") || c.Name.Contains("AI")))
                    {
                        var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                        _markdown.ListItem($"{MarkdownGenerator.CodeReference(service.Name, relativePath)}");
                    }
                }
            }

            // Handlers
            var handlerFiles = infraFiles.Where(f => f.Classes.Any(c => c.Name.Contains("Handler"))).ToList();
            if (handlerFiles.Any())
            {
                _markdown.H5("Handlers");
                foreach (var file in handlerFiles)
                {
                    foreach (var handler in file.Classes.Where(c => c.Name.Contains("Handler")))
                    {
                        var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                        _markdown.ListItem($"{MarkdownGenerator.CodeReference(handler.Name, relativePath)}");
                    }
                }
            }

            // Providers
            var providerFiles = infraFiles.Where(f => f.Classes.Any(c => c.Name.Contains("Provider"))).ToList();
            if (providerFiles.Any())
            {
                _markdown.H5("Providers");
                foreach (var file in providerFiles)
                {
                    foreach (var provider in file.Classes.Where(c => c.Name.Contains("Provider")))
                    {
                        var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                        _markdown.ListItem($"{MarkdownGenerator.CodeReference(provider.Name, relativePath)}");
                    }
                }
            }
        }
        else
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No Infrastructure layer components found."));
        }
    }

    /// <summary>
    /// Generates the Dependencies section showing dependency flow between layers.
    /// Requirement 1.2: WHEN documenting dependencies, THE System SHALL analyze using statements in source files to determine actual dependency flow
    /// </summary>
    private void GenerateDependenciesSection(IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.H2("Dependencies");
        _markdown.Paragraph("The architecture follows strict dependency rules. Dependencies flow inward toward the domain core.");

        _markdown.H3("Dependency Rules");
        _markdown.UnorderedList(new[]
        {
            $"{MarkdownGenerator.Bold("Domain Layer:")} Has no dependencies on other layers (pure business logic)",
            $"{MarkdownGenerator.Bold("Application Layer:")} Depends only on Domain layer",
            $"{MarkdownGenerator.Bold("Infrastructure Layer:")} Depends on Application and Domain layers",
            $"{MarkdownGenerator.Bold("Web Layer:")} Depends on Application, Domain, and Infrastructure layers"
        });

        _markdown.H3("Actual Dependencies");
        _markdown.Paragraph("The following dependencies are extracted from actual using statements in the source code:");

        // Analyze using statements to determine actual dependencies
        var dependencyMap = AnalyzeDependencies(sourceFiles);

        foreach (var layer in new[] { "Web", "Application", "Infrastructure", "Domain" })
        {
            if (dependencyMap.ContainsKey(layer))
            {
                _markdown.H4($"{layer} Layer Dependencies");
                var dependencies = dependencyMap[layer];
                
                if (dependencies.Any())
                {
                    var dependencyList = dependencies
                        .GroupBy(d => d)
                        .OrderBy(g => g.Key)
                        .Select(g => $"{MarkdownGenerator.InlineCode(g.Key)} (referenced {g.Count()} time(s))");
                    
                    _markdown.UnorderedList(dependencyList);
                }
                else
                {
                    _markdown.Paragraph(MarkdownGenerator.Italic("No external dependencies."));
                }
            }
        }
    }

    /// <summary>
    /// Analyzes using statements to determine dependencies between layers.
    /// </summary>
    private Dictionary<string, List<string>> AnalyzeDependencies(IEnumerable<SourceFileInfo> sourceFiles)
    {
        var dependencyMap = new Dictionary<string, List<string>>
        {
            { "Web", new List<string>() },
            { "Application", new List<string>() },
            { "Infrastructure", new List<string>() },
            { "Domain", new List<string>() }
        };

        foreach (var file in sourceFiles)
        {
            // Determine which layer this file belongs to
            string? fileLayer = null;
            if (file.Path.Contains("Web" + Path.DirectorySeparatorChar) || file.Path.Contains("Web/"))
                fileLayer = "Web";
            else if (file.Path.Contains("Application" + Path.DirectorySeparatorChar) || file.Path.Contains("Application/"))
                fileLayer = "Application";
            else if (file.Path.Contains("Infrastructure" + Path.DirectorySeparatorChar) || file.Path.Contains("Infrastructure/"))
                fileLayer = "Infrastructure";
            else if (file.Path.Contains("Domain" + Path.DirectorySeparatorChar) || file.Path.Contains("Domain/"))
                fileLayer = "Domain";

            if (fileLayer == null) continue;

            // Extract dependencies from using statements
            foreach (var usingStatement in file.Usings)
            {
                // Filter for project-specific namespaces (PocketLawyer.*)
                if (usingStatement.StartsWith("PocketLawyer"))
                {
                    dependencyMap[fileLayer].Add(usingStatement);
                }
            }
        }

        return dependencyMap;
    }

    /// <summary>
    /// Generates the Design Principles section with code examples.
    /// Requirement 1.4: WHEN documenting design principles, THE System SHALL reference specific code examples that demonstrate each principle
    /// </summary>
    private void GenerateDesignPrinciplesSection(IEnumerable<SourceFileInfo> sourceFiles, string projectRoot)
    {
        _markdown.H2("Design Principles");
        _markdown.Paragraph("The system follows established software design principles. " +
                          "The following principles are demonstrated in the actual codebase:");

        // SOLID Principles
        GenerateSolidPrinciplesSection(sourceFiles, projectRoot);

        // DRY (Don't Repeat Yourself)
        GenerateDryPrincipleSection(sourceFiles, projectRoot);

        // YAGNI (You Aren't Gonna Need It)
        GenerateYagniPrincipleSection();
    }

    private void GenerateSolidPrinciplesSection(IEnumerable<SourceFileInfo> sourceFiles, string projectRoot)
    {
        _markdown.H3("SOLID Principles");

        // Single Responsibility Principle
        _markdown.H4("Single Responsibility Principle (SRP)");
        _markdown.Paragraph("Each class has a single, well-defined responsibility.");
        
        var exampleClasses = sourceFiles
            .SelectMany(f => f.Classes)
            .Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Provider") || c.Name.EndsWith("Handler"))
            .Take(3)
            .ToList();

        if (exampleClasses.Any())
        {
            _markdown.Paragraph($"{MarkdownGenerator.Bold("Examples:")}");
            foreach (var cls in exampleClasses)
            {
                var file = sourceFiles.First(f => f.Classes.Contains(cls));
                var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                _markdown.ListItem($"{MarkdownGenerator.CodeReference(cls.Name, relativePath)} - Handles {GetClassResponsibility(cls.Name)}");
            }
        }

        // Dependency Inversion Principle
        _markdown.H4("Dependency Inversion Principle (DIP)");
        _markdown.Paragraph("High-level modules depend on abstractions (interfaces), not concrete implementations.");
        
        var interfaces = sourceFiles
            .SelectMany(f => f.Interfaces)
            .Take(3)
            .ToList();

        if (interfaces.Any())
        {
            _markdown.Paragraph($"{MarkdownGenerator.Bold("Examples:")}");
            foreach (var iface in interfaces)
            {
                var file = sourceFiles.First(f => f.Interfaces.Contains(iface));
                var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                _markdown.ListItem($"{MarkdownGenerator.CodeReference(iface.Name, relativePath)} - Interface for abstraction");
            }
        }

        // Interface Segregation Principle
        _markdown.H4("Interface Segregation Principle (ISP)");
        _markdown.Paragraph("Interfaces are focused and specific to client needs.");
        
        var specificInterfaces = sourceFiles
            .SelectMany(f => f.Interfaces)
            .Where(i => i.Methods.Count <= 5) // Small, focused interfaces
            .Take(2)
            .ToList();

        if (specificInterfaces.Any())
        {
            _markdown.Paragraph($"{MarkdownGenerator.Bold("Examples:")}");
            foreach (var iface in specificInterfaces)
            {
                var file = sourceFiles.First(f => f.Interfaces.Contains(iface));
                var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                _markdown.ListItem($"{MarkdownGenerator.CodeReference(iface.Name, relativePath)} - Focused interface with {iface.Methods.Count} method(s)");
            }
        }
    }

    private void GenerateDryPrincipleSection(IEnumerable<SourceFileInfo> sourceFiles, string projectRoot)
    {
        _markdown.H3("DRY (Don't Repeat Yourself)");
        _markdown.Paragraph("Common functionality is extracted into reusable components.");

        // Find base classes or utility classes
        var baseClasses = sourceFiles
            .SelectMany(f => f.Classes)
            .Where(c => c.Modifiers.Contains("abstract") || c.Name.Contains("Base") || c.Name.Contains("Helper") || c.Name.Contains("Utility"))
            .Take(3)
            .ToList();

        if (baseClasses.Any())
        {
            _markdown.Paragraph($"{MarkdownGenerator.Bold("Examples:")}");
            foreach (var cls in baseClasses)
            {
                var file = sourceFiles.First(f => f.Classes.Contains(cls));
                var relativePath = Path.GetRelativePath(projectRoot, file.Path).Replace("\\", "/");
                var classType = cls.Modifiers.Contains("abstract") ? "Abstract base class" : "Utility class";
                _markdown.ListItem($"{MarkdownGenerator.CodeReference(cls.Name, relativePath)} - {classType} for code reuse");
            }
        }
        else
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("Common functionality is encapsulated in service classes."));
        }
    }

    private void GenerateYagniPrincipleSection()
    {
        _markdown.H3("YAGNI (You Aren't Gonna Need It)");
        _markdown.Paragraph("The system implements only the features that are currently needed, " +
                          "avoiding speculative generality and unnecessary complexity.");
        _markdown.Paragraph("This principle is demonstrated through focused, purpose-driven classes " +
                          "that solve specific problems without over-engineering.");
    }

    private string GetClassResponsibility(string className)
    {
        if (className.EndsWith("Service"))
            return "service operations";
        if (className.EndsWith("Provider"))
            return "data/resource provision";
        if (className.EndsWith("Handler"))
            return "request/event handling";
        if (className.EndsWith("Agent"))
            return "agent workflow";
        if (className.EndsWith("Controller"))
            return "API endpoint routing";
        return "specific functionality";
    }

    /// <summary>
    /// Generates the Diagrams section with architecture visualizations.
    /// Requirement 1.3: WHEN creating the Layered Component Diagram, THE System SHALL base it on actual namespaces and classes found in the codebase
    /// </summary>
    private void GenerateDiagramsSection(string projectRoot, IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.H2("Diagrams");

        // Layered Component Diagram
        _markdown.H3("Layered Component Diagram");
        _markdown.Paragraph("High-level view of the system's layered architecture:");
        GenerateLayeredComponentDiagram(sourceFiles);

        // Dependency Graph
        _markdown.H3("Dependency Graph");
        _markdown.Paragraph("Dependency flow between layers:");
        GenerateDependencyGraph();

        // Package Structure
        _markdown.H3("Package Structure");
        _markdown.Paragraph("Organization of namespaces and packages:");
        GeneratePackageStructureDiagram(sourceFiles);
    }

    private void GenerateLayeredComponentDiagram(IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.Raw("```mermaid\n");
        _markdown.Raw("graph TB\n");
        _markdown.Raw("    subgraph Web[\"Web Layer\"]\n");
        
        // Add actual controllers
        var controllers = sourceFiles
            .Where(f => f.Path.Contains("Web"))
            .SelectMany(f => f.Classes)
            .Where(c => c.Name.EndsWith("Controller"))
            .Take(3)
            .ToList();
        
        foreach (var controller in controllers)
        {
            var safeName = controller.Name.Replace("Controller", "");
            _markdown.Raw($"        {safeName}[{controller.Name}]\n");
        }
        
        _markdown.Raw("    end\n\n");

        _markdown.Raw("    subgraph Application[\"Application Layer\"]\n");
        
        // Add actual agents and services
        var agents = sourceFiles
            .Where(f => f.Path.Contains("Application"))
            .SelectMany(f => f.Classes)
            .Where(c => c.Name.Contains("Agent") || c.Name.EndsWith("Service"))
            .Take(4)
            .ToList();
        
        foreach (var agent in agents)
        {
            var safeName = agent.Name.Replace("Agent", "").Replace("Service", "");
            _markdown.Raw($"        {safeName}[{agent.Name}]\n");
        }
        
        _markdown.Raw("    end\n\n");

        _markdown.Raw("    subgraph Domain[\"Domain Layer\"]\n");
        _markdown.Raw("        DTOs[Data Transfer Objects]\n");
        _markdown.Raw("        Entities[Domain Entities]\n");
        _markdown.Raw("    end\n\n");

        _markdown.Raw("    subgraph Infrastructure[\"Infrastructure Layer\"]\n");
        
        // Add actual infrastructure components
        var infraComponents = sourceFiles
            .Where(f => f.Path.Contains("Infrastructure"))
            .SelectMany(f => f.Classes)
            .Where(c => c.Name.Contains("Service") || c.Name.Contains("Handler") || c.Name.Contains("Provider"))
            .Take(3)
            .ToList();
        
        foreach (var component in infraComponents)
        {
            var safeName = component.Name.Replace("Service", "").Replace("Handler", "").Replace("Provider", "");
            _markdown.Raw($"        {safeName}[{component.Name}]\n");
        }
        
        _markdown.Raw("    end\n\n");

        // Dependencies
        _markdown.Raw("    Web --> Application\n");
        _markdown.Raw("    Web -.-> Infrastructure\n");
        _markdown.Raw("    Application --> Domain\n");
        _markdown.Raw("    Infrastructure --> Domain\n");
        _markdown.Raw("    Infrastructure --> Application\n");
        
        _markdown.Raw("```\n\n");
    }

    private void GenerateDependencyGraph()
    {
        _markdown.Raw("```mermaid\n");
        _markdown.Raw("graph TD\n");
        _markdown.Raw("    Web[Web Layer]\n");
        _markdown.Raw("    App[Application Layer]\n");
        _markdown.Raw("    Domain[Domain Layer]\n");
        _markdown.Raw("    Infra[Infrastructure Layer]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Web -->|depends on| App\n");
        _markdown.Raw("    Web -.->|uses| Infra\n");
        _markdown.Raw("    App -->|depends on| Domain\n");
        _markdown.Raw("    Infra -->|depends on| Domain\n");
        _markdown.Raw("    Infra -->|implements| App\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    style Domain fill:#90EE90\n");
        _markdown.Raw("    style App fill:#87CEEB\n");
        _markdown.Raw("    style Infra fill:#FFB6C1\n");
        _markdown.Raw("    style Web fill:#FFD700\n");
        _markdown.Raw("```\n\n");
    }

    private void GeneratePackageStructureDiagram(IEnumerable<SourceFileInfo> sourceFiles)
    {
        _markdown.Raw("```mermaid\n");
        _markdown.Raw("graph LR\n");
        _markdown.Raw("    Root[PocketLawyer]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Root --> Web\n");
        _markdown.Raw("    Root --> Application\n");
        _markdown.Raw("    Root --> Domain\n");
        _markdown.Raw("    Root --> Infrastructure\n");
        _markdown.Raw("    \n");

        // Domain subpackages
        var domainNamespaces = sourceFiles
            .Where(f => f.Namespace.Contains("Domain"))
            .Select(f => f.Namespace)
            .Distinct()
            .Take(5)
            .ToList();

        if (domainNamespaces.Any())
        {
            foreach (var ns in domainNamespaces)
            {
                var parts = ns.Split('.');
                if (parts.Length > 2)
                {
                    var subPackage = parts[2];
                    var safeName = subPackage.Replace(".", "_");
                    _markdown.Raw($"    Domain --> Domain_{safeName}[{subPackage}]\n");
                }
            }
        }

        // Application subpackages
        var appNamespaces = sourceFiles
            .Where(f => f.Namespace.Contains("Application"))
            .Select(f => f.Namespace)
            .Distinct()
            .Take(5)
            .ToList();

        if (appNamespaces.Any())
        {
            foreach (var ns in appNamespaces)
            {
                var parts = ns.Split('.');
                if (parts.Length > 2)
                {
                    var subPackage = parts[2];
                    var safeName = subPackage.Replace(".", "_");
                    _markdown.Raw($"    Application --> App_{safeName}[{subPackage}]\n");
                }
            }
        }

        _markdown.Raw("```\n\n");
    }

    /// <summary>
    /// Writes the generated documentation to a file.
    /// </summary>
    public void WriteToFile(string filePath)
    {
        _markdown.WriteToFile(filePath);
    }
}
