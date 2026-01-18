using PocketLawyer.Documentation.Extraction;

namespace PocketLawyer.Documentation.Verification;

/// <summary>
/// Core verification engine that coordinates all verification checks.
/// Collects and reports errors, generates verification reports.
/// </summary>
/// <remarks>
/// Validates Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6
/// </remarks>
public class VerificationEngine
{
    private readonly FileReferenceVerifier _fileReferenceVerifier;
    private readonly LinkVerifier _linkVerifier;
    private readonly ErrorReporter _errorReporter;

    public VerificationEngine(string repositoryRoot)
    {
        _fileReferenceVerifier = new FileReferenceVerifier(repositoryRoot);
        _linkVerifier = new LinkVerifier(repositoryRoot);
        _errorReporter = new ErrorReporter();
    }

    /// <summary>
    /// Runs all verification checks on a documentation file.
    /// </summary>
    /// <param name="documentPath">Path to the documentation file to verify</param>
    /// <returns>Verification result with all checks and errors</returns>
    public VerificationResult VerifyDocument(string documentPath)
    {
        var result = new VerificationResult
        {
            DocumentPath = documentPath,
            Checks = new List<Check>(),
            Errors = new List<VerificationError>()
        };

        if (!File.Exists(documentPath))
        {
            result.Errors.Add(new VerificationError
            {
                Type = "DocumentNotFound",
                Claim = $"Document: {documentPath}",
                ExpectedSource = documentPath,
                Message = "Documentation file does not exist"
            });
            result.Passed = false;
            return result;
        }

        var documentContent = File.ReadAllText(documentPath);

        // Run file reference verification
        var fileRefErrors = _fileReferenceVerifier.VerifyFileReferences(documentContent, documentPath);
        result.Errors.AddRange(fileRefErrors);

        // Run link verification
        var linkErrors = _linkVerifier.VerifyLinks(documentContent, documentPath);
        result.Errors.AddRange(linkErrors);

        // Determine if verification passed
        result.Passed = result.Errors.Count == 0;

        return result;
    }

    /// <summary>
    /// Runs verification on multiple documentation files.
    /// </summary>
    /// <param name="documentPaths">List of documentation file paths to verify</param>
    /// <returns>List of verification results</returns>
    public List<VerificationResult> VerifyDocuments(IEnumerable<string> documentPaths)
    {
        var results = new List<VerificationResult>();

        foreach (var documentPath in documentPaths)
        {
            var result = VerifyDocument(documentPath);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Generates a comprehensive verification report.
    /// </summary>
    /// <param name="results">List of verification results</param>
    /// <returns>Formatted verification report</returns>
    public string GenerateVerificationReport(List<VerificationResult> results)
    {
        return _errorReporter.GenerateReport(results);
    }

    /// <summary>
    /// Verifies that all documented endpoints exist in source code.
    /// </summary>
    /// <param name="documentedEndpoints">List of endpoints documented in API_REFERENCE.md</param>
    /// <param name="sourceEndpoints">List of endpoints extracted from source code</param>
    /// <returns>List of verification errors</returns>
    public List<VerificationError> VerifyEndpoints(
        List<DocumentedEndpoint> documentedEndpoints,
        List<EndpointInfo> sourceEndpoints)
    {
        var errors = new List<VerificationError>();

        foreach (var documented in documentedEndpoints)
        {
            var matchingEndpoint = sourceEndpoints.FirstOrDefault(e =>
                e.Path == documented.Path &&
                e.HttpMethod == documented.HttpMethod &&
                e.ControllerName == documented.ControllerName &&
                e.ActionName == documented.ActionName);

            if (matchingEndpoint == null)
            {
                errors.Add(new VerificationError
                {
                    Type = "EndpointNotFound",
                    Claim = $"{documented.HttpMethod} {documented.Path} in {documented.ControllerName}.{documented.ActionName}",
                    ExpectedSource = $"{documented.ControllerName}.cs",
                    Message = "Documented endpoint not found in source code"
                });
            }
            else if (documented.IsWebSocket && !matchingEndpoint.IsWebSocket)
            {
                errors.Add(new VerificationError
                {
                    Type = "WebSocketMismatch",
                    Claim = $"{documented.Path} documented as WebSocket",
                    ExpectedSource = $"{documented.ControllerName}.cs",
                    Message = "Endpoint documented as WebSocket but not detected as WebSocket in source"
                });
            }
        }

        return errors;
    }

    /// <summary>
    /// Verifies that all documented DTO properties match actual classes.
    /// </summary>
    /// <param name="documentedDtos">List of DTOs documented in DATA_MODELS.md</param>
    /// <param name="sourceDtos">List of DTOs extracted from source code</param>
    /// <returns>List of verification errors</returns>
    public List<VerificationError> VerifyDtoProperties(
        List<DocumentedDto> documentedDtos,
        List<ClassInfo> sourceDtos)
    {
        var errors = new List<VerificationError>();

        foreach (var documented in documentedDtos)
        {
            var matchingDto = sourceDtos.FirstOrDefault(d => d.Name == documented.Name);

            if (matchingDto == null)
            {
                errors.Add(new VerificationError
                {
                    Type = "DtoNotFound",
                    Claim = $"DTO class: {documented.Name}",
                    ExpectedSource = $"Domain/Dtos/{documented.Name}.cs",
                    Message = "Documented DTO not found in source code"
                });
                continue;
            }

            // Verify each documented property
            foreach (var docProperty in documented.Properties)
            {
                var matchingProperty = matchingDto.Properties.FirstOrDefault(p => p.Name == docProperty.Name);

                if (matchingProperty == null)
                {
                    errors.Add(new VerificationError
                    {
                        Type = "PropertyNotFound",
                        Claim = $"Property '{docProperty.Name}' in {documented.Name}",
                        ExpectedSource = $"Domain/Dtos/{documented.Name}.cs",
                        Message = $"Documented property not found in DTO class"
                    });
                }
                else if (matchingProperty.Type != docProperty.Type)
                {
                    errors.Add(new VerificationError
                    {
                        Type = "PropertyTypeMismatch",
                        Claim = $"Property '{docProperty.Name}' of type '{docProperty.Type}' in {documented.Name}",
                        ExpectedSource = $"Domain/Dtos/{documented.Name}.cs",
                        Message = $"Property type mismatch: expected '{docProperty.Type}', found '{matchingProperty.Type}'"
                    });
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Verifies that all documented configuration matches actual files.
    /// </summary>
    /// <param name="documentedConfig">List of configuration items documented in CONFIGURATION.md</param>
    /// <param name="sourceConfig">Configuration extracted from appsettings.json and option classes</param>
    /// <returns>List of verification errors</returns>
    public List<VerificationError> VerifyConfiguration(
        List<DocumentedConfigItem> documentedConfig,
        ConfigurationInfo sourceConfig)
    {
        var errors = new List<VerificationError>();

        foreach (var documented in documentedConfig)
        {
            // Check if configuration key exists in appsettings.json
            if (!string.IsNullOrEmpty(documented.JsonKey))
            {
                if (!sourceConfig.JsonKeys.Contains(documented.JsonKey))
                {
                    errors.Add(new VerificationError
                    {
                        Type = "ConfigKeyNotFound",
                        Claim = $"Configuration key: {documented.JsonKey}",
                        ExpectedSource = "appsettings.json",
                        Message = "Documented configuration key not found in appsettings.json"
                    });
                }
            }

            // Check if option class property exists
            if (!string.IsNullOrEmpty(documented.OptionClass) && !string.IsNullOrEmpty(documented.PropertyName))
            {
                var optionClass = sourceConfig.OptionClasses.FirstOrDefault(o => o.Name == documented.OptionClass);
                if (optionClass == null)
                {
                    errors.Add(new VerificationError
                    {
                        Type = "OptionClassNotFound",
                        Claim = $"Option class: {documented.OptionClass}",
                        ExpectedSource = $"Options/{documented.OptionClass}.cs",
                        Message = "Documented option class not found in source code"
                    });
                }
                else
                {
                    var property = optionClass.Properties.FirstOrDefault(p => p.Name == documented.PropertyName);
                    if (property == null)
                    {
                        errors.Add(new VerificationError
                        {
                            Type = "OptionPropertyNotFound",
                            Claim = $"Property '{documented.PropertyName}' in {documented.OptionClass}",
                            ExpectedSource = $"Options/{documented.OptionClass}.cs",
                            Message = "Documented option property not found in option class"
                        });
                    }
                }
            }
        }

        return errors;
    }
}

/// <summary>
/// Result of a verification check.
/// </summary>
public class VerificationResult
{
    public string DocumentPath { get; set; } = string.Empty;
    public List<Check> Checks { get; set; } = new();
    public bool Passed { get; set; }
    public List<VerificationError> Errors { get; set; } = new();
}

/// <summary>
/// Individual verification check.
/// </summary>
public class Check
{
    public string Type { get; set; } = string.Empty;
    public string Claim { get; set; } = string.Empty;
    public string SourceReference { get; set; } = string.Empty;
    public bool Passed { get; set; }
}

/// <summary>
/// Verification error with context.
/// </summary>
public class VerificationError
{
    public string Type { get; set; } = string.Empty;
    public string Claim { get; set; } = string.Empty;
    public string ExpectedSource { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents a documented endpoint for verification.
/// </summary>
public class DocumentedEndpoint
{
    public string Path { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public bool IsWebSocket { get; set; }
}

/// <summary>
/// Represents a documented DTO for verification.
/// </summary>
public class DocumentedDto
{
    public string Name { get; set; } = string.Empty;
    public List<DocumentedProperty> Properties { get; set; } = new();
}

/// <summary>
/// Represents a documented property for verification.
/// </summary>
public class DocumentedProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents a documented configuration item for verification.
/// </summary>
public class DocumentedConfigItem
{
    public string JsonKey { get; set; } = string.Empty;
    public string OptionClass { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
}

/// <summary>
/// Configuration information extracted from source.
/// </summary>
public class ConfigurationInfo
{
    public List<string> JsonKeys { get; set; } = new();
    public List<ClassInfo> OptionClasses { get; set; } = new();
}
