using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Employment;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Provides JSON schema for Employment contract working conditions analysis.
/// Schema is automatically generated from EmploymentBasicWorkingConditionsDto using SchemaGenerator.
/// </summary>
public sealed class EmploymentWorkingConditionsSchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => 
        _generator.GenerateSchema<EmploymentBasicWorkingConditionsDto>();
}
