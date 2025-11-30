namespace PocketLawyer.Application.Interfaces;

public interface IContextProvider
{
    Task<string> GetContextAsync(string agentName, string tier, string stepName);
    Task<string> GetContextForCurrentStepAsync();
}
