namespace PocketLawyer.Application.Models;

/// <summary>
/// Result of agent initialization indicating success or failure.
/// </summary>
/// <param name="Success">True if initialization succeeded, false otherwise</param>
/// <param name="ErrorMessage">Optional error message if initialization failed</param>
public record InitializationResult(
    bool Success,
    string? ErrorMessage = null
);
