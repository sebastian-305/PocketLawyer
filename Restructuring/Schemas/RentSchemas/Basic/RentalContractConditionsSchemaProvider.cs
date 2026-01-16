namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Basic;

using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;

/// <summary>
/// Provides JSON schema for rental contract conditions.
/// Schema is automatically generated from RentalBasicConditionsDto using SchemaGenerator.
/// </summary>
public sealed class RentalContractConditionsSchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalBasicConditionsDto>();
}
