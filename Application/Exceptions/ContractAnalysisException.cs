namespace PocketLawyer.Application.Exceptions;

/// <summary>
/// Base exception for all contract analysis related errors.
/// </summary>
public class ContractAnalysisException : Exception
{
    public ContractAnalysisException(string message)
        : base(message)
    {
    }

    public ContractAnalysisException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
