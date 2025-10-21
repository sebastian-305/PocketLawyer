namespace PocketLawyer.Application.Exceptions;

/// <summary>
/// Exception thrown when validation or decoding of an uploaded file fails.
/// </summary>
public class FileValidationException : Exception
{
    public string? FileName { get; }

    public FileValidationException(string message, string? fileName = null)
        : base(message)
    {
        FileName = fileName;
    }

    public FileValidationException(string message, string? fileName, Exception? innerException)
        : base(message, innerException)
    {
        FileName = fileName;
    }
}
