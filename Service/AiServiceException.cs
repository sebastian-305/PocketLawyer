namespace PocketLawyer.Service;

// Eigene Exception-Klasse, um Fehler aus den KI-Diensten
// eindeutig kennzeichnen zu können.

/// <summary>
/// Exception that represents errors returned from an AI service.
/// </summary>
public class AiServiceException : Exception
{
    public AiServiceException()
    {
    }

    public AiServiceException(string message) : base(message)
    {
    }

    public AiServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

