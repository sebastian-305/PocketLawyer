using FsCheck;
using FsCheck.Xunit;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders;

/// <summary>
/// Property-based tests for SchemaProvider.
/// Feature: agent-refactoring
/// </summary>
public class SchemaProviderPropertyTests
{
    private readonly SchemaProvider _schemaProvider;

    public SchemaProviderPropertyTests()
    {
        _schemaProvider = new SchemaProvider();
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 12: Schema retrieval for analysis steps**
    /// For any valid (contractType, tier, step) combination, GetSchema() should return a non-null BinaryData.
    /// **Validates: Requirements 4.5, 5.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaRetrievalForAnalysisSteps_ReturnsNonNullBinaryData()
    {
        // Define valid schema combinations
        var validCombinations = new[]
        {
            ("Rental", "Basic", "General"),
            ("Rental", "Basic", "Rent"),
            ("Rental", "Basic", "Conditions"),
            ("Rental", "Basic", "Summary"),
            ("Rental", "Premium", "Costs"),
            ("Rental", "Premium", "Terms"),
            ("Rental", "Premium", "Use"),
            ("Rental", "Premium", "Maintenance"),
            ("Rental", "Premium", "Duties"),
            ("Rental", "Premium", "Privacy"),
            ("Rental", "Premium", "Summary")
        };

        return Prop.ForAll(
            Gen.Elements(validCombinations).ToArbitrary(),
            combination =>
            {
                var (contractType, tier, step) = combination;
                
                // Act
                var schema = _schemaProvider.GetSchema(contractType, tier, step);
                
                // Assert
                return schema != null && schema.ToArray().Length > 0;
            });
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 12: Schema retrieval for analysis steps**
    /// For any valid (contractType, tier, step) combination, GetSchemaForEndpoint() should return a non-null JsonObject.
    /// **Validates: Requirements 4.5, 5.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaRetrievalForEndpoint_ReturnsNonNullJsonObject()
    {
        // Define valid schema combinations
        var validCombinations = new[]
        {
            ("Rental", "Basic", "General"),
            ("Rental", "Basic", "Rent"),
            ("Rental", "Basic", "Conditions"),
            ("Rental", "Basic", "Summary"),
            ("Rental", "Premium", "Costs"),
            ("Rental", "Premium", "Terms"),
            ("Rental", "Premium", "Use"),
            ("Rental", "Premium", "Maintenance"),
            ("Rental", "Premium", "Duties"),
            ("Rental", "Premium", "Privacy"),
            ("Rental", "Premium", "Summary")
        };

        return Prop.ForAll(
            Gen.Elements(validCombinations).ToArbitrary(),
            combination =>
            {
                var (contractType, tier, step) = combination;
                
                // Act
                var schema = _schemaProvider.GetSchemaForEndpoint(contractType, tier, step);
                
                // Assert
                return schema != null;
            });
    }

    /// <summary>
    /// For any invalid schema key, GetSchema() should throw SchemaNotFoundException.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property InvalidSchemaKey_ThrowsSchemaNotFoundException()
    {
        // Generate random invalid combinations
        var invalidContractTypes = Gen.Elements("Invalid", "Unknown", "Test", "");
        var invalidTiers = Gen.Elements("Invalid", "Unknown", "Test", "");
        var invalidSteps = Gen.Elements("Invalid", "Unknown", "Test", "");

        return Prop.ForAll(
            invalidContractTypes.ToArbitrary(),
            invalidTiers.ToArbitrary(),
            invalidSteps.ToArbitrary(),
            (contractType, tier, step) =>
            {
                // Skip if this happens to be a valid combination
                var validKey = $"{contractType}_{tier}_{step}";
                if (IsValidSchemaKey(validKey))
                    return true;

                // Act & Assert
                try
                {
                    _schemaProvider.GetSchema(contractType, tier, step);
                    return false; // Should have thrown
                }
                catch (SchemaNotFoundException)
                {
                    return true;
                }
                catch
                {
                    return false; // Wrong exception type
                }
            });
    }

    private static bool IsValidSchemaKey(string key)
    {
        var validKeys = new[]
        {
            "Rental_Basic_General",
            "Rental_Basic_Rent",
            "Rental_Basic_Conditions",
            "Rental_Basic_Summary",
            "Rental_Premium_Costs",
            "Rental_Premium_Terms",
            "Rental_Premium_Use",
            "Rental_Premium_Maintenance",
            "Rental_Premium_Duties",
            "Rental_Premium_Privacy",
            "Rental_Premium_Summary"
        };

        return validKeys.Contains(key);
    }
}
