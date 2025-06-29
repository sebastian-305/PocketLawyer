namespace PocketLawyer.Service;

public interface IGeminiContextManager
{
    Task<string?> GetContextKeyAsync();
    Task<string> SendContextAsync();
}
