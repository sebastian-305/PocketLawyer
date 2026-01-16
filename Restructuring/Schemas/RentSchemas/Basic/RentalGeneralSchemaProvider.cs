namespace PocketLawyer.Restructuring.Schemas.RentSchemas.Basic;

using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;

/// <summary>
/// Provides JSON schema for rental general information.
/// Schema is automatically generated from RentalBasicGeneralDto using SchemaGenerator.
/// </summary>
public sealed class RentalGeneralSchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalBasicGeneralDto>();
}

