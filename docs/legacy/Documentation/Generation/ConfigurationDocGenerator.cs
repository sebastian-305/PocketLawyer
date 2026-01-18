using PocketLawyer.Documentation.Extraction;
using System.Text;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Generates CONFIGURATION.md documentation from extracted configuration information.
/// Includes appsettings.json structure, option classes, logging configuration, and AI service configuration.
/// </summary>
/// <remarks>
/// Validates Requirements: 4.1, 4.2, 4.3, 4.4
/// </remarks>
public class ConfigurationDocGenerator
{
    private readonly MarkdownGenerator _markdown;
    private readonly MermaidDiagramGenerator _diagramGenerator;
    private readonly DocumentationLanguage _lang;

    public ConfigurationDocGenerator()
    {
        _markdown = new MarkdownGenerator();
        _diagramGenerator = new MermaidDiagramGenerator();
        _lang = DocumentationLanguage.German;
    }

    /// <summary>
    /// Generates complete CONFIGURATION.md documentation from extracted configuration.
    /// Requirement 4.1: WHEN documenting appsettings.json structure, THE System SHALL read the actual appsettings.json and appsettings.Development.json files
    /// Requirement 4.2: WHEN documenting configuration options, THE System SHALL extract option classes from Options/ directory
    /// </summary>
    /// <param name="configFiles">List of configuration files (appsettings.json, appsettings.Development.json)</param>
    /// <param name="optionClasses">List of option class information</param>
    /// <param name="mappings">Mappings between configuration sections and option classes</param>
    /// <param name="projectRoot">Root directory of the project for file references</param>
    /// <returns>Complete CONFIGURATION.md content</returns>
    public string GenerateConfigurationDocumentation(
        IEnumerable<ConfigurationFileInfo> configFiles,
        IEnumerable<OptionClassInfo> optionClasses,
        IEnumerable<ConfigurationMapping> mappings,
        string projectRoot = "")
    {
        _markdown.Clear();

        // Header
        _markdown.H1(_lang.ConfigurationTitle);
        _markdown.Paragraph("Dieses Dokument beschreibt alle Konfigurationsoptionen für das PocketLawyer-System. " +
                          "Alle Informationen werden aus den tatsächlichen Konfigurationsdateien und Options-Klassen extrahiert.");

        // Table of Contents
        GenerateTableOfContents(configFiles, optionClasses);

        // Overview
        GenerateOverviewSection(configFiles, optionClasses, projectRoot);

        // appsettings.json Structure
        GenerateAppSettingsSection(configFiles, projectRoot);

        // Logging Configuration
        GenerateLoggingConfigurationSection(configFiles, projectRoot);

        // AI Service Configuration
        GenerateAiServiceConfigurationSection(optionClasses, mappings, projectRoot);

        // Path Configuration
        GeneratePathConfigurationSection(optionClasses, mappings, projectRoot);

        // Upload Configuration
        GenerateUploadConfigurationSection(optionClasses, mappings, projectRoot);

        // Context Configuration
        GenerateContextConfigurationSection(optionClasses, mappings, projectRoot);

        // Other Option Classes
        GenerateOtherOptionClassesSection(optionClasses, mappings, projectRoot);

        // User Secrets
        GenerateUserSecretsSection();

        // Environment Variables
        GenerateEnvironmentVariablesSection();

        // Diagrams
        GenerateDiagramsSection(configFiles, optionClasses);

        return _markdown.Content;
    }

    private void GenerateTableOfContents(IEnumerable<ConfigurationFileInfo> configFiles, IEnumerable<OptionClassInfo> optionClasses)
    {
        _markdown.H2(_lang.TableOfContents);

        var tocItems = new List<string>
        {
            MarkdownGenerator.AnchorLink(_lang.Overview, AnchorGenerator.Generate(_lang.Overview)),
            MarkdownGenerator.AnchorLink(_lang.ConfigurationStructure, AnchorGenerator.Generate(_lang.ConfigurationStructure)),
            MarkdownGenerator.AnchorLink(_lang.ConfigurationLogging, AnchorGenerator.Generate(_lang.ConfigurationLogging)),
            MarkdownGenerator.AnchorLink(_lang.ConfigurationAiServices, AnchorGenerator.Generate(_lang.ConfigurationAiServices)),
            MarkdownGenerator.AnchorLink(_lang.ConfigurationPaths, AnchorGenerator.Generate(_lang.ConfigurationPaths)),
            MarkdownGenerator.AnchorLink(_lang.ConfigurationUpload, AnchorGenerator.Generate(_lang.ConfigurationUpload)),
            MarkdownGenerator.AnchorLink(_lang.ConfigurationContext, AnchorGenerator.Generate(_lang.ConfigurationContext))
        };

        // Add other option classes if they exist
        var standardOptions = new[] { "NebiusOptions", "ClaudeOptions", "GeminiOptions", "PathOptions", "UploadOptions", "ContextOptions" };
        var otherOptions = optionClasses.Where(o => !standardOptions.Contains(o.Name)).ToList();
        if (otherOptions.Any())
        {
            tocItems.Add(MarkdownGenerator.AnchorLink("Weitere Options-Klassen", AnchorGenerator.Generate("Weitere Options-Klassen")));
        }

        tocItems.Add(MarkdownGenerator.AnchorLink(_lang.ConfigurationSecrets, AnchorGenerator.Generate(_lang.ConfigurationSecrets)));
        tocItems.Add(MarkdownGenerator.AnchorLink(_lang.ConfigurationEnvVars, AnchorGenerator.Generate(_lang.ConfigurationEnvVars)));
        tocItems.Add(MarkdownGenerator.AnchorLink(_lang.Diagrams, AnchorGenerator.Generate(_lang.Diagrams)));

        _markdown.UnorderedList(tocItems);
    }

    private void GenerateOverviewSection(IEnumerable<ConfigurationFileInfo> configFiles, IEnumerable<OptionClassInfo> optionClasses, string projectRoot)
    {
        _markdown.H2("Overview");

        var configFilesList = configFiles.ToList();
        var optionClassesList = optionClasses.ToList();

        _markdown.Paragraph($"PocketLawyer uses {configFilesList.Count} configuration file(s) and {optionClassesList.Count} option class(es) " +
                          "to manage application settings. Configuration follows the ASP.NET Core configuration pattern with " +
                          "strongly-typed option classes bound to configuration sections.");

        _markdown.H3("Configuration Files");
        _markdown.Paragraph("The following configuration files are used:");

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

        _markdown.H3("Option Classes");
        _markdown.Paragraph("Configuration is mapped to the following strongly-typed option classes:");

        foreach (var optionClass in optionClassesList.OrderBy(o => o.Name))
        {
            var relativePath = string.IsNullOrEmpty(projectRoot)
                ? optionClass.Path
                : Path.GetRelativePath(projectRoot, optionClass.Path).Replace("\\", "/");
            _markdown.ListItem($"{MarkdownGenerator.InlineCode(optionClass.Name)} - {MarkdownGenerator.FileReference(relativePath)}");
        }

        _markdown.BlankLine();
    }

    private void GenerateAppSettingsSection(IEnumerable<ConfigurationFileInfo> configFiles, string projectRoot)
    {
        _markdown.H2("appsettings.json Structure");
        _markdown.Paragraph("The main configuration file structure with all available sections and keys.");

        var mainConfig = configFiles.FirstOrDefault(c => c.Path.EndsWith("appsettings.json"));
        if (mainConfig == null)
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No appsettings.json file found."));
            return;
        }

        var relativePath = string.IsNullOrEmpty(projectRoot)
            ? mainConfig.Path
            : Path.GetRelativePath(projectRoot, mainConfig.Path).Replace("\\", "/");

        _markdown.Paragraph($"**Source:** {MarkdownGenerator.FileReference(relativePath)}");

        // Group sections by category
        var sections = mainConfig.Sections.Where(s => s.Name != "Root").OrderBy(s => s.Name).ToList();

        foreach (var section in sections)
        {
            _markdown.H3(section.Name);

            if (section.Keys.Any())
            {
                var keyRows = section.Keys.Select(k => new[]
                {
                    k.Key,
                    k.Type,
                    k.Value ?? MarkdownGenerator.Italic("null")
                });

                _markdown.Table(
                    new[] { "Key", "Type", "Value" },
                    keyRows
                );
            }
            else
            {
                _markdown.Paragraph(MarkdownGenerator.Italic("No direct keys in this section. See nested sections."));
            }
        }

        // Root-level keys
        var rootSection = mainConfig.Sections.FirstOrDefault(s => s.Name == "Root");
        if (rootSection != null && rootSection.Keys.Any())
        {
            _markdown.H3("Root Level Keys");

            var keyRows = rootSection.Keys.Select(k => new[]
            {
                k.Key,
                k.Type,
                k.Value ?? MarkdownGenerator.Italic("null")
            });

            _markdown.Table(
                new[] { "Key", "Type", "Value" },
                keyRows
            );
        }
    }

    private void GenerateLoggingConfigurationSection(IEnumerable<ConfigurationFileInfo> configFiles, string projectRoot)
    {
        _markdown.H2("Logging Configuration");
        _markdown.Paragraph("Configuration for application logging, including file logging and log levels.");

        // Requirement 4.3: WHEN documenting logging configuration, THE System SHALL extract settings from actual configuration files
        var mainConfig = configFiles.FirstOrDefault(c => c.Path.EndsWith("appsettings.json"));
        if (mainConfig == null)
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No logging configuration found."));
            return;
        }

        var loggingSection = mainConfig.Sections.FirstOrDefault(s => s.Name.Equals("Logging", StringComparison.OrdinalIgnoreCase));
        if (loggingSection == null)
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No Logging section found in configuration."));
            return;
        }

        _markdown.Paragraph($"**Configuration Section:** {MarkdownGenerator.InlineCode(loggingSection.Path)}");

        if (loggingSection.Keys.Any())
        {
            var keyRows = loggingSection.Keys.Select(k => new[]
            {
                k.Key,
                k.Type,
                k.Value ?? MarkdownGenerator.Italic("null"),
                GetLoggingKeyDescription(k.Key)
            });

            _markdown.Table(
                new[] { "Key", "Type", "Value", "Description" },
                keyRows
            );
        }

        // Look for nested logging sections (like LogLevel)
        var nestedLoggingSections = mainConfig.Sections
            .Where(s => s.Path.StartsWith("Logging:", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var nestedSection in nestedLoggingSections)
        {
            _markdown.H3(nestedSection.Name);

            if (nestedSection.Keys.Any())
            {
                var keyRows = nestedSection.Keys.Select(k => new[]
                {
                    k.Key,
                    k.Value ?? MarkdownGenerator.Italic("null")
                });

                _markdown.Table(
                    new[] { "Category", "Level" },
                    keyRows
                );
            }
        }
    }

    private void GenerateAiServiceConfigurationSection(IEnumerable<OptionClassInfo> optionClasses, IEnumerable<ConfigurationMapping> mappings, string projectRoot)
    {
        _markdown.H2("AI Service Configuration");
        _markdown.Paragraph("Configuration for AI service providers (Nebius, Gemini, Claude) used for contract analysis.");

        // Requirement 4.4: WHEN documenting AI service configuration, THE System SHALL reference actual option classes (NebiusOptions, ClaudeOptions, GeminiOptions)
        var aiOptions = new[] { "NebiusOptions", "ClaudeOptions", "GeminiOptions" };
        var aiOptionClasses = optionClasses.Where(o => aiOptions.Contains(o.Name)).ToList();

        if (!aiOptionClasses.Any())
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No AI service option classes found."));
            return;
        }

        foreach (var optionClass in aiOptionClasses.OrderBy(o => o.Name))
        {
            GenerateOptionClassDocumentation(optionClass, mappings, projectRoot);
        }
    }

    private void GeneratePathConfigurationSection(IEnumerable<OptionClassInfo> optionClasses, IEnumerable<ConfigurationMapping> mappings, string projectRoot)
    {
        _markdown.H2("Path Configuration");
        _markdown.Paragraph("Configuration for file system paths used by the application.");

        var pathOptions = optionClasses.FirstOrDefault(o => o.Name == "PathOptions");
        if (pathOptions == null)
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No PathOptions class found."));
            return;
        }

        GenerateOptionClassDocumentation(pathOptions, mappings, projectRoot);
    }

    private void GenerateUploadConfigurationSection(IEnumerable<OptionClassInfo> optionClasses, IEnumerable<ConfigurationMapping> mappings, string projectRoot)
    {
        _markdown.H2("Upload Configuration");
        _markdown.Paragraph("Configuration for file upload limits and restrictions.");

        var uploadOptions = optionClasses.FirstOrDefault(o => o.Name == "UploadOptions");
        if (uploadOptions == null)
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No UploadOptions class found."));
            return;
        }

        GenerateOptionClassDocumentation(uploadOptions, mappings, projectRoot);
    }

    private void GenerateContextConfigurationSection(IEnumerable<OptionClassInfo> optionClasses, IEnumerable<ConfigurationMapping> mappings, string projectRoot)
    {
        _markdown.H2("Context Configuration");
        _markdown.Paragraph("Configuration for AI context files and prompts.");

        var contextOptions = optionClasses.FirstOrDefault(o => o.Name == "ContextOptions");
        if (contextOptions == null)
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No ContextOptions class found."));
            return;
        }

        GenerateOptionClassDocumentation(contextOptions, mappings, projectRoot);
    }

    private void GenerateOtherOptionClassesSection(IEnumerable<OptionClassInfo> optionClasses, IEnumerable<ConfigurationMapping> mappings, string projectRoot)
    {
        var standardOptions = new[] { "NebiusOptions", "ClaudeOptions", "GeminiOptions", "PathOptions", "UploadOptions", "ContextOptions" };
        var otherOptions = optionClasses.Where(o => !standardOptions.Contains(o.Name)).ToList();

        if (!otherOptions.Any())
        {
            return; // Skip section if no other options
        }

        _markdown.H2("Other Option Classes");
        _markdown.Paragraph("Additional configuration option classes.");

        foreach (var optionClass in otherOptions.OrderBy(o => o.Name))
        {
            GenerateOptionClassDocumentation(optionClass, mappings, projectRoot);
        }
    }

    /// <summary>
    /// Generates documentation for a single option class.
    /// Requirement 4.2: WHEN documenting configuration options, THE System SHALL extract option classes from Options/ directory
    /// </summary>
    private void GenerateOptionClassDocumentation(OptionClassInfo optionClass, IEnumerable<ConfigurationMapping> mappings, string projectRoot)
    {
        _markdown.H3(optionClass.Name);

        var relativePath = string.IsNullOrEmpty(projectRoot)
            ? optionClass.Path
            : Path.GetRelativePath(projectRoot, optionClass.Path).Replace("\\", "/");

        _markdown.Paragraph($"**Source:** {MarkdownGenerator.ClassReference(optionClass.Name, optionClass.Namespace, relativePath)}");

        // Find mapping for this option class
        var mapping = mappings.FirstOrDefault(m => m.OptionClassName == optionClass.Name);
        if (mapping != null)
        {
            _markdown.Paragraph($"**Configuration Section:** {MarkdownGenerator.InlineCode(mapping.SectionName)}");
        }

        // Properties
        if (optionClass.Properties.Any())
        {
            _markdown.H4("Properties");

            var propertyRows = optionClass.Properties.Select(p =>
            {
                var configKey = mapping?.PropertyMappings
                    .FirstOrDefault(pm => pm.PropertyName == p.Name)?.ConfigurationKey ?? "-";

                var defaultValue = p.DefaultValue ?? "-";

                return new[] { p.Name, p.Type, configKey, defaultValue };
            });

            _markdown.Table(
                new[] { "Property", "Type", "Configuration Key", "Default Value" },
                propertyRows
            );
        }

        _markdown.BlankLine();
    }

    private void GenerateUserSecretsSection()
    {
        _markdown.H2("User Secrets");
        _markdown.Paragraph("For development, sensitive configuration values (like API keys) should be stored in user secrets " +
                          "rather than in appsettings.json files.");

        _markdown.H3("Setting User Secrets");
        _markdown.Paragraph("Use the .NET CLI to manage user secrets:");

        _markdown.CodeBlock("dotnet user-secrets init\ndotnet user-secrets set \"Nebius:ApiKey\" \"your-api-key-here\"\ndotnet user-secrets set \"Claude:ApiKey\" \"your-api-key-here\"\ndotnet user-secrets set \"Gemini:ApiKey\" \"your-api-key-here\"", "bash");

        _markdown.H3("Listing User Secrets");
        _markdown.CodeBlock("dotnet user-secrets list", "bash");

        _markdown.H3("Removing User Secrets");
        _markdown.CodeBlock("dotnet user-secrets remove \"Nebius:ApiKey\"\ndotnet user-secrets clear  # Remove all secrets", "bash");
    }

    private void GenerateEnvironmentVariablesSection()
    {
        _markdown.H2("Environment Variables");
        _markdown.Paragraph("Configuration can also be provided via environment variables. " +
                          "Environment variables override values from appsettings.json and user secrets.");

        _markdown.H3("Format");
        _markdown.Paragraph("Use double underscores (__) to represent configuration hierarchy:");

        _markdown.CodeBlock("Nebius__ApiKey=your-api-key\nPaths__UploadDirectory=/app/uploads\nLogging__LogLevel__Default=Information", "bash");

        _markdown.H3("Setting Environment Variables");

        _markdown.H4("Windows (PowerShell)");
        _markdown.CodeBlock("$env:Nebius__ApiKey=\"your-api-key\"\n$env:ASPNETCORE_ENVIRONMENT=\"Production\"", "powershell");

        _markdown.H4("Linux/macOS");
        _markdown.CodeBlock("export Nebius__ApiKey=\"your-api-key\"\nexport ASPNETCORE_ENVIRONMENT=\"Production\"", "bash");

        _markdown.H4("Docker");
        _markdown.CodeBlock("docker run -e Nebius__ApiKey=\"your-api-key\" -e ASPNETCORE_ENVIRONMENT=\"Production\" pocketlawyer", "bash");
    }

    private void GenerateDiagramsSection(IEnumerable<ConfigurationFileInfo> configFiles, IEnumerable<OptionClassInfo> optionClasses)
    {
        _markdown.H2("Diagrams");

        // Configuration Hierarchy
        _markdown.H3("Configuration Hierarchy");
        _markdown.Paragraph("Overview of configuration sources and their precedence:");

        GenerateConfigurationHierarchyDiagram();

        // Configuration to Options Mapping
        _markdown.H3("Configuration to Options Mapping");
        _markdown.Paragraph("Mapping between configuration sections and option classes:");

        GenerateConfigurationMappingDiagram(configFiles, optionClasses);
    }

    private void GenerateConfigurationHierarchyDiagram()
    {
        _markdown.Raw("```mermaid\n");
        _markdown.Raw("graph TD\n");
        _markdown.Raw("    A[Application Configuration] --> B[appsettings.json]\n");
        _markdown.Raw("    A --> C[appsettings.{Environment}.json]\n");
        _markdown.Raw("    A --> D[User Secrets]\n");
        _markdown.Raw("    A --> E[Environment Variables]\n");
        _markdown.Raw("    A --> F[Command Line Arguments]\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    B -.->|Lowest Priority| G[Final Configuration]\n");
        _markdown.Raw("    C -.->|Overrides| G\n");
        _markdown.Raw("    D -.->|Overrides| G\n");
        _markdown.Raw("    E -.->|Overrides| G\n");
        _markdown.Raw("    F -.->|Highest Priority| G\n");
        _markdown.Raw("    \n");
        _markdown.Raw("    G --> H[Strongly-Typed Options]\n");
        _markdown.Raw("    H --> I[Application Code]\n");
        _markdown.Raw("```\n\n");
    }

    private void GenerateConfigurationMappingDiagram(IEnumerable<ConfigurationFileInfo> configFiles, IEnumerable<OptionClassInfo> optionClasses)
    {
        var mainConfig = configFiles.FirstOrDefault(c => c.Path.EndsWith("appsettings.json"));
        if (mainConfig == null)
        {
            _markdown.Paragraph(MarkdownGenerator.Italic("No configuration mapping available."));
            return;
        }

        _markdown.Raw("```mermaid\n");
        _markdown.Raw("graph LR\n");
        _markdown.Raw("    Config[appsettings.json]\n");
        _markdown.Raw("    \n");

        var optionsList = optionClasses.ToList();
        var sections = mainConfig.Sections.Where(s => s.Name != "Root").Take(10).ToList(); // Limit for diagram clarity

        foreach (var section in sections)
        {
            var safeSectionName = section.Name.Replace(".", "_").Replace(":", "_");
            _markdown.Raw($"    Config --> {safeSectionName}[{section.Name}]\n");

            // Try to find matching option class
            var matchingOption = optionsList.FirstOrDefault(o =>
                o.Name.Contains(section.Name, StringComparison.OrdinalIgnoreCase) ||
                section.Name.Contains(o.Name.Replace("Options", ""), StringComparison.OrdinalIgnoreCase));

            if (matchingOption != null)
            {
                var safeOptionName = matchingOption.Name.Replace(".", "_");
                _markdown.Raw($"    {safeSectionName} -.->|Binds to| {safeOptionName}[{matchingOption.Name}]\n");
            }
        }

        _markdown.Raw("```\n\n");
    }

    private string GetLoggingKeyDescription(string key)
    {
        return key.ToLower() switch
        {
            "includeScopes" => "Include logging scopes in output",
            "path" => "File path for log output",
            "append" => "Append to existing log file",
            "minLevel" => "Minimum log level",
            "maxLevel" => "Maximum log level",
            _ => "-"
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
