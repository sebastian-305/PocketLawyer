using PocketLawyer.Documentation.Extraction;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Generates DEPLOYMENT.md documentation from project files and directory structure.
/// Documents system requirements, installation steps, context files, and environment setup.
/// </summary>
/// <remarks>
/// Validates Requirements: 5.1, 5.2, 5.3, 5.4
/// </remarks>
public class DeploymentDocGenerator
{
    private readonly MarkdownGenerator _markdown;
    private readonly MermaidDiagramGenerator _diagramGenerator;
    private readonly DocumentationLanguage _lang;

    public DeploymentDocGenerator()
    {
        _markdown = new MarkdownGenerator();
        _diagramGenerator = new MermaidDiagramGenerator();
        _lang = DocumentationLanguage.German;
    }

    /// <summary>
    /// Generates complete DEPLOYMENT.md documentation from project information.
    /// Requirement 5.1: WHEN documenting system requirements, THE System SHALL extract .NET version from PocketLawyer.csproj file
    /// Requirement 5.2: WHEN documenting dependencies, THE System SHALL extract NuGet packages from PocketLawyer.csproj file
    /// </summary>
    /// <param name="projectInfo">Project file information (.csproj)</param>
    /// <param name="configFiles">Configuration files information</param>
    /// <param name="projectRoot">Root directory of the project</param>
    /// <returns>Complete DEPLOYMENT.md content</returns>
    public string GenerateDeploymentDocumentation(
        ProjectFileInfo projectInfo,
        IEnumerable<ConfigurationFileInfo> configFiles,
        string projectRoot)
    {
        _markdown.Clear();

        // Header
        _markdown.H1(_lang.DeploymentTitle);
        _markdown.Paragraph("Dieses Dokument bietet Deployment- und Setup-Anleitungen für das PocketLawyer-System. " +
                          "Alle Informationen werden aus den tatsächlichen Projektdateien und der Verzeichnisstruktur extrahiert.");

        // Table of Contents
        GenerateTableOfContents();

        // System Requirements
        GenerateSystemRequirementsSection(projectInfo);

        // Installation
        GenerateInstallationSection(projectInfo, projectRoot);

        // Configuration
        GenerateConfigurationSection(configFiles, projectRoot);

        // Context Files
        GenerateContextFilesSection(projectRoot);

        // Running the Application
        GenerateRunningApplicationSection(projectRoot);

        // Health Checks
        GenerateHealthChecksSection();

        // Troubleshooting
        GenerateTroubleshootingSection();

        // Diagrams
        GenerateDiagramsSection(projectRoot);

        return _markdown.Content;
    }

    private void GenerateTableOfContents()
    {
        _markdown.H2(_lang.TableOfContents);
        _markdown.UnorderedList(new[]
        {
            MarkdownGenerator.AnchorLink(_lang.DeploymentRequirements, AnchorGenerator.Generate(_lang.DeploymentRequirements)),
            MarkdownGenerator.AnchorLink(_lang.DeploymentInstallation, AnchorGenerator.Generate(_lang.DeploymentInstallation)),
            MarkdownGenerator.AnchorLink(_lang.DeploymentConfiguration, AnchorGenerator.Generate(_lang.DeploymentConfiguration)),
            MarkdownGenerator.AnchorLink(_lang.DeploymentContextFiles, AnchorGenerator.Generate(_lang.DeploymentContextFiles)),
            MarkdownGenerator.AnchorLink(_lang.DeploymentRunning, AnchorGenerator.Generate(_lang.DeploymentRunning)),
            MarkdownGenerator.AnchorLink(_lang.DeploymentHealthChecks, AnchorGenerator.Generate(_lang.DeploymentHealthChecks)),
            MarkdownGenerator.AnchorLink("Fehlerbehebung", AnchorGenerator.Generate("Fehlerbehebung")),
            MarkdownGenerator.AnchorLink(_lang.Diagrams, AnchorGenerator.Generate(_lang.Diagrams))
        });
    }

    /// <summary>
    /// Generates the System Requirements section.
    /// Requirement 5.1: WHEN documenting system requirements, THE System SHALL extract .NET version from PocketLawyer.csproj file
    /// Requirement 5.2: WHEN documenting dependencies, THE System SHALL extract NuGet packages from PocketLawyer.csproj file
    /// </summary>
    private void GenerateSystemRequirementsSection(ProjectFileInfo projectInfo)
    {
        _markdown.H2("System Requirements");
        _markdown.Paragraph("The following software and dependencies are required to run PocketLawyer:");

        _markdown.H3(".NET SDK");
        
        // Extract .NET version from TargetFramework
        var dotnetVersion = ExtractDotNetVersion(projectInfo.TargetFramework);
        _markdown.Paragraph($"**Required Version:** .NET {dotnetVersion} SDK or later");
        _markdown.Paragraph($"**Source:** {MarkdownGenerator.FileReference(projectInfo.Path)} " +
                          $"(TargetFramework: {MarkdownGenerator.InlineCode(projectInfo.TargetFramework)})");

        _markdown.H4("Download .NET SDK");
        _markdown.Paragraph("Download the .NET SDK from the official Microsoft website:");
        _markdown.UnorderedList(new[]
        {
            MarkdownGenerator.Link(".NET Downloads", "https://dotnet.microsoft.com/download")
        });

        _markdown.H4("Verify Installation");
        _markdown.Paragraph("After installation, verify the .NET SDK version:");
        _markdown.CodeBlock("dotnet --version", "bash");

        _markdown.H3("Operating System");
        _markdown.Paragraph("PocketLawyer can run on any operating system that supports .NET:");
        _markdown.UnorderedList(new[]
        {
            "Windows 10/11 or Windows Server 2016+",
            "macOS 10.15 (Catalina) or later",
            "Linux (Ubuntu 20.04+, Debian 10+, RHEL 8+, or equivalent)"
        });

        _markdown.H3("Hardware Requirements");
        _markdown.Paragraph("Minimum hardware requirements:");
        _markdown.UnorderedList(new[]
        {
            "**CPU:** 2 cores or more",
            "**RAM:** 4 GB minimum, 8 GB recommended",
            "**Disk Space:** 500 MB for application, additional space for uploaded files and logs"
        });

        _markdown.H3("Dependencies");
        _markdown.Paragraph("The following NuGet packages are required and will be automatically restored:");

        if (projectInfo.PackageReferences.Any())
        {
            var packageRows = projectInfo.PackageReferences
                .OrderBy(p => p.Name)
                .Select(p => new[] { p.Name, p.Version, GetPackageDescription(p.Name) });

            _markdown.Table(
                new[] { "Package", "Version", "Purpose" },
                packageRows
            );

            _markdown.Paragraph($"**Source:** {MarkdownGenerator.FileReference(projectInfo.Path)} (PackageReference elements)");
        }
        else
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No NuGet package dependencies found."));
        }
    }

    /// <summary>
    /// Generates the Installation section with step-by-step instructions.
    /// Requirement 5.5: THE System SHALL verify that installation steps reference actual files and directories that exist in the repository
    /// </summary>
    private void GenerateInstallationSection(ProjectFileInfo projectInfo, string projectRoot)
    {
        _markdown.H2("Installation");
        _markdown.Paragraph("Follow these steps to install and set up PocketLawyer:");

        _markdown.H3("1. Clone the Repository");
        _markdown.Paragraph("Clone the PocketLawyer repository to your local machine:");
        _markdown.CodeBlock("git clone <repository-url>\ncd PocketLawyer", "bash");

        _markdown.H3("2. Restore Dependencies");
        _markdown.Paragraph("Restore all NuGet package dependencies:");
        _markdown.CodeBlock("dotnet restore", "bash");
        _markdown.Paragraph("This command will download and install all packages listed in " +
                          $"{MarkdownGenerator.FileReference(projectInfo.Path)}.");

        _markdown.H3("3. Verify Project Structure");
        _markdown.Paragraph("Ensure the following directories exist:");
        
        var requiredDirectories = new[]
        {
            "Web/",
            "Application/",
            "Domain/",
            "Infrastructure/",
            "data/Context/"
        };

        foreach (var dir in requiredDirectories)
        {
            var fullPath = Path.Combine(projectRoot, dir);
            var exists = Directory.Exists(fullPath);
            var status = exists ? "✓" : "✗";
            _markdown.ListItem($"{status} {MarkdownGenerator.DirectoryReference(dir)}");
        }

        _markdown.BlankLine();

        _markdown.H3("4. Build the Project");
        _markdown.Paragraph("Build the project to verify everything is set up correctly:");
        _markdown.CodeBlock("dotnet build", "bash");
        _markdown.Paragraph("The build should complete without errors.");

        _markdown.H3("5. Run Tests (Optional)");
        _markdown.Paragraph("Run the test suite to verify the installation:");
        _markdown.CodeBlock("dotnet test", "bash");
    }

    /// <summary>
    /// Generates the Configuration section.
    /// Requirement 5.4: WHEN documenting environment setup, THE System SHALL reference actual configuration files and option classes
    /// </summary>
    private void GenerateConfigurationSection(IEnumerable<ConfigurationFileInfo> configFiles, string projectRoot)
    {
        _markdown.H2("Configuration");
        _markdown.Paragraph("PocketLawyer requires configuration before it can run. " +
                          "Configuration is managed through appsettings.json files and user secrets.");

        _markdown.H3("Configuration Files");
        _markdown.Paragraph("The following configuration files are used:");

        var configFilesList = configFiles.ToList();
        foreach (var configFile in configFilesList.OrderBy(c => c.Path))
        {
            var fileName = Path.GetFileName(configFile.Path);
            var description = fileName switch
            {
                "appsettings.json" => "Base configuration for all environments",
                "appsettings.Development.json" => "Development environment overrides",
                "appsettings.Production.json" => "Production environment overrides",
                _ => "Configuration file"
            };

            _markdown.ListItem($"{MarkdownGenerator.FileReference(configFile.Path)} - {description}");
        }

        _markdown.BlankLine();

        _markdown.H3("Required Configuration");
        _markdown.Paragraph("The following configuration values must be set before running the application:");

        _markdown.H4("AI Service API Keys");
        _markdown.Paragraph("PocketLawyer requires API keys for AI services. Set these using user secrets:");
        _markdown.CodeBlock(
            "dotnet user-secrets init\n" +
            "dotnet user-secrets set \"Nebius:ApiKey\" \"your-nebius-api-key\"\n" +
            "dotnet user-secrets set \"Claude:ApiKey\" \"your-claude-api-key\"\n" +
            "dotnet user-secrets set \"Gemini:ApiKey\" \"your-gemini-api-key\"",
            "bash"
        );

        _markdown.Note("Never commit API keys to source control. Always use user secrets for development " +
                      "and environment variables for production.");

        _markdown.H4("File Paths");
        _markdown.Paragraph("Configure file paths for uploads and context files:");
        _markdown.CodeBlock(
            "{\n" +
            "  \"Paths\": {\n" +
            "    \"UploadDirectory\": \"uploads\",\n" +
            "    \"ContextDirectory\": \"data/Context\"\n" +
            "  }\n" +
            "}",
            "json"
        );

        _markdown.H4("Logging");
        _markdown.Paragraph("Configure logging levels and output:");
        _markdown.CodeBlock(
            "{\n" +
            "  \"Logging\": {\n" +
            "    \"LogLevel\": {\n" +
            "      \"Default\": \"Information\",\n" +
            "      \"Microsoft.AspNetCore\": \"Warning\"\n" +
            "    }\n" +
            "  }\n" +
            "}",
            "json"
        );

        _markdown.H3("Environment Variables");
        _markdown.Paragraph("For production deployments, use environment variables instead of user secrets:");
        _markdown.CodeBlock(
            "export Nebius__ApiKey=\"your-api-key\"\n" +
            "export Claude__ApiKey=\"your-api-key\"\n" +
            "export Gemini__ApiKey=\"your-api-key\"\n" +
            "export ASPNETCORE_ENVIRONMENT=\"Production\"",
            "bash"
        );

        _markdown.Paragraph($"For complete configuration documentation, see {MarkdownGenerator.Link("CONFIGURATION.md", "CONFIGURATION.md")}.");
    }

    /// <summary>
    /// Generates the Context Files section.
    /// Requirement 5.3: WHEN documenting context files deployment, THE System SHALL reference the actual data/Context/ directory structure
    /// </summary>
    private void GenerateContextFilesSection(string projectRoot)
    {
        _markdown.H2("Context Files");
        _markdown.Paragraph("PocketLawyer uses context files to provide AI models with domain-specific knowledge. " +
                          "These files must be present for the application to function correctly.");

        _markdown.H3("Context Directory Structure");
        var contextPath = Path.Combine(projectRoot, "data", "Context");
        
        if (Directory.Exists(contextPath))
        {
            _markdown.Paragraph($"Context files are located in {MarkdownGenerator.DirectoryReference("data/Context")}:");

            // List context files
            var contextFiles = Directory.GetFiles(contextPath, "*.txt", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(projectRoot, f).Replace("\\", "/"))
                .OrderBy(f => f)
                .ToList();

            if (contextFiles.Any())
            {
                foreach (var file in contextFiles)
                {
                    _markdown.ListItem(MarkdownGenerator.FileReference(file));
                }
            }
            else
            {
                _markdown.Paragraph(MarkdownGenerator.Italic("No context files found in directory."));
            }

            _markdown.BlankLine();
        }
        else
        {
            _markdown.Warning($"Context directory {MarkdownGenerator.DirectoryReference("data/Context")} not found. " +
                            "The application may not function correctly without context files.");
        }

        _markdown.H3("Deploying Context Files");
        _markdown.Paragraph("When deploying PocketLawyer, ensure the context files are included:");

        _markdown.H4("Development");
        _markdown.Paragraph("Context files are included in the repository and should be present after cloning.");

        _markdown.H4("Production");
        _markdown.Paragraph("When deploying to production:");
        _markdown.OrderedList(new[]
        {
            $"Ensure the {MarkdownGenerator.DirectoryReference("data/Context")} directory is included in the deployment package",
            "Verify all .txt files in the context directory are present",
            "Set appropriate file permissions (read-only is sufficient)",
            "Configure the ContextDirectory path in appsettings.json if using a different location"
        });

        _markdown.H3("Context File Format");
        _markdown.Paragraph("Context files are plain text files (.txt) containing prompts and instructions for AI models. " +
                          "Do not modify these files unless you understand their purpose and format.");
    }

    /// <summary>
    /// Generates the Running the Application section.
    /// </summary>
    private void GenerateRunningApplicationSection(string projectRoot)
    {
        _markdown.H2("Running the Application");
        _markdown.Paragraph("After installation and configuration, you can run PocketLawyer:");

        _markdown.H3("Development Mode");
        _markdown.Paragraph("Run the application in development mode with hot reload:");
        _markdown.CodeBlock("dotnet run --project Web", "bash");
        _markdown.Paragraph("Or navigate to the Web directory:");
        _markdown.CodeBlock("cd Web\ndotnet run", "bash");

        _markdown.H4("Development Server");
        _markdown.Paragraph("The application will start and listen on:");
        _markdown.UnorderedList(new[]
        {
            "HTTP: http://localhost:5000",
            "HTTPS: https://localhost:5001"
        });

        _markdown.Paragraph("The exact ports may vary based on your " +
                          $"{MarkdownGenerator.FileReference("Properties/launchSettings.json")} configuration.");

        _markdown.H3("Production Mode");
        _markdown.Paragraph("For production deployment:");

        _markdown.H4("1. Publish the Application");
        _markdown.CodeBlock("dotnet publish -c Release -o ./publish", "bash");

        _markdown.H4("2. Run the Published Application");
        _markdown.CodeBlock("cd publish\ndotnet Web.dll", "bash");

        _markdown.H4("3. Configure Environment");
        _markdown.Paragraph("Set the environment to Production:");
        _markdown.CodeBlock("export ASPNETCORE_ENVIRONMENT=Production", "bash");

        _markdown.H3("Docker Deployment (Optional)");
        _markdown.Paragraph("If a Dockerfile is present, you can deploy using Docker:");
        
        var dockerfilePath = Path.Combine(projectRoot, "Dockerfile");
        if (File.Exists(dockerfilePath))
        {
            _markdown.H4("Build Docker Image");
            _markdown.CodeBlock("docker build -t pocketlawyer .", "bash");

            _markdown.H4("Run Docker Container");
            _markdown.CodeBlock(
                "docker run -d \\\n" +
                "  -p 5000:80 \\\n" +
                "  -e Nebius__ApiKey=\"your-api-key\" \\\n" +
                "  -e Claude__ApiKey=\"your-api-key\" \\\n" +
                "  -e Gemini__ApiKey=\"your-api-key\" \\\n" +
                "  -e ASPNETCORE_ENVIRONMENT=Production \\\n" +
                "  -v $(pwd)/data:/app/data \\\n" +
                "  pocketlawyer",
                "bash"
            );

            _markdown.Paragraph($"**Source:** {MarkdownGenerator.FileReference("Dockerfile")}");
        }
        else
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No Dockerfile found. Docker deployment not configured."));
        }

        _markdown.H3("Accessing the Application");
        _markdown.Paragraph("Once running, access the application:");
        _markdown.UnorderedList(new[]
        {
            "**Web Interface:** Navigate to the configured URL in your browser",
            "**WebSocket API:** Connect to /employment-analysis or /rental-analysis endpoints",
            "**HTTP API:** Use the /api endpoints for schema retrieval"
        });

        _markdown.Paragraph($"For complete API documentation, see {MarkdownGenerator.Link("API_REFERENCE.md", "API_REFERENCE.md")}.");
    }

    /// <summary>
    /// Generates the Health Checks section.
    /// </summary>
    private void GenerateHealthChecksSection()
    {
        _markdown.H2("Health Checks");
        _markdown.Paragraph("Verify that PocketLawyer is running correctly:");

        _markdown.H3("Application Startup");
        _markdown.Paragraph("Check the console output for successful startup messages:");
        _markdown.UnorderedList(new[]
        {
            "\"Now listening on: http://localhost:5000\"",
            "\"Application started. Press Ctrl+C to shut down.\"",
            "No error messages or exceptions"
        });

        _markdown.H3("Configuration Validation");
        _markdown.Paragraph("Verify configuration is loaded correctly:");
        _markdown.OrderedList(new[]
        {
            "Check that API keys are configured (application should not start without them)",
            "Verify context files are found (check logs for context loading messages)",
            "Confirm upload directory is accessible"
        });

        _markdown.H3("API Endpoint Testing");
        _markdown.Paragraph("Test that API endpoints are responding:");

        _markdown.H4("Test HTTP Endpoint");
        _markdown.CodeBlock("curl http://localhost:5000/api/schemas/employment/basic/1", "bash");
        _markdown.Paragraph("Should return a JSON schema response.");

        _markdown.H4("Test WebSocket Endpoint");
        _markdown.Paragraph("Use a WebSocket client to connect to:");
        _markdown.CodeBlock("ws://localhost:5000/employment-analysis", "");
        _markdown.Paragraph("The connection should be accepted.");

        _markdown.H3("Logs");
        _markdown.Paragraph("Check application logs for errors or warnings:");
        _markdown.UnorderedList(new[]
        {
            "Review console output for error messages",
            "Check log files if file logging is configured",
            "Look for AI service connection errors",
            "Verify context file loading messages"
        });
    }

    /// <summary>
    /// Generates the Troubleshooting section.
    /// </summary>
    private void GenerateTroubleshootingSection()
    {
        _markdown.H2("Troubleshooting");
        _markdown.Paragraph("Common issues and solutions:");

        _markdown.H3("Application Won't Start");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Problem:")} Application fails to start or crashes immediately.");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Solutions:")}");
        _markdown.UnorderedList(new[]
        {
            "Verify .NET SDK version matches requirements (dotnet --version)",
            "Check that all dependencies are restored (dotnet restore)",
            "Ensure configuration files exist (appsettings.json)",
            "Verify API keys are configured correctly",
            "Check console output for specific error messages"
        });

        _markdown.H3("Missing Context Files");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Problem:")} Application reports missing context files.");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Solutions:")}");
        _markdown.UnorderedList(new[]
        {
            $"Verify {MarkdownGenerator.DirectoryReference("data/Context")} directory exists",
            "Check that .txt files are present in the context directory",
            "Ensure ContextDirectory path is configured correctly in appsettings.json",
            "Verify file permissions allow reading context files"
        });

        _markdown.H3("API Key Errors");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Problem:")} AI service API calls fail with authentication errors.");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Solutions:")}");
        _markdown.UnorderedList(new[]
        {
            "Verify API keys are set correctly (dotnet user-secrets list)",
            "Check that API keys are valid and not expired",
            "Ensure correct configuration section names (Nebius:ApiKey, Claude:ApiKey, Gemini:ApiKey)",
            "For production, verify environment variables are set correctly"
        });

        _markdown.H3("Port Already in Use");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Problem:")} Application fails to start because port is already in use.");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Solutions:")}");
        _markdown.UnorderedList(new[]
        {
            "Stop other applications using the same port",
            "Change the port in Properties/launchSettings.json",
            "Use --urls parameter: dotnet run --urls \"http://localhost:5002\""
        });

        _markdown.H3("File Upload Errors");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Problem:")} File uploads fail or return errors.");
        _markdown.Paragraph($"{MarkdownGenerator.Bold("Solutions:")}");
        _markdown.UnorderedList(new[]
        {
            "Verify upload directory exists and is writable",
            "Check file size limits in UploadOptions configuration",
            "Ensure sufficient disk space is available",
            "Verify file permissions on upload directory"
        });

        _markdown.H3("Getting Help");
        _markdown.Paragraph("If you encounter issues not covered here:");
        _markdown.UnorderedList(new[]
        {
            "Check the application logs for detailed error messages",
            "Review the documentation in the docs/ directory",
            "Consult the README.md for additional information",
            "Report issues to the development team with log output and steps to reproduce"
        });
    }

    /// <summary>
    /// Generates the Diagrams section.
    /// </summary>
    private void GenerateDiagramsSection(string projectRoot)
    {
        _markdown.H2("Diagrams");

        // Deployment Architecture
        _markdown.H3("Deployment Architecture");
        _markdown.Paragraph("High-level deployment architecture:");
        GenerateDeploymentArchitectureDiagram();

        // Directory Structure
        _markdown.H3("Directory Structure");
        _markdown.Paragraph("Key directories in the deployment:");
        GenerateDirectoryStructureDiagram(projectRoot);
    }

    private void GenerateDeploymentArchitectureDiagram()
    {
        _markdown.Raw("```mermaid\n");
        _markdown.Raw("graph TB\n");
        _markdown.Raw("    Client[Client Browser/Application]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    subgraph \"PocketLawyer Server\"\n");
        _markdown.Raw("        Web[Web Layer<br/>ASP.NET Core]\n");
        _markdown.Raw("        App[Application Layer<br/>Business Logic]\n");
        _markdown.Raw("        Infra[Infrastructure Layer<br/>AI Services]\n");
        _markdown.Raw("    end\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    subgraph \"External Services\"\n");
        _markdown.Raw("        Nebius[Nebius AI API]\n");
        _markdown.Raw("        Claude[Claude AI API]\n");
        _markdown.Raw("        Gemini[Gemini AI API]\n");
        _markdown.Raw("    end\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    subgraph \"File System\"\n");
        _markdown.Raw("        Context[Context Files<br/>data/Context/]\n");
        _markdown.Raw("        Uploads[Upload Directory]\n");
        _markdown.Raw("        Logs[Log Files]\n");
        _markdown.Raw("    end\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Client -->|HTTP/WebSocket| Web\n");
        _markdown.Raw("    Web --> App\n");
        _markdown.Raw("    App --> Infra\n");
        _markdown.Raw("    Infra -->|API Calls| Nebius\n");
        _markdown.Raw("    Infra -->|API Calls| Claude\n");
        _markdown.Raw("    Infra -->|API Calls| Gemini\n");
        _markdown.Raw("    App -->|Read| Context\n");
        _markdown.Raw("    Web -->|Write| Uploads\n");
        _markdown.Raw("    Web -->|Write| Logs\n");
        _markdown.Raw("```\n\n");
    }

    private void GenerateDirectoryStructureDiagram(string projectRoot)
    {
        _markdown.Raw("```mermaid\n");
        _markdown.Raw("graph TD\n");
        _markdown.Raw("    Root[PocketLawyer/]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Root --> Web[Web/]\n");
        _markdown.Raw("    Root --> App[Application/]\n");
        _markdown.Raw("    Root --> Domain[Domain/]\n");
        _markdown.Raw("    Root --> Infra[Infrastructure/]\n");
        _markdown.Raw("    Root --> Data[data/]\n");
        _markdown.Raw("    Root --> Docs[docs/]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Web --> WebBin[bin/]\n");
        _markdown.Raw("    Web --> WebObj[obj/]\n");
        _markdown.Raw("    Web --> Props[Properties/]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Data --> Context[Context/]\n");
        _markdown.Raw("    Context --> ContextFiles[*.txt files]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    Root --> Config[appsettings.json]\n");
        _markdown.Raw("    Root --> Proj[PocketLawyer.csproj]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    style Context fill:#90EE90\n");
        _markdown.Raw("    style Config fill:#FFD700\n");
        _markdown.Raw("    style Proj fill:#FFD700\n");
        _markdown.Raw("```\n\n");
    }

    /// <summary>
    /// Extracts the .NET version from the TargetFramework string.
    /// For example, "net8.0" returns "8.0", "net6.0" returns "6.0".
    /// </summary>
    private string ExtractDotNetVersion(string targetFramework)
    {
        if (string.IsNullOrEmpty(targetFramework))
        {
            return "Unknown";
        }

        // Handle formats like "net8.0", "net6.0", "netcoreapp3.1", "net48"
        if (targetFramework.StartsWith("net"))
        {
            var versionPart = targetFramework.Substring(3);
            
            // Remove "coreapp" if present (e.g., "netcoreapp3.1")
            if (versionPart.StartsWith("coreapp"))
            {
                versionPart = versionPart.Substring(7);
            }
            
            // Handle "net48" style (no dot) - insert dot after first digit
            if (!versionPart.Contains('.') && versionPart.Length >= 2)
            {
                versionPart = versionPart.Insert(1, ".");
            }

            return versionPart;
        }

        return targetFramework;
    }

    /// <summary>
    /// Gets a description for a NuGet package based on its name.
    /// </summary>
    private string GetPackageDescription(string packageName)
    {
        return packageName switch
        {
            var name when name.Contains("Microsoft.AspNetCore") => "ASP.NET Core framework",
            var name when name.Contains("Swashbuckle") => "Swagger/OpenAPI documentation",
            var name when name.Contains("Serilog") => "Logging framework",
            var name when name.Contains("Newtonsoft.Json") => "JSON serialization",
            var name when name.Contains("System.Text.Json") => "JSON serialization",
            var name when name.Contains("xunit") || name.Contains("XUnit") => "Unit testing framework",
            var name when name.Contains("Moq") => "Mocking framework for tests",
            var name when name.Contains("FluentAssertions") => "Assertion library for tests",
            var name when name.Contains("FsCheck") => "Property-based testing",
            var name when name.Contains("Microsoft.Extensions") => ".NET extensions library",
            var name when name.Contains("EntityFramework") || name.Contains("EF") => "Entity Framework ORM",
            _ => "Application dependency"
        };
    }

    /// <summary>
    /// Writes the generated documentation to a file.
    /// </summary>
    public void WriteToFile(string filePath)
    {
        _markdown.WriteToFile(filePath);
    }
}
