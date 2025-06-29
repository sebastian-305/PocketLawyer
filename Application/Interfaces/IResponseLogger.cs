namespace PocketLawyer.Application.Interfaces;

// Ermöglicht das Protokollieren von KI-Antworten.

// Einfaches Interface zum Speichern von Antworten.
public interface IResponseLogger
{
    Task LogResponseAsync(string response);
}
