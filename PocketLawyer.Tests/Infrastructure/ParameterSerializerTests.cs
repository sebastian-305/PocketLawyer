using PocketLawyer.Infrastructure.Logging;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Unit tests for ParameterSerializer.
/// Feature: method-tracing
/// </summary>
public class ParameterSerializerTests
{
    private readonly ParameterSerializer _serializer = new();

    /// <summary>
    /// Test empty collections
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Fact]
    public void CollectionSerialization_EmptyCollection_ShowsZeroCount()
    {
        // Arrange
        var emptyList = new List<string>();
        
        // Act
        var result = _serializer.Serialize("testParam", emptyList);
        
        // Assert
        Assert.Equal("List`1[0]", result);
    }

    /// <summary>
    /// Test collections with less than 5 items
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Fact]
    public void CollectionSerialization_SmallCollection_ShowsAllItems()
    {
        // Arrange
        var smallList = new List<int> { 1, 2, 3 };
        
        // Act
        var result = _serializer.Serialize("testParam", smallList);
        
        // Assert
        Assert.Equal("List`1[3]: [1, 2, 3]", result);
    }

    /// <summary>
    /// Test collections with more than 5 items
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Fact]
    public void CollectionSerialization_LargeCollection_ShowsFirstFiveItems()
    {
        // Arrange
        var largeList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        // Act
        var result = _serializer.Serialize("testParam", largeList);
        
        // Assert
        Assert.Equal("List`1[10]: [1, 2, 3, 4, 5, ...]", result);
    }

    /// <summary>
    /// Test collection with null items
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Fact]
    public void CollectionSerialization_WithNullItems_HandlesNulls()
    {
        // Arrange
        var listWithNulls = new List<string?> { "a", null, "b" };
        
        // Act
        var result = _serializer.Serialize("testParam", listWithNulls);
        
        // Assert
        Assert.Equal("List`1[3]: [a, null, b]", result);
    }

    /// <summary>
    /// Test objects with no properties
    /// **Validates: Requirements 2.4**
    /// </summary>
    [Fact]
    public void ObjectSerialization_NoProperties_ShowsTypeName()
    {
        // Arrange
        var obj = new EmptyClass();
        
        // Act
        var result = _serializer.Serialize("testParam", obj);
        
        // Assert
        Assert.Equal("EmptyClass", result);
    }

    /// <summary>
    /// Test objects with properties
    /// **Validates: Requirements 2.4**
    /// </summary>
    [Fact]
    public void ObjectSerialization_WithProperties_ShowsTypeAndProperties()
    {
        // Arrange
        var obj = new SimpleClass { Name = "Test", Value = 42 };
        
        // Act
        var result = _serializer.Serialize("testParam", obj);
        
        // Assert
        Assert.Contains("SimpleClass", result);
        Assert.Contains("Name=Test", result);
        Assert.Contains("Value=42", result);
    }

    /// <summary>
    /// Test objects with long property values
    /// **Validates: Requirements 2.4**
    /// </summary>
    [Fact]
    public void ObjectSerialization_LongPropertyValue_TruncatesValue()
    {
        // Arrange
        var longString = new string('x', 100);
        var obj = new SimpleClass { Name = longString, Value = 1 };
        
        // Act
        var result = _serializer.Serialize("testParam", obj);
        
        // Assert
        Assert.Contains("SimpleClass", result);
        Assert.Contains("Name=" + new string('x', 50) + "...", result);
    }

    /// <summary>
    /// Test property reflection errors
    /// **Validates: Requirements 2.4**
    /// </summary>
    [Fact]
    public void ObjectSerialization_PropertyThrows_ShowsError()
    {
        // Arrange
        var obj = new ThrowingPropertyClass();
        
        // Act
        var result = _serializer.Serialize("testParam", obj);
        
        // Assert
        Assert.Contains("ThrowingPropertyClass", result);
        Assert.Contains("ThrowingProperty=<error>", result);
    }

    /// <summary>
    /// Test SerializeParameters method
    /// </summary>
    [Fact]
    public void SerializeParameters_MultipleParameters_SerializesAll()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["name"] = "test",
            ["count"] = 42,
            ["data"] = null
        };
        
        // Act
        var result = _serializer.SerializeParameters(parameters);
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("\"test\"", result["name"]);
        Assert.Equal("42", result["count"]);
        Assert.Equal("null", result["data"]);
    }

    // Test helper classes
    private class EmptyClass { }

    private class SimpleClass
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }

    private class ThrowingPropertyClass
    {
        public string ThrowingProperty => throw new InvalidOperationException("Test exception");
    }
}
