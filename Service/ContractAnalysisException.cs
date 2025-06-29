namespace PocketLawyer.Service;

public class ContractAnalysisException : Exception
{
    public ContractAnalysisException(string message) : base(message) {}
    public ContractAnalysisException(string message, Exception inner) : base(message, inner) {}
}
