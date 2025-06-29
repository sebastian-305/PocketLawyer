namespace PocketLawyer.Service;

public class FileReceiveException : Exception
{
    public FileReceiveException(string message) : base(message) {}
    public FileReceiveException(string message, Exception inner) : base(message, inner) {}
}
