namespace PocketLawyer.Service;

public class PreValidationException : Exception
{
    public PreValidationException(string message) : base(message) {}
    public PreValidationException(string message, Exception inner) : base(message, inner) {}
}
