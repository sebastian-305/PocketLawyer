using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders;

/// <summary>
/// Result of comparing two JSON schemas.
/// </summary>
public record SchemaComparisonResult
{
    /// <summary>
    /// True if the schemas are structurally and semantically identical.
    /// </summary>
    public bool IsIdentical { get; init; }
    
    /// <summary>
    /// List of differences found between the schemas.
    /// </summary>
    public List<string> Differences { get; init; } = new();
    
    /// <summary>
    /// The manual (original) schema that was compared.
    /// </summary>
    public JsonObject ManualSchema { get; init; } = new();
    
    /// <summary>
    /// The generated schema that was compared.
    /// </summary>
    public JsonObject GeneratedSchema { get; init; } = new();
}

/// <summary>
/// Compares JSON schemas to verify they are structurally and semantically identical.
/// Used to validate that generated schemas match manual schemas during migration.
/// </summary>
public class SchemaComparator
{
    /// <summary>
    /// Compares two JSON schemas and returns detailed comparison results.
    /// </summary>
    /// <param name="manual">The manual (original) schema</param>
    /// <param name="generated">The generated schema</param>
    /// <returns>Comparison result with list of differences</returns>
    public SchemaComparisonResult Compare(JsonObject manual, JsonObject generated)
    {
        var differences = new List<string>();
        
        // Compare top-level type
        CompareField(manual, generated, "type", differences, "");
        
        // Compare additionalProperties
        CompareField(manual, generated, "additionalProperties", differences, "");
        
        // Compare x-sectionTitle
        CompareField(manual, generated, "x-sectionTitle", differences, "");
        
        // Compare properties
        if (manual.ContainsKey("properties") && generated.ContainsKey("properties"))
        {
            var manualProps = manual["properties"]?.AsObject();
            var generatedProps = generated["properties"]?.AsObject();
            
            if (manualProps != null && generatedProps != null)
            {
                CompareProperties(manualProps, generatedProps, differences);
            }
        }
        else if (manual.ContainsKey("properties") != generated.ContainsKey("properties"))
        {
            differences.Add("One schema has 'properties' field while the other doesn't");
        }
        
        // Compare required arrays (order-independent)
        if (manual.ContainsKey("required") && generated.ContainsKey("required"))
        {
            var manualRequired = manual["required"]?.AsArray();
            var generatedRequired = generated["required"]?.AsArray();
            
            if (manualRequired != null && generatedRequired != null)
            {
                CompareRequiredArrays(manualRequired, generatedRequired, differences);
            }
        }
        else if (manual.ContainsKey("required") != generated.ContainsKey("required"))
        {
            differences.Add("One schema has 'required' field while the other doesn't");
        }
        
        return new SchemaComparisonResult
        {
            IsIdentical = differences.Count == 0,
            Differences = differences,
            ManualSchema = manual,
            GeneratedSchema = generated
        };
    }
    
    /// <summary>
    /// Recursively compares properties objects.
    /// </summary>
    private void CompareProperties(JsonObject manual, JsonObject generated, List<string> differences)
    {
        // Check for properties in manual that are missing in generated
        foreach (var prop in manual)
        {
            if (!generated.ContainsKey(prop.Key))
            {
                differences.Add($"Property '{prop.Key}' exists in manual schema but not in generated schema");
                continue;
            }
            
            var manualProp = prop.Value?.AsObject();
            var generatedProp = generated[prop.Key]?.AsObject();
            
            if (manualProp != null && generatedProp != null)
            {
                ComparePropertySchema(prop.Key, manualProp, generatedProp, differences);
            }
        }
        
        // Check for properties in generated that are missing in manual
        foreach (var prop in generated)
        {
            if (!manual.ContainsKey(prop.Key))
            {
                differences.Add($"Property '{prop.Key}' exists in generated schema but not in manual schema");
            }
        }
    }
    
    /// <summary>
    /// Compares a single property schema recursively.
    /// </summary>
    private void ComparePropertySchema(string propertyName, JsonObject manual, JsonObject generated, List<string> differences)
    {
        string path = $"properties.{propertyName}";
        
        // Compare type (can be string or array)
        CompareTypeField(manual, generated, differences, path);
        
        // Compare standard fields
        CompareField(manual, generated, "description", differences, path);
        CompareField(manual, generated, "format", differences, path);
        CompareField(manual, generated, "maxLength", differences, path);
        CompareField(manual, generated, "minLength", differences, path);
        CompareField(manual, generated, "minimum", differences, path);
        CompareField(manual, generated, "maximum", differences, path);
        
        // Compare x-displayName
        CompareField(manual, generated, "x-displayName", differences, path);
        
        // Compare x-styling object
        if (manual.ContainsKey("x-styling") && generated.ContainsKey("x-styling"))
        {
            var manualStyling = manual["x-styling"]?.AsObject();
            var generatedStyling = generated["x-styling"]?.AsObject();
            
            if (manualStyling != null && generatedStyling != null)
            {
                CompareField(manualStyling, generatedStyling, "icon", differences, $"{path}.x-styling");
                CompareField(manualStyling, generatedStyling, "displayClass", differences, $"{path}.x-styling");
                CompareField(manualStyling, generatedStyling, "importance", differences, $"{path}.x-styling");
                CompareField(manualStyling, generatedStyling, "format", differences, $"{path}.x-styling");
            }
        }
        else if (manual.ContainsKey("x-styling") != generated.ContainsKey("x-styling"))
        {
            differences.Add($"{path}: One schema has 'x-styling' while the other doesn't");
        }
        
        // Compare enum arrays (order-independent)
        if (manual.ContainsKey("enum") && generated.ContainsKey("enum"))
        {
            var manualEnum = manual["enum"]?.AsArray();
            var generatedEnum = generated["enum"]?.AsArray();
            
            if (manualEnum != null && generatedEnum != null)
            {
                CompareEnumArrays(manualEnum, generatedEnum, differences, path);
            }
        }
        else if (manual.ContainsKey("enum") != generated.ContainsKey("enum"))
        {
            differences.Add($"{path}: One schema has 'enum' while the other doesn't");
        }
        
        // Compare examples arrays (order-independent)
        if (manual.ContainsKey("examples") && generated.ContainsKey("examples"))
        {
            var manualExamples = manual["examples"]?.AsArray();
            var generatedExamples = generated["examples"]?.AsArray();
            
            if (manualExamples != null && generatedExamples != null)
            {
                CompareArrays(manualExamples, generatedExamples, differences, $"{path}.examples");
            }
        }
        else if (manual.ContainsKey("examples") != generated.ContainsKey("examples"))
        {
            differences.Add($"{path}: One schema has 'examples' while the other doesn't");
        }
        
        // Compare items (for array types)
        if (manual.ContainsKey("items") && generated.ContainsKey("items"))
        {
            var manualItems = manual["items"]?.AsObject();
            var generatedItems = generated["items"]?.AsObject();
            
            if (manualItems != null && generatedItems != null)
            {
                ComparePropertySchema($"{propertyName}.items", manualItems, generatedItems, differences);
            }
        }
        else if (manual.ContainsKey("items") != generated.ContainsKey("items"))
        {
            differences.Add($"{path}: One schema has 'items' while the other doesn't");
        }
        
        // Compare nested properties (for object types)
        if (manual.ContainsKey("properties") && generated.ContainsKey("properties"))
        {
            var manualProps = manual["properties"]?.AsObject();
            var generatedProps = generated["properties"]?.AsObject();
            
            if (manualProps != null && generatedProps != null)
            {
                CompareProperties(manualProps, generatedProps, differences);
            }
        }
        else if (manual.ContainsKey("properties") != generated.ContainsKey("properties"))
        {
            differences.Add($"{path}: One schema has nested 'properties' while the other doesn't");
        }
    }
    
    /// <summary>
    /// Compares a field value between two JSON objects.
    /// </summary>
    private void CompareField(JsonObject manual, JsonObject generated, string fieldName, List<string> differences, string path)
    {
        var manualValue = manual.ContainsKey(fieldName) ? manual[fieldName] : null;
        var generatedValue = generated.ContainsKey(fieldName) ? generated[fieldName] : null;
        
        string fullPath = string.IsNullOrEmpty(path) ? fieldName : $"{path}.{fieldName}";
        
        if (manualValue == null && generatedValue == null)
            return;
        
        if (manualValue == null)
        {
            differences.Add($"{fullPath}: Manual schema has no value, generated has '{generatedValue}'");
            return;
        }
        
        if (generatedValue == null)
        {
            differences.Add($"{fullPath}: Manual schema has '{manualValue}', generated has no value");
            return;
        }
        
        // Compare values
        string manualStr = manualValue.ToJsonString();
        string generatedStr = generatedValue.ToJsonString();
        
        if (manualStr != generatedStr)
        {
            differences.Add($"{fullPath}: Manual has '{manualStr}', generated has '{generatedStr}'");
        }
    }
    
    /// <summary>
    /// Compares type field which can be either a string or an array.
    /// </summary>
    private void CompareTypeField(JsonObject manual, JsonObject generated, List<string> differences, string path)
    {
        if (!manual.ContainsKey("type") && !generated.ContainsKey("type"))
            return;
        
        if (!manual.ContainsKey("type"))
        {
            differences.Add($"{path}.type: Manual schema has no type, generated has type");
            return;
        }
        
        if (!generated.ContainsKey("type"))
        {
            differences.Add($"{path}.type: Manual schema has type, generated has no type");
            return;
        }
        
        var manualType = manual["type"];
        var generatedType = generated["type"];
        
        // Both are strings
        if (manualType is JsonValue && generatedType is JsonValue)
        {
            if (manualType.ToJsonString() != generatedType.ToJsonString())
            {
                differences.Add($"{path}.type: Manual has {manualType.ToJsonString()}, generated has {generatedType.ToJsonString()}");
            }
            return;
        }
        
        // Both are arrays
        if (manualType is JsonArray manualArray && generatedType is JsonArray generatedArray)
        {
            CompareArrays(manualArray, generatedArray, differences, $"{path}.type");
            return;
        }
        
        // One is string, one is array
        differences.Add($"{path}.type: Type mismatch - one is string, one is array");
    }
    
    /// <summary>
    /// Compares required arrays (order-independent).
    /// </summary>
    private void CompareRequiredArrays(JsonArray manual, JsonArray generated, List<string> differences)
    {
        var manualSet = new HashSet<string>(
            manual.Select(v => v?.GetValue<string>() ?? "").Where(s => !string.IsNullOrEmpty(s))
        );
        var generatedSet = new HashSet<string>(
            generated.Select(v => v?.GetValue<string>() ?? "").Where(s => !string.IsNullOrEmpty(s))
        );
        
        // Find items in manual but not in generated
        var missingInGenerated = manualSet.Except(generatedSet).ToList();
        if (missingInGenerated.Any())
        {
            differences.Add($"required: Manual schema requires [{string.Join(", ", missingInGenerated)}] but generated doesn't");
        }
        
        // Find items in generated but not in manual
        var extraInGenerated = generatedSet.Except(manualSet).ToList();
        if (extraInGenerated.Any())
        {
            differences.Add($"required: Generated schema requires [{string.Join(", ", extraInGenerated)}] but manual doesn't");
        }
    }
    
    /// <summary>
    /// Compares enum arrays (order-independent).
    /// </summary>
    private void CompareEnumArrays(JsonArray manual, JsonArray generated, List<string> differences, string path)
    {
        var manualSet = new HashSet<string>(
            manual.Select(v => v?.GetValue<string>() ?? "").Where(s => !string.IsNullOrEmpty(s))
        );
        var generatedSet = new HashSet<string>(
            generated.Select(v => v?.GetValue<string>() ?? "").Where(s => !string.IsNullOrEmpty(s))
        );
        
        if (!manualSet.SetEquals(generatedSet))
        {
            differences.Add($"{path}.enum: Manual has [{string.Join(", ", manualSet)}], generated has [{string.Join(", ", generatedSet)}]");
        }
    }
    
    /// <summary>
    /// Compares JSON arrays (order-independent for simple values).
    /// </summary>
    private void CompareArrays(JsonArray manual, JsonArray generated, List<string> differences, string path)
    {
        if (manual.Count != generated.Count)
        {
            differences.Add($"{path}: Array length mismatch - manual has {manual.Count} items, generated has {generated.Count} items");
            return;
        }
        
        // For simple arrays (strings, numbers), compare as sets
        var manualStrings = manual.Select(v => v?.ToJsonString() ?? "").ToHashSet();
        var generatedStrings = generated.Select(v => v?.ToJsonString() ?? "").ToHashSet();
        
        if (!manualStrings.SetEquals(generatedStrings))
        {
            differences.Add($"{path}: Array content mismatch");
        }
    }
}
