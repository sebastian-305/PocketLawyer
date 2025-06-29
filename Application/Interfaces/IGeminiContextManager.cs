namespace PocketLawyer.Application.Interfaces;

public interface IGeminiContextManager
{
    Task<string?> GetContextKeyAsync();
    Task<string> SendContextAsync();
}
