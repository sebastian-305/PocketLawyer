using System.Text.Json.Nodes;
using NJsonSchema;
using PocketLawyer.Application.Interfaces;

namespace PocketLawyer.Infrastructure;

/// <summary>
/// Validates AI responses against JSON schemas using NJsonSchema.
/// </summary>
public class SchemaValidator : ISchemaValidator
{
    /// <summary>
    /// Validates a JSON response against a schema.
    /// </summary>
    /// <param name="schema">The JSON schema to validate against</param>
    /// <param name="response">The JSON response to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool Validate(JsonObject schema, JsonObject response)
    {
        try
        {
            var jsonSchema = JsonSchema.FromJsonAsync(schema.ToJsonString()).Result;
            var errors = jsonSchema.Validate(response.ToJsonString());
            return errors.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a JSON response against a schema with detailed error information.
    /// </summary>
    /// <param name="schema">The JSON schema to validate against</param>
    /// <param name="response">The JSON response to validate</param>
    /// <returns>Validation result with error details</returns>
    public ValidationResult ValidateWithDetails(JsonObject schema, JsonObject response)
    {
        try
        {
            var jsonSchema = JsonSchema.FromJsonAsync(schema.ToJsonString()).Result;
            var errors = jsonSchema.Validate(response.ToJsonString());
            
            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors.Select(e => FormatError(e)).ToList()
            };
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { $"Schema validation failed: {ex.Message}" }
            };
        }
    }

    private static string FormatError(NJsonSchema.Validation.ValidationError error)
    {
        var path = string.IsNullOrEmpty(error.Path) ? "root" : error.Path;
        return $"{path}: {error.Kind} - {error}";
    }
}
