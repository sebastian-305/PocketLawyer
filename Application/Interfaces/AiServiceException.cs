namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Ausnahme, die Fehler eines KI-Dienstes repräsentiert.
/// </summary>
// Basisfehler für Probleme beim Aufruf der KI.
public class AiServiceException : Exception
{
    public AiServiceException() { }
    public AiServiceException(string message) : base(message) { }
    public AiServiceException(string message, Exception innerException) : base(message, innerException) { }
}
