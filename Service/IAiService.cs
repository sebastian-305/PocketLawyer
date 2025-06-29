namespace PocketLawyer.Service;

public interface IAiService
{

    Task<string> SendPrompt(string prompt, object? outputSchema, bool useAltKey);
    Task<string> SendContext();
}
