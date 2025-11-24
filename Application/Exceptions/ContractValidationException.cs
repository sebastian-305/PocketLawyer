namespace PocketLawyer.Application.Exceptions;

/// <summary>
/// Exception thrown when contract validation fails during initialization.
/// This includes readability checks and contract type mismatches.
/// </summary>
public class ContractValidationException : ContractAnalysisException
{
    public string? ValidationStep { get; }

    public ContractValidationException(string message, string? validationStep = null)
        : base(message)
    {
        ValidationStep = validationStep;
    }

    public ContractValidationException(string message, string? validationStep, Exception? innerException)
        : base(message, innerException)
    {
        ValidationStep = validationStep;
    }
}
