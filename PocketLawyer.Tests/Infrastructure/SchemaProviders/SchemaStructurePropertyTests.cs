using FsCheck;
using FsCheck.Xunit;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders;

/// <summary>
/// Property-based tests for schema structure validation.
/// Feature: agent-refactoring
/// </summary>
public class SchemaStructurePropertyTests
{
    private readonly SchemaProvider _schemaProvider;

    public SchemaStructurePropertyTests()
    {
        _schemaProvider = new SchemaProvider();
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 18: Schema contains required fields**
    /// For any schema, when retrieved, it should contain "type" and "properties" fields.
    /// **Validates: Requirements 6.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaContainsRequiredFields()
    {
        // Define all valid schema combinations
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
                
                // Assert - Schema must have "type" and "properties" fields
                var hasType = schema.ContainsKey("type");
                var hasProperties = schema.ContainsKey("properties");
                
                // Verify type is "object"
                var typeIsObject = schema["type"]?.ToString() == "object";
                
                // Verify properties is a JsonObject
                var propertiesIsJsonObject = schema["properties"] is JsonObject;
                
                return hasType && hasProperties && typeIsObject && propertiesIsJsonObject;
            });
    }

    /// <summary>
    /// For any schema, all properties should have a "type" field.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AllSchemaPropertiesHaveType()
    {
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
                
                // Assert - All properties must have a "type" field
                if (schema["properties"] is not JsonObject properties)
                    return false;

                foreach (var property in properties)
                {
                    if (property.Value is not JsonObject propertySchema)
                        return false;

                    if (!propertySchema.ContainsKey("type"))
                        return false;
                }

                return true;
            });
    }

    /// <summary>
    /// For any schema, the "required" field should be an array if present.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property RequiredFieldIsArrayIfPresent()
    {
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
                
                // Assert - If "required" exists, it must be a JsonArray
                if (!schema.ContainsKey("required"))
                    return true; // Optional field, that's fine

                return schema["required"] is JsonArray;
            });
    }
}
