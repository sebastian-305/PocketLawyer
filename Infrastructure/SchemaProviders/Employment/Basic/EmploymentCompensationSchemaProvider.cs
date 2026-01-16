using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Provides JSON schema for Employment contract compensation analysis.
/// Schema is automatically generated from EmploymentBasicCompensationDto using SchemaGenerator.
/// </summary>
public sealed class EmploymentCompensationSchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<EmploymentBasicCompensationDto>();
}
