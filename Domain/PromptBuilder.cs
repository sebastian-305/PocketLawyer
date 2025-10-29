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
        promptBuilder.AppendLine("Die Analyse und Bewertung erfolgt für die Mieter:in. ");
        
        promptBuilder.AppendLine("Für die Analyse verweise auf Gesetze aus RIS, die fürs österreichische Mietrecht relevant sind, wie: MRG, GebG, HeizKG, KSchG, MaklerG, RichtWG, WEG, WGG letzter Fassung und Informationen der Arbeiterkammer Österreich.");
        promptBuilder.AppendLine("Bitte beachte:Durchschnittlich finden sich in jedem Mietvertrag ca. 14 Verstöße gegen Bestimmungen des MRG/WGG, ca. 17 Verstöße gegen Bestimmungen des ABGB sowie ca. 19 Verstöße gegen KSchG-Bestimmungen.\n");
        promptBuilder.AppendLine("Untersuche den Vertrag auf unzulässige, nachteilige, widersprüchliche oder vorteilhafte Klauseln.");

        promptBuilder.AppendLine("Unzulässige Klauseln sind Vertragsbestimmungen, die gegen zwingende gesetzliche Vorschriften verstoßen und daher rechtlich unwirksam sind.");
        promptBuilder.AppendLine("Nachteilige Klauseln sind nicht nur gesetzwidrig, sondern benachteiligen regelmäßig den Mieter, da sie von den gesetzlichen Vorschriften zu dessen Ungunsten – und damit zugunsten des Vermieters – abweichen.");
        promptBuilder.AppendLine("Unzulässige Klauseln sind Vertragsbestimmungen, die gegen zwingende gesetzliche Vorschriften verstoßen und daher rechtlich unwirksam sind.");
        promptBuilder.AppendLine("Vorteilhafte Klauseln gilt dann als vorteilhaft, wenn sie über das gesetzlich Übliche oder Marktübliche hinausgeht und ein Entgegenkommen des Vermieters darstellt. Sie verschaffen dem Vermieter Vorteile, auf die er rechtlich keinen Anspruch hat.");
        //      promptBuilder.AppendLine("Intransparente Klauseln sind dermaßen unklar und/oder unverständlich abgefasst, dass sie im\nRahmen der Vertragsprüfungen als intransparent im Sinne des § 6 Abs 3\nKSchG beurteilt werden mussten und aus diesem Grund auch rechtsunwirksam\nsind. ");

        promptBuilder.AppendLine("Für jede unzulässige, nachteilige, widersprüchliche oder einseitig vorteilhafte Klausel zitiere den exakten Wortlaut aus dem Vertrag wortwörtlich." +
            "\nGib diesen problematischen oder vorteilhaften Satz in Anführungszeichen („…“) wieder, genau so, wie er im Vertrag steht – ohne ihn zu verändern oder zu interpretieren.");
        promptBuilder.AppendLine("Im Feld „Problem“ gib eine juristisch fundierte, aber klar und verständlich formulierte Erklärung des jeweiligen Problems an." +
            "\nErkläre, warum die Klausel nach österreichischem Recht unzulässig, nachteilig oder widersprüchlich ist, und welchen rechtlichen Grundsatz oder welche gesetzliche Bestimmung sie verletzt." +
            "\nFormuliere die Begründung so, dass sie auch für Nicht-Juristen leicht verständlich, aber dennoch professionell und fachlich korrekt klingt.");

        
        promptBuilder.AppendLine("Die Property Rechtsgrundlage ist ein Array von Objekten mit den Feldern rechtsgrundlage, auf die sich das Problem referenziert (z. B. \"§ 30 MRG – Kündigungsgründe\") und Zitat (vollständiger Gesetzestext). Zitiere ganz genau und wortwörtlich den Satz aus dem entsprechendem RIS-Gesetz in Anführungszeichen. Beide Felder sind erforderlich.\n");

  //      promptBuilder.AppendLine("Die Zusammenfassung soll Stichwortartig formuliert sein. Nur die Information in das jeweilige Feld.");


        promptBuilder.AppendLine("Im Feld „Fazit“ gib eine umfassende Gesamtbewertung des gesamten Vertrags ab." +
            "\nFasse die wichtigsten rechtlichen Risiken und Auffälligkeiten zusammen und bewerte, wie ausgewogen und rechtskonform der Vertrag insgesamt ist." +
            "\nGib anschließend eine klare, begründete Empfehlung, ob eine weiterführende rechtliche Prüfung durch eine Fachanwältin oder einen Fachanwalt sinnvoll oder dringend angeraten ist.");
        promptBuilder.AppendLine("Bitte verwende keine Markdown-Formatierung, keine * und kein #");
        promptBuilder.AppendLine("Falls keine unzulässigen, nachteiligen oder widersprüchlichen Klauseln gefunden werden, erwähne dies ausdrücklich im entsprechenden Feld.");
        promptBuilder.AppendLine("Begrenze deine das Fazit auf maximal 2000 Zeichen.");
        promptBuilder.AppendLine("Für Regelungen, die im Mietvertrag mit „laut Gesetz“ oder ähnlichen Formulierungen bezeichnet sind, zitiere die entsprechende gesetzliche Bestimmung vollständig und füge sie der Analyse hinzu.\n");
        promptBuilder.AppendLine("----- ANALYSEBEGINN -----");
        promptBuilder.AppendLine("Rolle: erfahrener Rechtsanwalt für österreichisches Mietrecht (Mieterperspektive)");
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
        sb.AppendLine("3. Gib eine kurze, prägnante Begründung deiner Ergebnisse in deutscher Sprache in die Variable \"result\" Erwähne die Lesbarkeit nur, wenn die Datei nicht lesbar ist.");
        sb.AppendLine($"   Falls es sich um einen {typename} handelt, fasse den Vertragsinhalt nur in 7 - 10 Sätzen überblicksmäßig zusammen.");
        sb.AppendLine("Verwende keine Markdown-Formatierungen und keine Sonderzeichen wie * oder #.");
        sb.AppendLine("Begrenze deine gesamte Antwort auf maximal 2200 Zeichen.");
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
