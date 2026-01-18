using PocketLawyer.Documentation.Extraction;
using System.Text;

namespace PocketLawyer.Documentation.Generation;

/// <summary>
/// Generates Mermaid diagrams from extracted code structures.
/// Creates class diagrams, dependency graphs, and architecture diagrams based on actual code.
/// </summary>
/// <remarks>
/// Validates Requirements: 1.3, 3.4, 7.1, 7.4, 7.5
/// </remarks>
public class MermaidDiagramGenerator
{
    /// <summary>
    /// Generates a Mermaid class diagram from DTO structures.
    /// Requirement 3.4: WHEN creating the DTO Class Diagram, THE System SHALL include only classes that actually exist in the codebase
    /// Requirement 7.1: WHEN creating class diagrams, THE System SHALL include only classes that exist in the actual codebase
    /// Requirement 7.5: THE System SHALL verify that every class name in a diagram matches an actual class in the codebase
    /// </summary>
    /// <param name="classes">List of class information extracted from source files</param>
    /// <returns>Mermaid class diagram syntax</returns>
    public string GenerateClassDiagram(IEnumerable<ClassInfo> classes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("classDiagram");

        var classList = classes.ToList();

        foreach (var classInfo in classList)
        {
            // Add class definition
            if (classInfo.IsInterface)
            {
                sb.AppendLine($"    class {classInfo.Name} {{");
                sb.AppendLine("        <<interface>>");
            }
            else if (classInfo.IsRecord)
            {
                sb.AppendLine($"    class {classInfo.Name} {{");
                sb.AppendLine("        <<record>>");
            }
            else
            {
                sb.AppendLine($"    class {classInfo.Name} {{");
            }

            // Add properties
            foreach (var property in classInfo.Properties)
            {
                var accessModifier = GetAccessModifier(property.Modifiers);
                sb.AppendLine($"        {accessModifier}{property.Type} {property.Name}");
            }

            // Add methods (only public methods for clarity)
            foreach (var method in classInfo.Methods.Where(m => m.Modifiers.Contains("public")))
            {
                var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                sb.AppendLine($"        +{method.Name}({parameters}) {method.ReturnType}");
            }

            sb.AppendLine("    }");
        }

        // Add inheritance relationships
        foreach (var classInfo in classList)
        {
            if (!string.IsNullOrEmpty(classInfo.BaseClass))
            {
                // Check if base class is in the diagram
                if (classList.Any(c => c.Name == classInfo.BaseClass))
                {
                    sb.AppendLine($"    {classInfo.BaseClass} <|-- {classInfo.Name}");
                }
            }

            // Add interface implementations
            foreach (var interfaceName in classInfo.Interfaces)
            {
                // Check if interface is in the diagram
                if (classList.Any(c => c.Name == interfaceName))
                {
                    sb.AppendLine($"    {interfaceName} <|.. {classInfo.Name}");
                }
            }
        }

        sb.AppendLine("```");
        return sb.ToString();
    }

    /// <summary>
    /// Generates a Mermaid dependency graph from using statements.
    /// Requirement 1.2: WHEN documenting dependencies, THE System SHALL analyze using statements in source files to determine actual dependency flow
    /// Requirement 7.2: WHEN creating dependency diagrams, THE System SHALL show only dependencies that exist in actual using statements
    /// </summary>
    /// <param name="sourceFiles">List of source file information with using statements</param>
    /// <returns>Mermaid dependency graph syntax</returns>
    public string GenerateDependencyGraph(IEnumerable<SourceFileInfo> sourceFiles)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TD");

        // Group files by namespace to create layer-level dependencies
        var namespaceGroups = sourceFiles
            .Where(f => !string.IsNullOrEmpty(f.Namespace))
            .GroupBy(f => GetLayerFromNamespace(f.Namespace))
            .ToDictionary(g => g.Key, g => g.ToList());

        // Define layers
        var layers = new[] { "Web", "Application", "Domain", "Infrastructure" };
        var layerDependencies = new HashSet<(string From, string To)>();

        // Analyze dependencies between layers
        foreach (var sourceFile in sourceFiles)
        {
            var sourceLayer = GetLayerFromNamespace(sourceFile.Namespace);
            if (string.IsNullOrEmpty(sourceLayer)) continue;

            foreach (var usingNamespace in sourceFile.Usings)
            {
                var targetLayer = GetLayerFromNamespace(usingNamespace);
                if (!string.IsNullOrEmpty(targetLayer) && sourceLayer != targetLayer)
                {
                    layerDependencies.Add((sourceLayer, targetLayer));
                }
            }
        }

        // Add layer nodes
        foreach (var layer in layers)
        {
            if (namespaceGroups.ContainsKey(layer))
            {
                sb.AppendLine($"    {layer}[{layer} Layer]");
            }
        }

        // Add dependency edges
        foreach (var (from, to) in layerDependencies.OrderBy(d => d.From).ThenBy(d => d.To))
        {
            sb.AppendLine($"    {from} --> {to}");
        }

        sb.AppendLine("```");
        return sb.ToString();
    }

    /// <summary>
    /// Generates a Mermaid architecture diagram from folder structure.
    /// Requirement 1.3: WHEN creating the Layered Component Diagram, THE System SHALL base it on actual namespaces and classes found in the codebase
    /// Requirement 7.4: WHEN creating architecture diagrams, THE System SHALL reflect the actual folder and namespace structure
    /// </summary>
    /// <param name="sourceFiles">List of source file information</param>
    /// <returns>Mermaid architecture diagram syntax</returns>
    public string GenerateArchitectureDiagram(IEnumerable<SourceFileInfo> sourceFiles)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TB");

        // Group by layer
        var layerGroups = sourceFiles
            .Where(f => !string.IsNullOrEmpty(f.Namespace))
            .GroupBy(f => GetLayerFromNamespace(f.Namespace))
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .ToDictionary(g => g.Key, g => g.ToList());

        // Define subgraphs for each layer
        var layerOrder = new[] { "Web", "Application", "Domain", "Infrastructure" };
        
        foreach (var layer in layerOrder)
        {
            if (!layerGroups.ContainsKey(layer)) continue;

            sb.AppendLine($"    subgraph {layer}[{layer} Layer]");

            // Group by component (second part of namespace)
            var components = layerGroups[layer]
                .GroupBy(f => GetComponentFromNamespace(f.Namespace))
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToList();

            foreach (var component in components)
            {
                var componentName = component.Key;
                var classes = component
                    .SelectMany(f => f.Classes.Concat(f.Records).Concat(f.Interfaces))
                    .Select(c => c.Name)
                    .Distinct()
                    .Take(5) // Limit to avoid cluttering
                    .ToList();

                if (classes.Any())
                {
                    var classesText = string.Join(", ", classes);
                    if (classes.Count > 3)
                    {
                        classesText = string.Join(", ", classes.Take(3)) + "...";
                    }
                    sb.AppendLine($"        {layer}_{componentName}[{componentName}<br/>{classesText}]");
                }
            }

            sb.AppendLine("    end");
        }

        // Add layer dependencies
        if (layerGroups.ContainsKey("Web") && layerGroups.ContainsKey("Application"))
        {
            sb.AppendLine("    Web --> Application");
        }
        if (layerGroups.ContainsKey("Application") && layerGroups.ContainsKey("Domain"))
        {
            sb.AppendLine("    Application --> Domain");
        }
        if (layerGroups.ContainsKey("Application") && layerGroups.ContainsKey("Infrastructure"))
        {
            sb.AppendLine("    Application --> Infrastructure");
        }
        if (layerGroups.ContainsKey("Infrastructure") && layerGroups.ContainsKey("Domain"))
        {
            sb.AppendLine("    Infrastructure --> Domain");
        }

        sb.AppendLine("```");
        return sb.ToString();
    }

    /// <summary>
    /// Generates a Mermaid sequence diagram for API endpoint flow.
    /// Requirement 7.3: WHEN creating sequence diagrams, THE System SHALL base flows on actual method calls in the code
    /// </summary>
    /// <param name="endpoint">Endpoint information</param>
    /// <param name="handlerClass">Handler class information</param>
    /// <returns>Mermaid sequence diagram syntax</returns>
    public string GenerateSequenceDiagram(EndpointInfo endpoint, ClassInfo? handlerClass = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("sequenceDiagram");
        sb.AppendLine("    participant Client");
        sb.AppendLine($"    participant {endpoint.ControllerName}");

        if (handlerClass != null)
        {
            sb.AppendLine($"    participant {handlerClass.Name}");
        }

        // Client to Controller
        var parametersText = endpoint.Parameters.Any() 
            ? $"({string.Join(", ", endpoint.Parameters.Select(p => p.Name))})" 
            : "()";
        sb.AppendLine($"    Client->>+{endpoint.ControllerName}: {endpoint.HttpMethod} {endpoint.Path}");

        // Controller to Handler
        if (handlerClass != null)
        {
            sb.AppendLine($"    {endpoint.ControllerName}->>+{handlerClass.Name}: {endpoint.ActionName}{parametersText}");
            sb.AppendLine($"    {handlerClass.Name}-->>-{endpoint.ControllerName}: {endpoint.ReturnType}");
        }

        // Controller to Client
        sb.AppendLine($"    {endpoint.ControllerName}-->>-Client: {endpoint.ReturnType}");

        sb.AppendLine("```");
        return sb.ToString();
    }

    /// <summary>
    /// Generates a Mermaid ER diagram for DTO relationships.
    /// </summary>
    /// <param name="dtos">List of DTO class information</param>
    /// <returns>Mermaid ER diagram syntax</returns>
    public string GenerateEntityRelationshipDiagram(IEnumerable<ClassInfo> dtos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("erDiagram");

        var dtoList = dtos.ToList();

        foreach (var dto in dtoList)
        {
            sb.AppendLine($"    {dto.Name} {{");

            foreach (var property in dto.Properties)
            {
                var propertyType = SimplifyType(property.Type);
                sb.AppendLine($"        {propertyType} {property.Name}");
            }

            sb.AppendLine("    }");
        }

        // Add relationships based on property types
        foreach (var dto in dtoList)
        {
            foreach (var property in dto.Properties)
            {
                var propertyType = SimplifyType(property.Type);
                
                // Check if property type is another DTO in the list
                var relatedDto = dtoList.FirstOrDefault(d => propertyType.Contains(d.Name));
                if (relatedDto != null && relatedDto.Name != dto.Name)
                {
                    // Determine relationship type
                    var relationship = property.Type.Contains("List") || property.Type.Contains("[]") 
                        ? "||--o{" 
                        : "||--||";
                    
                    sb.AppendLine($"    {dto.Name} {relationship} {relatedDto.Name} : has");
                }
            }
        }

        sb.AppendLine("```");
        return sb.ToString();
    }

    /// <summary>
    /// Generates a Mermaid flowchart for a process or workflow.
    /// </summary>
    /// <param name="title">Flowchart title</param>
    /// <param name="steps">List of steps in the workflow</param>
    /// <returns>Mermaid flowchart syntax</returns>
    public string GenerateFlowchart(string title, IEnumerable<(string Id, string Label, string Type)> steps)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("flowchart TD");

        var stepList = steps.ToList();
        
        foreach (var (id, label, type) in stepList)
        {
            var nodeShape = type.ToLower() switch
            {
                "decision" => $"{id}{{{label}}}",
                "process" => $"{id}[{label}]",
                "start" => $"{id}([{label}])",
                "end" => $"{id}([{label}])",
                _ => $"{id}[{label}]"
            };
            sb.AppendLine($"    {nodeShape}");
        }

        // Add connections (sequential by default)
        for (int i = 0; i < stepList.Count - 1; i++)
        {
            sb.AppendLine($"    {stepList[i].Id} --> {stepList[i + 1].Id}");
        }

        sb.AppendLine("```");
        return sb.ToString();
    }

    #region Helper Methods

    private string GetAccessModifier(List<string> modifiers)
    {
        if (modifiers.Contains("public")) return "+";
        if (modifiers.Contains("private")) return "-";
        if (modifiers.Contains("protected")) return "#";
        if (modifiers.Contains("internal")) return "~";
        return "+"; // Default to public
    }

    private string GetLayerFromNamespace(string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName)) return string.Empty;

        var parts = namespaceName.Split('.');
        
        // Look for layer keywords
        foreach (var part in parts)
        {
            if (part.Equals("Web", StringComparison.OrdinalIgnoreCase)) return "Web";
            if (part.Equals("Application", StringComparison.OrdinalIgnoreCase)) return "Application";
            if (part.Equals("Domain", StringComparison.OrdinalIgnoreCase)) return "Domain";
            if (part.Equals("Infrastructure", StringComparison.OrdinalIgnoreCase)) return "Infrastructure";
        }

        return string.Empty;
    }

    private string GetComponentFromNamespace(string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName)) return string.Empty;

        var parts = namespaceName.Split('.');
        
        // Find the part after the layer
        var layerIndex = -1;
        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Equals("Web", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Application", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Domain", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Infrastructure", StringComparison.OrdinalIgnoreCase))
            {
                layerIndex = i;
                break;
            }
        }

        if (layerIndex >= 0 && layerIndex + 1 < parts.Length)
        {
            return parts[layerIndex + 1];
        }

        return string.Empty;
    }

    private string SimplifyType(string type)
    {
        // Remove generic type parameters for cleaner display
        var genericIndex = type.IndexOf('<');
        if (genericIndex > 0)
        {
            var baseType = type.Substring(0, genericIndex);
            var genericParam = type.Substring(genericIndex + 1, type.LastIndexOf('>') - genericIndex - 1);
            
            // For collections, show the element type
            if (baseType.Contains("List") || baseType.Contains("IEnumerable") || baseType.Contains("Collection"))
            {
                return $"List_{genericParam}";
            }
            
            return baseType;
        }

        // Remove namespace prefixes
        var lastDot = type.LastIndexOf('.');
        if (lastDot > 0)
        {
            return type.Substring(lastDot + 1);
        }

        return type;
    }

    #endregion
}
