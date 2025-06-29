namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Wird ausgelöst, wenn Validierung oder Dekodierung einer Datei scheitern.
/// </summary>
// Fehler die bei der Prüfung hochgeladener Dateien auftreten.
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
