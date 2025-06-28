namespace PocketLawyer.Service;

public interface IAiService
{

    string PromptBuilder(string filename, string fileContent);
    Task<string> SendPrompt(string prompt, object? outputSchema, bool useAltKey);
    Task<string> SendContext();
}
