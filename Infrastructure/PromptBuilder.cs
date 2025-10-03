using System.Text;
using System.Collections.Generic;

namespace PocketLawyer.Infrastructure;

public static class PromptBuilder
{
    public static string AddKvInfo(string prompt, string kvName, IEnumerable<string> kvFiles)
    {
        var sbPrompt = new StringBuilder(prompt);
        sbPrompt.AppendLine();
        sbPrompt.AppendLine($"Der folgende Text enthält den dazugehörigen Kollektivvertrag \"{kvName}\" und mögliche Anhänge. " +
            "Diese Informationen sind laut Gesetz zwingend Grundlage der Vertragsanalyse. Wenn der Vertrag in einem Punkt so etwas schreibt wie \"laut Kollektivvertrag\" soll die entsprechende Regelung" +
            "aus dem Kollektivvetrag angegeben werden mit dem Zusatz \"(lt. Kollektivvertrag)\"");
        sbPrompt.AppendLine(string.Join("\n\n", kvFiles));
        return sbPrompt.ToString();
    }
    public static string BuildAnalysisPrompt(string filename, string fileContent)
    {
        StringBuilder promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Ich sende dir eine Datei. Dabei handelt es sich um einen Arbeitsvertrag. M\u00f6glicherweise erh\u00e4ltst du auch den Kollektivvertrag dazu. Der Kollektivvertrag ist die wichtigste Grundlage der Analyse.");
        promptBuilder.AppendLine("Als Rechtsgrundlage der einzelnen Analysepunkte soll neben dem Paragrafen und dem Namen des jeweiligen Gesetzes auch der Wortlaut des Gesetzestextes angegeben werden.\n");
        promptBuilder.AppendLine("Die Property 'rechtsgrundlage' ist ein Array von Objekten mit den Feldern 'rechtsgrundlage' und 'wortlaut'. Beide Felder sind erforderlich.\n");
        //promptBuilder.AppendLine("Falls es sich um einen Arbeitsvertrag handelt, analysiere ihn bitte und pr\u00e4sentiere mir deine Auswertungen. ");
        promptBuilder.AppendLine("Bewerte ihn und untersuche ihn nach fragw\u00fcrdigen Klauseln.");
        promptBuilder.AppendLine("Finde alle vorteilhaften Klauseln, falls vorhanden.");
        promptBuilder.AppendLine("Die Zusammenfassung soll kurz und prägnant ausfallen. Die Informationen kurz und Präzise formulieren.");
        //promptBuilder.AppendLine("Wenn ein Kollektivvertrag beigef\u00fcgt ist, hebe alle Klauseln hervor, die f\u00fcr den Arbeitnehmer eine Verbesserung gegen\u00fcber diesem Kollektivvertrag darstellen.");
        promptBuilder.AppendLine("Die Analyse und Bewertung erfolgt aus Sicht des Arbeitnehmers. ");
        promptBuilder.AppendLine("Gib zu jeder fachlichen Begr\u00fcndung auch eine Erkl\u00e4rung in einfacher Sprache.");
        //promptBuilder.AppendLine("Bitte st\u00fctze deine Aussagen auf die Gesetze im Cache und auf den gegebenenfalls beigef\u00fcgten Kollektivvertrag. \n");
        promptBuilder.AppendLine("Gib ein sehr ausf\u00fchrliches Fazit an. ");
        promptBuilder.AppendLine("Bitte verwende keine Markdown-Formatierung, keine * und kein #");
        //promptBuilder.AppendLine("Formatiere deine Antwort in HTML.");
        promptBuilder.AppendLine("Gib neben dem Paragraphen der Rechtsgrundlage auch den Wortlaut des Gesetzestextes an.\n");
        promptBuilder.AppendLine("F\u00fcr Regelungen, die im Arbeitsvertrag mit \"laut Gesetz\" oder \"laut Kollektivvertrag\" bezeichnet sind, zitiere die entsprechende Bestimmung aus diesen Quellen, sofern vorhanden, und f\u00fcge die zugeh\u00f6rigen Informationen an.\n");
        promptBuilder.AppendLine("---------------------------------");
        promptBuilder.AppendLine("\n\nDateiname des Vertrags: ");
        promptBuilder.AppendLine(filename);
        promptBuilder.AppendLine("\n\nVertragsinhalt: \n");
        promptBuilder.AppendLine(fileContent);
        promptBuilder.AppendLine("---------------------------------");
        return promptBuilder.ToString();
    }

    public static string BuildValidationPrompt(string filename, string fileContent)
    {
        var sb = new StringBuilder();
        sb.Append("Ich sende dir eine Datei. Analysiere, ob der Inhalt der Datei lesbar ist. Schreibe das Ergebnis in die boolean Variable \"isReadable\".");
        sb.Append("Wenn der Text lesbar ist, analysiere ob es sich dabei um einen Arbeitsvertrag handelt. Unabh\u00e4ngig von der Sprache der Datei soll seine Antwort auf deutsch sein!");
        sb.Append("Schreibe das Ergebnis in die bool Variable \"isContract\". ");
        sb.Append("Gib eine Begr\u00fcndung deiner Ergebnisse und falls es sich um einen Arbeitsvertrag handelt, eine \u00fcberblicksm\u00e4\u00dfige Zusammenfassung vom Vertrag in die Variable \"result\". ");
        sb.Append("Verwende keine Markdown-Formatierungen! Hier die Datei:\n\n Dateiname: \n");
        sb.Append(filename);
        sb.Append("\nDateiinhalt: ");
        sb.Append(fileContent);
        return sb.ToString();
    }

    public static string BuildKvDetectionPrompt(string contractContent, string[] kvsFolders)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Arbeitsvertrag:");
        promptBuilder.AppendLine(contractContent);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Kollektivverträge:");
        promptBuilder.AppendLine(string.Join(", ", kvsFolders));
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Finde heraus, ob einer der bereits \u00fcbermittelten Kollektivvertr\u00e4ge zu dem Arbeitsvertrag passt. Schreibe Multiple wenn mehrere Kollektivvertr\u00e4ge passen w\u00fcrden.");
        promptBuilder.AppendLine("Falls keiner Kollektivvertrag ausfindig gemacht werden kann oder wenn keiner der Kollektivvertr\u00e4ge in der \u00fcbermittelten Liste zum Arbeitsvertrag passt.");
        promptBuilder.AppendLine("Antworte im JSON-Format {\"kollektivvertrag\":\"NAME Oder Multiple ODER Keiner\"}.");
        promptBuilder.AppendLine("Der Wortlaut deiner Antwort solle exakt dem Wortlaut der \u00fcbermittelten Namen der Kollektivvertr\u00e4ge entsprechen, weil diese danach in einem Programm verarbeitet werden.");
        return promptBuilder.ToString();
    }
}
