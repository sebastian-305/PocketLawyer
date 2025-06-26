namespace PocketLawyer.Service;

// Schnittstelle für unsere KI-Dienste. Sowohl Gemini als auch Claude
// implementieren diese Methoden, damit der Controller sie einheitlich
// verwenden kann.
public interface IAiService
{
    // Erzeugt aus Dateinamen und Inhalt ein Prompt für die KI.
    string PromptBuilder(string filename, string filecontent);
    // Sendet ein Prompt an den Dienst und liefert die rohe Antwort zurück.
    Task<string> SendPrompt(string prompt, object? outputSchema, bool sendAltKey);

    // Optionaler Aufruf zum Hinterlegen von Kontextdaten bei der KI.
    Task<string> SendContext();
}
