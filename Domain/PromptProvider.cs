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
}
