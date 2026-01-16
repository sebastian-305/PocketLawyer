using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Provides JSON schema for Rental contract basic analysis summary.
/// Schema is automatically generated from RentalBasicSummaryDto using SchemaGenerator.
/// </summary>
public sealed class RentalBasicSummarySchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalBasicSummaryDto>();
}
