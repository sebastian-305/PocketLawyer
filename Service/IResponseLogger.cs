namespace PocketLawyer.Service;

public interface IResponseLogger
{
    Task LogResponseAsync(string response);
}
