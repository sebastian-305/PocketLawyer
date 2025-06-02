namespace PocketLawyer.Service;

public interface IAiService
{

  

    public Task<string> SendPrompt(string prompt, string filename, string filecontent, object? outputSchema);

    public Task<string> SendPrompt(string prompt, object? outputSchema);

    [Obsolete("SendContext() is deprecated, please use SendPrompt(string userMessage) instead.")]
    public Task<string> SendContext();
}
