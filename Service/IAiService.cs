namespace PocketLawyer.Service;

public interface IAiService
{
    string PromptBuilder(string prompt, string filename, string filecontent);
    Task<string> SendPrompt(string prompt, object? outputSchema, bool sendAltKey);

    [Obsolete("SendContext() is deprecated, please use SendPrompt(string userMessage) instead.")]
    Task<string> SendContext();
}
