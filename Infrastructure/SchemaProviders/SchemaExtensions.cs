using System.Text.Json.Nodes;

namespace PocketLawyer.Infrastructure.SchemaProviders;

/// <summary>
/// Utility class for extending JSON schema objects with additional metadata.
/// </summary>
public static class SchemaExtensions
{
    /// <summary>
    /// Adds a display name to a JSON schema property for frontend rendering.
    /// The x-displayName field is used by the frontend to show user-friendly property names.
    /// </summary>
    /// <param name="property">The JSON schema property object to extend.</param>
    /// <param name="displayName">The user-friendly display name for the property.</param>
    /// <returns>The modified property object for method chaining.</returns>
    public static JsonObject AddDisplayName(this JsonObject property, string displayName)
    {
        property["x-displayName"] = displayName;
        return property;
    }
}
