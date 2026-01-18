using PocketLawyer.Application.Exceptions;

namespace PocketLawyer.Domain;

/// <summary>
/// Central provider for all AI prompts used in contract analysis.
/// Organizes prompts by contract type, tier (Basic/Premium), and analysis step.
/// </summary>
public static class PromptProvider
{
    #region System Prompts

    private const string SystemPrompt = @"Du bist ein Experte für Vertragsanalyse mit umfassender Kenntnis des österreichischen Rechts.
Analysiere den Vertrag präzise und strukturiert.
Antworte ausschließlich im angegebenen JSON-Format.
Stelle sicher, dass alle erforderlichen Felder ausgefüllt sind.
Verwende klare, verständliche Sprache für Nicht-Juristen.";

    private const string ReadabilityCheckPrompt = @"Prüfe, ob der folgende Text lesbar und verarbeitbar ist.

Bewerte:
- Ist der Text lesbar und enthält er sinnvollen Inhalt?
- Ist der Text nicht leer oder nur aus Sonderzeichen?
- Ist der Text nicht korrupt oder unleserlich?

Antworte mit einem JSON-Objekt mit einem einzigen Feld 'isReadable' (boolean).
Setze 'isReadable' auf true, wenn der Text lesbar ist, sonst false.";

    private const string ContractTypeDetectionPrompt = @"Erkenne die Art des vorliegenden Vertrags.

Analysiere den Vertragstext und bestimme, um welche Art von Vertrag es sich handelt.

Mögliche Vertragstypen:
- Mietvertrag (für Wohnraum oder Gewerbe)
- Arbeitsvertrag (Anstellungsvertrag, Dienstvertrag)
- Kaufvertrag
- Sonstiger Vertrag

Antworte mit einem JSON-Objekt mit einem einzigen Feld 'contractType' (string).
Verwende genau einen der folgenden Werte:
- 'Mietvertrag'
- 'Arbeitsvertrag'
- 'Kaufvertrag'
- 'Sonstiger'";

    #endregion

    #region Rental Contract Prompts

    #region Rental Basic Analysis Prompts

    private const string RentalBasicGeneralPrompt = @"Analysiere die allgemeinen Vertragsdaten des Mietvertrags.

Extrahiere folgende Informationen:
- Vertragstyp (z.B. Vollanwendungsbereich I, II, III, WG, etc.)
- Name des/der Mieter:in
- Name des/der Vermieter:in
- Adresse des Mietobjekts
- Vertragsbeginn (Datum)
- Vertragsart (befristet oder unbefristet)

Achte auf:
- Korrekte Identifikation des Vertragstyps nach MRG
- Vollständige Namen und Adressen
- Präzise Datumsangaben";

    private const string RentalBasicRentPrompt = @"Analysiere die Mietkonditionen des Mietvertrags.

Extrahiere folgende Informationen:
- Kaltmiete (Nettomiete in EUR)
- Nebenkosten (Betriebskosten in EUR)
- Warmmiete (Gesamtmiete in EUR)
- Kaution (Höhe in EUR)
- Zahlungsweise (monatlich, vierteljährlich, etc.)
- Nebenkostenregelung (Pauschale, Vorauszahlung mit Abrechnung, etc.)

Achte auf:
- Korrekte Berechnung der Warmmiete (Kaltmiete + Nebenkosten)
- Zulässige Kautionshöhe (max. 3 Monatsmieten)
- Klare Unterscheidung zwischen Miete und Nebenkosten";

    private const string RentalBasicConditionsPrompt = @"Analysiere die Vertragsbedingungen des Mietvertrags.

Extrahiere folgende Informationen:
- Kündigungsfrist (für Mieter und Vermieter)
- Tierhaltung (erlaubt, verboten, mit Zustimmung)
- Untervermietung (erlaubt, verboten, mit Zustimmung)
- Besonderheiten (z.B. Möblierung, Garten, Parkplatz, etc.)

Achte auf:
- Gesetzliche vs. vertragliche Kündigungsfristen
- Einschränkungen bei Tierhaltung und Untervermietung
- Zusätzliche Rechte und Pflichten";

    private const string RentalBasicSummaryPrompt = @"Erstelle eine Zusammenfassung der Basic-Analyse des Mietvertrags.

Erstelle:
- Kurzfazit (2-3 Sätze): Die wichtigsten Eckdaten des Vertrags
- Gesamtfazit (1 Absatz): Überblick über die wesentlichen Vertragsinhalte
- Gesamtbewertung (Text): Erste Einschätzung der Vertragsqualität
- RiskLevel (Gruen/Gelb/Rot): Erste Risikobewertung basierend auf den Grunddaten

Achte auf:
- Verständliche Sprache für Laien
- Hervorhebung wichtiger Punkte
- Objektive Bewertung";

    #endregion

    #region Rental Premium Analysis Prompts

    private const string RentalPremiumCostsPrompt = @"Analysiere die Mietzahlungs- und Nebenkostenklauseln des Mietvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Zulässigkeit der Miethöhe (Richtwertmiete, Kategoriemiete, freie Miete)
- Nebenkostenregelung (Pauschale vs. Abrechnung)
- Wertsicherungsklauseln (VPI-Anpassung)
- Kautionsregelung (Höhe, Verzinsung, Rückgabe)
- Zahlungsmodalitäten

Erstelle:
- Zusammenfassung: Überblick über die Kostenregelungen
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (MRG, ABGB)
- RisikoScore: Zulaessig/Nachteilhaft/Unzulaessig
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Einhaltung der gesetzlichen Mietobergrenzen
- Zulässigkeit von Nebenkostenpauschalen
- Rechtmäßigkeit der Kaution";

    private const string RentalPremiumTermsPrompt = @"Analysiere die Mietdauer- und Kündigungsklauseln des Mietvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Befristung (zulässig, Mindestdauer)
- Kündigungsfristen (gesetzlich vs. vertraglich)
- Kündigungstermine (gesetzliche Termine)
- Kündigungsverzicht
- Verlängerungsoptionen

Erstelle:
- Zusammenfassung: Überblick über Laufzeit und Kündigungsregelungen
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (MRG, ABGB)
- RisikoScore: Zulaessig/Nachteilhaft/Unzulaessig
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Zulässigkeit von Befristungen
- Einhaltung gesetzlicher Kündigungsfristen
- Unwirksame Kündigungsverzichte";

    private const string RentalPremiumUsePrompt = @"Analysiere die Klauseln zum Gebrauch der Mietsache rechtlich detailliert.

Bewerte folgende Aspekte:
- Nutzungseinschränkungen
- Tierhaltung
- Untervermietung
- Bauliche Veränderungen
- Gewerbliche Nutzung

Erstelle:
- Zusammenfassung: Überblick über Nutzungsrechte und -pflichten
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (MRG, ABGB)
- RisikoScore: Zulaessig/Nachteilhaft/Unzulaessig
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Unzulässige Einschränkungen der Nutzung
- Rechte bei Tierhaltung und Untervermietung
- Zustimmungserfordernisse";

    private const string RentalPremiumMaintenancePrompt = @"Analysiere die Instandhaltungs- und Reparaturklauseln des Mietvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Erhaltungspflichten (Vermieter vs. Mieter)
- Reparaturpflichten
- Schönheitsreparaturen
- Kostenverteilung
- Mängelbeseitigung

Erstelle:
- Zusammenfassung: Überblick über Instandhaltungspflichten
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (MRG, ABGB)
- RisikoScore: Zulaessig/Nachteilhaft/Unzulaessig
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Unzulässige Überwälzung von Erhaltungspflichten
- Rechtmäßigkeit von Schönheitsreparaturklauseln
- Mängelrechte des Mieters";

    private const string RentalPremiumDutiesPrompt = @"Analysiere die Rechte- und Pflichtenklauseln des Mietvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Besichtigungsrechte
- Hausordnung
- Versicherungspflichten
- Haftungsregelungen
- Sonstige Pflichten

Erstelle:
- Zusammenfassung: Überblick über Rechte und Pflichten
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (MRG, ABGB)
- RisikoScore: Zulaessig/Nachteilhaft/Unzulaessig
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Zulässigkeit von Besichtigungen
- Angemessenheit der Hausordnung
- Rechtmäßigkeit von Haftungsausschlüssen";

    private const string RentalPremiumPrivacyPrompt = @"Analysiere die Datenschutz- und Kommunikationsklauseln des Mietvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Datenschutzbestimmungen (DSGVO)
- Kommunikationswege
- Dokumentationspflichten
- Informationspflichten
- Einwilligungen

Erstelle:
- Zusammenfassung: Überblick über Datenschutz und Kommunikation
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (DSGVO, MRG)
- RisikoScore: Zulaessig/Nachteilhaft/Unzulaessig
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- DSGVO-Konformität
- Rechtmäßigkeit von Einwilligungen
- Informationspflichten";

    private const string RentalPremiumSummaryPrompt = @"Erstelle eine Gesamtzusammenfassung der Premium-Analyse des Mietvertrags.

Erstelle:
- Gesamtfazit (2-3 Absätze): Zusammenfassung aller rechtlichen Bewertungen
- Gesamtbewertung (Gruen/Gelb/Rot): Gesamtrisikobewertung des Vertrags

Berücksichtige:
- Alle analysierten Sektionen (Kosten, Laufzeit, Nutzung, Instandhaltung, Rechte, Datenschutz)
- Schwerwiegende vs. geringfügige Probleme
- Gesamtrisiko für den Mieter

Achte auf:
- Klare, verständliche Zusammenfassung
- Priorisierung der wichtigsten Punkte
- Handlungsempfehlungen";

    #endregion

    #endregion

    #region Employment Contract Prompts

    // TODO: Employment contract prompts will be added when EmploymentContractAgent is fully implemented
    // For now, we provide placeholder prompts to support the skeleton implementation

    #region Employment Basic Analysis Prompts

    private const string EmploymentBasicGeneralPrompt = @"Analysiere die allgemeinen Vertragsdaten des Arbeitsvertrags.

Extrahiere folgende Informationen:
- Arbeitgeber (Name und ggf. Adresse)
- Arbeitnehmer (Name)
- Position/Tätigkeit
- Startdatum (Vertragsbeginn)
- Vertragsart (befristet oder unbefristet)
- Probezeit (Dauer)
- Vertragsende (falls befristet)

Achte auf:
- Vollständige Namen
- Präzise Tätigkeitsbeschreibung
- Zulässigkeit der Befristung
- Korrekte Datumsangaben";

    private const string EmploymentBasicCompensationPrompt = @"Analysiere die Vergütungsregelungen des Arbeitsvertrags.

Extrahiere folgende Informationen:
- Gehalt Brutto (Betrag und Währung)
- Gehalt Netto ca. (geschätzter Nettobetrag)
- Zahlungsweise (monatlich, wöchentlich, etc.)
- Sonderzahlungen (13./14. Gehalt, Boni, etc.)
- Zuschläge (Überstunden, Nacht, Wochenende, etc.)
- Zusatzleistungen (Firmenwagen, Handy, Essensgutscheine, etc.)

Achte auf:
- Einhaltung kollektivvertraglicher Mindestlöhne
- Klare Angabe der Vergütungsbestandteile
- Regelungen zu Sonderzahlungen";

    private const string EmploymentBasicWorkingConditionsPrompt = @"Analysiere die Arbeitsbedingungen des Arbeitsvertrags.

Extrahiere folgende Informationen:
- Wochenarbeitszeit (Stunden pro Woche)
- Arbeitszeitmodell (Vollzeit, Teilzeit, Gleitzeit, etc.)
- Überstundenregelung (Abgeltung, Zeitausgleich, etc.)
- Urlaubstage (Anzahl pro Jahr)
- Arbeitsort (Adresse, Standort)
- Remote-Arbeit (Homeoffice-Regelung)

Achte auf:
- Einhaltung gesetzlicher Arbeitszeitgrenzen
- Urlaubsanspruch nach Gesetz/Kollektivvertrag
- Klare Regelungen zu Überstunden
- Flexibilität bei Arbeitsort";

    private const string EmploymentBasicSummaryPrompt = @"Erstelle eine Zusammenfassung der Basic-Analyse des Arbeitsvertrags.

Erstelle:
- Kurzfazit (2-3 Sätze): Die wichtigsten Eckdaten des Vertrags
- Kollektivvertrag (Name des erkannten KV oder 'Kein Kollektivvertrag erkannt')
- Gesamtbewertung (Text): Erste Einschätzung der Vertragsqualität
- RiskLevel (Gruen/Gelb/Rot): Erste Risikobewertung basierend auf den Grunddaten

Achte auf:
- Verständliche Sprache für Laien
- Hervorhebung wichtiger Punkte (Gehalt, Arbeitszeit, Befristung)
- Erwähnung des Kollektivvertrags falls erkannt
- Objektive Bewertung";

    #endregion

    #region Employment Premium Analysis Prompts

    private const string EmploymentPremiumCompensationBenefitsPrompt = @"Analysiere die Vergütungs- und Zusatzleistungsklauseln des Arbeitsvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Einhaltung des Kollektivvertrags (Mindestgehalt, Einstufung)
- Sonderzahlungen (13./14. Gehalt, Boni, Prämien)
- Überstundenvergütung (Zuschläge, Abgeltung)
- Zusatzleistungen (Firmenwagen, Handy, Essensgutscheine, etc.)
- Gehaltsanpassungen (Valorisierung, Leistungssteigerungen)

Erstelle:
- Zusammenfassung: Überblick über die Vergütungsregelungen
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (ArbVG, Kollektivvertrag)
- RisikoScore: Vorteilhaft/Neutral/Nachteilhaft
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Einhaltung kollektivvertraglicher Mindeststandards
- Zulässigkeit von Gehaltsklauseln
- Transparenz der Vergütungsbestandteile";

    private const string EmploymentPremiumWorkingTimeVacationPrompt = @"Analysiere die Arbeitszeit- und Urlaubsklauseln des Arbeitsvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Arbeitszeitregelungen (Normalarbeitszeit, Höchstarbeitszeit)
- Überstundenregelungen (Anordnung, Abgeltung, Zeitausgleich)
- Urlaubsanspruch (gesetzlich, kollektivvertraglich, vertraglich)
- Urlaubsverbrauch (Zeitpunkt, Verfallsregelungen)
- Arbeitszeitmodelle (Gleitzeit, Schichtarbeit, etc.)

Erstelle:
- Zusammenfassung: Überblick über Arbeitszeit- und Urlaubsregelungen
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (AZG, UrlG, Kollektivvertrag)
- RisikoScore: Vorteilhaft/Neutral/Nachteilhaft
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Einhaltung gesetzlicher Arbeitszeitgrenzen
- Zulässigkeit von Überstundenregelungen
- Urlaubsanspruch nach Gesetz und Kollektivvertrag";

    private const string EmploymentPremiumTerminationNoticePrompt = @"Analysiere die Kündigungs- und Kündigungsfristklauseln des Arbeitsvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Kündigungsfristen (gesetzlich, kollektivvertraglich, vertraglich)
- Kündigungstermine (Monatsletzter, Quartalsletzter, etc.)
- Probezeit (Dauer, Kündigungsmöglichkeiten)
- Kündigungsgründe (ordentlich, außerordentlich)
- Abfindungsregelungen

Erstelle:
- Zusammenfassung: Überblick über Kündigungsregelungen
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (AngG, Kollektivvertrag)
- RisikoScore: Vorteilhaft/Neutral/Nachteilhaft
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Einhaltung gesetzlicher Kündigungsfristen
- Zulässigkeit von Probezeitregelungen
- Kündigungsschutzbestimmungen";

    private const string EmploymentPremiumNonCompeteConfidentialityPrompt = @"Analysiere die Wettbewerbs- und Vertraulichkeitsklauseln des Arbeitsvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Konkurrenzklauseln (während und nach dem Dienstverhältnis)
- Vertraulichkeitsvereinbarungen (Geschäftsgeheimnisse, Datenschutz)
- Geheimhaltungspflichten
- Karenzentschädigungen (bei Konkurrenzklauseln)
- Vertragsstrafen

Erstelle:
- Zusammenfassung: Überblick über Wettbewerbs- und Vertraulichkeitsregelungen
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (AngG, UWG, DSGVO)
- RisikoScore: Vorteilhaft/Neutral/Nachteilhaft
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Zulässigkeit von Konkurrenzklauseln
- Angemessenheit von Karenzentschädigungen
- Verhältnismäßigkeit von Geheimhaltungspflichten";

    private const string EmploymentPremiumRightsDutiesPrompt = @"Analysiere die Rechte- und Pflichtenklauseln des Arbeitsvertrags rechtlich detailliert.

Bewerte folgende Aspekte:
- Arbeitspflichten (Tätigkeitsbereich, Weisungsrecht)
- Treuepflichten (Loyalität, Interessenkonflikte)
- Nebentätigkeiten (Zustimmungspflicht, Verbote)
- Versetzungsklauseln (örtlich, fachlich)
- Haftungsregelungen

Erstelle:
- Zusammenfassung: Überblick über Rechte und Pflichten
- Bewertung: Rechtliche Einschätzung der Klauseln
- Rechtsgrundlage: Relevante Gesetzesbestimmungen (ABGB, AngG)
- RisikoScore: Vorteilhaft/Neutral/Nachteilhaft
- RiskLevel: Gruen/Gelb/Rot

Achte auf:
- Angemessenheit von Versetzungsklauseln
- Zulässigkeit von Nebentätigkeitsverboten
- Rechtmäßigkeit von Haftungsausschlüssen";

    private const string EmploymentPremiumSummaryPrompt = @"Erstelle eine Gesamtzusammenfassung der Premium-Analyse des Arbeitsvertrags.

Erstelle:
- Gesamtfazit (2-3 Absätze): Zusammenfassung aller rechtlichen Bewertungen
- Gesamtbewertung (Text): Gesamteinschätzung des Vertrags
- Handlungsempfehlungen (Array): Konkrete Empfehlungen für den Arbeitnehmer

Berücksichtige:
- Alle analysierten Sektionen (Vergütung, Arbeitszeit, Kündigung, Wettbewerb, Rechte)
- Schwerwiegende vs. geringfügige Probleme
- Gesamtrisiko für den Arbeitnehmer
- Kollektivvertragliche Bestimmungen

Achte auf:
- Klare, verständliche Zusammenfassung
- Priorisierung der wichtigsten Punkte
- Konkrete, umsetzbare Handlungsempfehlungen";

    #endregion

    #endregion

    #region Kollektivvertrag Detection Prompts

    private const string KvDetectionPrompt = @"# AUFGABE: Kollektivvertragserkennung

## SCHRITT 1: Analysiere den Arbeitsvertrag

Identifiziere aus dem Vertrag:
1. **Branche/Sektor**: Welche Branche wird erwähnt? (z.B. Handel, Gastronomie, Gesundheit, Verkehr, etc.)
2. **Position/Tätigkeit**: Was ist die genaue Berufsbezeichnung?
3. **Arbeitgeber**: Welche Art von Unternehmen/Organisation?
4. **Explizite KV-Erwähnung**: Wird ein Kollektivvertrag im Vertrag erwähnt?

## SCHRITT 2: Verfügbare Kollektivverträge

=== VOLLSTÄNDIGE LISTE (NUR DIESE NAMEN VERWENDEN!) ===
{KV_LIST}
=== ENDE DER LISTE ===

## SCHRITT 3: Matching

Suche in der obigen Liste nach einem KV, der zur identifizierten Branche/Position passt.

**WICHTIGE HINWEISE:**
- Die KV-Namen enthalten oft Schlüsselwörter zur Branche (z.B. 'handel', 'hotel', 'pharmazie')
- Achte auf Teilstrings: 'kollektivvertrag_hotel_und_gastgewerbe' passt zu Gastronomie/Hotel
- Bei Unsicherheit: Prüfe mehrere mögliche KVs aus der Liste

## SCHRITT 4: Antwort

Antworte im JSON-Format:
```json
{
  ""kollektivvertrag"": ""EXAKTER_NAME_AUS_DER_LISTE"" ODER ""Multiple"" ODER ""Keiner"",
  ""begruendung"": ""Deine Begründung in 2-3 Sätzen""
}
```

### Beispiel-Antworten:

**Beispiel 1 - Eindeutige Zuordnung:**
```json
{
  ""kollektivvertrag"": ""kollektivvertrag_handel"",
  ""begruendung"": ""Der Vertrag erwähnt explizit 'Einzelhandel' und die Position ist 'Verkäufer'. Der KV 'kollektivvertrag_handel' aus der Liste passt eindeutig zu dieser Branche.""
}
```

**Beispiel 2 - Mehrere mögliche KVs:**
```json
{
  ""kollektivvertrag"": ""Multiple"",
  ""begruendung"": ""Der Vertrag erwähnt sowohl Verkaufstätigkeiten als auch Serviceleistungen. Sowohl 'kollektivvertrag_handel' als auch 'kollektivvertrag_hotel_und_gastgewerbe' aus der Liste könnten passen. Eine eindeutige Zuordnung ist nicht möglich.""
}
```

**Beispiel 3 - Kein passender KV:**
```json
{
  ""kollektivvertrag"": ""Keiner"",
  ""begruendung"": ""Der Vertrag bezieht sich auf Software-Entwicklung im IT-Bereich. Ich habe die gesamte Liste durchsucht, aber keiner der verfügbaren KVs deckt den IT-Bereich ab.""
}
```

## KRITISCHE REGELN:

1. ✅ **VERWENDE NUR NAMEN AUS DER OBIGEN LISTE!**
2. ✅ **Kopiere den Namen EXAKT** (mit Unterstrichen, Kleinbuchstaben, etc.)
3. ✅ **Wenn KEIN Name passt**: Schreibe ""Keiner"" - NICHT einen ähnlichen Namen!
4. ✅ **Wenn MEHRERE passen**: Schreibe ""Multiple"" und nenne die konkreten Namen in der Begründung
5. ✅ **Begründung muss konkret sein**: Erwähne die Branche/Position aus dem Vertrag UND den gewählten KV-Namen

## HÄUFIGE FEHLER (VERMEIDE DIESE!):

❌ **FALSCH**: ""kollektivvertrag_metallindustrie"" (wenn dieser Name NICHT in der Liste steht)
❌ **FALSCH**: ""KV Handel"" (verwende den exakten Namen aus der Liste)
❌ **FALSCH**: ""Kollektivvertrag_Handel"" (Groß-/Kleinschreibung beachten!)
❌ **FALSCH**: Einen Namen erfinden, der logisch klingt, aber nicht in der Liste ist

✅ **RICHTIG**: ""kollektivvertrag_handel"" (exakt wie in der Liste)
✅ **RICHTIG**: ""Keiner"" (wenn kein passender KV in der Liste ist)
✅ **RICHTIG**: ""Multiple"" (wenn mehrere aus der Liste passen)

## VORGEHEN:

1. Lies den Vertrag und identifiziere Branche/Position
2. Durchsuche die KV-Liste nach passenden Schlüsselwörtern
3. Wähle den passendsten KV aus der Liste (oder ""Multiple""/""Keiner"")
4. Kopiere den Namen EXAKT aus der Liste
5. Schreibe eine konkrete Begründung";

    #endregion

    /// <summary>
    /// Retrieves the appropriate prompt for a given contract type, tier, and analysis step.
    /// </summary>
    /// <param name="contractType">The type of contract (e.g., "Rental", "Employment")</param>
    /// <param name="tier">The analysis tier ("Basic" or "Premium")</param>
    /// <param name="step">The specific analysis step (e.g., "General", "Rent", "Costs")</param>
    /// <returns>The complete prompt including system prompt and step-specific prompt</returns>
    /// <exception cref="PromptNotFoundException">Thrown when no prompt exists for the given combination</exception>
    public static string GetPrompt(string contractType, string tier, string step)
    {
        string key = $"{contractType}{tier}{step}";

        string stepPrompt = key switch
        {
            // Rental Basic
            "RentalBasicGeneral" => RentalBasicGeneralPrompt,
            "RentalBasicRent" => RentalBasicRentPrompt,
            "RentalBasicConditions" => RentalBasicConditionsPrompt,
            "RentalBasicSummary" => RentalBasicSummaryPrompt,

            // Rental Premium
            "RentalPremiumCosts" => RentalPremiumCostsPrompt,
            "RentalPremiumTerms" => RentalPremiumTermsPrompt,
            "RentalPremiumUse" => RentalPremiumUsePrompt,
            "RentalPremiumMaintenance" => RentalPremiumMaintenancePrompt,
            "RentalPremiumDuties" => RentalPremiumDutiesPrompt,
            "RentalPremiumPrivacy" => RentalPremiumPrivacyPrompt,
            "RentalPremiumSummary" => RentalPremiumSummaryPrompt,

            // Employment Basic
            "EmploymentBasicGeneral" => EmploymentBasicGeneralPrompt,
            "EmploymentBasicCompensation" => EmploymentBasicCompensationPrompt,
            "EmploymentBasicWorkingConditions" => EmploymentBasicWorkingConditionsPrompt,
            "EmploymentBasicSummary" => EmploymentBasicSummaryPrompt,

            // Employment Premium
            "EmploymentPremiumCompensationBenefits" => EmploymentPremiumCompensationBenefitsPrompt,
            "EmploymentPremiumWorkingTimeVacation" => EmploymentPremiumWorkingTimeVacationPrompt,
            "EmploymentPremiumTerminationNotice" => EmploymentPremiumTerminationNoticePrompt,
            "EmploymentPremiumNonCompeteConfidentiality" => EmploymentPremiumNonCompeteConfidentialityPrompt,
            "EmploymentPremiumRightsDuties" => EmploymentPremiumRightsDutiesPrompt,
            "EmploymentPremiumSummary" => EmploymentPremiumSummaryPrompt,

            _ => throw new PromptNotFoundException(key)
        };

        return SystemPrompt + "\n\n" + stepPrompt;
    }

    /// <summary>
    /// Retrieves the prompt for checking contract readability.
    /// </summary>
    /// <returns>The readability check prompt</returns>
    public static string GetReadabilityCheckPrompt()
    {
        return SystemPrompt + "\n\n" + ReadabilityCheckPrompt;
    }

    /// <summary>
    /// Retrieves the prompt for detecting contract type.
    /// </summary>
    /// <returns>The contract type detection prompt</returns>
    public static string GetContractTypeDetectionPrompt()
    {
        return SystemPrompt + "\n\n" + ContractTypeDetectionPrompt;
    }

    /// <summary>
    /// Retrieves the prompt for detecting which Kollektivvertrag (collective agreement) applies to an employment contract.
    /// </summary>
    /// <param name="contractContent">The content of the employment contract</param>
    /// <param name="availableKvs">Array of available Kollektivvertrag directory names</param>
    /// <returns>The complete KV detection prompt with contract content and available KVs</returns>
    public static string GetKvDetectionPrompt(string contractContent, string[] availableKvs)
    {
        string kvList = string.Join(", ", availableKvs);
        string promptWithKvList = KvDetectionPrompt.Replace("{KV_LIST}", kvList);
        
        return $@"Arbeitsvertrag:
{contractContent}

{promptWithKvList}";
    }

    /// <summary>
    /// Adds Kollektivvertrag information to an existing prompt.
    /// This method appends KV files and instructions to analyze the contract based on the collective agreement.
    /// </summary>
    /// <param name="basePrompt">The base prompt to extend</param>
    /// <param name="kvName">The name of the Kollektivvertrag</param>
    /// <param name="kvFiles">The content of the KV files</param>
    /// <returns>The extended prompt with KV information</returns>
    public static string AddKvInfo(string basePrompt, string kvName, IEnumerable<string> kvFiles)
    {
        var kvContent = string.Join("\n\n", kvFiles);
        
        return $@"{basePrompt}

Der folgende Text enthält den dazugehörigen Kollektivvertrag ""{kvName}"" und mögliche Anhänge.
Diese Informationen sind laut Gesetz zwingend Grundlage der Vertragsanalyse.
Wenn der Vertrag in einem Punkt so etwas schreibt wie ""laut Kollektivvertrag"" soll die entsprechende Regelung aus dem Kollektivvertrag angegeben werden mit dem Zusatz ""(lt. Kollektivvertrag)"".

{kvContent}";
    }
}
