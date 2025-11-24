namespace PocketLawyer.Application.Models;

/// <summary>
/// Represents an AI response with token usage metadata.
/// </summary>
/// <param name="Content">The AI response content as a JSON string</param>
/// <param name="InputTokens">Number of input tokens consumed</param>
/// <param name="OutputTokens">Number of output tokens generated</param>
/// <param name="TotalTokens">Total tokens (InputTokens + OutputTokens)</param>
public record AiResponse(
    string Content,
    int InputTokens,
    int OutputTokens,
    int TotalTokens
);
