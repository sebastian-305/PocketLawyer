using System.Text.Json.Nodes;

namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Provides JSON schemas for contract analysis steps.
/// Serves as the Single Source of Truth for data structures.
/// </summary>
public interface ISchemaProvider
{
    /// <summary>
    /// Gets the JSON schema for a specific analysis step as BinaryData (for Nebius API).
    /// </summary>
    /// <param name="contractType">Type of contract (e.g., "Rental", "Employment")</param>
    /// <param name="tier">Analysis tier (e.g., "Basic", "Premium")</param>
    /// <param name="step">Analysis step name (e.g., "General", "Costs")</param>
    /// <returns>Schema as BinaryData for AI service</returns>
    BinaryData GetSchema(string contractType, string tier, string step);

    /// <summary>
    /// Gets the JSON schema for a specific analysis step as JsonObject (for frontend endpoint).
    /// </summary>
    /// <param name="contractType">Type of contract (e.g., "Rental", "Employment")</param>
    /// <param name="tier">Analysis tier (e.g., "Basic", "Premium")</param>
    /// <param name="step">Analysis step name (e.g., "General", "Costs")</param>
    /// <returns>Schema as JsonObject for HTTP response</returns>
    JsonObject GetSchemaForEndpoint(string contractType, string tier, string step);
}
