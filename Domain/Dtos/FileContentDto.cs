// Einfaches Datenobjekt zur Übergabe des Dateinamens
// und des dekodierten Dateiinhalts.
namespace PocketLawyer.Domain.Dtos;

public record FileContentDto(string FileName, string FileContent);
