using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PocketLawyer.Infrastructure.SchemaProviders;

/// <summary>
/// Generates JSON schemas from DTOs with custom attributes.
/// Ensures strict compliance with Nebius/OpenAI strict mode requirements.
/// </summary>
public class SchemaGenerator
{
    /// <summary>
    /// Generates a JSON schema from a DTO type.
    /// </summary>
    /// <typeparam name="T">The DTO type to generate schema from</typeparam>
    /// <returns>A JSON schema object compatible with Nebius strict mode</returns>
    public JsonObject GenerateSchema<T>()
    {
        return GenerateSchema(typeof(T));
    }

    /// <summary>
    /// Generates a JSON schema from a DTO type.
    /// </summary>
    /// <param name="type">The DTO type to generate schema from</param>
    /// <returns>A JSON schema object compatible with Nebius strict mode</returns>
    public JsonObject GenerateSchema(Type type)
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false // Required for strict mode
        };

        // Add x-sectionTitle from class attribute (ignored by AI, used by frontend)
        var sectionAttr = type.GetCustomAttribute<SchemaSectionAttribute>();
        if (sectionAttr != null)
        {
            schema["x-sectionTitle"] = sectionAttr.SectionTitle;
        }

        var properties = new JsonObject();
        var required = new JsonArray();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip properties marked with JsonIgnore
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                continue;

            var jsonNameAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonNameAttr == null)
                continue; // Skip properties without JsonPropertyName

            string propertyName = jsonNameAttr.Name;
            var propSchema = GeneratePropertySchema(prop);

            properties[propertyName] = propSchema;

            // Add to required if:
            // 1. Marked with required keyword, OR
            // 2. Not nullable (value type without Nullable<T>, or reference type in non-nullable context)
            if (IsPropertyRequired(prop) || !IsNullable(prop.PropertyType))
            {
                required.Add(propertyName);
            }
        }

        schema["properties"] = properties;
        
        if (required.Count > 0)
        {
            schema["required"] = required;
        }

        return schema;
    }

    private JsonObject GeneratePropertySchema(PropertyInfo prop)
    {
        var propSchema = new JsonObject();
        var displayAttr = prop.GetCustomAttribute<SchemaDisplayAttribute>();
        var type = prop.PropertyType;

        // Handle nullable types
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        bool isNullable = IsNullable(type);

        // Set JSON type
        if (underlyingType.IsArray || (underlyingType.IsGenericType && 
            underlyingType.GetGenericTypeDefinition() == typeof(List<>)))
        {
            propSchema["type"] = "array";
            Type elementType = underlyingType.IsArray 
                ? underlyingType.GetElementType()! 
                : underlyingType.GetGenericArguments()[0];
            
            propSchema["items"] = GenerateTypeSchema(elementType);
        }
        else if (underlyingType.IsEnum)
        {
            // Enum type - treat as string with enum values
            string jsonType = "string";
            
            if (isNullable)
            {
                propSchema["type"] = new JsonArray { jsonType, "null" };
            }
            else
            {
                propSchema["type"] = jsonType;
            }

            var enumValues = new JsonArray();
            foreach (var value in Enum.GetNames(underlyingType))
            {
                enumValues.Add(value);
            }
            propSchema["enum"] = enumValues;
        }
        else if (underlyingType.IsClass && underlyingType != typeof(string))
        {
            // Nested object - merge the generated schema into propSchema
            var nestedSchema = GenerateSchema(underlyingType);
            foreach (var kvp in nestedSchema)
            {
                propSchema[kvp.Key] = kvp.Value?.DeepClone();
            }
        }
        else
        {
            // Primitive type
            string jsonType = MapCSharpTypeToJsonType(underlyingType);
            
            if (isNullable)
            {
                propSchema["type"] = new JsonArray { jsonType, "null" };
            }
            else
            {
                propSchema["type"] = jsonType;
            }
        }

        // Add metadata from attributes (x-* fields ignored by AI, used by frontend)
        if (displayAttr != null)
        {
            propSchema["x-displayName"] = displayAttr.DisplayName;

            if (!string.IsNullOrEmpty(displayAttr.Description))
            {
                propSchema["description"] = displayAttr.Description;
            }

            if (!string.IsNullOrEmpty(displayAttr.Format))
            {
                propSchema["format"] = displayAttr.Format;
            }

            if (displayAttr.MaxLength > 0)
            {
                propSchema["maxLength"] = displayAttr.MaxLength;
            }

            if (displayAttr.MinLength > 0)
            {
                propSchema["minLength"] = displayAttr.MinLength;
            }

            if (displayAttr.Examples != null && displayAttr.Examples.Length > 0)
            {
                var examples = new JsonArray();
                foreach (var example in displayAttr.Examples)
                {
                    examples.Add(example);
                }
                propSchema["examples"] = examples;
            }

            // Add x-styling (ignored by AI, used by frontend)
            var styling = new JsonObject();
            if (!string.IsNullOrEmpty(displayAttr.Icon))
                styling["icon"] = displayAttr.Icon;
            if (!string.IsNullOrEmpty(displayAttr.DisplayClass))
                styling["displayClass"] = displayAttr.DisplayClass;
            if (!string.IsNullOrEmpty(displayAttr.Importance))
                styling["importance"] = displayAttr.Importance;

            if (styling.Count > 0)
            {
                propSchema["x-styling"] = styling;
            }
        }

        return propSchema;
    }

    private JsonObject GenerateTypeSchema(Type type)
    {
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType.IsClass && underlyingType != typeof(string))
        {
            // Nested object - generate full schema
            return GenerateSchema(underlyingType);
        }
        else
        {
            // Primitive type
            var schema = new JsonObject
            {
                ["type"] = MapCSharpTypeToJsonType(underlyingType)
            };

            if (underlyingType.IsEnum)
            {
                var enumValues = new JsonArray();
                foreach (var value in Enum.GetNames(underlyingType))
                {
                    enumValues.Add(value);
                }
                schema["enum"] = enumValues;
            }

            return schema;
        }
    }

    private static string MapCSharpTypeToJsonType(Type type)
    {
        if (type == typeof(string))
            return "string";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            return "integer";
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            return "number";
        if (type == typeof(bool))
            return "boolean";
        if (type.IsArray)
            return "array";
        if (type.IsClass || type.IsValueType)
            return "object";

        return "string"; // Fallback
    }

    private static bool IsNullable(Type type)
    {
        if (!type.IsValueType)
            return true; // Reference types are nullable by default (unless marked with required or non-nullable context)
        
        return Nullable.GetUnderlyingType(type) != null;
    }
    
    private static bool IsPropertyRequired(PropertyInfo prop)
    {
        // Check for required modifier (C# 11+)
        return prop.GetCustomAttribute<System.Runtime.CompilerServices.RequiredMemberAttribute>() != null;
    }
}
