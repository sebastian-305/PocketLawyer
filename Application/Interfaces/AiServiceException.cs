namespace PocketLawyer.Application.Interfaces;

/// <summary>
/// Exception that represents errors returned from an AI service.
/// </summary>
public class AiServiceException : Exception
{
    public AiServiceException() { }
    public AiServiceException(string message) : base(message) { }
    public AiServiceException(string message, Exception innerException) : base(message, innerException) { }
}
