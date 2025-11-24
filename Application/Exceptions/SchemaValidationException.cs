namespace PocketLawyer.Application.Exceptions;

/// <summary>
/// Exception thrown when AI response fails JSON schema validation.
/// </summary>
public class SchemaValidationException : ContractAnalysisException
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public SchemaValidationException(string message, IReadOnlyList<string> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors ?? Array.Empty<string>();
    }

    public SchemaValidationException(string message, IReadOnlyList<string> validationErrors, Exception? innerException)
        : base(message, innerException)
    {
        ValidationErrors = validationErrors ?? Array.Empty<string>();
    }
}
