namespace PocketLawyer.Options;

public class GeminiOptions
{
    public string Key { get; set; } = string.Empty;
    public string? AltKey { get; set; }
    public string ModelId { get; set; } = "gemini-1.5-flash-002";
    public bool WriteResponseInFile { get; set; }
}
