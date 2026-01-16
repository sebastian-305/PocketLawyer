using System.Collections.Generic;
using System.Text;

namespace PocketLawyer.Domain;

/// <summary>
/// Legacy class for building prompts. This class is deprecated and will be removed in a future version.
/// Use PromptProvider instead for new implementations.
/// </summary>
[Obsolete("This class is deprecated. Use PromptProvider instead.")]
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
    public static string BuildEmploymentAnalysisPrompt(string contractType, string filename, string fileContent)
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


    public static string BuildRentAnalysisPrompt(string contractType, string filename, string fileContent)
    {
        StringBuilder promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Du bist ein erfahrener Rechtsanwalt, der sich auf das österreichische Mietrecht spezialisiert hat, insbesondere auf die Prüfung von Mietverträgen aus Sicht des Mieters.");
        promptBuilder.AppendLine("Ich sende dir den vollständigen Text eines Mietvertrags zur Analyse.");
        promptBuilder.AppendLine("Die Analyse und Bewertung erfolgt für die Mieter:in. ");
        
        promptBuilder.AppendLine("Für die Analyse verweise auf Gesetze aus RIS, die fürs österreichische Mietrecht relevant sind, wie: MRG, GebG, HeizKG, KSchG, MaklerG, RichtWG, WEG, WGG letzter Fassung und Informationen der Arbeiterkammer Österreich.");
        promptBuilder.AppendLine("Bitte beachte:Durchschnittlich finden sich in jedem Mietvertrag ca. 14 Verstöße gegen Bestimmungen des MRG/WGG, ca. 17 Verstöße gegen Bestimmungen des ABGB sowie ca. 19 Verstöße gegen KSchG-Bestimmungen.\n");
        promptBuilder.AppendLine("Untersuche den Vertrag auf unzulässige, nachteilige, widersprüchliche oder vorteilhafte Klauseln.");

        promptBuilder.AppendLine("Finde alle unzulässige Klauseln. Unzulässige Klauseln sind die Vertragsbestimmungen, die gegen zwingende gesetzliche Vorschriften verstoßen und daher rechtlich unwirksam sind.");
        promptBuilder.AppendLine("Finde alle nachteilige Klauseln. Nachteilige Klauseln sind nicht nur gesetzwidrig, sondern benachteiligen regelmäßig den Mieter, da sie von den gesetzlichen Vorschriften zu dessen Ungunsten – und damit zugunsten des Vermieters – abweichen.");
        promptBuilder.AppendLine("Finde alle widersprüchliche Klauseln. Widersprüchliche Klauseln sind Formulierungen in dem Vertrag, die sich gegenseitig ausschließen oder nicht miteinander übereinstimmen.");
        promptBuilder.AppendLine("Finde alle vorteilhafte Klauseln. Vorteilhafte Klauseln gelten dann als vorteilhaft, wenn sie über das gesetzlich Übliche oder Marktübliche hinausgeht und ein Entgegenkommen des Vermieters darstellt. Sie verschaffen dem Vermieter Vorteile, auf die er rechtlich keinen Anspruch hat.");
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

    public static string BuildValidationPrompt(string contractType, string filename, string fileContent)
    {
        string typename = contractType switch
        {
            "Employment" => "Arbeitsvertrag",
            "Rent" => "Mietvertrag",
            _ => throw new NotImplementedException(contractType)
        };

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
        promptBuilder.AppendLine("# AUFGABE: Kollektivvertragserkennung");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## SCHRITT 1: Analysiere den Arbeitsvertrag");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Arbeitsvertrag:");
        promptBuilder.AppendLine(contractContent);
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Identifiziere aus dem Vertrag:");
        promptBuilder.AppendLine("1. **Branche/Sektor**: Welche Branche wird erwähnt? (z.B. Handel, Gastronomie, Gesundheit, Verkehr, etc.)");
        promptBuilder.AppendLine("2. **Position/Tätigkeit**: Was ist die genaue Berufsbezeichnung?");
        promptBuilder.AppendLine("3. **Arbeitgeber**: Welche Art von Unternehmen/Organisation?");
        promptBuilder.AppendLine("4. **Explizite KV-Erwähnung**: Wird ein Kollektivvertrag im Vertrag erwähnt?");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## SCHRITT 2: Verfügbare Kollektivverträge");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("=== VOLLSTÄNDIGE LISTE (NUR DIESE NAMEN VERWENDEN!) ===");
        promptBuilder.AppendLine(string.Join(", ", kvsFolders));
        promptBuilder.AppendLine("=== ENDE DER LISTE ===");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## SCHRITT 3: Matching");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Suche in der obigen Liste nach einem KV, der zur identifizierten Branche/Position passt.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("**WICHTIGE HINWEISE:**");
        promptBuilder.AppendLine("- Die KV-Namen enthalten oft Schlüsselwörter zur Branche (z.B. 'handel', 'hotel', 'pharmazie')");
        promptBuilder.AppendLine("- Achte auf Teilstrings: 'kollektivvertrag_hotel_und_gastgewerbe' passt zu Gastronomie/Hotel");
        promptBuilder.AppendLine("- Bei Unsicherheit: Prüfe mehrere mögliche KVs aus der Liste");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## SCHRITT 4: Antwort");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Antworte im JSON-Format:");
        promptBuilder.AppendLine("```json");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"kollektivvertrag\": \"EXAKTER_NAME_AUS_DER_LISTE\" ODER \"Multiple\" ODER \"Keiner\",");
        promptBuilder.AppendLine("  \"begruendung\": \"Deine Begründung in 2-3 Sätzen\"");
        promptBuilder.AppendLine("}");
        promptBuilder.AppendLine("```");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("### Beispiel-Antworten:");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("**Beispiel 1 - Eindeutige Zuordnung:**");
        promptBuilder.AppendLine("```json");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"kollektivvertrag\": \"kollektivvertrag_handel\",");
        promptBuilder.AppendLine("  \"begruendung\": \"Der Vertrag erwähnt explizit 'Einzelhandel' und die Position ist 'Verkäufer'. Der KV 'kollektivvertrag_handel' aus der Liste passt eindeutig zu dieser Branche.\"");
        promptBuilder.AppendLine("}");
        promptBuilder.AppendLine("```");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("**Beispiel 2 - Mehrere mögliche KVs:**");
        promptBuilder.AppendLine("```json");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"kollektivvertrag\": \"Multiple\",");
        promptBuilder.AppendLine("  \"begruendung\": \"Der Vertrag erwähnt sowohl Verkaufstätigkeiten als auch Serviceleistungen. Sowohl 'kollektivvertrag_handel' als auch 'kollektivvertrag_hotel_und_gastgewerbe' aus der Liste könnten passen. Eine eindeutige Zuordnung ist nicht möglich.\"");
        promptBuilder.AppendLine("}");
        promptBuilder.AppendLine("```");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("**Beispiel 3 - Kein passender KV:**");
        promptBuilder.AppendLine("```json");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"kollektivvertrag\": \"Keiner\",");
        promptBuilder.AppendLine("  \"begruendung\": \"Der Vertrag bezieht sich auf Software-Entwicklung im IT-Bereich. Ich habe die gesamte Liste durchsucht, aber keiner der verfügbaren KVs deckt den IT-Bereich ab.\"");
        promptBuilder.AppendLine("}");
        promptBuilder.AppendLine("```");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## KRITISCHE REGELN:");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("1. ✅ **VERWENDE NUR NAMEN AUS DER OBIGEN LISTE!**");
        promptBuilder.AppendLine("2. ✅ **Kopiere den Namen EXAKT** (mit Unterstrichen, Kleinbuchstaben, etc.)");
        promptBuilder.AppendLine("3. ✅ **Wenn KEIN Name passt**: Schreibe \"Keiner\" - NICHT einen ähnlichen Namen!");
        promptBuilder.AppendLine("4. ✅ **Wenn MEHRERE passen**: Schreibe \"Multiple\" und nenne die konkreten Namen in der Begründung");
        promptBuilder.AppendLine("5. ✅ **Begründung muss konkret sein**: Erwähne die Branche/Position aus dem Vertrag UND den gewählten KV-Namen");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## HÄUFIGE FEHLER (VERMEIDE DIESE!):");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("❌ **FALSCH**: \"kollektivvertrag_metallindustrie\" (wenn dieser Name NICHT in der Liste steht)");
        promptBuilder.AppendLine("❌ **FALSCH**: \"KV Handel\" (verwende den exakten Namen aus der Liste)");
        promptBuilder.AppendLine("❌ **FALSCH**: \"Kollektivvertrag_Handel\" (Groß-/Kleinschreibung beachten!)");
        promptBuilder.AppendLine("❌ **FALSCH**: Einen Namen erfinden, der logisch klingt, aber nicht in der Liste ist");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("✅ **RICHTIG**: \"kollektivvertrag_handel\" (exakt wie in der Liste)");
        promptBuilder.AppendLine("✅ **RICHTIG**: \"Keiner\" (wenn kein passender KV in der Liste ist)");
        promptBuilder.AppendLine("✅ **RICHTIG**: \"Multiple\" (wenn mehrere aus der Liste passen)");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("## VORGEHEN:");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("1. Lies den Vertrag und identifiziere Branche/Position");
        promptBuilder.AppendLine("2. Durchsuche die KV-Liste nach passenden Schlüsselwörtern");
        promptBuilder.AppendLine("3. Wähle den passendsten KV aus der Liste (oder \"Multiple\"/\"Keiner\")");
        promptBuilder.AppendLine("4. Kopiere den Namen EXAKT aus der Liste");
        promptBuilder.AppendLine("5. Schreibe eine konkrete Begründung");
        return promptBuilder.ToString();
    }
}
