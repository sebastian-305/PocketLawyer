namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Premium;

using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;

/// <summary>
/// Provides JSON schema for rental premium summary.
/// Schema is automatically generated from RentalPremiumSummaryDto using SchemaGenerator.
/// </summary>
public sealed class RentalPremiumSummarySchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalPremiumSummaryDto>();
}
