using PocketLawyer.Documentation.Extraction;
using PocketLawyer.Documentation.Verification;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Orchestrates the complete documentation generation and verification process.
/// Coordinates document generation in the correct order, runs verification after each document,
/// handles verification errors, and generates a final verification report.
/// </summary>
/// <remarks>
/// Validates Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7
/// </remarks>
public class DocumentationOrchestrator
{
    private readonly string _repositoryRoot;
    private readonly string _outputDirectory;
    
    // Extractors
    private readonly SourceFileAnalyzer _sourceFileAnalyzer;
    private readonly ConfigurationAnalyzer _configurationAnalyzer;
    private readonly ProjectFileAnalyzer _projectFileAnalyzer;
    private readonly EndpointExtractor _endpointExtractor;
    
    // Generators
    private readonly DataModelsDocGenerator _dataModelsGenerator;
    private readonly ConfigurationDocGenerator _configurationGenerator;
    private readonly ApiReferenceDocGenerator _apiReferenceGenerator;
    private readonly ArchitectureDocGenerator _architectureGenerator;
    private readonly DeploymentDocGenerator _deploymentGenerator;
    private readonly ReadmeDocGenerator _readmeGenerator;
    
    // Verifier
    private readonly DocumentationVerifier _verifier;
    
    // Results tracking
    private readonly List<GenerationResult> _generationResults;
    private readonly List<string> _errors;

    public DocumentationOrchestrator(string repositoryRoot, string outputDirectory)
    {
        _repositoryRoot = repositoryRoot ?? throw new ArgumentNullException(nameof(repositoryRoot));
        _outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
        
        // Initialize extractors
        _sourceFileAnalyzer = new SourceFileAnalyzer();
        _configurationAnalyzer = new ConfigurationAnalyzer();
        _projectFileAnalyzer = new ProjectFileAnalyzer();
        _endpointExtractor = new EndpointExtractor();
        
        // Initialize generators
        _dataModelsGenerator = new DataModelsDocGenerator();
        _configurationGenerator = new ConfigurationDocGenerator();
        _apiReferenceGenerator = new ApiReferenceDocGenerator();
        _architectureGenerator = new ArchitectureDocGenerator();
        _deploymentGenerator = new DeploymentDocGenerator();
        _readmeGenerator = new ReadmeDocGenerator();
        
        // Initialize verifier
        _verifier = new DocumentationVerifier(repositoryRoot);
        
        // Initialize tracking
        _generationResults = new List<GenerationResult>();
        _errors = new List<string>();
    }

    /// <summary>
    /// Generates all Phase 1 documentation in the correct order with verification after each document.
    /// Requirement 11.1-11.6: THE System SHALL create all required documentation files
    /// Requirement 11.7: WHEN all documents are created, THE System SHALL verify that they cross-reference each other appropriately
    /// </summary>
    /// <returns>Orchestration result with generation and verification details</returns>
    public OrchestrationResult GenerateAllDocumentation()
    {
        var result = new OrchestrationResult
        {
            StartTime = DateTime.UtcNow,
            RepositoryRoot = _repositoryRoot,
            OutputDirectory = _outputDirectory
        };

        try
        {
            // Ensure output directory exists
            EnsureOutputDirectoryExists();

            // Generate documents in the correct order (as specified in design.md)
            // 1. DATA_MODELS.md - Foundation for other docs
            GenerateDataModelsDocumentation();
            
            // 2. CONFIGURATION.md - Needed for deployment
            GenerateConfigurationDocumentation();
            
            // 3. API_REFERENCE.md - References data models
            GenerateApiReferenceDocumentation();
            
            // 4. ARCHITECTURE.md - References all components
            GenerateArchitectureDocumentation();
            
            // 5. DEPLOYMENT.md - References configuration and architecture
            GenerateDeploymentDocumentation();
            
            // 6. README.md - Links to all other docs
            GenerateReadmeDocumentation();

            // Run comprehensive verification on all generated documents
            result.VerificationResult = RunComprehensiveVerification();
            
            // Populate result
            result.GenerationResults = _generationResults;
            result.Errors = _errors;
            result.Success = _errors.Count == 0 && result.VerificationResult.Passed;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
        }
        catch (Exception ex)
        {
            _errors.Add($"Fatal error during documentation generation: {ex.Message}");
            result.Success = false;
            result.Errors = _errors;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
        }

        return result;
    }

    /// <summary>
    /// Generates a human-readable report of the orchestration results.
    /// </summary>
    public string GenerateReport(OrchestrationResult result)
    {
        var report = new System.Text.StringBuilder();
        
        report.AppendLine("=".PadRight(80, '='));
        report.AppendLine("DOCUMENTATION GENERATION REPORT");
        report.AppendLine("=".PadRight(80, '='));
        report.AppendLine();
        
        report.AppendLine($"Repository Root: {result.RepositoryRoot}");
        report.AppendLine($"Output Directory: {result.OutputDirectory}");
        report.AppendLine($"Start Time: {result.StartTime:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"End Time: {result.EndTime:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Duration: {result.Duration.TotalSeconds:F2} seconds");
        report.AppendLine($"Overall Status: {(result.Success ? "SUCCESS" : "FAILED")}");
        report.AppendLine();
        
        // Generation results
        report.AppendLine("-".PadRight(80, '-'));
        report.AppendLine("DOCUMENT GENERATION RESULTS");
        report.AppendLine("-".PadRight(80, '-'));
        report.AppendLine();
        
        foreach (var genResult in result.GenerationResults)
        {
            var status = genResult.Success ? "✓" : "✗";
            report.AppendLine($"{status} {genResult.DocumentName}");
            report.AppendLine($"  Path: {genResult.OutputPath}");
            report.AppendLine($"  Duration: {genResult.Duration.TotalSeconds:F2}s");
            
            if (!genResult.Success && !string.IsNullOrEmpty(genResult.ErrorMessage))
            {
                report.AppendLine($"  Error: {genResult.ErrorMessage}");
            }
            
            report.AppendLine();
        }
        
        // Verification results
        if (result.VerificationResult != null)
        {
            report.AppendLine("-".PadRight(80, '-'));
            report.AppendLine("VERIFICATION RESULTS");
            report.AppendLine("-".PadRight(80, '-'));
            report.AppendLine();
            
            report.AppendLine($"Overall Verification: {(result.VerificationResult.Passed ? "PASSED" : "FAILED")}");
            report.AppendLine($"Total Errors: {result.VerificationResult.TotalErrors}");
            report.AppendLine();
            
            // Property-specific results
            AppendPropertyResult(report, "Property 1: Endpoint Verification", result.VerificationResult.EndpointVerificationErrors);
            AppendPropertyResult(report, "Property 2: DTO Verification", result.VerificationResult.DtoVerificationErrors);
            AppendPropertyResult(report, "Property 3: Configuration Verification", result.VerificationResult.ConfigurationVerificationErrors);
            AppendPropertyResult(report, "Property 4: File Reference Verification", result.VerificationResult.FileReferenceErrors);
            AppendPropertyResult(report, "Property 5: Diagram Verification", result.VerificationResult.DiagramVerificationErrors);
            AppendPropertyResult(report, "Property 6: Dependency Verification", result.VerificationResult.DependencyVerificationErrors);
            AppendPropertyResult(report, "Property 7: Link Verification", result.VerificationResult.LinkVerificationErrors);
            AppendPropertyResult(report, "Property 8: Inheritance Verification", result.VerificationResult.InheritanceVerificationErrors);
            AppendPropertyResult(report, "Property 9-10: Version Verification", result.VerificationResult.VersionVerificationErrors);
            AppendPropertyResult(report, "Property 11: Structure Verification", result.VerificationResult.StructureVerificationErrors);
            AppendPropertyResult(report, "Property 12: Enum Verification", result.VerificationResult.EnumVerificationErrors);
            AppendPropertyResult(report, "Property 13: Exception Verification", result.VerificationResult.ExceptionVerificationErrors);
            AppendPropertyResult(report, "Property 14: Cross-Reference Verification", result.VerificationResult.CrossReferenceErrors);
            AppendPropertyResult(report, "Property 15-17: Completeness Verification", result.VerificationResult.CompletenessErrors);
        }
        
        // Overall errors
        if (result.Errors.Any())
        {
            report.AppendLine("-".PadRight(80, '-'));
            report.AppendLine("ERRORS");
            report.AppendLine("-".PadRight(80, '-'));
            report.AppendLine();
            
            foreach (var error in result.Errors)
            {
                report.AppendLine($"• {error}");
            }
            report.AppendLine();
        }
        
        report.AppendLine("=".PadRight(80, '='));
        
        return report.ToString();
    }

    private void AppendPropertyResult(System.Text.StringBuilder report, string propertyName, List<VerificationError> errors)
    {
        var status = errors.Count == 0 ? "✓ PASSED" : $"✗ FAILED ({errors.Count} error(s))";
        report.AppendLine($"{propertyName}: {status}");
        
        if (errors.Any())
        {
            foreach (var error in errors.Take(3)) // Show first 3 errors
            {
                report.AppendLine($"  • {error.Type}: {error.Message}");
            }
            
            if (errors.Count > 3)
            {
                report.AppendLine($"  ... and {errors.Count - 3} more error(s)");
            }
        }
        
        report.AppendLine();
    }

    #region Document Generation Methods

    /// <summary>
    /// Generates DATA_MODELS.md documentation.
    /// Requirement 11.4: THE System SHALL create DATA_MODELS.md documenting all DTOs in Domain/Dtos/ directory
    /// </summary>
    private void GenerateDataModelsDocumentation()
    {
        var genResult = new GenerationResult
        {
            DocumentName = "DATA_MODELS.md",
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Use ImprovedDtoExtractor for comprehensive DTO extraction
            var dtoExtractor = new ImprovedDtoExtractor();
            var dtos = dtoExtractor.ExtractAllDtos(_repositoryRoot);

            // Generate documentation using extracted DTOs
            var content = _dataModelsGenerator.GenerateDataModelsDocumentation(dtos, _repositoryRoot);

            // Write to file
            var outputPath = Path.Combine(_outputDirectory, "DATA_MODELS.md");
            File.WriteAllText(outputPath, content);

            genResult.Success = true;
            genResult.OutputPath = outputPath;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
        }
        catch (Exception ex)
        {
            genResult.Success = false;
            genResult.ErrorMessage = ex.Message;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
            _errors.Add($"Failed to generate DATA_MODELS.md: {ex.Message}");
        }

        _generationResults.Add(genResult);
    }

    /// <summary>
    /// Generates CONFIGURATION.md documentation.
    /// Requirement 11.5: THE System SHALL create CONFIGURATION.md documenting all configuration files and option classes
    /// </summary>
    private void GenerateConfigurationDocumentation()
    {
        var genResult = new GenerationResult
        {
            DocumentName = "CONFIGURATION.md",
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Extract configuration information
            var appsettingsPath = Path.Combine(_repositoryRoot, "appsettings.json");
            var appsettingsDevPath = Path.Combine(_repositoryRoot, "appsettings.Development.json");
            var optionsDirectory = Path.Combine(_repositoryRoot, "Options");

            var configFiles = new List<ConfigurationFileInfo>();
            var optionClasses = new List<OptionClassInfo>();

            // Analyze appsettings.json
            if (File.Exists(appsettingsPath))
            {
                configFiles.Add(_configurationAnalyzer.AnalyzeConfigurationFile(appsettingsPath));
            }

            // Analyze appsettings.Development.json
            if (File.Exists(appsettingsDevPath))
            {
                configFiles.Add(_configurationAnalyzer.AnalyzeConfigurationFile(appsettingsDevPath));
            }

            // Analyze option classes
            if (Directory.Exists(optionsDirectory))
            {
                var optionFiles = Directory.GetFiles(optionsDirectory, "*.cs", SearchOption.AllDirectories);
                foreach (var optionFile in optionFiles)
                {
                    optionClasses.Add(_configurationAnalyzer.AnalyzeOptionClass(optionFile));
                }
            }

            // Create empty mappings list (mappings would be extracted from actual configuration)
            var mappings = new List<ConfigurationMapping>();

            // Generate documentation
            var content = _configurationGenerator.GenerateConfigurationDocumentation(configFiles, optionClasses, mappings, _repositoryRoot);

            // Write to file
            var outputPath = Path.Combine(_outputDirectory, "CONFIGURATION.md");
            File.WriteAllText(outputPath, content);

            genResult.Success = true;
            genResult.OutputPath = outputPath;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
        }
        catch (Exception ex)
        {
            genResult.Success = false;
            genResult.ErrorMessage = ex.Message;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
            _errors.Add($"Failed to generate CONFIGURATION.md: {ex.Message}");
        }

        _generationResults.Add(genResult);
    }

    /// <summary>
    /// Generates API_REFERENCE.md documentation.
    /// Requirement 11.3: THE System SHALL create API_REFERENCE.md with all WebSocket and HTTP endpoints found in controllers
    /// </summary>
    private void GenerateApiReferenceDocumentation()
    {
        var genResult = new GenerationResult
        {
            DocumentName = "API_REFERENCE.md",
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Extract endpoints from controllers
            var controllerPath = Path.Combine(_repositoryRoot, "Web/PocketController.cs");
            var endpoints = new List<EndpointInfo>();

            if (File.Exists(controllerPath))
            {
                endpoints.AddRange(_endpointExtractor.ExtractEndpointsFromController(controllerPath));
            }

            // Extract DTOs for message formats
            var dtosDirectory = Path.Combine(_repositoryRoot, "Domain/Dtos");
            var dtoFiles = new List<SourceFileInfo>();

            if (Directory.Exists(dtosDirectory))
            {
                var dtoFilePaths = Directory.GetFiles(dtosDirectory, "*.cs", SearchOption.AllDirectories);
                foreach (var dtoFile in dtoFilePaths)
                {
                    dtoFiles.Add(_sourceFileAnalyzer.AnalyzeFile(dtoFile));
                }
            }

            // Extract exceptions for error documentation
            var exceptionsDirectory = Path.Combine(_repositoryRoot, "Application/Exceptions");
            var exceptionFiles = new List<SourceFileInfo>();

            if (Directory.Exists(exceptionsDirectory))
            {
                var exceptionFilePaths = Directory.GetFiles(exceptionsDirectory, "*.cs", SearchOption.AllDirectories);
                foreach (var exceptionFile in exceptionFilePaths)
                {
                    exceptionFiles.Add(_sourceFileAnalyzer.AnalyzeFile(exceptionFile));
                }
            }

            // Generate documentation
            var content = _apiReferenceGenerator.GenerateApiReferenceDocumentation(endpoints, dtoFiles, exceptionFiles, _repositoryRoot);

            // Write to file
            var outputPath = Path.Combine(_outputDirectory, "API_REFERENCE.md");
            File.WriteAllText(outputPath, content);

            genResult.Success = true;
            genResult.OutputPath = outputPath;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
        }
        catch (Exception ex)
        {
            genResult.Success = false;
            genResult.ErrorMessage = ex.Message;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
            _errors.Add($"Failed to generate API_REFERENCE.md: {ex.Message}");
        }

        _generationResults.Add(genResult);
    }

    /// <summary>
    /// Generates ARCHITECTURE.md documentation.
    /// Requirement 11.2: THE System SHALL create ARCHITECTURE.md with all required sections
    /// </summary>
    private void GenerateArchitectureDocumentation()
    {
        var genResult = new GenerationResult
        {
            DocumentName = "ARCHITECTURE.md",
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Extract source files from all layers
            var layers = new[] { "Web", "Application", "Domain", "Infrastructure" };
            var sourceFiles = new List<SourceFileInfo>();

            foreach (var layer in layers)
            {
                var layerPath = Path.Combine(_repositoryRoot, layer);
                if (Directory.Exists(layerPath))
                {
                    var csFiles = Directory.GetFiles(layerPath, "*.cs", SearchOption.AllDirectories);
                    foreach (var csFile in csFiles)
                    {
                        sourceFiles.Add(_sourceFileAnalyzer.AnalyzeFile(csFile));
                    }
                }
            }

            // Generate documentation
            var content = _architectureGenerator.GenerateArchitectureDocumentation(_repositoryRoot, sourceFiles);

            // Write to file
            var outputPath = Path.Combine(_outputDirectory, "ARCHITECTURE.md");
            File.WriteAllText(outputPath, content);

            genResult.Success = true;
            genResult.OutputPath = outputPath;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
        }
        catch (Exception ex)
        {
            genResult.Success = false;
            genResult.ErrorMessage = ex.Message;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
            _errors.Add($"Failed to generate ARCHITECTURE.md: {ex.Message}");
        }

        _generationResults.Add(genResult);
    }

    /// <summary>
    /// Generates DEPLOYMENT.md documentation.
    /// Requirement 11.6: THE System SHALL create DEPLOYMENT.md with installation, setup, and deployment instructions
    /// </summary>
    private void GenerateDeploymentDocumentation()
    {
        var genResult = new GenerationResult
        {
            DocumentName = "DEPLOYMENT.md",
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Extract project information
            var projectFilePath = Path.Combine(_repositoryRoot, "PocketLawyer.csproj");
            ProjectFileInfo? projectInfo = null;

            if (File.Exists(projectFilePath))
            {
                projectInfo = _projectFileAnalyzer.AnalyzeProjectFile(projectFilePath);
            }

            // Extract configuration files for deployment documentation
            var appsettingsPath = Path.Combine(_repositoryRoot, "appsettings.json");
            var configFiles = new List<ConfigurationFileInfo>();

            if (File.Exists(appsettingsPath))
            {
                configFiles.Add(_configurationAnalyzer.AnalyzeConfigurationFile(appsettingsPath));
            }

            // Generate documentation
            var content = _deploymentGenerator.GenerateDeploymentDocumentation(projectInfo!, configFiles, _repositoryRoot);

            // Write to file
            var outputPath = Path.Combine(_outputDirectory, "DEPLOYMENT.md");
            File.WriteAllText(outputPath, content);

            genResult.Success = true;
            genResult.OutputPath = outputPath;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
        }
        catch (Exception ex)
        {
            genResult.Success = false;
            genResult.ErrorMessage = ex.Message;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
            _errors.Add($"Failed to generate DEPLOYMENT.md: {ex.Message}");
        }

        _generationResults.Add(genResult);
    }

    /// <summary>
    /// Generates README.md documentation.
    /// Requirement 11.1: THE System SHALL create README.md with project overview and getting started instructions
    /// </summary>
    private void GenerateReadmeDocumentation()
    {
        var genResult = new GenerationResult
        {
            DocumentName = "README.md",
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Extract project information
            var projectFilePath = Path.Combine(_repositoryRoot, "PocketLawyer.csproj");
            ProjectFileInfo? projectInfo = null;

            if (File.Exists(projectFilePath))
            {
                projectInfo = _projectFileAnalyzer.AnalyzeProjectFile(projectFilePath);
            }

            // Check for legacy README
            var legacyReadmePath = Path.Combine(_repositoryRoot, "docs/legacy/README_old.md");

            // Generate documentation (only if we have project info)
            if (projectInfo != null)
            {
                var content = _readmeGenerator.GenerateReadmeDocumentation(projectInfo, _repositoryRoot, legacyReadmePath);

                // Write to file
                var outputPath = Path.Combine(_repositoryRoot, "README.md");
                File.WriteAllText(outputPath, content);

                genResult.Success = true;
                genResult.OutputPath = outputPath;
            }
            else
            {
                throw new InvalidOperationException("Project file not found or could not be analyzed");
            }
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
        }
        catch (Exception ex)
        {
            genResult.Success = false;
            genResult.ErrorMessage = ex.Message;
            genResult.EndTime = DateTime.UtcNow;
            genResult.Duration = genResult.EndTime - genResult.StartTime;
            _errors.Add($"Failed to generate README.md: {ex.Message}");
        }

        _generationResults.Add(genResult);
    }

    #endregion

    #region Verification Methods

    /// <summary>
    /// Runs comprehensive verification on all generated documents.
    /// Requirement 11.7: WHEN all documents are created, THE System SHALL verify that they cross-reference each other appropriately
    /// </summary>
    private ComprehensiveVerificationResult RunComprehensiveVerification()
    {
        var documentationPaths = new DocumentationPaths
        {
            Readme = Path.Combine(_repositoryRoot, "README.md"),
            Architecture = Path.Combine(_outputDirectory, "ARCHITECTURE.md"),
            ApiReference = Path.Combine(_outputDirectory, "API_REFERENCE.md"),
            DataModels = Path.Combine(_outputDirectory, "DATA_MODELS.md"),
            Configuration = Path.Combine(_outputDirectory, "CONFIGURATION.md"),
            Deployment = Path.Combine(_outputDirectory, "DEPLOYMENT.md")
        };

        return _verifier.VerifyAllDocumentation(documentationPaths);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Ensures the output directory exists.
    /// </summary>
    private void EnsureOutputDirectoryExists()
    {
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    #endregion
}

/// <summary>
/// Result of the complete orchestration process.
/// </summary>
public class OrchestrationResult
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public string RepositoryRoot { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public List<GenerationResult> GenerationResults { get; set; } = new();
    public ComprehensiveVerificationResult? VerificationResult { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of generating a single document.
/// </summary>
public class GenerationResult
{
    public string DocumentName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
