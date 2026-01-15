using System.Text.Json.Nodes;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.SchemaProviders.Rental.Basic;
using PocketLawyer.Infrastructure.SchemaProviders.Rental.Premium;
using PocketLawyer.Infrastructure.SchemaProviders.Employment.Basic;
using PocketLawyer.Infrastructure.SchemaProviders.Employment.Premium;

namespace PocketLawyer.Infrastructure.SchemaProviders;

/// <summary>
/// Central schema provider factory that manages all JSON schemas.
/// Implements the Single Source of Truth pattern for data structures.
/// </summary>
public class SchemaProvider : ISchemaProvider
{
    private readonly Dictionary<string, Func<JsonObject>> _schemaFactories;

    public SchemaProvider()
    {
        // Initialize schema factories - Single Source of Truth
        _schemaFactories = new Dictionary<string, Func<JsonObject>>
        {
            // Rental Basic Schemas
            ["Rental_Basic_General"] = () => new RentalGeneralSchemaProvider().GetSchema(),
            ["Rental_Basic_Rent"] = () => new RentalRentConditionsSchemaProvider().GetSchema(),
            ["Rental_Basic_Conditions"] = () => new RentalContractConditionsSchemaProvider().GetSchema(),
            ["Rental_Basic_Summary"] = () => new RentalBasicSummarySchemaProvider().GetSchema(),

            // Rental Premium Schemas - each gets its own section title via step name
            ["Rental_Premium_Costs"] = () => new ClauseAnalysisSchemaProvider().GetSchema("Costs"),
            ["Rental_Premium_Terms"] = () => new ClauseAnalysisSchemaProvider().GetSchema("Terms"),
            ["Rental_Premium_Use"] = () => new ClauseAnalysisSchemaProvider().GetSchema("Use"),
            ["Rental_Premium_Maintenance"] = () => new ClauseAnalysisSchemaProvider().GetSchema("Maintenance"),
            ["Rental_Premium_Duties"] = () => new ClauseAnalysisSchemaProvider().GetSchema("Duties"),
            ["Rental_Premium_Privacy"] = () => new ClauseAnalysisSchemaProvider().GetSchema("Privacy"),
            ["Rental_Premium_Summary"] = () => new RentalPremiumSummarySchemaProvider().GetSchema(),

            // Employment Basic Schemas
            ["Employment_Basic_General"] = () => new EmploymentGeneralSchemaProvider().GetSchema(),
            ["Employment_Basic_Compensation"] = () => new EmploymentCompensationSchemaProvider().GetSchema(),
            ["Employment_Basic_WorkingConditions"] = () => new EmploymentWorkingConditionsSchemaProvider().GetSchema(),
            ["Employment_Basic_Summary"] = () => new EmploymentBasicSummarySchemaProvider().GetSchema(),

            // Employment Premium Schemas - each gets its own section title via step name
            ["Employment_Premium_CompensationBenefits"] = () => new EmploymentClauseAnalysisSchemaProvider().GetSchema("CompensationBenefits"),
            ["Employment_Premium_WorkingTimeVacation"] = () => new EmploymentClauseAnalysisSchemaProvider().GetSchema("WorkingTimeVacation"),
            ["Employment_Premium_TerminationNotice"] = () => new EmploymentClauseAnalysisSchemaProvider().GetSchema("TerminationNotice"),
            ["Employment_Premium_NonCompeteConfidentiality"] = () => new EmploymentClauseAnalysisSchemaProvider().GetSchema("NonCompeteConfidentiality"),
            ["Employment_Premium_RightsDuties"] = () => new EmploymentClauseAnalysisSchemaProvider().GetSchema("RightsDuties"),
            ["Employment_Premium_Summary"] = () => new EmploymentPremiumSummarySchemaProvider().GetSchema()
        };
    }

    /// <inheritdoc/>
    public BinaryData GetSchema(string contractType, string tier, string step)
    {
        // Get JsonObject and convert to BinaryData for Nebius API
        var jsonObject = GetSchemaForEndpoint(contractType, tier, step);
        return BinaryData.FromString(jsonObject.ToJsonString());
    }

    /// <inheritdoc/>
    public JsonObject GetSchemaForEndpoint(string contractType, string tier, string step)
    {
        string key = BuildSchemaKey(contractType, tier, step);

        if (!_schemaFactories.TryGetValue(key, out var factory))
        {
            throw new SchemaNotFoundException(key);
        }

        return factory();
    }

    private static string BuildSchemaKey(string contractType, string tier, string step)
    {
        return $"{contractType}_{tier}_{step}";
    }
}
