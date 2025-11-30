using System.Collections;
using System.Reflection;
using PocketLawyer.Application.Interfaces;

namespace PocketLawyer.Infrastructure.Logging;

/// <summary>
/// Implements intelligent parameter serialization for method tracing.
/// Handles various types with appropriate formatting and redacts sensitive data.
/// </summary>
public class ParameterSerializer : IParameterSerializer
{
    private const int MaxStringLength = 200;
    private const int MaxCollectionItems = 5;
    private const int MaxObjectProperties = 5;
    private const int MaxPropertyValueLength = 50;
    
    private static readonly string[] SensitiveKeywords = 
    {
        "password", "secret", "token", "key", "apikey", "api_key"
    };
    
    public string Serialize(string parameterName, object? value)
    {
        // Check for sensitive parameters
        if (IsSensitiveParameter(parameterName))
        {
            return "[REDACTED]";
        }
        
        // Handle null
        if (value == null)
        {
            return "null";
        }
        
        // Handle strings
        if (value is string str)
        {
            return SerializeString(str);
        }
        
        // Handle byte arrays
        if (value is byte[] bytes)
        {
            return $"byte[{bytes.Length}]";
        }
        
        // Handle primitives and common value types
        if (IsPrimitiveOrValueType(value))
        {
            return value.ToString() ?? "null";
        }
        
        // Handle collections (but not strings)
        if (value is IEnumerable enumerable)
        {
            return SerializeCollection(enumerable);
        }
        
        // Handle complex objects
        return SerializeObject(value);
    }
    
    public Dictionary<string, string> SerializeParameters(IDictionary<string, object?> parameters)
    {
        return parameters.ToDictionary(
            kvp => kvp.Key,
            kvp => Serialize(kvp.Key, kvp.Value)
        );
    }
    
    private bool IsSensitiveParameter(string parameterName)
    {
        return SensitiveKeywords.Any(keyword => 
            parameterName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
    
    private bool IsPrimitiveOrValueType(object value)
    {
        var type = value.GetType();
        return type.IsPrimitive 
            || value is decimal 
            || value is DateTime 
            || value is DateTimeOffset
            || value is TimeSpan
            || value is Guid;
    }
    
    private string SerializeString(string str)
    {
        if (str.Length <= MaxStringLength)
        {
            return $"\"{str}\"";
        }
        
        return $"\"{str.Substring(0, MaxStringLength)}...\" (truncated, total: {str.Length} chars)";
    }
    
    private string SerializeCollection(IEnumerable enumerable)
    {
        var items = new List<object?>();
        var count = 0;
        
        foreach (var item in enumerable)
        {
            if (count < MaxCollectionItems)
            {
                items.Add(item);
            }
            count++;
        }
        
        var typeName = enumerable.GetType().Name;
        
        if (count == 0)
        {
            return $"{typeName}[0]";
        }
        
        var itemsStr = string.Join(", ", items.Select(i => i?.ToString() ?? "null"));
        
        if (count > MaxCollectionItems)
        {
            return $"{typeName}[{count}]: [{itemsStr}, ...]";
        }
        
        return $"{typeName}[{count}]: [{itemsStr}]";
    }
    
    private string SerializeObject(object obj)
    {
        var type = obj.GetType();
        var typeName = type.Name;
        
        // Try to get key properties (limit to avoid performance issues)
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Take(MaxObjectProperties)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();
        
        if (properties.Count == 0)
        {
            return typeName;
        }
        
        var propValues = new List<string>();
        
        foreach (var prop in properties)
        {
            try
            {
                var value = prop.GetValue(obj);
                var valueStr = value?.ToString() ?? "null";
                
                if (valueStr.Length > MaxPropertyValueLength)
                {
                    valueStr = valueStr.Substring(0, MaxPropertyValueLength) + "...";
                }
                
                propValues.Add($"{prop.Name}={valueStr}");
            }
            catch
            {
                propValues.Add($"{prop.Name}=<error>");
            }
        }
        
        return $"{typeName} {{ {string.Join(", ", propValues)} }}";
    }
}
