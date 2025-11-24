using FsCheck;
using FsCheck.Xunit;
using PocketLawyer.Infrastructure.SchemaProviders;
using System.Text.Json.Nodes;

namespace PocketLawyer.Tests.Infrastructure.SchemaProviders;

/// <summary>
/// Property-based tests for schema styling metadata.
/// Feature: agent-refactoring
/// </summary>
public class SchemaStylingPropertyTests
{
    private readonly SchemaProvider _schemaProvider;

    public SchemaStylingPropertyTests()
    {
        _schemaProvider = new SchemaProvider();
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 19: Schema contains styling information**
    /// For any schema property, when retrieved, it should contain "x-styling" metadata.
    /// **Validates: Requirements 6.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaContainsStylingInformation()
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
                
                // Assert - All properties should have "x-styling" metadata
                if (schema["properties"] is not JsonObject properties)
                    return false;

                foreach (var property in properties)
                {
                    if (property.Value is not JsonObject propertySchema)
                        return false;

                    // Each property should have x-styling metadata
                    if (!propertySchema.ContainsKey("x-styling"))
                        return false;

                    // x-styling should be a JsonObject
                    if (propertySchema["x-styling"] is not JsonObject)
                        return false;
                }

                return true;
            });
    }

    /// <summary>
    /// For any schema property with x-styling, it should contain at least one styling attribute.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property StylingMetadataContainsAttributes()
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
                
                // Assert - x-styling should have at least one attribute
                if (schema["properties"] is not JsonObject properties)
                    return false;

                foreach (var property in properties)
                {
                    if (property.Value is not JsonObject propertySchema)
                        return false;

                    if (propertySchema["x-styling"] is not JsonObject styling)
                        return false;

                    // x-styling should have at least one attribute
                    if (styling.Count == 0)
                        return false;
                }

                return true;
            });
    }

    /// <summary>
    /// For any schema property with x-styling, common attributes should be strings.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property StylingAttributesAreStrings()
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
                
                // Assert - Common styling attributes should be strings
                if (schema["properties"] is not JsonObject properties)
                    return false;

                var stringAttributes = new[] { "displayClass", "importance", "icon", "format" };

                foreach (var property in properties)
                {
                    if (property.Value is not JsonObject propertySchema)
                        return false;

                    if (propertySchema["x-styling"] is not JsonObject styling)
                        return false;

                    // Check that string attributes are actually strings
                    foreach (var attr in stringAttributes)
                    {
                        if (styling.ContainsKey(attr))
                        {
                            var value = styling[attr];
                            if (value is not JsonValue jsonValue)
                                return false;

                            // Try to get as string
                            if (!jsonValue.TryGetValue<string>(out _))
                                return false;
                        }
                    }
                }

                return true;
            });
    }
}
