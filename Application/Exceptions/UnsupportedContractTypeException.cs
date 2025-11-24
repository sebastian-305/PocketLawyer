namespace PocketLawyer.Application.Exceptions;

/// <summary>
/// Exception thrown when an unsupported contract type is requested.
/// </summary>
public class UnsupportedContractTypeException : ContractAnalysisException
{
    public string ContractType { get; }

    public UnsupportedContractTypeException(string contractType)
        : base($"Unsupported contract type: {contractType}")
    {
        ContractType = contractType;
    }

    public UnsupportedContractTypeException(string contractType, Exception? innerException)
        : base($"Unsupported contract type: {contractType}", innerException)
    {
        ContractType = contractType;
    }
}
