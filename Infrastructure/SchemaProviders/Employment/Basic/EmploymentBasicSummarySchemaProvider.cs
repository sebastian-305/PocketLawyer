using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Employment;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;

/// <summary>
/// Provides JSON schema for Employment contract basic analysis summary.
/// Schema is automatically generated from EmploymentBasicSummaryDto using SchemaGenerator.
/// </summary>
public sealed class EmploymentBasicSummarySchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<EmploymentBasicSummaryDto>();
}
