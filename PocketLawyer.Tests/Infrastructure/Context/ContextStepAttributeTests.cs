using PocketLawyer.Infrastructure.Context;
using Xunit;

namespace PocketLawyer.Tests.Infrastructure.Context;

/// <summary>
/// Unit tests for ContextStepAttribute.
/// Feature: context-management
/// </summary>
public class ContextStepAttributeTests
{
    /// <summary>
    /// Test that attribute can be applied to methods only
    /// **Validates: Requirements 10.1, 10.2**
    /// </summary>
    [Fact]
    public void AttributeUsage_AppliedToMethod_IsValid()
    {
        // Arrange & Act
        var method = typeof(TestClass).GetMethod(nameof(TestClass.TestMethod));
        var attribute = method?.GetCustomAttributes(typeof(ContextStepAttribute), false).FirstOrDefault();
        
        // Assert
        Assert.NotNull(attribute);
        Assert.IsType<ContextStepAttribute>(attribute);
    }

    /// <summary>
    /// Test that attribute targets only methods
    /// **Validates: Requirements 10.1, 10.2**
    /// </summary>
    [Fact]
    public void AttributeUsage_TargetsMethod_IsCorrect()
    {
        // Arrange
        var attributeType = typeof(ContextStepAttribute);
        var attributeUsage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(
            attributeType, 
            typeof(AttributeUsageAttribute));
        
        // Assert
        Assert.NotNull(attributeUsage);
        Assert.Equal(AttributeTargets.Method, attributeUsage.ValidOn);
        Assert.False(attributeUsage.AllowMultiple);
    }

    /// <summary>
    /// Test that StepName is required
    /// **Validates: Requirements 10.1, 10.2**
    /// </summary>
    [Fact]
    public void Constructor_WithStepName_SetsStepName()
    {
        // Arrange & Act
        var attribute = new ContextStepAttribute("test-step");
        
        // Assert
        Assert.Equal("test-step", attribute.StepName);
    }

    /// <summary>
    /// Test that StepName cannot be null
    /// **Validates: Requirements 10.1, 10.2**
    /// </summary>
    [Fact]
    public void Constructor_WithNullStepName_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ContextStepAttribute(null!));
    }

    /// <summary>
    /// Test that Tier is optional
    /// **Validates: Requirements 10.1, 10.2**
    /// </summary>
    [Fact]
    public void Tier_WhenNotSet_IsNull()
    {
        // Arrange & Act
        var attribute = new ContextStepAttribute("test-step");
        
        // Assert
        Assert.Null(attribute.Tier);
    }

    /// <summary>
    /// Test that Tier can be set
    /// **Validates: Requirements 10.1, 10.2**
    /// </summary>
    [Fact]
    public void Tier_WhenSet_RetainsValue()
    {
        // Arrange & Act
        var attribute = new ContextStepAttribute("test-step")
        {
            Tier = "premium"
        };
        
        // Assert
        Assert.Equal("premium", attribute.Tier);
    }

    /// <summary>
    /// Test that attribute can be retrieved from method with Tier set
    /// **Validates: Requirements 10.1, 10.2**
    /// </summary>
    [Fact]
    public void AttributeUsage_WithTier_RetrievesCorrectly()
    {
        // Arrange & Act
        var method = typeof(TestClass).GetMethod(nameof(TestClass.TestMethodWithTier));
        var attribute = method?.GetCustomAttributes(typeof(ContextStepAttribute), false)
            .FirstOrDefault() as ContextStepAttribute;
        
        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("test-step-with-tier", attribute.StepName);
        Assert.Equal("basic", attribute.Tier);
    }

    // Test helper class
    private class TestClass
    {
        [ContextStep("test-step")]
        public void TestMethod() { }

        [ContextStep("test-step-with-tier", Tier = "basic")]
        public void TestMethodWithTier() { }
    }
}
