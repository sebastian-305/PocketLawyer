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
    Task<string> SendPrompt(string prompt, object? outputSchema);
}
