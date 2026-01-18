# Datenmodelle

[← Zurück zur Hauptdokumentation](../README.md)

> **Hinweis:** Diese Datei wurde automatisch generiert und enthält möglicherweise veraltete Einträge zu gelöschten Gemini Models. Die Datei sollte neu generiert werden, um nur aktuelle DTOs aus dem `Domain/Dtos/` Verzeichnis zu dokumentieren.

Dieses Dokument beschreibt alle Data Transfer Objects (DTOs) im PocketLawyer-System. Alle Informationen werden aus dem tatsächlichen Quellcode im Domain/Dtos/ Verzeichnis extrahiert.

## Inhaltsverzeichnis

- [DTOs](#dtos)
- [Enumerationen](#enumerationen)

## DTOs

Data Transfer Objects für die Vertragsanalyse.

### AddressDto

**Quelle:** `PocketLawyer.Tests/Infrastructure/SchemaGeneratorTests.cs`

**Namespace:** `PocketLawyer.Tests.Infrastructure`

**Typ:** Record

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| City | string | - | Erforderlich |
| Street | string | - | Erforderlich |


### AdvantageClauseInfo

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| properties | PocketLawyer.Infrastructure.AiServices.Gemini.Models.AdvantageClauseInfoProperties | - | Erforderlich |
| required | string[] | - | Erforderlich |
| type | string | - | Erforderlich |


### AdvantageClauseInfoProperties

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| titel | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| vorteil | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| zitat | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |


### AiResponse

**Quelle:** `Application/Models/AiResponse.cs`

**Namespace:** `PocketLawyer.Application.Models`

**Typ:** Record

**Beschreibung:** Represents an AI response with token usage metadata.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Content | string | The AI response content as a JSON string | Erforderlich |
| InputTokens | int | Number of input tokens consumed | Erforderlich |
| OutputTokens | int | Number of output tokens generated | Erforderlich |
| TotalTokens | int | Total tokens (InputTokens + OutputTokens) | Erforderlich |


### AnalysisDto

**Quelle:** `Domain/Dtos/Shared/AnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Record

**Beschreibung:** Generic wrapper for analysis results containing the result and metadata.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Metadata | PocketLawyer.Domain.Dtos.Shared.AnalysisMetadataDto | - | Erforderlich |
| Result | TResult | - | Erforderlich |


### AnalysisMetadataDto

**Quelle:** `Domain/Dtos/Shared/AnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Record

**Beschreibung:** Metadata for analysis results.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| AnalyzedAt | DateTimeOffset | - | Erforderlich |
| ContractId | string | - | Erforderlich |
| CountryCode | string | - | Erforderlich |
| IsPreview | bool | - | Erforderlich |
| Language | string | - | Erforderlich |
| ModelVersion | string | - | Erforderlich |


### AnalyzerSchema

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| properties | PocketLawyer.Infrastructure.AiServices.Gemini.Models.AnalyzerSchemaProperties | - | Erforderlich |
| required | string[] | - | Erforderlich |
| type | string | - | Erforderlich |


### AnalyzerSchemaProperties

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| fazit | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| keypoints | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SummarySchema | - | Erforderlich |
| nachteile_klauseln | PocketLawyer.Infrastructure.AiServices.Gemini.Models.ArraySchema<PocketLawyer.Infrastructure.AiServices.Gemini.Models.ClauseInfo> | - | Erforderlich |
| unzulaessige_klauseln | PocketLawyer.Infrastructure.AiServices.Gemini.Models.ArraySchema<PocketLawyer.Infrastructure.AiServices.Gemini.Models.ClauseInfo> | - | Erforderlich |
| vorteilhafte_klauseln | PocketLawyer.Infrastructure.AiServices.Gemini.Models.ArraySchema<PocketLawyer.Infrastructure.AiServices.Gemini.Models.AdvantageClauseInfo> | - | Erforderlich |
| widerspruechliche_klauseln | PocketLawyer.Infrastructure.AiServices.Gemini.Models.ArraySchema<PocketLawyer.Infrastructure.AiServices.Gemini.Models.ConflictingClauseInfo> | - | Erforderlich |


### ArraySchema

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| items | T | - | Erforderlich |
| type | string | - | Erforderlich |


### BaseDto

**Quelle:** `TestData/TestDtos.cs`

**Namespace:** `PocketLawyer.TestData.Dtos`

**Typ:** Klasse

**Beschreibung:** Base DTO for testing inheritance

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| CreatedAt | DateTime | Creation timestamp | Erforderlich |
| Description | string? | Optional description | Optional |
| Id | int | Base identifier | Erforderlich |


### CachedContents

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiContext.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| CreateTime | DateTime | - | Erforderlich |
| DisplayName | string? | - | Optional |
| ExpireTime | string? | - | Optional |
| Model | string? | - | Optional |
| Name | string? | - | Optional |
| UpdateTime | DateTime | - | Erforderlich |
| UsageMetadata | PocketLawyer.Infrastructure.AiServices.Gemini.Models.CachedContents.Usagemetadata? | - | Optional |


### Candidates

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiResponse.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| AvgLogprobs | double | - | Erforderlich |
| Content | PocketLawyer.Infrastructure.AiServices.Gemini.Models.Content? | - | Optional |
| FinishReason | string? | - | Optional |


### CandidatesTokensDetails

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiResponse.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Modality | string? | - | Optional |
| TokenCount | int | - | Erforderlich |


### ClauseInfo

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| properties | PocketLawyer.Infrastructure.AiServices.Gemini.Models.ClauseInfoProperties | - | Erforderlich |
| required | string[] | - | Erforderlich |
| type | string | - | Erforderlich |


### ClauseInfoProperties

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| problem | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| rechtsgrundlage | PocketLawyer.Infrastructure.AiServices.Gemini.Models.ArraySchema<PocketLawyer.Infrastructure.AiServices.Gemini.Models.LegalReference> | - | Erforderlich |
| titel | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| zitat | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |


### ConflictingClauseInfo

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| properties | PocketLawyer.Infrastructure.AiServices.Gemini.Models.ConflictingClauseInfoProperties | - | Erforderlich |
| required | string[] | - | Erforderlich |
| type | string | - | Erforderlich |


### ConflictingClauseInfoProperties

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| problem | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| rechtsgrundlage | PocketLawyer.Infrastructure.AiServices.Gemini.Models.ArraySchema<PocketLawyer.Infrastructure.AiServices.Gemini.Models.LegalReference> | - | Erforderlich |
| titel | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| zitat_klausel_a | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| zitat_klausel_b | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |


### Content

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiResponse.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Parts | PocketLawyer.Infrastructure.AiServices.Gemini.Models.Parts[]? | - | Optional |
| Role | string? | - | Optional |


### ContractValidationDto

**Quelle:** `Domain/Dtos/Shared/ContractValidationDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| IsContract | bool | - | Erforderlich |
| IsReadable | bool | - | Erforderlich |
| Kollektivvertrag | string? | - | Optional |
| Result | string | - | Erforderlich |


### DerivedDto

**Quelle:** `TestData/TestDtos.cs`

**Namespace:** `PocketLawyer.TestData.Dtos`

**Typ:** Klasse

**Beschreibung:** Derived DTO for testing property inheritance

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Email | string? | Optional email | Optional |
| IsActive | bool | Required status | Erforderlich |
| Name | string | Derived name property | Erforderlich |
| CreatedAt (geerbt) | DateTime | Creation timestamp | Erforderlich |
| Description (geerbt) | string? | Optional description | Optional |
| Id (geerbt) | int | Base identifier | Erforderlich |


### DocumentedDto

**Quelle:** `docs/legacy/Documentation/Verification/VerificationEngine.cs`

**Namespace:** `PocketLawyer.Documentation.Verification`

**Typ:** Klasse

**Beschreibung:** Represents a documented DTO for verification.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Name | string | - | Erforderlich |
| Properties | List<PocketLawyer.Documentation.Verification.DocumentedProperty> | - | Erforderlich |


### EmploymentBasicAnalysisDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO for Employment contract basic analysis results. Contains all basic analysis sections: General, Compensation, WorkingConditions, and Summary. Property names match the step names in SchemaProvider for consistency. JSON serialization uses camelCase for backward compatibility.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Compensation | PocketLawyer.Domain.Dtos.Employment.EmploymentBasicCompensationDto | - | Erforderlich |
| ContractType | string | - | Erforderlich |
| General | PocketLawyer.Domain.Dtos.Employment.EmploymentBasicGeneralDto | - | Erforderlich |
| Summary | PocketLawyer.Domain.Dtos.Employment.EmploymentBasicSummaryDto | - | Erforderlich |
| Tier | string | - | Erforderlich |
| WorkingConditions | PocketLawyer.Domain.Dtos.Employment.EmploymentBasicWorkingConditionsDto | - | Erforderlich |


### EmploymentBasicCompensationDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO for employment compensation and benefits.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| GehaltBrutto | string | - | Erforderlich |
| GehaltNettoCa | string | - | Erforderlich |
| Sonderzahlungen | string | - | Erforderlich |
| Zahlungsweise | string | - | Erforderlich |
| Zusatzleistungen | string[] | - | Erforderlich |
| Zuschlaege | string | - | Erforderlich |


### EmploymentBasicGeneralDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO for general employment contract data.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Arbeitgeber | string | - | Erforderlich |
| Arbeitnehmer | string | - | Erforderlich |
| Position | string | - | Erforderlich |
| Probezeit | string | - | Erforderlich |
| Startdatum | string | - | Erforderlich |
| Vertragsart | string | - | Erforderlich |
| Vertragsende | string? | - | Optional |


### EmploymentBasicSummaryDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO for basic analysis summary with KV mention.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Gesamtbewertung | string | - | Erforderlich |
| Kollektivvertrag | string | - | Erforderlich |
| Kurzfazit | string | - | Erforderlich |
| RiskLevel | string | - | Erforderlich |


### EmploymentBasicWorkingConditionsDto

**Quelle:** `Domain/Dtos/Employment/EmploymentBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO for employment working conditions.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Arbeitsort | string | - | Erforderlich |
| Arbeitszeitmodell | string | - | Erforderlich |
| RemoteArbeit | string | - | Erforderlich |
| Ueberstundenregelung | string | - | Erforderlich |
| Urlaubstage | string | - | Erforderlich |
| Wochenarbeitszeit | string | - | Erforderlich |


### EmploymentContractAnalysisDto

**Quelle:** `Domain/Dtos/Employment/EmploymentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| ContractType | string | - | Erforderlich |
| Fazit | string | - | Erforderlich |
| Keypoints | PocketLawyer.Domain.Dtos.Employment.EmploymentContractAnalysisDto.SummaryClass | - | Erforderlich |
| Kollektivvertrag | string? | - | Optional |
| NachteileKlauseln | PocketLawyer.Domain.Dtos.Employment.EmploymentContractAnalysisDto.ProblemKlausel[] | - | Erforderlich |
| Tier | string | - | Erforderlich |
| UnzulaessigeKlauseln | PocketLawyer.Domain.Dtos.Employment.EmploymentContractAnalysisDto.ProblemKlausel[] | - | Erforderlich |
| VorteilhafteKlauseln | PocketLawyer.Domain.Dtos.Employment.EmploymentContractAnalysisDto.VorteilKlausel[] | - | Erforderlich |
| WiderspruechlicheKlauseln | PocketLawyer.Domain.Dtos.Employment.EmploymentContractAnalysisDto.WiderspruechlicheKlausel[] | - | Erforderlich |


### EmploymentPremiumAnalysisDto

**Quelle:** `Domain/Dtos/Employment/EmploymentPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO for Employment contract premium analysis results. Contains detailed legal analysis of all contract sections. Property names match the step names in SchemaProvider for consistency. JSON serialization uses camelCase for backward compatibility.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| CompensationBenefits | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| ContractType | string | - | Erforderlich |
| NonCompeteConfidentiality | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| RightsDuties | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| Summary | PocketLawyer.Domain.Dtos.Employment.EmploymentPremiumSummaryDto | - | Erforderlich |
| TerminationNotice | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| Tier | string | - | Erforderlich |
| WorkingTimeVacation | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |


### EmploymentPremiumSummaryDto

**Quelle:** `Domain/Dtos/Employment/EmploymentPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Record

**Beschreibung:** DTO for premium analysis summary.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Gesamtbewertung | string | - | Erforderlich |
| Gesamtfazit | string | - | Erforderlich |
| Handlungsempfehlungen | string[] | - | Erforderlich |


### FileContentDto

**Quelle:** `Domain/Dtos/Shared/FileContentDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Record

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| FileContent | string | - | Erforderlich |
| FileName | string | - | Erforderlich |


### GeminiContext

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiContext.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| CachedContents | PocketLawyer.Infrastructure.AiServices.Gemini.Models.CachedContents[]? | - | Optional |


### GeminiResponse

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiResponse.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Candidates | PocketLawyer.Infrastructure.AiServices.Gemini.Models.Candidates[]? | - | Optional |
| ModelVersion | string? | - | Optional |
| UsageMetadata | PocketLawyer.Infrastructure.AiServices.Gemini.Models.UsageMetadata? | - | Optional |


### InitializationResult

**Quelle:** `Application/Models/InitializationResult.cs`

**Namespace:** `PocketLawyer.Application.Models`

**Typ:** Record

**Beschreibung:** Result of agent initialization indicating success or failure.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| ErrorMessage | string? | Optional error message if initialization failed | Optional |
| Success | bool | True if initialization succeeded, false otherwise | Erforderlich |


### ItemDto

**Quelle:** `PocketLawyer.Tests/Infrastructure/SchemaGeneratorTests.cs`

**Namespace:** `PocketLawyer.Tests.Infrastructure`

**Typ:** Record

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Id | int | - | Erforderlich |
| Value | string | - | Erforderlich |


### KvDetectionAnswer

**Quelle:** `Domain/Dtos/Shared/KvDetectionAnswer.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Begruendung | string | - | Erforderlich |
| Kollektivvertrag | string | - | Erforderlich |


### KvDetectionResult

**Quelle:** `Domain/Dtos/Shared/KvDetectionResult.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| AllEqual | bool | - | Erforderlich |
| Begruendungen | string[] | - | Erforderlich |
| DetectedKvs | string[] | - | Erforderlich |
| KvName | string | - | Erforderlich |
| Message | string | - | Erforderlich |


### LegalReference

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| properties | PocketLawyer.Infrastructure.AiServices.Gemini.Models.LegalReferenceProperties | - | Erforderlich |
| required | string[] | - | Erforderlich |
| type | string | - | Erforderlich |


### LegalReferenceProperties

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| rechtsgrundlage | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| wortlaut | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |


### MultiLevelDto

**Quelle:** `TestData/TestDtos.cs`

**Namespace:** `PocketLawyer.TestData.Dtos`

**Typ:** Klasse

**Beschreibung:** Multi-level inheritance DTO

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Score | int | Additional property at third level | Erforderlich |
| CreatedAt (geerbt) | DateTime | Creation timestamp | Erforderlich |
| Description (geerbt) | string? | Optional description | Optional |
| Email (geerbt) | string? | Optional email | Optional |
| Id (geerbt) | int | Base identifier | Erforderlich |
| IsActive (geerbt) | bool | Required status | Erforderlich |
| Name (geerbt) | string | Derived name property | Erforderlich |


### Parts

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiResponse.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Text | string? | - | Optional |


### PreAnalyzerProperties

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| isContract | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| isReadable | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| result | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |


### PreAnalyzerSchema

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| properties | PocketLawyer.Infrastructure.AiServices.Gemini.Models.PreAnalyzerProperties | - | Erforderlich |
| required | string[] | - | Erforderlich |
| type | string | - | Erforderlich |


### PremiumSectionDto

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO for a premium analysis section. Contains legal evaluation of a specific contract area.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Bewertung | string | - | Erforderlich |
| Rechtsgrundlage | string | - | Erforderlich |
| RisikoScore | string | - | Erforderlich |
| RiskLevel | string | - | Erforderlich |
| Zusammenfassung | string | - | Erforderlich |


### ProblemKlausel

**Quelle:** `Domain/Dtos/Employment/EmploymentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Problem | string | - | Erforderlich |
| Rechtsgrundlage | PocketLawyer.Domain.Dtos.Employment.EmploymentContractAnalysisDto.RechtsgrundlageEintrag[] | - | Erforderlich |
| Titel | string | - | Erforderlich |
| Zitat | string | - | Erforderlich |


### ProblemKlausel

**Quelle:** `Domain/Dtos/Rental/RentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Problem | string | - | Erforderlich |
| Rechtsgrundlage | PocketLawyer.Domain.Dtos.Rental.RechtsgrundlageEintrag[] | - | Erforderlich |
| Titel | string | - | Erforderlich |
| Zitat | string | - | Erforderlich |


### PromptTokensDetails

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiResponse.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Modality | string? | - | Optional |
| TokenCount | int | - | Erforderlich |


### RechtsgrundlageEintrag

**Quelle:** `Domain/Dtos/Employment/EmploymentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Rechtsgrundlage | string | - | Erforderlich |
| Wortlaut | string | - | Erforderlich |


### RechtsgrundlageEintrag

**Quelle:** `Domain/Dtos/Rental/RentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Rechtsgrundlage | string | - | Erforderlich |
| Wortlaut | string | - | Erforderlich |


### RentalBasicAnalysisDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO for Rental contract basic analysis results. Contains all basic analysis sections: General, Rent, Conditions, and Summary. Property names match the step names in SchemaProvider for consistency. JSON serialization uses technical keys for backward compatibility.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Conditions | PocketLawyer.Domain.Dtos.Rental.RentalBasicConditionsDto | - | Erforderlich |
| ContractType | string | - | Erforderlich |
| General | PocketLawyer.Domain.Dtos.Rental.RentalBasicGeneralDto | - | Erforderlich |
| Rent | PocketLawyer.Domain.Dtos.Rental.RentalBasicRentDto | - | Erforderlich |
| Summary | PocketLawyer.Domain.Dtos.Rental.RentalBasicSummaryDto | - | Erforderlich |
| Tier | string | - | Erforderlich |


### RentalBasicConditionsDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO for rental contract conditions (termination, pets, subletting).

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Besonderheiten | string | - | Erforderlich |
| Betriebskostenabrechnung | string? | - | Optional |
| Kuendigungsfrist | string | - | Erforderlich |
| Schoenheitsreparaturen | string? | - | Optional |
| Tierhaltung | string | - | Erforderlich |
| Untervermietung | string | - | Erforderlich |


### RentalBasicGeneralDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO for general rental contract data.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Mieter | string | - | Erforderlich |
| MietobjektAdresse | string | - | Erforderlich |
| Vermieter | string | - | Erforderlich |
| Vertragsart | string | - | Erforderlich |
| Vertragsbeginn | string | - | Erforderlich |
| Vertragsende | string? | - | Optional |
| Vertragstyp | string | - | Erforderlich |


### RentalBasicRentDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO for rental conditions (rent, costs, deposit).

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Kaltmiete | decimal | - | Erforderlich |
| Kaution | decimal | - | Erforderlich |
| Nebenkosten | decimal | - | Erforderlich |
| Nebenkostenregelung | string | - | Erforderlich |
| Warmmiete | decimal | - | Erforderlich |
| Zahlungsweise | string | - | Erforderlich |


### RentalBasicSummaryDto

**Quelle:** `Domain/Dtos/Rental/RentalBasicAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO for basic analysis summary.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Gesamtbewertung | string | - | Erforderlich |
| Gesamtfazit | string | - | Erforderlich |
| Kurzfazit | string | - | Erforderlich |
| RiskLevel | string | - | Erforderlich |


### RentalPremiumAnalysisDto

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO for Rental contract premium analysis results. Contains detailed legal analysis of all contract sections. Property names match the step names in SchemaProvider for consistency. JSON serialization uses technical keys for backward compatibility.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| ContractType | string | - | Erforderlich |
| Costs | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| Duties | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| Maintenance | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| Privacy | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| Summary | PocketLawyer.Domain.Dtos.Rental.RentalPremiumSummaryDto | - | Erforderlich |
| Terms | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |
| Tier | string | - | Erforderlich |
| Use | PocketLawyer.Domain.Dtos.Rental.PremiumSectionDto | - | Erforderlich |


### RentalPremiumSummaryDto

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Record

**Beschreibung:** DTO for premium analysis summary.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Gesamtbewertung | string | - | Erforderlich |
| Gesamtfazit | string | - | Erforderlich |
| Handlungsempfehlungen | string[] | - | Erforderlich |


### RentContractAnalysisDto

**Quelle:** `Domain/Dtos/Rental/RentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| ContractType | string | - | Erforderlich |
| Fazit | string | - | Erforderlich |
| Kollektivvertrag | string? | - | Optional |
| NachteileKlauseln | PocketLawyer.Domain.Dtos.Rental.ProblemKlausel[] | - | Erforderlich |
| Summary | PocketLawyer.Domain.Dtos.Rental.Summary | - | Erforderlich |
| Tier | string | - | Erforderlich |
| UnzulaessigeKlauseln | PocketLawyer.Domain.Dtos.Rental.ProblemKlausel[] | - | Erforderlich |
| VorteilhafteKlauseln | PocketLawyer.Domain.Dtos.Rental.VorteilKlausel[] | - | Erforderlich |
| WiderspruechlicheKlauseln | PocketLawyer.Domain.Dtos.Rental.WiderspruechlicheKlausel[] | - | Erforderlich |


### SimpleSchema

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| type | string | - | Erforderlich |


### Summary

**Quelle:** `Domain/Dtos/Rental/RentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Beginn | string | - | Erforderlich |
| Betriebskosten | string | - | Erforderlich |
| Dauer | string | - | Erforderlich |
| ImMietgegenstandEnthalten | string | - | Erforderlich |
| Kuendigungsfrist | string | - | Erforderlich |
| MieteKalt | string | - | Erforderlich |
| Mieter | string | - | Erforderlich |
| Mindestdauer | string | - | Erforderlich |
| Vermieter | string | - | Erforderlich |
| Wohnflaeche | string | - | Erforderlich |
| Zimmer | int | - | Erforderlich |


### SummaryClass

**Quelle:** `Domain/Dtos/Employment/EmploymentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Arbeitgeber | string | - | Erforderlich |
| Arbeitsstunden | string | - | Erforderlich |
| Beginn | string | - | Erforderlich |
| Dauer | string | - | Erforderlich |
| GehaltBrutto | string | - | Erforderlich |
| Probezeit | string | - | Erforderlich |
| Taetigkeit | string | - | Erforderlich |
| Ueberstunden | string | - | Erforderlich |
| Zuschlaege | string | - | Erforderlich |


### SummarySchema

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| properties | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SummarySchemaProperties | - | Erforderlich |
| required | string[] | - | Erforderlich |
| type | string | - | Erforderlich |


### SummarySchemaProperties

**Quelle:** `Infrastructure/AiServices/Gemini/Models/AnalyzerSchemas.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| arbeitgeber | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| arbeitsstunden | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| beginn | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| dauer | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| gehalt_brutto | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| probezeit | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| taetigkeit | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| ueberstunden | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |
| zuschlaege | PocketLawyer.Infrastructure.AiServices.Gemini.Models.SimpleSchema | - | Erforderlich |


### TestComplexDto

**Quelle:** `PocketLawyer.Tests/Infrastructure/SchemaGeneratorTests.cs`

**Namespace:** `PocketLawyer.Tests.Infrastructure`

**Typ:** Record

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Items | PocketLawyer.Tests.Infrastructure.SchemaGeneratorTests.ItemDto[] | - | Erforderlich |
| Name | string | - | Erforderlich |
| Optional | string? | - | Optional |


### TestSimpleDto

**Quelle:** `PocketLawyer.Tests/Infrastructure/SchemaGeneratorTests.cs`

**Namespace:** `PocketLawyer.Tests.Infrastructure`

**Typ:** Record

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Age | int | - | Erforderlich |
| Name | string | - | Erforderlich |


### Usagemetadata

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiContext.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| TotalTokenCount | int | - | Erforderlich |


### UsageMetadata

**Quelle:** `Infrastructure/AiServices/Gemini/Models/GeminiResponse.cs`

**Namespace:** `PocketLawyer.Infrastructure.AiServices.Gemini.Models`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| CandidatesTokenCount | int | - | Erforderlich |
| CandidatesTokensDetails | PocketLawyer.Infrastructure.AiServices.Gemini.Models.CandidatesTokensDetails[]? | - | Optional |
| PromptTokenCount | int | - | Erforderlich |
| PromptTokensDetails | PocketLawyer.Infrastructure.AiServices.Gemini.Models.PromptTokensDetails[]? | - | Optional |
| TotalTokenCount | int | - | Erforderlich |


### VorteilKlausel

**Quelle:** `Domain/Dtos/Employment/EmploymentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Titel | string | - | Erforderlich |
| Vorteil | string | - | Erforderlich |
| Zitat | string | - | Erforderlich |


### VorteilKlausel

**Quelle:** `Domain/Dtos/Rental/RentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Titel | string | - | Erforderlich |
| Vorteil | string | - | Erforderlich |
| Zitat | string | - | Erforderlich |


### WebSocketFileDto

**Quelle:** `Domain/Dtos/Shared/WebSocketFileDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Shared`

**Typ:** Klasse

**Beschreibung:** DTO for receiving file data via WebSocket. ContractType is optional since the endpoint determines which agent to use.

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| ContractType | string? | Optional contract type for validation purposes. The actual agent is determined by the endpoint URL. | Optional |
| Data | string | - | Erforderlich |
| FileName | string | - | Erforderlich |


### WiderspruechlicheKlausel

**Quelle:** `Domain/Dtos/Employment/EmploymentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Employment`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Problem | string | - | Erforderlich |
| Rechtsgrundlage | PocketLawyer.Domain.Dtos.Employment.EmploymentContractAnalysisDto.RechtsgrundlageEintrag[] | - | Erforderlich |
| Titel | string | - | Erforderlich |
| ZitatKlauselA | string | - | Erforderlich |
| ZitatKlauselB | string | - | Erforderlich |


### WiderspruechlicheKlausel

**Quelle:** `Domain/Dtos/Rental/RentContractAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Klasse

#### Eigenschaften

| Name | Typ | Beschreibung | Status |
| --- | --- | --- | --- |
| Problem | string | - | Erforderlich |
| Rechtsgrundlage | PocketLawyer.Domain.Dtos.Rental.RechtsgrundlageEintrag[] | - | Erforderlich |
| Titel | string | - | Erforderlich |
| ZitatKlauselA | string | - | Erforderlich |
| ZitatKlauselB | string | - | Erforderlich |


## Enumerationen

Enumerationstypen, die in DTOs verwendet werden.

### ClauseRiskScore

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Enumeration

**Beschreibung:** Risk score for individual clauses.

#### Werte

| Name | Wert | Beschreibung |
| --- | --- | --- |
| Zulaessig | 0 | - |
| Nachteilhaft | 1 | - |
| Unzulaessig | 2 | - |


### OverallRiskLevel

**Quelle:** `Domain/Dtos/Rental/RentalPremiumAnalysisDto.cs`

**Namespace:** `PocketLawyer.Domain.Dtos.Rental`

**Typ:** Enumeration

**Beschreibung:** Overall risk level for contract or section.

#### Werte

| Name | Wert | Beschreibung |
| --- | --- | --- |
| Gruen | 0 | - |
| Gelb | 1 | - |
| Rot | 2 | - |


