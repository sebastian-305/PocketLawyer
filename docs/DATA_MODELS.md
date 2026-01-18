# Datenmodelle

[← Zurück zur Hauptdokumentation](../README.md)

Dieses Dokument beschreibt alle Data Transfer Objects (DTOs) im PocketLawyer-System. Alle DTOs befinden sich im `Domain/Dtos/` Verzeichnis und folgen dem Domain-Driven Design Prinzip.

## Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [Shared DTOs](#shared-dtos)
- [Employment DTOs](#employment-dtos)
- [Rental DTOs](#rental-dtos)
- [Enumerationen](#enumerationen)

---

## Übersicht

Das PocketLawyer-System verwendet DTOs zur Datenübertragung zwischen den verschiedenen Schichten der Anwendung. Die DTOs sind in drei Hauptkategorien organisiert:

- **Shared DTOs** (`Domain/Dtos/Shared/`) - Gemeinsame DTOs für alle Vertragstypen
- **Employment DTOs** (`Domain/Dtos/Employment/`) - Spezifische DTOs für Arbeitsverträge
- **Rental DTOs** (`Domain/Dtos/Rental/`) - Spezifische DTOs für Mietverträge

---

## Shared DTOs

Gemeinsame DTOs, die von allen Vertragstypen verwendet werden.

### AnalysisDto<TResult>

**Quelle:** `Domain/Dtos/Shared/AnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Generic Record

**Beschreibung:** Generischer Wrapper für Analyseergebnisse, der das Ergebnis und Metadaten enthält.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Result | TResult | Das Analyseergebnis (muss IAnalysisDto implementieren) | Erforderlich |
| Metadata | AnalysisMetadataDto | Metadaten zur Analyse | Erforderlich |

---

### AnalysisMetadataDto

**Quelle:** `Domain/Dtos/Shared/AnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Record

**Beschreibung:** Metadaten für Analyseergebnisse.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| ContractId | string | Eindeutige ID des analysierten Vertrags | Erforderlich |
| CountryCode | string | Ländercode (z.B. "AT" für Österreich) | Erforderlich |
| Language | string | Sprache der Analyse (z.B. "de") | Erforderlich |
| AnalyzedAt | DateTimeOffset | Zeitpunkt der Analyse | Erforderlich |
| IsPreview | bool | Gibt an, ob es sich um eine Vorschau handelt | Erforderlich |
| ModelVersion | string | Version des verwendeten AI-Modells | Erforderlich |

---

### IAnalysisDto

**Quelle:** `Domain/Dtos/Shared/IAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Interface

**Beschreibung:** Basis-Interface für Analyse-DTOs.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| ContractType | string | Vertragstyp der Analyse | Erforderlich |
| Tier | string | Analyse-Tier (z. B. Basic oder Premium) | Erforderlich |

---

### ContractValidationDto

**Quelle:** `Domain/Dtos/Shared/ContractValidationDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Class

**Beschreibung:** DTO für Vertragvalidierungsergebnisse.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| IsReadable | bool | Gibt an, ob der Vertrag lesbar ist | Erforderlich |
| IsContract | bool | Gibt an, ob es sich um einen Vertrag handelt | Erforderlich |
| Result | string | Validierungsergebnis als Text | Erforderlich |
| Kollektivvertrag | string? | Erkannter Kollektivvertrag (falls vorhanden) | Optional |

---

### IValidationDto

**Quelle:** `Domain/Dtos/Shared/IValidationDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Interface

**Beschreibung:** Marker-Interface für Validierungs-DTOs.

---

### FileContentDto

**Quelle:** `Domain/Dtos/Shared/FileContentDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Record

**Beschreibung:** DTO für Dateiinhalte.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| FileName | string | Name der Datei | Erforderlich |
| FileContent | string | Inhalt der Datei als Text | Erforderlich |

---

### WebSocketFileDto

**Quelle:** `Domain/Dtos/Shared/WebSocketFileDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Class

**Beschreibung:** DTO für den Empfang von Dateidaten über WebSocket. Der ContractType ist optional, da der Endpunkt bestimmt, welcher Agent verwendet wird.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| FileName | string | Name der hochgeladenen Datei | Erforderlich |
| Data | string | Base64-kodierte Dateidaten | Erforderlich |
| ContractType | string? | Optionaler Vertragstyp für Validierungszwecke | Optional |

---

### KvDetectionAnswer

**Quelle:** `Domain/Dtos/Shared/KvDetectionAnswer.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Class

**Beschreibung:** DTO für eine einzelne Kollektivvertrag-Erkennungsantwort.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Kollektivvertrag | string | Name des erkannten Kollektivvertrags | Erforderlich |
| Begruendung | string | Begründung für die Erkennung | Erforderlich |

---

### KvDetectionResult

**Quelle:** `Domain/Dtos/Shared/KvDetectionResult.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Class

**Beschreibung:** DTO für das Gesamtergebnis der Kollektivvertrag-Erkennung.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| KvName | string | Endgültig erkannter Kollektivvertrag-Name | Erforderlich |
| AllEqual | bool | Gibt an, ob alle Erkennungsversuche zum gleichen Ergebnis führten | Erforderlich |
| DetectedKvs | string[] | Liste aller erkannten Kollektivverträge aus verschiedenen Versuchen | Erforderlich |
| Begruendungen | string[] | Liste aller Begründungen | Erforderlich |
| Message | string | Zusammenfassende Nachricht über das Erkennungsergebnis | Erforderlich |

---

## Employment DTOs

DTOs für die Analyse von Arbeitsverträgen.

### EmploymentBasicAnalysisDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO für Basis-Analyseergebnisse von Arbeitsverträgen. Enthält alle Basis-Analysesektionen: General, Compensation, WorkingConditions und Summary.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| General | EmploymentBasicGeneralDto | Allgemeine Arbeitsdaten | Erforderlich |
| Compensation | EmploymentBasicCompensationDto | Vergütung und Zusatzleistungen | Erforderlich |
| WorkingConditions | EmploymentBasicWorkingConditionsDto | Arbeitsbedingungen | Erforderlich |
| Summary | EmploymentBasicSummaryDto | Zusammenfassung | Erforderlich |
| ContractType | string | "Arbeitsvertrag" (nur lesbar) | Erforderlich |
| Tier | string | "Basic" (nur lesbar) | Erforderlich |

---

### EmploymentBasicGeneralDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO für allgemeine Arbeitsdaten.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Arbeitgeber | string | Name des Arbeitgebers | Erforderlich |
| Arbeitnehmer | string | Name des/der Arbeitnehmer:in | Erforderlich |
| Position | string | Bezeichnung der Position/Tätigkeit | Erforderlich |
| Startdatum | string | Datum des Arbeitsbeginns (Format: YYYY-MM-DD) | Erforderlich |
| Vertragsart | string | Art des Vertrags (befristet/unbefristet) | Erforderlich |
| Probezeit | string | Dauer der Probezeit | Erforderlich |
| Vertragsende | string? | Datum des Vertragsendes (nur bei befristeten Verträgen) | Optional |

---

### EmploymentBasicCompensationDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO für Vergütung und Zusatzleistungen.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| GehaltBrutto | string | Bruttogehalt | Erforderlich |
| GehaltNettoCa | string | Geschätztes Nettogehalt | Erforderlich |
| Zahlungsweise | string | Zahlungsweise des Gehalts | Erforderlich |
| Sonderzahlungen | string | Sonderzahlungen (13./14. Gehalt, etc.) | Erforderlich |
| Zuschlaege | string | Zuschläge für Überstunden, Nachtarbeit, etc. | Erforderlich |
| Zusatzleistungen | string[] | Zusatzleistungen (Firmenwagen, Handy, etc.) | Erforderlich |

---

### EmploymentBasicWorkingConditionsDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO für Arbeitsbedingungen.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Wochenarbeitszeit | string | Wöchentliche Arbeitszeit | Erforderlich |
| Arbeitszeitmodell | string | Arbeitszeitmodell (Vollzeit, Teilzeit, Gleitzeit) | Erforderlich |
| Ueberstundenregelung | string | Regelung zu Überstunden | Erforderlich |
| Urlaubstage | string | Anzahl der Urlaubstage pro Jahr | Erforderlich |
| Arbeitsort | string | Arbeitsort/Standort | Erforderlich |
| RemoteArbeit | string | Regelung zu Remote-Arbeit/Homeoffice | Erforderlich |

---

### EmploymentBasicSummaryDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO für die Basis-Analyse-Zusammenfassung mit KV-Erwähnung.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Kurzfazit | string | Kurze Zusammenfassung (2-3 Sätze) | Erforderlich |
| Kollektivvertrag | string | Erkannter Kollektivvertrag oder "Kein Kollektivvertrag erkannt" | Erforderlich |
| Gesamtbewertung | string | Gesamtbewertung des Vertrags | Erforderlich |
| RiskLevel | string | Risikostufe (Gruen/Gelb/Rot) | Erforderlich |

---

### EmploymentPremiumAnalysisDto

**Quelle:** `Domain/Dtos/Employment/EmploymentPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO für Premium-Analyseergebnisse von Arbeitsverträgen. Enthält detaillierte rechtliche Analysen aller Vertragssektionen.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| CompensationBenefits | PremiumSectionDto | Vergütungs- und Zusatzleistungsklauseln | Erforderlich |
| WorkingTimeVacation | PremiumSectionDto | Arbeitszeit- und Urlaubsklauseln | Erforderlich |
| TerminationNotice | PremiumSectionDto | Kündigungs- und Kündigungsfristklauseln | Erforderlich |
| NonCompeteConfidentiality | PremiumSectionDto | Wettbewerbs- und Vertraulichkeitsklauseln | Erforderlich |
| RightsDuties | PremiumSectionDto | Rechte und Pflichten | Erforderlich |
| Summary | EmploymentPremiumSummaryDto | Gesamtfazit | Erforderlich |
| ContractType | string | "Arbeitsvertrag" (nur lesbar) | Erforderlich |
| Tier | string | "Premium" (nur lesbar) | Erforderlich |

---

### EmploymentPremiumSummaryDto

**Quelle:** `Domain/Dtos/Employment/EmploymentPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO für die Premium-Analyse-Zusammenfassung.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Gesamtfazit | string | Ausführliches Gesamtfazit zur rechtlichen Bewertung | Erforderlich |
| Gesamtbewertung | string | Gesamtbewertung des Vertrags | Erforderlich |
| Handlungsempfehlungen | string[] | Liste konkreter Handlungsempfehlungen | Erforderlich |

---

### EmploymentContractAnalysisDto (Legacy)

**Quelle:** `Domain/Dtos/Employment/EmploymentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Class

**Beschreibung:** Legacy-DTO für Arbeitsvertragsanalysen. Wird durch EmploymentBasicAnalysisDto und EmploymentPremiumAnalysisDto ersetzt.

**Status:** ⚠️ Legacy - Wird für Rückwärtskompatibilität beibehalten

---

## Rental DTOs

DTOs für die Analyse von Mietverträgen.

### RentalBasicAnalysisDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO für Basis-Analyseergebnisse von Mietverträgen. Enthält alle Basis-Analysesektionen: General, Rent, Conditions und Summary.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| General | RentalBasicGeneralDto | Allgemeine Vertragsdaten | Erforderlich |
| Rent | RentalBasicRentDto | Mietkonditionen | Erforderlich |
| Conditions | RentalBasicConditionsDto | Vertragsbedingungen | Erforderlich |
| Summary | RentalBasicSummaryDto | Zusammenfassung | Erforderlich |
| ContractType | string | "Mietvertrag" (nur lesbar) | Erforderlich |
| Tier | string | "Basic" (nur lesbar) | Erforderlich |

---

### RentalBasicGeneralDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO für allgemeine Mietvertragsdaten.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Vertragstyp | string | Typ des Mietvertrags (I, II, III, WG) | Erforderlich |
| Mieter | string | Name des/der Mieter:in | Erforderlich |
| Vermieter | string | Name des/der Vermieter:in | Erforderlich |
| MietobjektAdresse | string | Vollständige Adresse des Mietobjekts | Erforderlich |
| Vertragsbeginn | string | Datum des Vertragsbeginns (Format: YYYY-MM-DD) | Erforderlich |
| Vertragsart | string | Art des Vertrags (befristet/unbefristet) | Erforderlich |
| Vertragsende | string? | Datum des Vertragsendes (nur bei befristeten Verträgen) | Optional |

---

### RentalBasicRentDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO für Mietkonditionen (Miete, Kosten, Kaution).

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Kaltmiete | decimal | Monatliche Kaltmiete in Euro | Erforderlich |
| Nebenkosten | decimal | Monatliche Nebenkosten in Euro | Erforderlich |
| Warmmiete | decimal | Monatliche Warmmiete in Euro | Erforderlich |
| Kaution | decimal | Höhe der Kaution in Euro | Erforderlich |
| Zahlungsweise | string | Zahlungsweise der Miete | Erforderlich |
| Nebenkostenregelung | string | Beschreibung der Nebenkostenregelung | Erforderlich |

---

### RentalBasicConditionsDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO für Vertragsbedingungen (Kündigung, Tierhaltung, Untervermietung).

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Kuendigungsfrist | string | Kündigungsfrist für den Mietvertrag | Erforderlich |
| Tierhaltung | string | Regelung zur Tierhaltung | Erforderlich |
| Untervermietung | string | Regelung zur Untervermietung | Erforderlich |
| Besonderheiten | string | Weitere besondere Vertragsbedingungen | Erforderlich |
| Schoenheitsreparaturen | string? | Regelung zu Schönheitsreparaturen | Optional |
| Betriebskostenabrechnung | string? | Regelung zur Betriebskostenabrechnung | Optional |

---

### RentalBasicSummaryDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO für die Basis-Analyse-Zusammenfassung.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Kurzfazit | string | Kurze Zusammenfassung (2-3 Sätze) | Erforderlich |
| Gesamtfazit | string | Ausführliches Fazit zur Vertragsgestaltung | Erforderlich |
| Gesamtbewertung | string | Gesamtbewertung des Vertrags | Erforderlich |
| RiskLevel | string | Risikostufe (Gruen/Gelb/Rot) | Erforderlich |

---

### RentalPremiumAnalysisDto

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO für Premium-Analyseergebnisse von Mietverträgen. Enthält detaillierte rechtliche Analysen aller Vertragssektionen.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Costs | PremiumSectionDto | Mietzahlungs- und Nebenkostenklauseln | Erforderlich |
| Terms | PremiumSectionDto | Vertragslaufzeit und Kündigungsklauseln | Erforderlich |
| Use | PremiumSectionDto | Nutzungsklauseln | Erforderlich |
| Maintenance | PremiumSectionDto | Instandhaltungs- und Reparaturklauseln | Erforderlich |
| Duties | PremiumSectionDto | Pflichten des Mieters | Erforderlich |
| Privacy | PremiumSectionDto | Datenschutz- und Besichtigungsklauseln | Erforderlich |
| Summary | RentalPremiumSummaryDto | Gesamtzusammenfassung | Erforderlich |
| ContractType | string | "Mietvertrag" (nur lesbar) | Erforderlich |
| Tier | string | "Premium" (nur lesbar) | Erforderlich |

---

### RentalPremiumSummaryDto

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO für die Premium-Analyse-Zusammenfassung.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Gesamtfazit | string | Gesamtfazit mit Zusammenfassung aller Risiken | Erforderlich |
| Gesamtbewertung | string | Gesamtrisikostufe (Gruen/Gelb/Rot) | Erforderlich |
| Handlungsempfehlungen | string[] | Liste konkreter Handlungsempfehlungen | Erforderlich |

---

### PremiumSectionDto

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO für eine Premium-Analysesektion. Enthält rechtliche Bewertung eines spezifischen Vertragsbereichs. Wird sowohl für Rental als auch Employment Premium-Analysen verwendet.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Zusammenfassung | string | Zusammenfassung der relevanten Klauseln | Erforderlich |
| Bewertung | string | Rechtliche Bewertung mit Hinweisen auf Risiken | Erforderlich |
| Rechtsgrundlage | string | Relevante Rechtsgrundlagen und Paragraphen | Erforderlich |
| RisikoScore | string | Risikobewertung der Klauseln | Erforderlich |
| RiskLevel | string | Risikostufe (Gruen/Gelb/Rot) | Erforderlich |

---

### RentContractAnalysisDto (Legacy)

**Quelle:** `Domain/Dtos/Rental/RentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Class

**Beschreibung:** Legacy-DTO für Mietvertragsanalysen. Wird durch RentalBasicAnalysisDto und RentalPremiumAnalysisDto ersetzt.

**Status:** ⚠️ Legacy - Wird für Rückwärtskompatibilität beibehalten

---

## Enumerationen

Enumerationstypen, die in DTOs verwendet werden.

### ClauseRiskScore

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Enumeration

**Beschreibung:** Risikobewertung für einzelne Klauseln.

#### Werte

| Name | Wert | Beschreibung |
| --- | --- | --- |
| Zulaessig | 0 | Klausel ist rechtlich zulässig |
| Nachteilhaft | 1 | Klausel ist nachteilhaft für den Vertragspartner |
| Unzulaessig | 2 | Klausel ist rechtlich unzulässig |

---

### OverallRiskLevel

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Enumeration

**Beschreibung:** Gesamtrisikostufe für Vertrag oder Sektion.

#### Werte

| Name | Wert | Beschreibung |
| --- | --- | --- |
| Gruen | 0 | Unkritisch - keine wesentlichen Risiken |
| Gelb | 1 | Beachtenswert - einige Punkte sollten geprüft werden |
| Rot | 2 | Problematisch - erhebliche Risiken vorhanden |

---

## Hinweise zur Verwendung

### JSON-Serialisierung

Alle DTOs verwenden `JsonPropertyName`-Attribute für die Serialisierung. Die Namenskonventionen sind:

- **Employment DTOs:** camelCase (z.B. `arbeitgeber`, `gehaltBrutto`)
- **Rental DTOs:** camelCase (z.B. `vermieter`, `kaltmiete`)
- **Shared DTOs:** camelCase (z.B. `fileName`, `isReadable`)

### Schema-Attribute

Viele DTOs verwenden `SchemaDisplay`- und `SchemaSection`-Attribute aus `PocketLawyer.Infrastructure.SchemaProviders` für:

- UI-Rendering-Hinweise
- Validierungsregeln
- Anzeigeformatierung
- Icon-Zuordnungen

### Vererbung

Alle Analyse-DTOs implementieren das `IAnalysisDto`-Interface aus `Domain/Dtos/Shared/`, um eine einheitliche Behandlung zu ermöglichen.

---

**Letzte Aktualisierung:** 18. Januar 2026  
**Version:** 2.0 (Neu generiert ohne Legacy Gemini/Claude DTOs)
