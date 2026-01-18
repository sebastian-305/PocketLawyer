using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Rental;

namespace PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;

/// <summary>
/// Provides JSON schema for Rental contract rent conditions analysis.
/// Schema is automatically generated from RentalBasicRentDto using SchemaGenerator.
/// </summary>
public sealed class RentalRentConditionsSchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<RentalBasicRentDto>();
}
