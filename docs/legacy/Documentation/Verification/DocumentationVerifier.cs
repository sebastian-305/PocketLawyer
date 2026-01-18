using PocketLawyer.Documentation.Extraction;

namespace PocketLawyer.Documentation.Verification;

/// <summary>
/// Comprehensive documentation verifier that runs all verification properties on generated docs.
/// Coordinates verification of endpoints, DTOs, configuration, file references, diagrams,
/// dependencies, links, inheritance, versions, structure, enums, exceptions, cross-references,
/// and completeness.
/// </summary>
/// <remarks>
/// Validates Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 9.1, 9.2, 9.3, 9.4
/// Runs all verification properties (Properties 1-17)
/// </remarks>
public class DocumentationVerifier
{
    private readonly string _repositoryRoot;
    private readonly VerificationEngine _verificationEngine;
    private readonly FileReferenceVerifier _fileReferenceVerifier;
    private readonly LinkVerifier _linkVerifier;
    private readonly ErrorReporter _errorReporter;
    private readonly EndpointExtractor _endpointExtractor;
    private readonly SourceFileAnalyzer _sourceFileAnalyzer;
    private readonly ConfigurationAnalyzer _configurationAnalyzer;
    private readonly ProjectFileAnalyzer _projectFileAnalyzer;

    public DocumentationVerifier(string repositoryRoot)
    {
        _repositoryRoot = repositoryRoot ?? throw new ArgumentNullException(nameof(repositoryRoot));
        _verificationEngine = new VerificationEngine(repositoryRoot);
        _fileReferenceVerifier = new FileReferenceVerifier(repositoryRoot);
        _linkVerifier = new LinkVerifier(repositoryRoot);
        _errorReporter = new ErrorReporter();
        _endpointExtractor = new EndpointExtractor();
        _sourceFileAnalyzer = new SourceFileAnalyzer();
        _configurationAnalyzer = new ConfigurationAnalyzer();
        _projectFileAnalyzer = new ProjectFileAnalyzer();
    }

    /// <summary>
    /// Runs all verification properties on generated documentation.
    /// </summary>
    /// <param name="documentationPaths">Paths to all generated documentation files</param>
    /// <returns>Comprehensive verification result</returns>
    public ComprehensiveVerificationResult VerifyAllDocumentation(DocumentationPaths documentationPaths)
    {
        var result = new ComprehensiveVerificationResult
        {
            DocumentationPaths = documentationPaths,
            Timestamp = DateTime.UtcNow
        };

        // Run all verification properties
        result.EndpointVerificationErrors = VerifyEndpoints(documentationPaths);
        result.DtoVerificationErrors = VerifyDtos(documentationPaths);
        result.ConfigurationVerificationErrors = VerifyConfiguration(documentationPaths);
        result.FileReferenceErrors = VerifyFileReferences(documentationPaths);
        result.DiagramVerificationErrors = VerifyDiagrams(documentationPaths);
        result.DependencyVerificationErrors = VerifyDependencies(documentationPaths);
        result.LinkVerificationErrors = VerifyLinks(documentationPaths);
        result.InheritanceVerificationErrors = VerifyInheritance(documentationPaths);
        result.VersionVerificationErrors = VerifyVersions(documentationPaths);
        result.StructureVerificationErrors = VerifyStructure(documentationPaths);
        result.EnumVerificationErrors = VerifyEnums(documentationPaths);
        result.ExceptionVerificationErrors = VerifyExceptions(documentationPaths);
        result.CrossReferenceErrors = VerifyCrossReferences(documentationPaths);
        result.CompletenessErrors = VerifyCompleteness(documentationPaths);

        // Calculate overall status
        result.TotalErrors = result.EndpointVerificationErrors.Count +
                            result.DtoVerificationErrors.Count +
                            result.ConfigurationVerificationErrors.Count +
                            result.FileReferenceErrors.Count +
                            result.DiagramVerificationErrors.Count +
                            result.DependencyVerificationErrors.Count +
                            result.LinkVerificationErrors.Count +
                            result.InheritanceVerificationErrors.Count +
                            result.VersionVerificationErrors.Count +
                            result.StructureVerificationErrors.Count +
                            result.EnumVerificationErrors.Count +
                            result.ExceptionVerificationErrors.Count +
                            result.CrossReferenceErrors.Count +
                            result.CompletenessErrors.Count;

        result.Passed = result.TotalErrors == 0;

        return result;
    }

    /// <summary>
    /// Property 1: Verifies that all documented endpoints exist in source code.
    /// </summary>
    private List<VerificationError> VerifyEndpoints(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(paths.ApiReference))
        {
            errors.Add(new VerificationError
            {
                Type = "DocumentNotFound",
                Claim = "API_REFERENCE.md",
                ExpectedSource = paths.ApiReference,
                Message = "API reference documentation not found"
            });
            return errors;
        }

        // Extract documented endpoints from API_REFERENCE.md
        var documentedEndpoints = ExtractDocumentedEndpoints(paths.ApiReference);

        // Extract actual endpoints from source code
        var sourceEndpoints = new List<EndpointInfo>();
        
        // Extract from PocketController.cs
        var controllerPath = Path.Combine(_repositoryRoot, "Web/PocketController.cs");
        if (File.Exists(controllerPath))
        {
            sourceEndpoints.AddRange(_endpointExtractor.ExtractEndpointsFromController(controllerPath));
        }

        // Verify each documented endpoint exists in source
        errors.AddRange(_verificationEngine.VerifyEndpoints(documentedEndpoints, sourceEndpoints));

        return errors;
    }

    /// <summary>
    /// Property 2: Verifies that all documented DTO properties match actual classes.
    /// </summary>
    private List<VerificationError> VerifyDtos(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(paths.DataModels))
        {
            errors.Add(new VerificationError
            {
                Type = "DocumentNotFound",
                Claim = "DATA_MODELS.md",
                ExpectedSource = paths.DataModels,
                Message = "Data models documentation not found"
            });
            return errors;
        }

        // Extract documented DTOs from DATA_MODELS.md
        var documentedDtos = ExtractDocumentedDtos(paths.DataModels);

        // Extract actual DTOs from source code
        var sourceDtos = new List<ClassInfo>();
        var dtosDirectory = Path.Combine(_repositoryRoot, "Domain/Dtos");
        
        if (Directory.Exists(dtosDirectory))
        {
            var dtoFiles = Directory.GetFiles(dtosDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (var dtoFile in dtoFiles)
            {
                var sourceFileInfo = _sourceFileAnalyzer.AnalyzeFile(dtoFile);
                sourceDtos.AddRange(sourceFileInfo.Classes);
                sourceDtos.AddRange(sourceFileInfo.Records);
                sourceDtos.AddRange(sourceFileInfo.Interfaces);
            }
        }

        // Verify each documented DTO matches source
        errors.AddRange(_verificationEngine.VerifyDtoProperties(documentedDtos, sourceDtos));

        return errors;
    }

    /// <summary>
    /// Property 3: Verifies that all documented configuration matches actual files.
    /// </summary>
    private List<VerificationError> VerifyConfiguration(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(paths.Configuration))
        {
            errors.Add(new VerificationError
            {
                Type = "DocumentNotFound",
                Claim = "CONFIGURATION.md",
                ExpectedSource = paths.Configuration,
                Message = "Configuration documentation not found"
            });
            return errors;
        }

        // Extract documented configuration from CONFIGURATION.md
        var documentedConfig = ExtractDocumentedConfiguration(paths.Configuration);

        // Extract actual configuration from source
        var appsettingsPath = Path.Combine(_repositoryRoot, "appsettings.json");
        var optionsDirectory = Path.Combine(_repositoryRoot, "Options");

        // Build ConfigurationInfo from actual sources
        var sourceConfig = new ConfigurationInfo();

        // Extract JSON keys from appsettings.json
        if (File.Exists(appsettingsPath))
        {
            var configFileInfo = _configurationAnalyzer.AnalyzeConfigurationFile(appsettingsPath);
            foreach (var section in configFileInfo.Sections)
            {
                foreach (var key in section.Keys)
                {
                    var fullKey = string.IsNullOrEmpty(section.Path) ? key.Key : $"{section.Path}:{key.Key}";
                    sourceConfig.JsonKeys.Add(fullKey);
                }
            }
        }

        // Extract option classes
        if (Directory.Exists(optionsDirectory))
        {
            var optionFiles = Directory.GetFiles(optionsDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (var optionFile in optionFiles)
            {
                var optionClassInfo = _configurationAnalyzer.AnalyzeOptionClass(optionFile);
                var classInfo = new ClassInfo
                {
                    Name = optionClassInfo.Name,
                    Namespace = optionClassInfo.Namespace,
                    Properties = optionClassInfo.Properties.Select(p => new PropertyInfo
                    {
                        Name = p.Name,
                        Type = p.Type,
                        Attributes = p.Attributes
                    }).ToList()
                };
                sourceConfig.OptionClasses.Add(classInfo);
            }
        }

        // Verify each documented configuration item matches source
        errors.AddRange(_verificationEngine.VerifyConfiguration(documentedConfig, sourceConfig));

        return errors;
    }

    /// <summary>
    /// Property 4: Verifies that all file references point to existing files.
    /// </summary>
    private List<VerificationError> VerifyFileReferences(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        var allDocPaths = new[]
        {
            paths.Readme,
            paths.Architecture,
            paths.ApiReference,
            paths.DataModels,
            paths.Configuration,
            paths.Deployment
        };

        foreach (var docPath in allDocPaths)
        {
            if (!File.Exists(docPath))
            {
                continue;
            }

            var content = File.ReadAllText(docPath);
            var fileRefErrors = _fileReferenceVerifier.VerifyFileReferences(content, docPath);
            errors.AddRange(fileRefErrors);
        }

        return errors;
    }

    /// <summary>
    /// Property 5: Verifies that all diagram classes exist in codebase.
    /// </summary>
    private List<VerificationError> VerifyDiagrams(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        var allDocPaths = new[]
        {
            paths.Architecture,
            paths.DataModels
        };

        foreach (var docPath in allDocPaths)
        {
            if (!File.Exists(docPath))
            {
                continue;
            }

            var content = File.ReadAllText(docPath);
            var diagramErrors = VerifyMermaidDiagramClasses(content, docPath);
            errors.AddRange(diagramErrors);
        }

        return errors;
    }

    /// <summary>
    /// Property 6: Verifies that documented dependencies match using statements.
    /// </summary>
    private List<VerificationError> VerifyDependencies(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(paths.Architecture))
        {
            return errors;
        }

        // Extract documented dependencies from ARCHITECTURE.md
        var documentedDependencies = ExtractDocumentedDependencies(paths.Architecture);

        // Extract actual dependencies from source code using statements
        var sourceDependencies = ExtractSourceDependencies();

        // Verify each documented dependency exists in source
        foreach (var docDep in documentedDependencies)
        {
            if (!sourceDependencies.Contains(docDep))
            {
                errors.Add(new VerificationError
                {
                    Type = "DependencyNotFound",
                    Claim = $"Documented dependency: {docDep}",
                    ExpectedSource = "Source code using statements",
                    Message = $"Documented dependency not found in actual using statements: {docDep}"
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Property 7: Verifies that markdown links resolve correctly.
    /// </summary>
    private List<VerificationError> VerifyLinks(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        var allDocPaths = new[]
        {
            paths.Readme,
            paths.Architecture,
            paths.ApiReference,
            paths.DataModels,
            paths.Configuration,
            paths.Deployment
        };

        foreach (var docPath in allDocPaths)
        {
            if (!File.Exists(docPath))
            {
                continue;
            }

            var content = File.ReadAllText(docPath);
            var linkErrors = _linkVerifier.VerifyLinks(content, docPath);
            errors.AddRange(linkErrors);
        }

        return errors;
    }

    /// <summary>
    /// Property 8: Verifies that documented inheritance matches class declarations.
    /// </summary>
    private List<VerificationError> VerifyInheritance(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(paths.DataModels))
        {
            return errors;
        }

        // Extract documented inheritance relationships from DATA_MODELS.md
        var documentedInheritance = ExtractDocumentedInheritance(paths.DataModels);

        // Extract actual inheritance from source code
        var sourceDtos = new List<ClassInfo>();
        var dtosDirectory = Path.Combine(_repositoryRoot, "Domain/Dtos");
        
        if (Directory.Exists(dtosDirectory))
        {
            var dtoFiles = Directory.GetFiles(dtosDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (var dtoFile in dtoFiles)
            {
                var sourceFileInfo = _sourceFileAnalyzer.AnalyzeFile(dtoFile);
                sourceDtos.AddRange(sourceFileInfo.Classes);
                sourceDtos.AddRange(sourceFileInfo.Records);
                sourceDtos.AddRange(sourceFileInfo.Interfaces);
            }
        }

        // Verify each documented inheritance relationship
        foreach (var (childClass, baseClass) in documentedInheritance)
        {
            var sourceClass = sourceDtos.FirstOrDefault(c => c.Name == childClass);
            
            if (sourceClass == null)
            {
                errors.Add(new VerificationError
                {
                    Type = "InheritanceClassNotFound",
                    Claim = $"Class {childClass} inherits from {baseClass}",
                    ExpectedSource = $"Domain/Dtos/{childClass}.cs",
                    Message = $"Child class not found in source code: {childClass}"
                });
                continue;
            }

            var actualBaseClass = sourceClass.BaseClass;
            var actualInterfaces = sourceClass.Interfaces;

            // Check if documented base class matches actual base class or interfaces
            var matchesBase = actualBaseClass == baseClass;
            var matchesInterface = actualInterfaces.Contains(baseClass);

            if (!matchesBase && !matchesInterface)
            {
                errors.Add(new VerificationError
                {
                    Type = "InheritanceMismatch",
                    Claim = $"Class {childClass} inherits from {baseClass}",
                    ExpectedSource = $"Domain/Dtos/{childClass}.cs",
                    Message = $"Inheritance mismatch: {childClass} does not inherit from {baseClass}. " +
                             $"Actual base: {actualBaseClass ?? "none"}, Interfaces: {string.Join(", ", actualInterfaces)}"
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Properties 9 & 10: Verifies that documented .NET version and NuGet packages match project file.
    /// </summary>
    private List<VerificationError> VerifyVersions(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        var docsToCheck = new[] { paths.Readme, paths.Deployment };

        foreach (var docPath in docsToCheck)
        {
            if (!File.Exists(docPath))
            {
                continue;
            }

            var content = File.ReadAllText(docPath);

            // Extract documented .NET version
            var documentedDotNetVersion = ExtractDocumentedDotNetVersion(content);

            // Extract documented NuGet packages
            var documentedPackages = ExtractDocumentedNuGetPackages(content);

            // Extract actual versions from project file
            var projectFilePath = Path.Combine(_repositoryRoot, "PocketLawyer.csproj");
            if (File.Exists(projectFilePath))
            {
                var projectInfo = _projectFileAnalyzer.AnalyzeProjectFile(projectFilePath);

                // Verify .NET version
                if (!string.IsNullOrEmpty(documentedDotNetVersion) && 
                    documentedDotNetVersion != projectInfo.TargetFramework)
                {
                    errors.Add(new VerificationError
                    {
                        Type = "DotNetVersionMismatch",
                        Claim = $"Documented .NET version: {documentedDotNetVersion}",
                        ExpectedSource = "PocketLawyer.csproj",
                        Message = $".NET version mismatch: documented '{documentedDotNetVersion}', " +
                                 $"actual '{projectInfo.TargetFramework}'"
                    });
                }

                // Verify NuGet packages
                foreach (var docPackage in documentedPackages)
                {
                    var actualPackage = projectInfo.PackageReferences
                        .FirstOrDefault(p => p.Name.Equals(docPackage, StringComparison.OrdinalIgnoreCase));

                    if (actualPackage == null)
                    {
                        errors.Add(new VerificationError
                        {
                            Type = "NuGetPackageNotFound",
                            Claim = $"Documented NuGet package: {docPackage}",
                            ExpectedSource = "PocketLawyer.csproj",
                            Message = $"Documented NuGet package not found in project file: {docPackage}"
                        });
                    }
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Property 11: Verifies that documented folder structure matches actual directories.
    /// </summary>
    private List<VerificationError> VerifyStructure(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(paths.Architecture))
        {
            return errors;
        }

        var content = File.ReadAllText(paths.Architecture);

        // Extract documented directories
        var documentedDirectories = ExtractDocumentedDirectories(content);

        // Verify each documented directory exists
        foreach (var directory in documentedDirectories)
        {
            var fullPath = Path.Combine(_repositoryRoot, directory);
            if (!Directory.Exists(fullPath))
            {
                errors.Add(new VerificationError
                {
                    Type = "DirectoryNotFound",
                    Claim = $"Documented directory: {directory}",
                    ExpectedSource = directory,
                    Message = $"Documented directory does not exist: {directory}"
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Property 12: Verifies that documented enums match actual definitions.
    /// </summary>
    private List<VerificationError> VerifyEnums(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(paths.DataModels))
        {
            return errors;
        }

        // Extract documented enums from DATA_MODELS.md
        var documentedEnums = ExtractDocumentedEnums(paths.DataModels);

        // Extract actual enums from source code
        var sourceEnums = new List<EnumInfo>();
        var dtosDirectory = Path.Combine(_repositoryRoot, "Domain/Dtos");
        
        if (Directory.Exists(dtosDirectory))
        {
            var dtoFiles = Directory.GetFiles(dtosDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (var dtoFile in dtoFiles)
            {
                var sourceFileInfo = _sourceFileAnalyzer.AnalyzeFile(dtoFile);
                sourceEnums.AddRange(sourceFileInfo.Enums);
            }
        }

        // Verify each documented enum and its values
        foreach (var (enumName, enumValues) in documentedEnums)
        {
            var sourceEnum = sourceEnums.FirstOrDefault(e => e.Name == enumName);
            
            if (sourceEnum == null)
            {
                errors.Add(new VerificationError
                {
                    Type = "EnumNotFound",
                    Claim = $"Documented enum: {enumName}",
                    ExpectedSource = $"Domain/Dtos/{enumName}.cs",
                    Message = $"Documented enum not found in source code: {enumName}"
                });
                continue;
            }

            // Verify each documented enum value
            foreach (var docValue in enumValues)
            {
                var sourceValue = sourceEnum.Members.FirstOrDefault(m => m.Name == docValue);
                
                if (sourceValue == null)
                {
                    errors.Add(new VerificationError
                    {
                        Type = "EnumValueNotFound",
                        Claim = $"Enum value {enumName}.{docValue}",
                        ExpectedSource = $"Domain/Dtos/{enumName}.cs",
                        Message = $"Documented enum value not found: {enumName}.{docValue}"
                    });
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Property 13: Verifies that documented exceptions exist in codebase.
    /// </summary>
    private List<VerificationError> VerifyExceptions(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(paths.ApiReference))
        {
            return errors;
        }

        // Extract documented exceptions from API_REFERENCE.md
        var documentedExceptions = ExtractDocumentedExceptions(paths.ApiReference);

        // Extract actual exceptions from source code
        var sourceExceptions = new List<string>();
        var exceptionsDirectory = Path.Combine(_repositoryRoot, "Application/Exceptions");
        
        if (Directory.Exists(exceptionsDirectory))
        {
            var exceptionFiles = Directory.GetFiles(exceptionsDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (var exceptionFile in exceptionFiles)
            {
                var sourceFileInfo = _sourceFileAnalyzer.AnalyzeFile(exceptionFile);
                sourceExceptions.AddRange(sourceFileInfo.Classes.Select(c => c.Name));
            }
        }

        // Also include standard .NET exceptions
        var standardExceptions = new[]
        {
            "ArgumentException", "ArgumentNullException", "InvalidOperationException",
            "NotSupportedException", "NotImplementedException", "FileNotFoundException",
            "DirectoryNotFoundException", "IOException", "UnauthorizedAccessException"
        };
        sourceExceptions.AddRange(standardExceptions);

        // Verify each documented exception exists
        foreach (var docException in documentedExceptions)
        {
            if (!sourceExceptions.Contains(docException))
            {
                errors.Add(new VerificationError
                {
                    Type = "ExceptionNotFound",
                    Claim = $"Documented exception: {docException}",
                    ExpectedSource = $"Application/Exceptions/{docException}.cs",
                    Message = $"Documented exception not found in source code: {docException}"
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Property 14: Verifies that cross-references between documents are valid.
    /// </summary>
    private List<VerificationError> VerifyCrossReferences(DocumentationPaths paths)
    {
        var allDocPaths = new[]
        {
            paths.Readme,
            paths.Architecture,
            paths.ApiReference,
            paths.DataModels,
            paths.Configuration,
            paths.Deployment
        };

        return _linkVerifier.VerifyCrossReferences(allDocPaths);
    }

    /// <summary>
    /// Properties 15, 16, 17: Verifies completeness of documentation.
    /// </summary>
    private List<VerificationError> VerifyCompleteness(DocumentationPaths paths)
    {
        var errors = new List<VerificationError>();

        // Property 15: All DTOs in directory are documented
        errors.AddRange(VerifyAllDtosDocumented(paths.DataModels));

        // Property 16: All option classes are documented
        errors.AddRange(VerifyAllOptionClassesDocumented(paths.Configuration));

        // Property 17: All controller endpoints are documented
        errors.AddRange(VerifyAllEndpointsDocumented(paths.ApiReference));

        return errors;
    }

    private List<VerificationError> VerifyAllDtosDocumented(string dataModelsPath)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(dataModelsPath))
        {
            return errors;
        }

        // Get all DTOs from source code
        var sourceDtos = new List<string>();
        var dtosDirectory = Path.Combine(_repositoryRoot, "Domain/Dtos");
        
        if (Directory.Exists(dtosDirectory))
        {
            var dtoFiles = Directory.GetFiles(dtosDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (var dtoFile in dtoFiles)
            {
                var sourceFileInfo = _sourceFileAnalyzer.AnalyzeFile(dtoFile);
                sourceDtos.AddRange(sourceFileInfo.Classes.Select(c => c.Name));
                sourceDtos.AddRange(sourceFileInfo.Records.Select(r => r.Name));
                sourceDtos.AddRange(sourceFileInfo.Interfaces.Select(i => i.Name));
            }
        }

        // Get documented DTOs
        var documentedDtos = ExtractDocumentedDtos(dataModelsPath);
        var documentedDtoNames = documentedDtos.Select(d => d.Name).ToHashSet();

        // Check for undocumented DTOs
        foreach (var sourceDto in sourceDtos)
        {
            if (!documentedDtoNames.Contains(sourceDto))
            {
                errors.Add(new VerificationError
                {
                    Type = "DtoNotDocumented",
                    Claim = $"DTO class: {sourceDto}",
                    ExpectedSource = dataModelsPath,
                    Message = $"DTO exists in source code but is not documented: {sourceDto}"
                });
            }
        }

        return errors;
    }

    private List<VerificationError> VerifyAllOptionClassesDocumented(string configurationPath)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(configurationPath))
        {
            return errors;
        }

        // Get all option classes from source code
        var sourceOptionClasses = new List<string>();
        var optionsDirectory = Path.Combine(_repositoryRoot, "Options");
        
        if (Directory.Exists(optionsDirectory))
        {
            var optionFiles = Directory.GetFiles(optionsDirectory, "*.cs", SearchOption.AllDirectories);
            foreach (var optionFile in optionFiles)
            {
                var sourceFileInfo = _sourceFileAnalyzer.AnalyzeFile(optionFile);
                sourceOptionClasses.AddRange(sourceFileInfo.Classes.Select(c => c.Name));
            }
        }

        // Get documented option classes
        var documentedConfig = ExtractDocumentedConfiguration(configurationPath);
        var documentedOptionClasses = documentedConfig
            .Where(c => !string.IsNullOrEmpty(c.OptionClass))
            .Select(c => c.OptionClass)
            .ToHashSet();

        // Check for undocumented option classes
        foreach (var sourceOptionClass in sourceOptionClasses)
        {
            if (!documentedOptionClasses.Contains(sourceOptionClass))
            {
                errors.Add(new VerificationError
                {
                    Type = "OptionClassNotDocumented",
                    Claim = $"Option class: {sourceOptionClass}",
                    ExpectedSource = configurationPath,
                    Message = $"Option class exists in source code but is not documented: {sourceOptionClass}"
                });
            }
        }

        return errors;
    }

    private List<VerificationError> VerifyAllEndpointsDocumented(string apiReferencePath)
    {
        var errors = new List<VerificationError>();

        if (!File.Exists(apiReferencePath))
        {
            return errors;
        }

        // Get all endpoints from source code
        var sourceEndpoints = new List<EndpointInfo>();
        var controllerPath = Path.Combine(_repositoryRoot, "Web/PocketController.cs");
        
        if (File.Exists(controllerPath))
        {
            sourceEndpoints.AddRange(_endpointExtractor.ExtractEndpointsFromController(controllerPath));
        }

        // Get documented endpoints
        var documentedEndpoints = ExtractDocumentedEndpoints(apiReferencePath);
        var documentedEndpointKeys = documentedEndpoints
            .Select(e => $"{e.HttpMethod}:{e.Path}")
            .ToHashSet();

        // Check for undocumented endpoints
        foreach (var sourceEndpoint in sourceEndpoints)
        {
            var key = $"{sourceEndpoint.HttpMethod}:{sourceEndpoint.Path}";
            if (!documentedEndpointKeys.Contains(key))
            {
                errors.Add(new VerificationError
                {
                    Type = "EndpointNotDocumented",
                    Claim = $"Endpoint: {sourceEndpoint.HttpMethod} {sourceEndpoint.Path}",
                    ExpectedSource = apiReferencePath,
                    Message = $"Endpoint exists in source code but is not documented: " +
                             $"{sourceEndpoint.HttpMethod} {sourceEndpoint.Path} in {sourceEndpoint.ControllerName}.{sourceEndpoint.ActionName}"
                });
            }
        }

        return errors;
    }

    #region Helper Methods for Extracting Documented Information

    /// <summary>
    /// Extracts documented endpoints from API_REFERENCE.md.
    /// </summary>
    private List<DocumentedEndpoint> ExtractDocumentedEndpoints(string apiReferencePath)
    {
        // This is a simplified extraction - in a real implementation, this would parse
        // the markdown structure to extract endpoint information
        var endpoints = new List<DocumentedEndpoint>();
        
        if (!File.Exists(apiReferencePath))
        {
            return endpoints;
        }

        var content = File.ReadAllText(apiReferencePath);
        var lines = content.Split('\n');

        // Simple pattern matching for endpoint documentation
        // Format: ### GET /api/path or ### POST /api/path
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("###") && (line.Contains("GET") || line.Contains("POST") || 
                line.Contains("PUT") || line.Contains("DELETE") || line.Contains("PATCH")))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    var method = parts[1];
                    var path = parts[2];
                    
                    endpoints.Add(new DocumentedEndpoint
                    {
                        Path = path,
                        HttpMethod = method,
                        ControllerName = "PocketController", // Default
                        ActionName = "Unknown",
                        IsWebSocket = line.Contains("WebSocket") || path.Contains("-analysis")
                    });
                }
            }
        }

        return endpoints;
    }

    /// <summary>
    /// Extracts documented DTOs from DATA_MODELS.md.
    /// </summary>
    private List<DocumentedDto> ExtractDocumentedDtos(string dataModelsPath)
    {
        var dtos = new List<DocumentedDto>();
        
        if (!File.Exists(dataModelsPath))
        {
            return dtos;
        }

        var content = File.ReadAllText(dataModelsPath);
        var lines = content.Split('\n');

        DocumentedDto? currentDto = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Look for DTO class headers (### ClassName or #### ClassName)
            if ((line.StartsWith("###") || line.StartsWith("####")) && !line.Contains("Properties"))
            {
                if (currentDto != null)
                {
                    dtos.Add(currentDto);
                }
                
                var className = line.TrimStart('#').Trim();
                currentDto = new DocumentedDto { Name = className };
            }
            // Look for property definitions (- PropertyName: Type)
            else if (line.StartsWith("-") && line.Contains(":") && currentDto != null)
            {
                var parts = line.TrimStart('-').Split(':');
                if (parts.Length >= 2)
                {
                    var propName = parts[0].Trim().TrimStart('*').TrimEnd('*').Trim();
                    var propType = parts[1].Trim().Split(' ')[0].Trim();
                    
                    currentDto.Properties.Add(new DocumentedProperty
                    {
                        Name = propName,
                        Type = propType
                    });
                }
            }
        }

        if (currentDto != null)
        {
            dtos.Add(currentDto);
        }

        return dtos;
    }

    /// <summary>
    /// Extracts documented configuration from CONFIGURATION.md.
    /// </summary>
    private List<DocumentedConfigItem> ExtractDocumentedConfiguration(string configurationPath)
    {
        var configItems = new List<DocumentedConfigItem>();
        
        if (!File.Exists(configurationPath))
        {
            return configItems;
        }

        var content = File.ReadAllText(configurationPath);
        
        // Extract JSON keys (format: "Key:SubKey" or `Key:SubKey`)
        var jsonKeyPattern = new System.Text.RegularExpressions.Regex(@"[`""]([A-Za-z0-9:_]+)[`""]");
        foreach (System.Text.RegularExpressions.Match match in jsonKeyPattern.Matches(content))
        {
            var key = match.Groups[1].Value;
            if (key.Contains(":"))
            {
                configItems.Add(new DocumentedConfigItem { JsonKey = key });
            }
        }

        // Extract option classes (format: OptionsClassName or `OptionsClassName`)
        var optionClassPattern = new System.Text.RegularExpressions.Regex(@"([A-Za-z]+Options)");
        foreach (System.Text.RegularExpressions.Match match in optionClassPattern.Matches(content))
        {
            var optionClass = match.Groups[1].Value;
            configItems.Add(new DocumentedConfigItem { OptionClass = optionClass });
        }

        return configItems;
    }

    /// <summary>
    /// Extracts documented dependencies from ARCHITECTURE.md.
    /// </summary>
    private List<string> ExtractDocumentedDependencies(string architecturePath)
    {
        var dependencies = new List<string>();
        
        if (!File.Exists(architecturePath))
        {
            return dependencies;
        }

        var content = File.ReadAllText(architecturePath);
        
        // Look for dependency mentions (e.g., "Web → Application", "Application → Domain")
        // Match either → or -> (arrow symbols)
        var dependencyPattern = new System.Text.RegularExpressions.Regex(@"([A-Za-z]+)\s*(?:→|->)\s*([A-Za-z]+)");
        foreach (System.Text.RegularExpressions.Match match in dependencyPattern.Matches(content))
        {
            var from = match.Groups[1].Value;
            var to = match.Groups[2].Value;
            dependencies.Add($"{from} → {to}");
        }

        return dependencies;
    }

    /// <summary>
    /// Extracts source dependencies from actual code.
    /// </summary>
    private List<string> ExtractSourceDependencies()
    {
        var dependencies = new HashSet<string>();
        
        var layerDirectories = new[] { "Web", "Application", "Domain", "Infrastructure" };
        
        foreach (var layerDir in layerDirectories)
        {
            var fullPath = Path.Combine(_repositoryRoot, layerDir);
            if (!Directory.Exists(fullPath))
            {
                continue;
            }

            var csFiles = Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories);
            foreach (var csFile in csFiles)
            {
                var sourceFileInfo = _sourceFileAnalyzer.AnalyzeFile(csFile);
                
                // Analyze using statements to determine dependencies
                foreach (var usingStatement in sourceFileInfo.Usings)
                {
                    if (usingStatement.StartsWith("PocketLawyer."))
                    {
                        var parts = usingStatement.Split('.');
                        if (parts.Length >= 2)
                        {
                            var targetLayer = parts[1];
                            if (layerDirectories.Contains(targetLayer) && targetLayer != layerDir)
                            {
                                dependencies.Add($"{layerDir} → {targetLayer}");
                            }
                        }
                    }
                }
            }
        }

        return dependencies.ToList();
    }

    /// <summary>
    /// Extracts documented inheritance relationships from DATA_MODELS.md.
    /// </summary>
    private List<(string childClass, string baseClass)> ExtractDocumentedInheritance(string dataModelsPath)
    {
        var inheritance = new List<(string, string)>();
        
        if (!File.Exists(dataModelsPath))
        {
            return inheritance;
        }

        var content = File.ReadAllText(dataModelsPath);
        
        // Look for inheritance patterns in mermaid diagrams or text
        // Format: "ChildClass inherits from BaseClass" or "BaseClass <|-- ChildClass"
        var inheritancePattern1 = new System.Text.RegularExpressions.Regex(@"([A-Za-z]+)\s+inherits\s+from\s+([A-Za-z]+)");
        var inheritancePattern2 = new System.Text.RegularExpressions.Regex(@"([A-Za-z]+)\s*<\|--\s*([A-Za-z]+)");
        
        foreach (System.Text.RegularExpressions.Match match in inheritancePattern1.Matches(content))
        {
            var childClass = match.Groups[1].Value;
            var baseClass = match.Groups[2].Value;
            inheritance.Add((childClass, baseClass));
        }
        
        foreach (System.Text.RegularExpressions.Match match in inheritancePattern2.Matches(content))
        {
            var baseClass = match.Groups[1].Value;
            var childClass = match.Groups[2].Value;
            inheritance.Add((childClass, baseClass));
        }

        return inheritance;
    }

    /// <summary>
    /// Extracts documented .NET version from documentation.
    /// </summary>
    private string ExtractDocumentedDotNetVersion(string content)
    {
        // Look for .NET version patterns (e.g., ".NET 8", "net8.0")
        var versionPattern = new System.Text.RegularExpressions.Regex(@"\.NET\s+(\d+)|net(\d+\.\d+)");
        var match = versionPattern.Match(content);
        
        if (match.Success)
        {
            if (match.Groups[1].Success)
            {
                return $"net{match.Groups[1].Value}.0";
            }
            else if (match.Groups[2].Success)
            {
                return $"net{match.Groups[2].Value}";
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Extracts documented NuGet packages from documentation.
    /// </summary>
    private List<string> ExtractDocumentedNuGetPackages(string content)
    {
        var packages = new List<string>();
        
        // Look for common NuGet package patterns
        var packagePattern = new System.Text.RegularExpressions.Regex(@"([A-Za-z0-9\.]+)\s+\d+\.\d+\.\d+");
        
        foreach (System.Text.RegularExpressions.Match match in packagePattern.Matches(content))
        {
            var packageName = match.Groups[1].Value;
            // Filter to likely package names (contain dots, start with capital letter)
            if (packageName.Contains(".") && char.IsUpper(packageName[0]))
            {
                packages.Add(packageName);
            }
        }

        return packages.Distinct().ToList();
    }

    /// <summary>
    /// Extracts documented directories from documentation.
    /// </summary>
    private List<string> ExtractDocumentedDirectories(string content)
    {
        var directories = new List<string>();
        
        // Look for directory patterns (e.g., `Web/`, `Application/`, `Domain/Dtos/`)
        var directoryPattern = new System.Text.RegularExpressions.Regex(@"`([A-Za-z0-9_/]+/)`");
        
        foreach (System.Text.RegularExpressions.Match match in directoryPattern.Matches(content))
        {
            var directory = match.Groups[1].Value;
            directories.Add(directory);
        }

        return directories.Distinct().ToList();
    }

    /// <summary>
    /// Extracts documented enums from DATA_MODELS.md.
    /// </summary>
    private List<(string enumName, List<string> values)> ExtractDocumentedEnums(string dataModelsPath)
    {
        var enums = new List<(string, List<string>)>();
        
        if (!File.Exists(dataModelsPath))
        {
            return enums;
        }

        var content = File.ReadAllText(dataModelsPath);
        var lines = content.Split('\n');

        string? currentEnum = null;
        var currentValues = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Look for enum headers (### EnumName or #### EnumName)
            if ((line.StartsWith("###") || line.StartsWith("####")) && line.Contains("Enum"))
            {
                if (currentEnum != null)
                {
                    enums.Add((currentEnum, currentValues));
                    currentValues = new List<string>();
                }
                
                currentEnum = line.TrimStart('#').Trim().Replace("Enum", "").Trim();
            }
            // Look for enum values (- ValueName)
            else if (line.StartsWith("-") && currentEnum != null)
            {
                var value = line.TrimStart('-').Trim().Split(' ')[0].Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    currentValues.Add(value);
                }
            }
        }

        if (currentEnum != null)
        {
            enums.Add((currentEnum, currentValues));
        }

        return enums;
    }

    /// <summary>
    /// Extracts documented exceptions from API_REFERENCE.md.
    /// </summary>
    private List<string> ExtractDocumentedExceptions(string apiReferencePath)
    {
        var exceptions = new List<string>();
        
        if (!File.Exists(apiReferencePath))
        {
            return exceptions;
        }

        var content = File.ReadAllText(apiReferencePath);
        
        // Look for exception class names (typically end with "Exception")
        var exceptionPattern = new System.Text.RegularExpressions.Regex(@"([A-Za-z]+Exception)");
        
        foreach (System.Text.RegularExpressions.Match match in exceptionPattern.Matches(content))
        {
            var exceptionName = match.Groups[1].Value;
            exceptions.Add(exceptionName);
        }

        return exceptions.Distinct().ToList();
    }

    /// <summary>
    /// Verifies that classes mentioned in Mermaid diagrams exist in the codebase.
    /// </summary>
    private List<VerificationError> VerifyMermaidDiagramClasses(string content, string documentPath)
    {
        var errors = new List<VerificationError>();
        
        // Extract class names from mermaid diagrams
        var classPattern = new System.Text.RegularExpressions.Regex(@"class\s+([A-Za-z0-9_]+)");
        var classNames = new HashSet<string>();
        
        foreach (System.Text.RegularExpressions.Match match in classPattern.Matches(content))
        {
            classNames.Add(match.Groups[1].Value);
        }

        // Get all classes from source code
        var sourceClasses = new HashSet<string>();
        var directories = new[] { "Web", "Application", "Domain", "Infrastructure" };
        
        foreach (var dir in directories)
        {
            var fullPath = Path.Combine(_repositoryRoot, dir);
            if (!Directory.Exists(fullPath))
            {
                continue;
            }

            var csFiles = Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories);
            foreach (var csFile in csFiles)
            {
                var sourceFileInfo = _sourceFileAnalyzer.AnalyzeFile(csFile);
                sourceClasses.UnionWith(sourceFileInfo.Classes.Select(c => c.Name));
                sourceClasses.UnionWith(sourceFileInfo.Records.Select(r => r.Name));
                sourceClasses.UnionWith(sourceFileInfo.Interfaces.Select(i => i.Name));
            }
        }

        // Verify each diagram class exists in source
        foreach (var className in classNames)
        {
            if (!sourceClasses.Contains(className))
            {
                errors.Add(new VerificationError
                {
                    Type = "DiagramClassNotFound",
                    Claim = $"Class in diagram: {className}",
                    ExpectedSource = $"Source code",
                    Message = $"Class mentioned in Mermaid diagram not found in codebase: {className}"
                });
            }
        }

        return errors;
    }

    #endregion

    /// <summary>
    /// Generates a comprehensive verification report.
    /// </summary>
    public string GenerateReport(ComprehensiveVerificationResult result)
    {
        return _errorReporter.GenerateReport(ConvertToVerificationResults(result));
    }

    /// <summary>
    /// Converts comprehensive verification result to list of verification results for reporting.
    /// </summary>
    private List<VerificationResult> ConvertToVerificationResults(ComprehensiveVerificationResult result)
    {
        var results = new List<VerificationResult>();

        // Create a verification result for each property
        results.Add(CreatePropertyResult("Property 1: Endpoint Verification", result.EndpointVerificationErrors));
        results.Add(CreatePropertyResult("Property 2: DTO Verification", result.DtoVerificationErrors));
        results.Add(CreatePropertyResult("Property 3: Configuration Verification", result.ConfigurationVerificationErrors));
        results.Add(CreatePropertyResult("Property 4: File Reference Verification", result.FileReferenceErrors));
        results.Add(CreatePropertyResult("Property 5: Diagram Verification", result.DiagramVerificationErrors));
        results.Add(CreatePropertyResult("Property 6: Dependency Verification", result.DependencyVerificationErrors));
        results.Add(CreatePropertyResult("Property 7: Link Verification", result.LinkVerificationErrors));
        results.Add(CreatePropertyResult("Property 8: Inheritance Verification", result.InheritanceVerificationErrors));
        results.Add(CreatePropertyResult("Property 9-10: Version Verification", result.VersionVerificationErrors));
        results.Add(CreatePropertyResult("Property 11: Structure Verification", result.StructureVerificationErrors));
        results.Add(CreatePropertyResult("Property 12: Enum Verification", result.EnumVerificationErrors));
        results.Add(CreatePropertyResult("Property 13: Exception Verification", result.ExceptionVerificationErrors));
        results.Add(CreatePropertyResult("Property 14: Cross-Reference Verification", result.CrossReferenceErrors));
        results.Add(CreatePropertyResult("Property 15-17: Completeness Verification", result.CompletenessErrors));

        return results;
    }

    private VerificationResult CreatePropertyResult(string propertyName, List<VerificationError> errors)
    {
        return new VerificationResult
        {
            DocumentPath = propertyName,
            Errors = errors,
            Passed = errors.Count == 0,
            Checks = new List<Check>()
        };
    }
}

/// <summary>
/// Paths to all documentation files.
/// </summary>
public class DocumentationPaths
{
    public string Readme { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public string ApiReference { get; set; } = string.Empty;
    public string DataModels { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;
}

/// <summary>
/// Comprehensive verification result containing all property verification results.
/// </summary>
public class ComprehensiveVerificationResult
{
    public DocumentationPaths DocumentationPaths { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public bool Passed { get; set; }
    public int TotalErrors { get; set; }

    // Property-specific error lists
    public List<VerificationError> EndpointVerificationErrors { get; set; } = new();
    public List<VerificationError> DtoVerificationErrors { get; set; } = new();
    public List<VerificationError> ConfigurationVerificationErrors { get; set; } = new();
    public List<VerificationError> FileReferenceErrors { get; set; } = new();
    public List<VerificationError> DiagramVerificationErrors { get; set; } = new();
    public List<VerificationError> DependencyVerificationErrors { get; set; } = new();
    public List<VerificationError> LinkVerificationErrors { get; set; } = new();
    public List<VerificationError> InheritanceVerificationErrors { get; set; } = new();
    public List<VerificationError> VersionVerificationErrors { get; set; } = new();
    public List<VerificationError> StructureVerificationErrors { get; set; } = new();
    public List<VerificationError> EnumVerificationErrors { get; set; } = new();
    public List<VerificationError> ExceptionVerificationErrors { get; set; } = new();
    public List<VerificationError> CrossReferenceErrors { get; set; } = new();
    public List<VerificationError> CompletenessErrors { get; set; } = new();
}
