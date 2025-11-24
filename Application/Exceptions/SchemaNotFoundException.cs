namespace PocketLawyer.Application.Exceptions;

/// <summary>
/// Exception thrown when a requested JSON schema cannot be found.
/// </summary>
public class SchemaNotFoundException : ContractAnalysisException
{
    public string SchemaKey { get; }

    public SchemaNotFoundException(string schemaKey)
        : base($"Schema not found for key: {schemaKey}")
    {
        SchemaKey = schemaKey;
    }

    public SchemaNotFoundException(string schemaKey, Exception? innerException)
        : base($"Schema not found for key: {schemaKey}", innerException)
    {
        SchemaKey = schemaKey;
    }
}
