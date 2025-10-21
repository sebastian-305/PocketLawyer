using System.Collections.Generic;
using System.Text;
using PocketLawyer.Web;

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
    public static string BuildEmploymentAnalysisPrompt(AgentType type, string filename, string fileContent)
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


    public static string BuildRentAnalysisPrompt(AgentType type, string filename, string fileContent)
    {
        StringBuilder promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Du bist ein erfahrener Rechtsanwalt, der sich auf das österreichische Mietrecht spezialisiert hat, insbesondere auf die Prüfung von Mietverträgen aus Sicht des Mieters.");
        promptBuilder.AppendLine("Ich sende dir den vollständigen Text eines Mietvertrags zur Analyse.");
        promptBuilder.AppendLine("Die Property rechtsgrundlage ist ein Array von Objekten mit den Feldern rechtsgrundlage (z. B. \"§ 30 MRG – Kündigungsgründe\") und wortlaut (vollständiger Gesetzestext). Beide Felder sind erforderlich.\n");
        promptBuilder.AppendLine("Untersuche den Vertrag auf unzulässige, nachteilige oder widersprüchliche Klauseln.");
        promptBuilder.AppendLine("Finde außerdem alle vorteilhaften Klauseln, sofern vorhanden. Eine Klausel gilt nur dann als vorteilhaft, wenn sie über das gesetzlich Übliche hinausgeht und ein besonderes Entgegenkommen des Vermieters darstellt.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Die Zusammenfassung soll Stichwortartig formuliert sein. Nur die Information in das jeweilige Feld.");
        promptBuilder.AppendLine("Die Analyse und Bewertung erfolgt ausschließlich aus Sicht des Mieters. ");
        promptBuilder.AppendLine("Gib zu jeder fachlichen Begr\u00fcndung auch eine Erkl\u00e4rung in einfacher Sprache.");
        promptBuilder.AppendLine("Gib ein ausführliches Fazit ab, das den Vertrag insgesamt bewertet und eine klare Empfehlung enthält, ob eine rechtliche Prüfung durch einen Anwalt angeraten ist. ");
        promptBuilder.AppendLine("Bitte verwende keine Markdown-Formatierung, keine * und kein #");
        promptBuilder.AppendLine("Falls keine unzulässigen, nachteiligen oder widersprüchlichen Klauseln gefunden werden, lasse diesen Abschnitt weg und erwähne dies ausdrücklich im Fazit.");
        promptBuilder.AppendLine("Begrenze deine das Fazit auf maximal 1200 Zeichen.");
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

    public static string BuildValidationPrompt(AgentType type, string filename, string fileContent)
    {
        string typename = String.Empty;

        switch (type)
        {
            case AgentType.Employment:
                typename = "Arbeitsvertrag";
                break;
            case AgentType.Rent:
                typename = "Mietvertrag";
                break;
            default:
                throw new NotImplementedException(type.ToString()); 
        }

        var sb = new StringBuilder();
        sb.AppendLine("Analysiere die folgende Datei.");
        sb.AppendLine("1. Prüfe, ob der Inhalt lesbar ist (kein Binär- oder Bildformat).");
        sb.AppendLine("   Schreibe das Ergebnis in die boolesche Variable \"isReadable\".");
        sb.AppendLine($"2. Falls der Inhalt lesbar ist, prüfe, ob es sich um einen {typename} handelt.");
        sb.AppendLine("   Schreibe das Ergebnis in die boolesche Variable \"isContract\".");
        sb.AppendLine("3. Gib eine kurze, prägnante Begründung deiner Ergebnisse in deutscher Sprache in die Variable \"result\".");
        sb.AppendLine($"   Falls es sich um einen {typename} handelt, fasse den Vertragsinhalt nur in 5–7 Sätzen überblicksmäßig zusammen.");
        sb.AppendLine("Verwende keine Markdown-Formatierungen und keine Sonderzeichen wie * oder #.");
        sb.AppendLine("Begrenze deine gesamte Antwort auf maximal 1500 Zeichen.");
        sb.AppendLine();
        sb.AppendLine("Dateiname:");
        sb.AppendLine(filename);
        sb.AppendLine();
        sb.AppendLine("Dateiinhalt:");
        sb.AppendLine(fileContent);
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
