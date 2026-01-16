using System.Text.Json.Nodes;
using PocketLawyer.Infrastructure.SchemaProviders;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Tests for SchemaComparator to ensure accurate comparison of JSON schemas.
/// Validates Requirements 3.1-3.10 for schema comparison infrastructure.
/// </summary>
public class SchemaComparatorTests
{
    private readonly SchemaComparator _comparator = new();

    [Fact]
    public void Compare_IdenticalSchemas_ReturnsIsIdenticalTrue()
    {
        // Arrange
        var schema1 = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["x-displayName"] = "Name"
                }
            },
            ["required"] = new JsonArray { "name" }
        };
        
        var schema2 = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["x-displayName"] = "Name"
                }
            },
            ["required"] = new JsonArray { "name" }
        };

        // Act
        var result = _comparator.Compare(schema1, schema2);

        // Assert
        Assert.True(result.IsIdentical);
        Assert.Empty(result.Differences);
    }

    [Fact]
    public void Compare_TypeMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false
        };
        
        var generated = new JsonObject
        {
            ["type"] = "array",
            ["additionalProperties"] = false
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("type"));
    }

    [Fact]
    public void Compare_MissingProperty_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject { ["type"] = "string" },
                ["age"] = new JsonObject { ["type"] = "integer" }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject { ["type"] = "string" }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("age") && d.Contains("exists in manual"));
    }

    [Fact]
    public void Compare_ExtraProperty_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject { ["type"] = "string" }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject { ["type"] = "string" },
                ["extra"] = new JsonObject { ["type"] = "string" }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("extra") && d.Contains("exists in generated"));
    }

    [Fact]
    public void Compare_RequiredArrayDifference_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray { "name", "age" }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray { "name" }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("required") && d.Contains("age"));
    }

    [Fact]
    public void Compare_RequiredArrayOrderIndependent_ReturnsIdentical()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray { "name", "age", "email" }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["required"] = new JsonArray { "email", "name", "age" }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.True(result.IsIdentical);
        Assert.Empty(result.Differences);
    }

    [Fact]
    public void Compare_XDisplayNameMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["x-displayName"] = "Full Name"
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["x-displayName"] = "Name"
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("x-displayName"));
    }

    [Fact]
    public void Compare_XStylingMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["x-styling"] = new JsonObject
                    {
                        ["icon"] = "user",
                        ["displayClass"] = "text-primary"
                    }
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["x-styling"] = new JsonObject
                    {
                        ["icon"] = "person",
                        ["displayClass"] = "text-primary"
                    }
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("x-styling") && d.Contains("icon"));
    }

    [Fact]
    public void Compare_XSectionTitleMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["x-sectionTitle"] = "Section A"
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["x-sectionTitle"] = "Section B"
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("x-sectionTitle"));
    }

    [Fact]
    public void Compare_DescriptionMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The person's name"
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "Name of person"
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("description"));
    }

    [Fact]
    public void Compare_FormatMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["date"] = new JsonObject
                {
                    ["type"] = "string",
                    ["format"] = "date"
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["date"] = new JsonObject
                {
                    ["type"] = "string",
                    ["format"] = "date-time"
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("format"));
    }

    [Fact]
    public void Compare_EnumValuesMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["status"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray { "active", "inactive" }
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["status"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray { "active", "pending" }
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("enum"));
    }

    [Fact]
    public void Compare_EnumValuesOrderIndependent_ReturnsIdentical()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["status"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray { "active", "inactive", "pending" }
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["status"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray { "pending", "active", "inactive" }
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.True(result.IsIdentical);
        Assert.Empty(result.Differences);
    }

    [Fact]
    public void Compare_AdditionalPropertiesMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = true
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("additionalProperties"));
    }

    [Fact]
    public void Compare_NestedObjectComparison_DetectsDifferences()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["address"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["street"] = new JsonObject { ["type"] = "string" },
                        ["city"] = new JsonObject { ["type"] = "string" }
                    }
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["address"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["street"] = new JsonObject { ["type"] = "string" }
                    }
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("city"));
    }

    [Fact]
    public void Compare_NullableTypeMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["optional"] = new JsonObject
                {
                    ["type"] = new JsonArray { "string", "null" }
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["optional"] = new JsonObject
                {
                    ["type"] = "string"
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("type"));
    }

    [Fact]
    public void Compare_ArrayItemsComparison_DetectsDifferences()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["tags"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "string"
                    }
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["tags"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "integer"
                    }
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("items") && d.Contains("type"));
    }

    [Fact]
    public void Compare_ComplexSchemaIdentical_ReturnsIsIdenticalTrue()
    {
        // Arrange - Complex schema with all features
        var schema1 = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["x-sectionTitle"] = "Test Section",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The name",
                    ["x-displayName"] = "Name",
                    ["x-styling"] = new JsonObject
                    {
                        ["icon"] = "user",
                        ["displayClass"] = "text-primary",
                        ["importance"] = "high"
                    }
                },
                ["status"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray { "active", "inactive" },
                    ["x-displayName"] = "Status"
                },
                ["optional"] = new JsonObject
                {
                    ["type"] = new JsonArray { "string", "null" },
                    ["x-displayName"] = "Optional"
                }
            },
            ["required"] = new JsonArray { "name", "status" }
        };
        
        var schema2 = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["x-sectionTitle"] = "Test Section",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The name",
                    ["x-displayName"] = "Name",
                    ["x-styling"] = new JsonObject
                    {
                        ["icon"] = "user",
                        ["displayClass"] = "text-primary",
                        ["importance"] = "high"
                    }
                },
                ["status"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray { "inactive", "active" }, // Different order
                    ["x-displayName"] = "Status"
                },
                ["optional"] = new JsonObject
                {
                    ["type"] = new JsonArray { "string", "null" },
                    ["x-displayName"] = "Optional"
                }
            },
            ["required"] = new JsonArray { "status", "name" } // Different order
        };

        // Act
        var result = _comparator.Compare(schema1, schema2);

        // Assert
        Assert.True(result.IsIdentical);
        Assert.Empty(result.Differences);
    }

    [Fact]
    public void Compare_MaxLengthMismatch_DetectsDifference()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["maxLength"] = 100
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["maxLength"] = 50
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("maxLength"));
    }

    [Fact]
    public void Compare_ExamplesComparison_DetectsDifferences()
    {
        // Arrange
        var manual = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["email"] = new JsonObject
                {
                    ["type"] = "string",
                    ["examples"] = new JsonArray { "user@example.com", "admin@example.com" }
                }
            }
        };
        
        var generated = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["email"] = new JsonObject
                {
                    ["type"] = "string",
                    ["examples"] = new JsonArray { "user@example.com" }
                }
            }
        };

        // Act
        var result = _comparator.Compare(manual, generated);

        // Assert
        Assert.False(result.IsIdentical);
        Assert.Contains(result.Differences, d => d.Contains("examples"));
    }
}
