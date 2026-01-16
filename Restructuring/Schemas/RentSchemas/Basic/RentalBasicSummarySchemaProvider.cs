namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Basic;

using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;

/// <summary>
/// Provides JSON schema for rental basic summary.
/// Schema is automatically generated from RentalBasicSummaryDto using SchemaGenerator.
/// </summary>
public sealed class RentalBasicSummarySchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalBasicSummaryDto>();
}
