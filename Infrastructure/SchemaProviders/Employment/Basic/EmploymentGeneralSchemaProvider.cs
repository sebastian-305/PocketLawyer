using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Employment;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Provides JSON schema for Employment contract general data analysis.
/// Schema is automatically generated from EmploymentBasicGeneralDto using SchemaGenerator.
/// </summary>
public sealed class EmploymentGeneralSchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<EmploymentBasicGeneralDto>();
}
