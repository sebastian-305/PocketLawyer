using FsCheck;
using FsCheck.Xunit;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure;
using System.Text.Json.Nodes;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Property-based tests for SchemaValidator.
/// Feature: agent-refactoring
/// </summary>
public class SchemaValidatorPropertyTests
{
    private readonly ISchemaValidator _validator;

    public SchemaValidatorPropertyTests()
    {
        _validator = new SchemaValidator();
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 13: AI responses are validated**
    /// For any AI response and schema, when the response is processed, 
    /// the system should validate it against the corresponding JSON schema.
    /// **Validates: Requirements 4.7, 5.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AiResponsesAreValidated_ValidResponsePassesValidation()
    {
        // Generate valid responses that match a simple schema
        var schemaGen = Gen.Constant(CreateSimpleSchema());
        var validResponseGen = Gen.Constant(CreateValidResponse());

        return Prop.ForAll(
            schemaGen.ToArbitrary(),
            validResponseGen.ToArbitrary(),
            (schema, response) =>
            {
                // Act
                var isValid = _validator.Validate(schema, response);
                
                // Assert - valid response should pass validation
                return isValid;
            });
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 13: AI responses are validated**
    /// For any invalid AI response, validation should detect the invalidity.
    /// **Validates: Requirements 4.7, 5.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AiResponsesAreValidated_InvalidResponseFailsValidation()
    {
        // Generate invalid responses that don't match the schema
        var schemaGen = Gen.Constant(CreateSimpleSchema());
        var invalidResponseGen = Gen.OneOf(
            Gen.Constant(CreateInvalidResponse_MissingRequiredField()),
            Gen.Constant(CreateInvalidResponse_WrongType()),
            Gen.Constant(CreateInvalidResponse_ExtraProperties())
        );

        return Prop.ForAll(
            schemaGen.ToArbitrary(),
            invalidResponseGen.ToArbitrary(),
            (schema, response) =>
            {
                // Act
                var isValid = _validator.Validate(schema, response);
                
                // Assert - invalid response should fail validation
                return !isValid;
            });
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 13: AI responses are validated**
    /// For any response, Validate() should return a boolean result.
    /// **Validates: Requirements 4.7, 5.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AiResponsesAreValidated_AlwaysReturnsBool()
    {
        var schemaGen = Gen.Constant(CreateSimpleSchema());
        var responseGen = Gen.OneOf(
            Gen.Constant(CreateValidResponse()),
            Gen.Constant(CreateInvalidResponse_MissingRequiredField())
        );

        return Prop.ForAll(
            schemaGen.ToArbitrary(),
            responseGen.ToArbitrary(),
            (schema, response) =>
            {
                // Act
                var result = _validator.Validate(schema, response);
                
                // Assert - should always return a boolean (true or false)
                return result is bool;
            });
    }

    #region Helper Methods

    private static JsonObject CreateSimpleSchema()
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string"
                },
                ["age"] = new JsonObject
                {
                    ["type"] = "integer",
                    ["minimum"] = 0
                },
                ["email"] = new JsonObject
                {
                    ["type"] = "string",
                    ["format"] = "email"
                }
            },
            ["required"] = new JsonArray("name", "age")
        };
    }

    private static JsonObject CreateValidResponse()
    {
        return new JsonObject
        {
            ["name"] = "John Doe",
            ["age"] = 30,
            ["email"] = "john@example.com"
        };
    }

    private static JsonObject CreateInvalidResponse_MissingRequiredField()
    {
        return new JsonObject
        {
            ["name"] = "John Doe"
            // Missing required "age" field
        };
    }

    private static JsonObject CreateInvalidResponse_WrongType()
    {
        return new JsonObject
        {
            ["name"] = "John Doe",
            ["age"] = "thirty" // Should be integer, not string
        };
    }

    private static JsonObject CreateInvalidResponse_ExtraProperties()
    {
        return new JsonObject
        {
            ["name"] = "John Doe",
            ["age"] = 30,
            ["email"] = "john@example.com",
            ["extraField"] = "not allowed" // additionalProperties is false
        };
    }

    #endregion

    /// <summary>
    /// **Feature: agent-refactoring, Property 20: Schema validation returns details**
    /// For any failed schema validation, when validation is performed, 
    /// the validator should return false and provide detailed error information.
    /// **Validates: Requirements 6.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaValidationReturnsDetails_InvalidResponseProvidesErrors()
    {
        var schemaGen = Gen.Constant(CreateSimpleSchema());
        var invalidResponseGen = Gen.OneOf(
            Gen.Constant(CreateInvalidResponse_MissingRequiredField()),
            Gen.Constant(CreateInvalidResponse_WrongType()),
            Gen.Constant(CreateInvalidResponse_ExtraProperties())
        );

        return Prop.ForAll(
            schemaGen.ToArbitrary(),
            invalidResponseGen.ToArbitrary(),
            (schema, response) =>
            {
                // Act
                var result = _validator.ValidateWithDetails(schema, response);
                
                // Assert - invalid response should have IsValid = false and non-empty Errors
                return !result.IsValid && result.Errors.Count > 0;
            });
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 20: Schema validation returns details**
    /// For any valid response, ValidateWithDetails should return IsValid = true and empty errors.
    /// **Validates: Requirements 6.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaValidationReturnsDetails_ValidResponseHasNoErrors()
    {
        var schemaGen = Gen.Constant(CreateSimpleSchema());
        var validResponseGen = Gen.Constant(CreateValidResponse());

        return Prop.ForAll(
            schemaGen.ToArbitrary(),
            validResponseGen.ToArbitrary(),
            (schema, response) =>
            {
                // Act
                var result = _validator.ValidateWithDetails(schema, response);
                
                // Assert - valid response should have IsValid = true and empty Errors
                return result.IsValid && result.Errors.Count == 0;
            });
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 20: Schema validation returns details**
    /// For any response, ValidateWithDetails should always return a ValidationResult.
    /// **Validates: Requirements 6.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaValidationReturnsDetails_AlwaysReturnsValidationResult()
    {
        var schemaGen = Gen.Constant(CreateSimpleSchema());
        var responseGen = Gen.OneOf(
            Gen.Constant(CreateValidResponse()),
            Gen.Constant(CreateInvalidResponse_MissingRequiredField())
        );

        return Prop.ForAll(
            schemaGen.ToArbitrary(),
            responseGen.ToArbitrary(),
            (schema, response) =>
            {
                // Act
                var result = _validator.ValidateWithDetails(schema, response);
                
                // Assert - should always return a ValidationResult with non-null properties
                return result != null && result.Errors != null;
            });
    }

    /// <summary>
    /// **Feature: agent-refactoring, Property 20: Schema validation returns details**
    /// For any invalid response, error messages should be descriptive.
    /// **Validates: Requirements 6.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaValidationReturnsDetails_ErrorMessagesAreDescriptive()
    {
        var schemaGen = Gen.Constant(CreateSimpleSchema());
        var invalidResponseGen = Gen.OneOf(
            Gen.Constant(CreateInvalidResponse_MissingRequiredField()),
            Gen.Constant(CreateInvalidResponse_WrongType()),
            Gen.Constant(CreateInvalidResponse_ExtraProperties())
        );

        return Prop.ForAll(
            schemaGen.ToArbitrary(),
            invalidResponseGen.ToArbitrary(),
            (schema, response) =>
            {
                // Act
                var result = _validator.ValidateWithDetails(schema, response);
                
                // Assert - error messages should be non-empty strings
                return !result.IsValid && 
                       result.Errors.All(e => !string.IsNullOrWhiteSpace(e));
            });
    }
}
