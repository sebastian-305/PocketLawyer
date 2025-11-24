using FsCheck;
using FsCheck.Xunit;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Tests.Domain;

/// <summary>
/// Property-based tests for Analysis DTO structure.
/// **Feature: agent-refactoring**
/// </summary>
public class AnalysisDtoPropertyTests
{
    #region Generators

    /// <summary>
    /// Generator for AnalysisMetadataDto with random valid values.
    /// </summary>
    private static Gen<AnalysisMetadataDto> GenMetadata() =>
        from contractId in Arb.Generate<NonEmptyString>()
        from countryCode in Gen.Elements("AT", "DE", "CH")
        from language in Gen.Elements("de", "en")
        from analyzedAt in Arb.Generate<DateTimeOffset>()
        from isPreview in Arb.Generate<bool>()
        from modelVersion in Gen.Elements("1.0", "2.0", "3.0")
        select new AnalysisMetadataDto(
            contractId.Get,
            countryCode,
            language,
            analyzedAt,
            isPreview,
            modelVersion
        );

    /// <summary>
    /// Generator for RentalBasicGeneralDto with random valid values.
    /// </summary>
    private static Gen<RentalBasicGeneralDto> GenRentalBasicGeneral() =>
        from vertragstyp in Gen.Elements("I", "II", "III", "WG")
        from mieter in Arb.Generate<NonEmptyString>()
        from vermieter in Arb.Generate<NonEmptyString>()
        from adresse in Arb.Generate<NonEmptyString>()
        from beginn in Arb.Generate<NonEmptyString>()
        from art in Gen.Elements("Unbefristet", "Befristet")
        from ende in Arb.Generate<string?>()
        select new RentalBasicGeneralDto(
            vertragstyp,
            mieter.Get,
            vermieter.Get,
            adresse.Get,
            beginn.Get,
            art,
            ende
        );

    /// <summary>
    /// Generator for RentalBasicRentDto with random valid values.
    /// </summary>
    private static Gen<RentalBasicRentDto> GenRentalBasicRent() =>
        from kaltmiete in Gen.Choose(100, 5000).Select(x => (decimal)x)
        from nebenkosten in Gen.Choose(50, 500).Select(x => (decimal)x)
        from kaution in Gen.Choose(300, 15000).Select(x => (decimal)x)
        from zahlungsweise in Gen.Elements("Monatlich", "Vierteljährlich")
        from regelung in Arb.Generate<NonEmptyString>()
        select new RentalBasicRentDto(
            kaltmiete,
            nebenkosten,
            kaltmiete + nebenkosten,
            kaution,
            zahlungsweise,
            regelung.Get
        );

    /// <summary>
    /// Generator for RentalBasicConditionsDto with random valid values.
    /// </summary>
    private static Gen<RentalBasicConditionsDto> GenRentalBasicConditions() =>
        from frist in Arb.Generate<NonEmptyString>()
        from tier in Gen.Elements("Erlaubt", "Nicht erlaubt", "Mit Zustimmung")
        from unterv in Gen.Elements("Erlaubt", "Nicht erlaubt", "Mit Zustimmung")
        from besond in Arb.Generate<NonEmptyString>()
        from schoen in Arb.Generate<string?>()
        from betriebs in Arb.Generate<string?>()
        select new RentalBasicConditionsDto(
            frist.Get,
            tier,
            unterv,
            besond.Get,
            schoen,
            betriebs
        );

    /// <summary>
    /// Generator for RentalBasicSummaryDto with random valid values.
    /// </summary>
    private static Gen<RentalBasicSummaryDto> GenRentalBasicSummary() =>
        from kurz in Arb.Generate<NonEmptyString>()
        from gesamt in Arb.Generate<NonEmptyString>()
        from bewertung in Arb.Generate<NonEmptyString>()
        from risk in Gen.Elements("Gruen", "Gelb", "Rot")
        select new RentalBasicSummaryDto(
            kurz.Get,
            gesamt.Get,
            bewertung.Get,
            risk
        );

    /// <summary>
    /// Generator for RentalBasicAnalysisDto with random valid values.
    /// </summary>
    private static Gen<RentalBasicAnalysisDto> GenRentalBasicAnalysis() =>
        from general in GenRentalBasicGeneral()
        from rent in GenRentalBasicRent()
        from conditions in GenRentalBasicConditions()
        from summary in GenRentalBasicSummary()
        select new RentalBasicAnalysisDto
        {
            AllgemeineVertragsdaten = general,
            Mietkonditionen = rent,
            Vertragsbedingungen = conditions,
            Summary = summary
        };

    /// <summary>
    /// Generator for PremiumSectionDto with random valid values.
    /// </summary>
    private static Gen<PremiumSectionDto> GenPremiumSection() =>
        from zusammenfassung in Arb.Generate<NonEmptyString>()
        from bewertung in Arb.Generate<NonEmptyString>()
        from rechtsgrundlage in Arb.Generate<NonEmptyString>()
        from risikoScore in Gen.Elements("Zulaessig", "Nachteilhaft", "Unzulaessig")
        from riskLevel in Gen.Elements("Gruen", "Gelb", "Rot")
        select new PremiumSectionDto(
            zusammenfassung.Get,
            bewertung.Get,
            rechtsgrundlage.Get,
            risikoScore,
            riskLevel
        );

    /// <summary>
    /// Generator for RentalPremiumAnalysisDto with random valid values.
    /// </summary>
    private static Gen<RentalPremiumAnalysisDto> GenRentalPremiumAnalysis() =>
        from costs in GenPremiumSection()
        from duration in GenPremiumSection()
        from usage in GenPremiumSection()
        from maintenance in GenPremiumSection()
        from rights in GenPremiumSection()
        from privacy in GenPremiumSection()
        from fazit in Arb.Generate<NonEmptyString>()
        from bewertung in Gen.Elements("Gruen", "Gelb", "Rot")
        select new RentalPremiumAnalysisDto
        {
            MietzahlungUndNebenkosten = costs,
            MietdauerUndKuendigung = duration,
            GebrauchDerMietsache = usage,
            InstandhaltungUndReparaturen = maintenance,
            RechteUndPflichten = rights,
            DatenschutzUndKommunikation = privacy,
            Gesamtfazit = fazit.Get,
            Gesamtbewertung = bewertung
        };

    #endregion

    #region Property 24: Analysis DTOs contain metadata

    /// <summary>
    /// **Feature: agent-refactoring, Property 24: Analysis DTOs contain metadata**
    /// **Validates: Requirements 8.1, 8.2**
    /// 
    /// For any completed analysis, when the AnalysisDto is created, 
    /// it should contain both the result and metadata with all required fields.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AnalysisDtoContainsMetadata()
    {
        var genAnalysisDto = 
            from result in GenRentalBasicAnalysis()
            from metadata in GenMetadata()
            select new AnalysisDto<RentalBasicAnalysisDto>(result, metadata);

        return Prop.ForAll(
            genAnalysisDto.ToArbitrary(),
            dto =>
            {
                // AnalysisDto should have non-null Result and Metadata
                Assert.NotNull(dto.Result);
                Assert.NotNull(dto.Metadata);

                // Metadata should have all required fields
                Assert.NotNull(dto.Metadata.ContractId);
                Assert.NotEmpty(dto.Metadata.ContractId);
                Assert.NotNull(dto.Metadata.CountryCode);
                Assert.NotEmpty(dto.Metadata.CountryCode);
                Assert.NotNull(dto.Metadata.Language);
                Assert.NotEmpty(dto.Metadata.Language);
                Assert.NotEqual(default(DateTimeOffset), dto.Metadata.AnalyzedAt);
                Assert.NotNull(dto.Metadata.ModelVersion);
                Assert.NotEmpty(dto.Metadata.ModelVersion);

                return true;
            }
        );
    }

    #endregion

    #region Property 25: Rental basic DTO structure

    /// <summary>
    /// **Feature: agent-refactoring, Property 25: Rental basic DTO structure**
    /// **Validates: Requirements 8.3**
    /// 
    /// For any completed rental basic analysis, when the DTO is created, 
    /// it should contain AllgemeineVertragsdaten, Mietkonditionen, 
    /// Vertragsbedingungen, and Summary sections.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property RentalBasicDtoHasAllSections()
    {
        return Prop.ForAll(
            GenRentalBasicAnalysis().ToArbitrary(),
            dto =>
            {
                // All sections should be non-null
                Assert.NotNull(dto.AllgemeineVertragsdaten);
                Assert.NotNull(dto.Mietkonditionen);
                Assert.NotNull(dto.Vertragsbedingungen);
                Assert.NotNull(dto.Summary);

                // ContractType and Tier should be correct
                Assert.Equal("Mietvertrag", dto.ContractType);
                Assert.Equal("Basic", dto.Tier);

                // Each section should have valid data
                Assert.NotNull(dto.AllgemeineVertragsdaten.Vertragstyp);
                Assert.NotEmpty(dto.AllgemeineVertragsdaten.Vertragstyp);
                Assert.NotNull(dto.AllgemeineVertragsdaten.Mieter);
                Assert.NotEmpty(dto.AllgemeineVertragsdaten.Mieter);

                Assert.True(dto.Mietkonditionen.Kaltmiete > 0);
                Assert.True(dto.Mietkonditionen.Warmmiete >= dto.Mietkonditionen.Kaltmiete);

                Assert.NotNull(dto.Vertragsbedingungen.Kuendigungsfrist);
                Assert.NotEmpty(dto.Vertragsbedingungen.Kuendigungsfrist);

                Assert.NotNull(dto.Summary.Kurzfazit);
                Assert.NotEmpty(dto.Summary.Kurzfazit);

                return true;
            }
        );
    }

    #endregion

    #region Property 26: Rental premium DTO structure

    /// <summary>
    /// **Feature: agent-refactoring, Property 26: Rental premium DTO structure**
    /// **Validates: Requirements 8.4**
    /// 
    /// For any completed rental premium analysis, when the DTO is created, 
    /// it should contain all six premium sections (Mietzahlung, Mietdauer, 
    /// Gebrauch, Instandhaltung, Rechte, Datenschutz) plus Gesamtfazit 
    /// and Gesamtbewertung.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property RentalPremiumDtoHasAllSections()
    {
        return Prop.ForAll(
            GenRentalPremiumAnalysis().ToArbitrary(),
            dto =>
            {
                // All six sections should be non-null
                Assert.NotNull(dto.MietzahlungUndNebenkosten);
                Assert.NotNull(dto.MietdauerUndKuendigung);
                Assert.NotNull(dto.GebrauchDerMietsache);
                Assert.NotNull(dto.InstandhaltungUndReparaturen);
                Assert.NotNull(dto.RechteUndPflichten);
                Assert.NotNull(dto.DatenschutzUndKommunikation);

                // Summary fields should be non-null
                Assert.NotNull(dto.Gesamtfazit);
                Assert.NotEmpty(dto.Gesamtfazit);
                Assert.NotNull(dto.Gesamtbewertung);
                Assert.NotEmpty(dto.Gesamtbewertung);

                // ContractType and Tier should be correct
                Assert.Equal("Mietvertrag", dto.ContractType);
                Assert.Equal("Premium", dto.Tier);

                return true;
            }
        );
    }

    #endregion

    #region Property 27: Premium section structure

    /// <summary>
    /// **Feature: agent-refactoring, Property 27: Premium section structure**
    /// **Validates: Requirements 8.5**
    /// 
    /// For any premium section, when the section is created, 
    /// it should contain Zusammenfassung, Bewertung, Rechtsgrundlage, 
    /// RisikoScore, and RiskLevel.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property PremiumSectionHasAllFields()
    {
        return Prop.ForAll(
            GenPremiumSection().ToArbitrary(),
            section =>
            {
                // All fields should be non-null and non-empty
                Assert.NotNull(section.Zusammenfassung);
                Assert.NotEmpty(section.Zusammenfassung);
                
                Assert.NotNull(section.Bewertung);
                Assert.NotEmpty(section.Bewertung);
                
                Assert.NotNull(section.Rechtsgrundlage);
                Assert.NotEmpty(section.Rechtsgrundlage);
                
                Assert.NotNull(section.RisikoScore);
                Assert.NotEmpty(section.RisikoScore);
                
                Assert.NotNull(section.RiskLevel);
                Assert.NotEmpty(section.RiskLevel);

                // RisikoScore should be one of the valid values
                Assert.Contains(section.RisikoScore, new[] { "Zulaessig", "Nachteilhaft", "Unzulaessig" });
                
                // RiskLevel should be one of the valid values
                Assert.Contains(section.RiskLevel, new[] { "Gruen", "Gelb", "Rot" });

                return true;
            }
        );
    }

    #endregion
}
