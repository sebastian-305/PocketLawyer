namespace PocketLawyer.Options;

// Konfiguration für die Gemini-API Anbindung.

// Enthält API-Schlüssel und Modellinformationen für Gemini.
public class GeminiOptions
{
    public string Key { get; set; } = string.Empty;
    public string AltKey { get; set; } = string.Empty;
    public string ModelId { get; set; } = "gemini-1.5-flash-002";
    public bool WriteGeminiResponseInFile { get; set; }
}
