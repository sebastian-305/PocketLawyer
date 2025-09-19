namespace PocketLawyer.Application.Interfaces;

public interface IResponseLogger
{
    Task LogResponseAsync(string response);
}
