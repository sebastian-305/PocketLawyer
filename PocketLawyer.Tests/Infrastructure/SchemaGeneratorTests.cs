using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using PocketLawyer.Domain.Dtos.Employment;
using PocketLawyer.Domain.Attributes;
using PocketLawyer.Infrastructure.SchemaProviders;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Tests for SchemaGenerator to ensure it produces valid schemas for Nebius strict mode.
/// </summary>
public class SchemaGeneratorTests
{
    private readonly SchemaGenerator _generator = new();

    [Fact]
    public void GenerateSchema_SimpleDto_ProducesValidSchema()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestSimpleDto>();

        // Assert
        Assert.Equal("object", schema["type"]?.GetValue<string>());
        Assert.False(schema["additionalProperties"]?.GetValue<bool>()); // Required for strict mode
        Assert.NotNull(schema["properties"]);
        
        var properties = schema["properties"]!.AsObject();
        Assert.Contains("name", properties.Select(p => p.Key));
        Assert.Contains("age", properties.Select(p => p.Key));
    }

    [Fact]
    public void GenerateSchema_WithSectionAttribute_IncludesXSectionTitle()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithSection>();

        // Assert
        Assert.Equal("Test Section", schema["x-sectionTitle"]?.GetValue<string>());
    }

    [Fact]
    public void GenerateSchema_WithDisplayAttribute_IncludesMetadata()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithDisplay>();

        // Assert
        var properties = schema["properties"]!.AsObject();
        var nameProperty = properties["name"]!.AsObject();
        
        Assert.Equal("Name", nameProperty["x-displayName"]?.GetValue<string>());
        Assert.Equal("The person's name", nameProperty["description"]?.GetValue<string>());
        
        var styling = nameProperty["x-styling"]?.AsObject();
        Assert.NotNull(styling);
        Assert.Equal("user", styling!["icon"]?.GetValue<string>());
        Assert.Equal("text-primary", styling["displayClass"]?.GetValue<string>());
    }

    [Fact]
    public void GenerateSchema_NullableProperty_UsesArrayType()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithNullable>();

        // Assert
        var properties = schema["properties"]!.AsObject();
        var optionalProperty = properties["optional"]!.AsObject();
        
        var typeArray = optionalProperty["type"]!.AsArray();
        Assert.Equal(2, typeArray.Count);
        Assert.Contains("string", typeArray.Select(t => t?.GetValue<string>()));
        Assert.Contains("null", typeArray.Select(t => t?.GetValue<string>()));
    }

    [Fact]
    public void GenerateSchema_RequiredProperty_IncludedInRequired()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithRequired>();

        // Assert
        Assert.NotNull(schema["required"]);
        var required = schema["required"]!.AsArray();
        Assert.Contains("name", required.Select(r => r?.GetValue<string>()));
        Assert.DoesNotContain("optional", required.Select(r => r?.GetValue<string>()));
    }

    [Fact]
    public void GenerateSchema_ArrayProperty_HasItemsSchema()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithArray>();

        // Assert
        var properties = schema["properties"]!.AsObject();
        var tagsProperty = properties["tags"]!.AsObject();
        
        Assert.Equal("array", tagsProperty["type"]?.GetValue<string>());
        Assert.NotNull(tagsProperty["items"]);
        
        var items = tagsProperty["items"]!.AsObject();
        Assert.Equal("string", items["type"]?.GetValue<string>());
    }

    [Fact]
    public void GenerateSchema_NestedObject_HasFullObjectSchema()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithNested>();

        // Assert
        var properties = schema["properties"]!.AsObject();
        var addressProperty = properties["address"]!.AsObject();
        
        Assert.Equal("object", addressProperty["type"]?.GetValue<string>());
        Assert.False(addressProperty["additionalProperties"]?.GetValue<bool>());
        Assert.NotNull(addressProperty["properties"]);
        
        var nestedProperties = addressProperty["properties"]!.AsObject();
        Assert.Contains("street", nestedProperties.Select(p => p.Key));
        Assert.Contains("city", nestedProperties.Select(p => p.Key));
    }

    [Fact]
    public void GenerateSchema_EnumProperty_HasEnumValues()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithEnum>();

        // Assert
        var properties = schema["properties"]!.AsObject();
        var statusProperty = properties["status"]!.AsObject();
        
        Assert.Equal("string", statusProperty["type"]?.GetValue<string>());
        Assert.NotNull(statusProperty["enum"]);
        
        var enumValues = statusProperty["enum"]!.AsArray();
        Assert.Contains("Active", enumValues.Select(e => e?.GetValue<string>()));
        Assert.Contains("Inactive", enumValues.Select(e => e?.GetValue<string>()));
    }

    [Fact]
    public void GenerateSchema_WithMaxLength_IncludesConstraint()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithConstraints>();

        // Assert
        var properties = schema["properties"]!.AsObject();
        var nameProperty = properties["name"]!.AsObject();
        
        Assert.Equal(100, nameProperty["maxLength"]?.GetValue<int>());
    }

    [Fact]
    public void GenerateSchema_WithExamples_IncludesExamples()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithExamples>();

        // Assert
        var properties = schema["properties"]!.AsObject();
        var emailProperty = properties["email"]!.AsObject();
        
        var examples = emailProperty["examples"]!.AsArray();
        Assert.Contains("user@example.com", examples.Select(e => e?.GetValue<string>()));
    }

    [Fact]
    public void GenerateSchema_IgnoresJsonIgnoreProperties()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestDtoWithIgnore>();

        // Assert
        var properties = schema["properties"]!.AsObject();
        Assert.Contains("name", properties.Select(p => p.Key));
        Assert.DoesNotContain("ignored", properties.Select(p => p.Key));
    }

    [Fact]
    public void GenerateSchema_ProducesStrictModeCompliantSchema()
    {
        // Arrange
        var schema = _generator.GenerateSchema<TestComplexDto>();

        // Assert - Check all strict mode requirements
        Assert.Equal("object", schema["type"]?.GetValue<string>());
        Assert.False(schema["additionalProperties"]?.GetValue<bool>());
        Assert.NotNull(schema["properties"]);
        
        // Required may or may not exist depending on whether there are required properties
        if (schema.ContainsKey("required"))
        {
            Assert.NotNull(schema["required"]);
        }

        // Verify no forbidden keywords
        Assert.DoesNotContain("anyOf", schema.Select(p => p.Key));
        Assert.DoesNotContain("oneOf", schema.Select(p => p.Key));
        Assert.DoesNotContain("allOf", schema.Select(p => p.Key));
        Assert.DoesNotContain("$ref", schema.Select(p => p.Key));
    }

    [Fact]
    public void GenerateSchema_EmploymentBasicGeneralDto_IncludesCorrectSectionTitle()
    {
        // Arrange & Act
        var schema = _generator.GenerateSchema<EmploymentBasicGeneralDto>();

        // Assert
        Assert.Equal("Allgemeine Arbeitsdaten", schema["x-sectionTitle"]?.GetValue<string>());
        Assert.Equal("object", schema["type"]?.GetValue<string>());
        Assert.False(schema["additionalProperties"]?.GetValue<bool>());
    }

    #region Test DTOs

    public record TestSimpleDto(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("age")] int Age
    );

    [SchemaSection("Test Section")]
    public record TestDtoWithSection(
        [property: JsonPropertyName("value")] string Value
    );

    public record TestDtoWithDisplay(
        [property: JsonPropertyName("name")]
        [property: SchemaDisplay("Name", Description = "The person's name", Icon = "user", DisplayClass = "text-primary")]
        string Name
    );

    public record TestDtoWithNullable(
        [property: JsonPropertyName("required")] string Required,
        [property: JsonPropertyName("optional")] string? Optional
    );

    public class TestDtoWithRequired
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }
        
        [JsonPropertyName("optional")]
        public string? Optional { get; init; }
    }

    public record TestDtoWithArray(
        [property: JsonPropertyName("tags")] string[] Tags
    );

    public record TestDtoWithNested(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("address")] AddressDto Address
    );

    public record AddressDto(
        [property: JsonPropertyName("street")] string Street,
        [property: JsonPropertyName("city")] string City
    );

    public record TestDtoWithEnum(
        [property: JsonPropertyName("status")] TestStatus Status
    );

    public enum TestStatus
    {
        Active,
        Inactive
    }

    public record TestDtoWithConstraints(
        [property: JsonPropertyName("name")]
        [property: SchemaDisplay("Name", MaxLength = 100)]
        string Name
    );

    public record TestDtoWithExamples(
        [property: JsonPropertyName("email")]
        [property: SchemaDisplay("Email", Examples = new[] { "user@example.com", "admin@example.com" })]
        string Email
    );

    public record TestDtoWithIgnore(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonIgnore] string Ignored
    );

    [SchemaSection("Complex Test")]
    public record TestComplexDto(
        [property: JsonPropertyName("name")]
        [property: SchemaDisplay("Name", Description = "The name", MaxLength = 100)]
        string Name,
        
        [property: JsonPropertyName("items")] ItemDto[] Items,
        
        [property: JsonPropertyName("optional")] string? Optional
    );

    public record ItemDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("value")] string Value
    );

    #endregion
}
