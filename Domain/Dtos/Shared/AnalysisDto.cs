namespace PocketLawyer.Domain.Dtos.Shared;

/// <summary>
/// Generic wrapper for analysis results containing the result and metadata.
/// </summary>
/// <typeparam name="TResult">The type of analysis result</typeparam>
public record AnalysisDto<TResult>(
    TResult Result,
    AnalysisMetadataDto Metadata
) where TResult : IAnalysisDto;

/// <summary>
/// Metadata for analysis results.
/// </summary>
public record AnalysisMetadataDto(
    string ContractId,
    string CountryCode,
    string Language,
    DateTimeOffset AnalyzedAt,
    bool IsPreview,
    string ModelVersion
);
