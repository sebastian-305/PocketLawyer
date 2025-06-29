namespace PocketLawyer.Application.Interfaces;

// Verwaltet den globalen Kontext der Gemini-API.

// Schnittstelle zum Laden und Speichern des Kontextschlüssels.
public interface IGeminiContextManager
{
    Task<string?> GetContextKeyAsync();
    Task<string> SendContextAsync();
}
