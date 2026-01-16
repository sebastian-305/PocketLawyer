using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Provides JSON schema for Rental contract conditions analysis.
/// Schema is automatically generated from RentalBasicConditionsDto using SchemaGenerator.
/// </summary>
public sealed class RentalContractConditionsSchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalBasicConditionsDto>();
}
