using System.Text.Json.Nodes;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Domain.Dtos.Employment;

namespace PocketLawyer.Infrastructure.SchemaProviders.Employment.Premium;

/// <summary>
/// Provides JSON schema for Employment contract premium analysis summary.
/// Schema is automatically generated from EmploymentPremiumSummaryDto using SchemaGenerator.
/// </summary>
public sealed class EmploymentPremiumSummarySchemaProvider
{
    private readonly SchemaGenerator _generator = new();

    public JsonObject GetSchema() => _generator.GenerateSchema<EmploymentPremiumSummaryDto>();
}
