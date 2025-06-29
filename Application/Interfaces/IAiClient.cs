namespace PocketLawyer.Application.Interfaces;

// Abstraktion eines beliebigen KI-Dienstes.

// Interface für alle KI-Clients (Gemini, Claude, ...)
public interface IAiClient
{
    Task<string> SendPrompt(string prompt, object? outputSchema, bool useAltKey);
}
