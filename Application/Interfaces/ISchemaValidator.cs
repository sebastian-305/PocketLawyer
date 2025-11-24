using System.Text.Json.Nodes;

namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Validates AI responses against JSON schemas.
/// </summary>
public interface ISchemaValidator
{
    /// <summary>
    /// Validates a JSON response against a schema.
    /// </summary>
    /// <param name="schema">The JSON schema to validate against</param>
    /// <param name="response">The JSON response to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool Validate(JsonObject schema, JsonObject response);

    /// <summary>
    /// Validates a JSON response against a schema with detailed error information.
    /// </summary>
    /// <param name="schema">The JSON schema to validate against</param>
    /// <param name="response">The JSON response to validate</param>
    /// <returns>Validation result with error details</returns>
    ValidationResult ValidateWithDetails(JsonObject schema, JsonObject response);
}

/// <summary>
/// Result of schema validation with error details.
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
