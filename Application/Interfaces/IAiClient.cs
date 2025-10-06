namespace PocketLawyer.Application.Interfaces;

public interface IAiClient
{
    Task<string> SendPrompt(string prompt, object? outputSchema);
}
