using PocketLawyer.Application.Models;

namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Interface for AI service clients that send prompts and receive responses.
/// </summary>
public interface IAiClient
{
    /// <summary>
    /// Sends a prompt to the AI service with an optional output schema.
    /// </summary>
    /// <param name="prompt">The prompt text to send to the AI</param>
    /// <param name="outputSchema">Optional JSON schema defining the expected response structure</param>
    /// <returns>The AI response as a JSON string</returns>
    /// <exception cref="AiServiceException">Thrown when the AI service returns an error</exception>
    [Obsolete("Use SendPromptWithMetadata instead to get token usage information. This method will be removed in a future version.")]
    Task<string> SendPrompt(string prompt, object? outputSchema);

    /// <summary>
    /// Sends a prompt to the AI service with an optional output schema and returns response with token metadata.
    /// </summary>
    /// <param name="prompt">The prompt text to send to the AI</param>
    /// <param name="outputSchema">Optional JSON schema defining the expected response structure</param>
    /// <returns>The AI response with token usage information</returns>
    /// <exception cref="AiServiceException">Thrown when the AI service returns an error</exception>
    Task<AiResponse> SendPromptWithMetadata(string prompt, object? outputSchema);
}
