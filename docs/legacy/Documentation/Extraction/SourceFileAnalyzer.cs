using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PocketLawyer.Documentation.Extraction;

/// <summary>
/// Analyzes C# source files using Roslyn to extract class information, properties, methods,
/// attributes, using statements, namespaces, and inheritance relationships.
/// </summary>
/// <remarks>
/// Validates Requirements: 1.1, 1.2, 3.1, 3.2, 3.3, 3.5
/// </remarks>
public class SourceFileAnalyzer
{
    /// <summary>
    /// Analyzes a C# source file and extracts all relevant information.
    /// </summary>
    /// <param name="filePath">Path to the C# source file</param>
    /// <returns>Information about the source file</returns>
    public SourceFileInfo AnalyzeFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Source file not found: {filePath}");
        }

        var sourceCode = File.ReadAllText(filePath);
        return AnalyzeSource(sourceCode, filePath);
    }

    /// <summary>
    /// Analyzes C# source code and extracts all relevant information.
    /// </summary>
    /// <param name="sourceCode">The C# source code to analyze</param>
    /// <param name="filePath">Optional file path for reference</param>
    /// <returns>Information about the source file</returns>
    public SourceFileInfo AnalyzeSource(string sourceCode, string? filePath = null)
    {
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();

        var namespaceDeclaration = root.DescendantNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        var namespaceName = namespaceDeclaration?.Name.ToString() ?? string.Empty;

        var usingDirectives = root.DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name?.ToString() ?? string.Empty)
            .Where(u => !string.IsNullOrEmpty(u))
            .ToList();

        var classes = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(c => ExtractClassInfo(c, namespaceName))
            .ToList();

        var records = root.DescendantNodes()
            .OfType<RecordDeclarationSyntax>()
            .Select(r => ExtractRecordInfo(r, namespaceName))
            .ToList();

        var interfaces = root.DescendantNodes()
            .OfType<InterfaceDeclarationSyntax>()
            .Select(i => ExtractInterfaceInfo(i, namespaceName))
            .ToList();

        var enums = root.DescendantNodes()
            .OfType<EnumDeclarationSyntax>()
            .Select(e => ExtractEnumInfo(e, namespaceName))
            .ToList();

        return new SourceFileInfo
        {
            Path = filePath ?? string.Empty,
            Namespace = namespaceName,
            Usings = usingDirectives,
            Classes = classes,
            Records = records,
            Interfaces = interfaces,
            Enums = enums
        };
    }

    private ClassInfo ExtractClassInfo(ClassDeclarationSyntax classDeclaration, string namespaceName)
    {
        var className = classDeclaration.Identifier.Text;
        var baseList = classDeclaration.BaseList?.Types.Select(t => t.ToString()).ToList() ?? new List<string>();
        
        // Separate base class from interfaces
        string? baseClass = null;
        var interfaces = new List<string>();
        
        foreach (var baseType in baseList)
        {
            // Simple heuristic: if it starts with 'I' and next char is uppercase, it's likely an interface
            if (baseType.StartsWith("I") && baseType.Length > 1 && char.IsUpper(baseType[1]))
            {
                interfaces.Add(baseType);
            }
            else
            {
                baseClass = baseType;
            }
        }

        var properties = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(ExtractPropertyInfo)
            .ToList();

        var methods = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Select(ExtractMethodInfo)
            .ToList();

        var attributes = classDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(ExtractAttributeInfo)
            .ToList();

        var modifiers = classDeclaration.Modifiers.Select(m => m.Text).ToList();

        // Check for nested classes
        var nestedClasses = classDeclaration.Members
            .OfType<ClassDeclarationSyntax>()
            .Select(c => ExtractClassInfo(c, namespaceName))
            .ToList();

        return new ClassInfo
        {
            Name = className,
            Namespace = namespaceName,
            BaseClass = baseClass,
            Interfaces = interfaces,
            Properties = properties,
            Methods = methods,
            Attributes = attributes,
            Modifiers = modifiers,
            NestedClasses = nestedClasses,
            IsNested = classDeclaration.Parent is ClassDeclarationSyntax
        };
    }

    private ClassInfo ExtractRecordInfo(RecordDeclarationSyntax recordDeclaration, string namespaceName)
    {
        var recordName = recordDeclaration.Identifier.Text;
        var baseList = recordDeclaration.BaseList?.Types.Select(t => t.ToString()).ToList() ?? new List<string>();
        
        // Separate base class from interfaces
        string? baseClass = null;
        var interfaces = new List<string>();
        
        foreach (var baseType in baseList)
        {
            if (baseType.StartsWith("I") && baseType.Length > 1 && char.IsUpper(baseType[1]))
            {
                interfaces.Add(baseType);
            }
            else
            {
                baseClass = baseType;
            }
        }

        var properties = new List<PropertyInfo>();
        
        // Extract properties from record body
        properties.AddRange(recordDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(ExtractPropertyInfo));

        // Extract primary constructor parameters as properties
        if (recordDeclaration.ParameterList != null)
        {
            properties.AddRange(recordDeclaration.ParameterList.Parameters
                .Select(p => new PropertyInfo
                {
                    Name = p.Identifier.Text,
                    Type = p.Type?.ToString() ?? "unknown",
                    Attributes = p.AttributeLists
                        .SelectMany(al => al.Attributes)
                        .Select(ExtractAttributeInfo)
                        .ToList(),
                    Modifiers = new List<string> { "public" }, // Primary constructor parameters are public
                    HasGetter = true,
                    HasSetter = false // Records with primary constructors have init-only properties
                }));
        }

        var methods = recordDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Select(ExtractMethodInfo)
            .ToList();

        var attributes = recordDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(ExtractAttributeInfo)
            .ToList();

        var modifiers = recordDeclaration.Modifiers.Select(m => m.Text).ToList();

        return new ClassInfo
        {
            Name = recordName,
            Namespace = namespaceName,
            BaseClass = baseClass,
            Interfaces = interfaces,
            Properties = properties,
            Methods = methods,
            Attributes = attributes,
            Modifiers = modifiers,
            IsRecord = true,
            NestedClasses = new List<ClassInfo>(),
            IsNested = recordDeclaration.Parent is ClassDeclarationSyntax or RecordDeclarationSyntax
        };
    }

    private ClassInfo ExtractInterfaceInfo(InterfaceDeclarationSyntax interfaceDeclaration, string namespaceName)
    {
        var interfaceName = interfaceDeclaration.Identifier.Text;
        var baseInterfaces = interfaceDeclaration.BaseList?.Types.Select(t => t.ToString()).ToList() ?? new List<string>();

        var properties = interfaceDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(ExtractPropertyInfo)
            .ToList();

        var methods = interfaceDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Select(ExtractMethodInfo)
            .ToList();

        var attributes = interfaceDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(ExtractAttributeInfo)
            .ToList();

        var modifiers = interfaceDeclaration.Modifiers.Select(m => m.Text).ToList();

        return new ClassInfo
        {
            Name = interfaceName,
            Namespace = namespaceName,
            BaseClass = null,
            Interfaces = baseInterfaces,
            Properties = properties,
            Methods = methods,
            Attributes = attributes,
            Modifiers = modifiers,
            IsInterface = true,
            NestedClasses = new List<ClassInfo>(),
            IsNested = false
        };
    }

    private EnumInfo ExtractEnumInfo(EnumDeclarationSyntax enumDeclaration, string namespaceName)
    {
        var enumName = enumDeclaration.Identifier.Text;
        
        var members = enumDeclaration.Members
            .Select(m => new EnumMemberInfo
            {
                Name = m.Identifier.Text,
                Value = m.EqualsValue?.Value.ToString()
            })
            .ToList();

        var attributes = enumDeclaration.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(ExtractAttributeInfo)
            .ToList();

        return new EnumInfo
        {
            Name = enumName,
            Namespace = namespaceName,
            Members = members,
            Attributes = attributes
        };
    }

    private PropertyInfo ExtractPropertyInfo(PropertyDeclarationSyntax property)
    {
        var attributes = property.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(ExtractAttributeInfo)
            .ToList();

        var modifiers = property.Modifiers.Select(m => m.Text).ToList();

        var hasGetter = property.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? 
                       property.ExpressionBody != null;
        var hasSetter = property.AccessorList?.Accessors.Any(a => 
            a.IsKind(SyntaxKind.SetAccessorDeclaration) || 
            a.IsKind(SyntaxKind.InitAccessorDeclaration)) ?? false;

        return new PropertyInfo
        {
            Name = property.Identifier.Text,
            Type = property.Type.ToString(),
            Attributes = attributes,
            Modifiers = modifiers,
            HasGetter = hasGetter,
            HasSetter = hasSetter
        };
    }

    private MethodInfo ExtractMethodInfo(MethodDeclarationSyntax method)
    {
        var attributes = method.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(ExtractAttributeInfo)
            .ToList();

        var modifiers = method.Modifiers.Select(m => m.Text).ToList();

        var parameters = method.ParameterList.Parameters
            .Select(p => new ParameterInfo
            {
                Name = p.Identifier.Text,
                Type = p.Type?.ToString() ?? "unknown",
                DefaultValue = p.Default?.Value.ToString()
            })
            .ToList();

        return new MethodInfo
        {
            Name = method.Identifier.Text,
            ReturnType = method.ReturnType.ToString(),
            Parameters = parameters,
            Attributes = attributes,
            Modifiers = modifiers
        };
    }

    private AttributeInfo ExtractAttributeInfo(AttributeSyntax attribute)
    {
        var arguments = attribute.ArgumentList?.Arguments
            .Select(arg => arg.ToString())
            .ToList() ?? new List<string>();

        return new AttributeInfo
        {
            Name = attribute.Name.ToString(),
            Arguments = arguments
        };
    }
}

/// <summary>
/// Information about a C# source file.
/// </summary>
public class SourceFileInfo
{
    public string Path { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public List<string> Usings { get; set; } = new();
    public List<ClassInfo> Classes { get; set; } = new();
    public List<ClassInfo> Records { get; set; } = new();
    public List<ClassInfo> Interfaces { get; set; } = new();
    public List<EnumInfo> Enums { get; set; } = new();
}

/// <summary>
/// Information about a class, record, or interface.
/// </summary>
public class ClassInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string? BaseClass { get; set; }
    public List<string> Interfaces { get; set; } = new();
    public List<PropertyInfo> Properties { get; set; } = new();
    public List<MethodInfo> Methods { get; set; } = new();
    public List<AttributeInfo> Attributes { get; set; } = new();
    public List<string> Modifiers { get; set; } = new();
    public List<ClassInfo> NestedClasses { get; set; } = new();
    public bool IsNested { get; set; }
    public bool IsRecord { get; set; }
    public bool IsInterface { get; set; }
}

/// <summary>
/// Information about a property.
/// </summary>
public class PropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<AttributeInfo> Attributes { get; set; } = new();
    public List<string> Modifiers { get; set; } = new();
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
}

/// <summary>
/// Information about a method.
/// </summary>
public class MethodInfo
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public List<AttributeInfo> Attributes { get; set; } = new();
    public List<string> Modifiers { get; set; } = new();
}

/// <summary>
/// Information about a method parameter.
/// </summary>
public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Information about an attribute.
/// </summary>
public class AttributeInfo
{
    public string Name { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = new();
}

/// <summary>
/// Information about an enumeration.
/// </summary>
public class EnumInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public List<EnumMemberInfo> Members { get; set; } = new();
    public List<AttributeInfo> Attributes { get; set; } = new();
}

/// <summary>
/// Information about an enum member.
/// </summary>
public class EnumMemberInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
}
