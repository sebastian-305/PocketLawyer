namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Basic;

using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;

/// <summary>
/// Provides JSON schema for rental rent conditions.
/// Schema is automatically generated from RentalBasicRentDto using SchemaGenerator.
/// </summary>
public sealed class RentalRentConditionsSchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalBasicRentDto>();
}
