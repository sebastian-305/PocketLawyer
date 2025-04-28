namespace PocketLawyer.Service;

public interface IAiService
{
    public Task<string> SendPrompt(string prompt, List<KeyValuePair<string, string>> files);

    public Task<string> SendPrompt(string userMessage);

    public Task<string> SendContext(List<KeyValuePair<string, string>> files);
}
