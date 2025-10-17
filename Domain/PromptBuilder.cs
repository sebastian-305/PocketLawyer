using System.Text;
using System.Collections.Generic;

namespace PocketLawyer.Domain;

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
        promptBuilder.AppendLine("Du bist ein erfahrener Rechtsanwalt, der sich auf das österreichische Mietrecht spezialisiert hat, insbesondere auf die Prüfung von Mietverträgen aus Sicht des Mieters.");
        promptBuilder.AppendLine("Ich sende dir den vollständigen Text eines Mietvertrags zur Analyse.");
        promptBuilder.AppendLine("Die Property rechtsgrundlage ist ein Array von Objekten mit den Feldern rechtsgrundlage (z. B. \"§ 30 MRG – Kündigungsgründe\") und wortlaut (vollständiger Gesetzestext). Beide Felder sind erforderlich.\n");
        promptBuilder.AppendLine("Untersuche den Vertrag auf unzulässige, nachteilige oder widersprüchliche Klauseln.");
        promptBuilder.AppendLine("Finde außerdem alle vorteilhaften Klauseln, sofern vorhanden. Eine Klausel gilt nur dann als vorteilhaft, wenn sie über das gesetzlich Übliche hinausgeht und ein besonderes Entgegenkommen des Vermieters darstellt.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Die Zusammenfassung soll kurz, prägnant und in klarer Sprache formuliert sein.");
        promptBuilder.AppendLine("Die Analyse und Bewertung erfolgt ausschließlich aus Sicht des Mieters. ");
        promptBuilder.AppendLine("Gib zu jeder fachlichen Begr\u00fcndung auch eine Erkl\u00e4rung in einfacher Sprache.");
        promptBuilder.AppendLine("Gib ein ausführliches Fazit ab, das den Vertrag insgesamt bewertet und eine klare Empfehlung enthält, ob eine rechtliche Prüfung durch einen Anwalt angeraten ist. ");
        promptBuilder.AppendLine("Bitte verwende keine Markdown-Formatierung, keine * und kein #");
        promptBuilder.AppendLine("Falls keine unzulässigen, nachteiligen oder widersprüchlichen Klauseln gefunden werden, lasse diesen Abschnitt weg und erwähne dies ausdrücklich im Fazit.");
        promptBuilder.AppendLine("Für Regelungen, die im Mietvertrag mit „laut Gesetz“ oder ähnlichen Formulierungen bezeichnet sind, zitiere die entsprechende gesetzliche Bestimmung vollständig und füge sie der Analyse hinzu.\n");
        promptBuilder.AppendLine("----- ANALYSEBEGINN -----");
        promptBuilder.AppendLine("Rolle: Rechtsanwalt für österreichisches Mietrecht (Mieterperspektive)");
        promptBuilder.AppendLine("Aufgabe: Analyse des Mietvertrags nach rechtlichen Risiken und Vorteilen");
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
        sb.Append("Wenn der Text lesbar ist, analysiere ob es sich dabei um einen Mietvertrag handelt. Unabh\u00e4ngig von der Sprache der Datei soll seine Antwort auf deutsch sein!");
        sb.Append("Schreibe das Ergebnis in die bool Variable \"isContract\". ");
        sb.Append("Gib eine Begr\u00fcndung deiner Ergebnisse und falls es sich um einen Mietvertrag handelt, eine \u00fcberblicksm\u00e4\u00dfige Zusammenfassung vom Vertrag in die Variable \"result\". ");
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
