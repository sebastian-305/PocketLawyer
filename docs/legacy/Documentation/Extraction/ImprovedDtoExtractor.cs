using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

namespace PocketLawyer.Documentation.Extraction;

/// <summary>
/// Improved DTO extractor that uses Roslyn for comprehensive source code analysis.
/// Extracts all DTOs with complete metadata including inherited properties, XML documentation, and enum values.
/// </summary>
/// <remarks>
/// Validates Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 7.1, 7.2
/// </remarks>
public class ImprovedDtoExtractor
{
    private readonly SourceFileAnalyzer _sourceAnalyzer;

    public ImprovedDtoExtractor()
    {
        _sourceAnalyzer = new SourceFileAnalyzer();
    }

    /// <summary>
    /// Extracts all DTOs from the specified project path.
    /// </summary>
    /// <param name="projectPath">Root path of the project to analyze</param>
    /// <returns>List of all extracted DTOs with complete metadata</returns>
    public List<DtoInfo> ExtractAllDtos(string projectPath)
    {
        if (!Directory.Exists(projectPath))
        {
            throw new DirectoryNotFoundException($"Project path not found: {projectPath}");
        }

        var dtos = new List<DtoInfo>();
        
        // Find all .cs files recursively
        var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\")) // Exclude build artifacts
            .ToList();

        // Create a compilation for semantic analysis
        var compilation = CreateCompilation(csFiles);

        foreach (var filePath in csFiles)
        {
            try
            {
                var sourceCode = File.ReadAllText(filePath);
                var tree = compilation.SyntaxTrees.FirstOrDefault(t => t.FilePath == filePath);
                
                if (tree == null)
                {
                    // If tree not found in compilation, parse it again
                    tree = CSharpSyntaxTree.ParseText(sourceCode, path: filePath);
                }
                
                var semanticModel = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();

                // Extract DTOs from classes
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classDecl in classDeclarations)
                {
                    if (IsDtoClass(classDecl, semanticModel))
                    {
                        var dtoInfo = ExtractDtoFromClass(classDecl, semanticModel, filePath);
                        if (dtoInfo != null)
                        {
                            dtos.Add(dtoInfo);
                        }
                    }
                }

                // Extract DTOs from records
                var recordDeclarations = root.DescendantNodes().OfType<RecordDeclarationSyntax>();
                foreach (var recordDecl in recordDeclarations)
                {
                    if (IsDtoRecord(recordDecl, semanticModel))
                    {
                        var dtoInfo = ExtractDtoFromRecord(recordDecl, semanticModel, filePath);
                        if (dtoInfo != null)
                        {
                            dtos.Add(dtoInfo);
                        }
                    }
                }

                // Extract Enums
                var enumDeclarations = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
                foreach (var enumDecl in enumDeclarations)
                {
                    if (IsDtoEnum(enumDecl, semanticModel))
                    {
                        var enumInfo = ExtractEnum(enumDecl, semanticModel, filePath);
                        if (enumInfo != null)
                        {
                            dtos.Add(enumInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log warning but continue processing other files
                Console.WriteLine($"Warning: Failed to analyze file {filePath}: {ex.Message}");
            }
        }

        return dtos;
    }

    /// <summary>
    /// Creates a Roslyn compilation for semantic analysis.
    /// </summary>
    private CSharpCompilation CreateCompilation(List<string> csFiles)
    {
        var syntaxTrees = csFiles.Select(file =>
        {
            try
            {
                var sourceCode = File.ReadAllText(file);
                return CSharpSyntaxTree.ParseText(sourceCode, path: file);
            }
            catch
            {
                return null;
            }
        })
        .Where(tree => tree != null)
        .Cast<SyntaxTree>()
        .ToList();

        // Add references to common assemblies needed for DTO analysis
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute).Assembly.Location),
        };

        // Add reference to System.Runtime
        var systemRuntimePath = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "System.Runtime.dll");
        if (File.Exists(systemRuntimePath))
        {
            references.Add(MetadataReference.CreateFromFile(systemRuntimePath));
        }

        return CSharpCompilation.Create(
            "DtoAnalysis",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    /// <summary>
    /// Determines if a class is a DTO based on attributes or namespace conventions.
    /// </summary>
    private bool IsDtoClass(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (symbol == null) return false;

        // Check for [Dto] attribute
        if (symbol.GetAttributes().Any(attr => attr.AttributeClass?.Name.Contains("Dto") == true))
        {
            return true;
        }

        // Check namespace convention (contains "Dtos" or "Models")
        var namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (namespaceName.Contains("Dtos") || namespaceName.Contains("Models") || namespaceName.Contains("DTOs"))
        {
            return true;
        }

        // Check class name convention (ends with "Dto" or "Model")
        if (symbol.Name.EndsWith("Dto") || symbol.Name.EndsWith("Model") || symbol.Name.EndsWith("DTO"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if a record is a DTO based on attributes or namespace conventions.
    /// </summary>
    private bool IsDtoRecord(RecordDeclarationSyntax recordDecl, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(recordDecl);
        if (symbol == null) return false;

        // Check for [Dto] attribute
        if (symbol.GetAttributes().Any(attr => attr.AttributeClass?.Name.Contains("Dto") == true))
        {
            return true;
        }

        // Check namespace convention
        var namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (namespaceName.Contains("Dtos") || namespaceName.Contains("Models") || namespaceName.Contains("DTOs"))
        {
            return true;
        }

        // Check record name convention
        if (symbol.Name.EndsWith("Dto") || symbol.Name.EndsWith("Model") || symbol.Name.EndsWith("DTO"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if an enum should be included in DTO documentation.
    /// </summary>
    private bool IsDtoEnum(EnumDeclarationSyntax enumDecl, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(enumDecl);
        if (symbol == null) return false;

        // Include enums from Dtos, Models, or Enums namespaces
        var namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        return namespaceName.Contains("Dtos") || 
               namespaceName.Contains("Models") || 
               namespaceName.Contains("Enums") ||
               namespaceName.Contains("DTOs");
    }

    /// <summary>
    /// Extracts DTO information from a class declaration.
    /// </summary>
    private DtoInfo? ExtractDtoFromClass(ClassDeclarationSyntax classDecl, SemanticModel semanticModel, string filePath)
    {
        var symbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (symbol == null) return null;

        var dtoInfo = new DtoInfo
        {
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Description = ExtractXmlDocumentation(symbol),
            Properties = ExtractProperties(symbol),
            IsEnum = false,
            EnumValues = new List<EnumValue>(),
            FilePath = filePath
        };

        return dtoInfo;
    }

    /// <summary>
    /// Extracts DTO information from a record declaration.
    /// </summary>
    private DtoInfo? ExtractDtoFromRecord(RecordDeclarationSyntax recordDecl, SemanticModel semanticModel, string filePath)
    {
        var symbol = semanticModel.GetDeclaredSymbol(recordDecl);
        if (symbol == null) return null;

        var dtoInfo = new DtoInfo
        {
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Description = ExtractXmlDocumentation(symbol),
            Properties = ExtractProperties(symbol),
            IsEnum = false,
            EnumValues = new List<EnumValue>(),
            FilePath = filePath,
            IsRecord = true
        };

        return dtoInfo;
    }

    /// <summary>
    /// Extracts enum information.
    /// </summary>
    private DtoInfo? ExtractEnum(EnumDeclarationSyntax enumDecl, SemanticModel semanticModel, string filePath)
    {
        var symbol = semanticModel.GetDeclaredSymbol(enumDecl);
        if (symbol == null) return null;

        var enumValues = new List<EnumValue>();
        
        foreach (var member in symbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.HasConstantValue)
            {
                var enumValue = new EnumValue
                {
                    Name = member.Name,
                    Value = Convert.ToInt32(member.ConstantValue),
                    Description = ExtractXmlDocumentation(member)
                };
                enumValues.Add(enumValue);
            }
        }

        var dtoInfo = new DtoInfo
        {
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Description = ExtractXmlDocumentation(symbol),
            Properties = new List<DtoPropertyInfo>(),
            IsEnum = true,
            EnumValues = enumValues,
            FilePath = filePath
        };

        return dtoInfo;
    }

    /// <summary>
    /// Extracts all properties from a type symbol, including inherited properties.
    /// </summary>
    private List<DtoPropertyInfo> ExtractProperties(INamedTypeSymbol typeSymbol)
    {
        var properties = new List<DtoPropertyInfo>();
        var processedPropertyNames = new HashSet<string>();

        // Extract properties from the current type and all base types
        var currentType = typeSymbol;
        while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
        {
            var isInherited = currentType != typeSymbol;

            foreach (var member in currentType.GetMembers().OfType<IPropertySymbol>())
            {
                // Skip if we've already processed this property (overridden)
                if (processedPropertyNames.Contains(member.Name))
                {
                    continue;
                }

                // Only include public properties
                if (member.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var propertyInfo = new DtoPropertyInfo
                {
                    Name = member.Name,
                    Type = member.Type.ToDisplayString(),
                    Description = ExtractXmlDocumentation(member),
                    IsRequired = IsPropertyRequired(member),
                    IsInherited = isInherited
                };

                properties.Add(propertyInfo);
                processedPropertyNames.Add(member.Name);
            }

            currentType = currentType.BaseType;
        }

        // Sort properties: non-inherited first, then inherited, both alphabetically
        return properties
            .OrderBy(p => p.IsInherited)
            .ThenBy(p => p.Name)
            .ToList();
    }

    /// <summary>
    /// Determines if a property is required based on nullable context and attributes.
    /// </summary>
    private bool IsPropertyRequired(IPropertySymbol property)
    {
        // Check for [Required] attribute
        if (property.GetAttributes().Any(attr => attr.AttributeClass?.Name == "RequiredAttribute" || attr.AttributeClass?.Name == "Required"))
        {
            return true;
        }

        // Check nullable reference type context
        if (property.Type.NullableAnnotation == NullableAnnotation.NotAnnotated && 
            !property.Type.IsValueType)
        {
            return true;
        }

        // Non-nullable value types are required
        if (property.Type.IsValueType && property.Type.NullableAnnotation != NullableAnnotation.Annotated)
        {
            var namedType = property.Type as INamedTypeSymbol;
            if (namedType == null || namedType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts XML documentation from a symbol.
    /// </summary>
    private string ExtractXmlDocumentation(ISymbol symbol)
    {
        var xmlDoc = symbol.GetDocumentationCommentXml();
        
        if (string.IsNullOrWhiteSpace(xmlDoc))
        {
            return string.Empty;
        }

        try
        {
            var doc = XDocument.Parse(xmlDoc);
            var summary = doc.Descendants("summary").FirstOrDefault();
            
            if (summary != null)
            {
                var summaryText = summary.Value
                    .Trim()
                    .Replace("\n", " ")
                    .Replace("\r", "");
                
                // Remove extra whitespace
                return System.Text.RegularExpressions.Regex.Replace(summaryText, @"\s+", " ");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to parse XML documentation for {symbol.Name}: {ex.Message}");
        }

        return string.Empty;
    }
}

/// <summary>
/// Information about a DTO or Enum.
/// </summary>
public class DtoInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DtoPropertyInfo> Properties { get; set; } = new();
    public bool IsEnum { get; set; }
    public List<EnumValue> EnumValues { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public bool IsRecord { get; set; }
}

/// <summary>
/// Information about a DTO property.
/// </summary>
public class DtoPropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsInherited { get; set; }
}

/// <summary>
/// Information about an enum value.
/// </summary>
public class EnumValue
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
}
