namespace PocketLawyer.Application.Exceptions;

/// <summary>
/// Exception thrown when a requested prompt cannot be found.
/// </summary>
public class PromptNotFoundException : ContractAnalysisException
{
    public string PromptKey { get; }

    public PromptNotFoundException(string promptKey)
        : base($"Prompt not found for key: {promptKey}")
    {
        PromptKey = promptKey;
    }

    public PromptNotFoundException(string promptKey, Exception? innerException)
        : base($"Prompt not found for key: {promptKey}", innerException)
    {
        PromptKey = promptKey;
    }
}
