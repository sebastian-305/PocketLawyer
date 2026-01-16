using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Premium;

/// <summary>
/// Provides JSON schema for Rental contract premium analysis summary.
/// Schema is automatically generated from RentalPremiumSummaryDto using SchemaGenerator.
/// </summary>
public sealed class RentalPremiumSummarySchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalPremiumSummaryDto>();
}
