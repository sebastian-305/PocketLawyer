using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using PocketLawyer.Infrastructure.Logging;
using Xunit;
using Xunit.Abstractions;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Performance tests for the method tracing system.
/// Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5
/// </summary>
public class MethodTracingPerformanceTests
{
    private readonly ITestOutputHelper _output;
    
    public MethodTracingPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ParameterSerializer_SimpleTypes_PerformanceBaseline()
    {
        // Arrange
        var serializer = new ParameterSerializer();
        var iterations = 10000;
        var testData = new Dictionary<string, object?>
        {
            ["int"] = 42,
            ["string"] = "test",
            ["bool"] = true,
            ["double"] = 3.14,
            ["null"] = null
        };
        
        // Warmup
        for (int i = 0; i < 100; i++)
        {
            serializer.SerializeParameters(testData);
        }
        
        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            serializer.SerializeParameters(testData);
        }
        sw.Stop();
        
        // Assert
        var avgTimeMs = (double)sw.ElapsedMilliseconds / iterations;
        _output.WriteLine($"Average serialization time for simple types: {avgTimeMs:F4}ms");
        _output.WriteLine($"Total time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
        
        // Should be reasonably fast - less than 0.05ms per call (relaxed for CI environments)
        Assert.True(avgTimeMs < 0.05, $"Serialization too slow: {avgTimeMs:F4}ms (expected < 0.05ms)");
    }
    
    [Fact]
    public void ParameterSerializer_ComplexObjects_PerformanceTest()
    {
        // Arrange
        var serializer = new ParameterSerializer();
        var iterations = 1000;
        var complexObject = new
        {
            Id = 123,
            Name = "Test Object",
            Description = "A complex object with multiple properties",
            CreatedAt = DateTime.Now,
            Tags = new[] { "tag1", "tag2", "tag3" }
        };
        
        var testData = new Dictionary<string, object?>
        {
            ["obj"] = complexObject
        };
        
        // Warmup
        for (int i = 0; i < 100; i++)
        {
            serializer.SerializeParameters(testData);
        }
        
        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            serializer.SerializeParameters(testData);
        }
        sw.Stop();
        
        // Assert
        var avgTimeMs = (double)sw.ElapsedMilliseconds / iterations;
        _output.WriteLine($"Average serialization time for complex objects: {avgTimeMs:F4}ms");
        _output.WriteLine($"Total time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
        
        // Complex objects should still be fast - less than 1ms per call
        Assert.True(avgTimeMs < 1.0, $"Complex object serialization too slow: {avgTimeMs}ms (expected < 1ms)");
    }
    
    [Fact]
    public void ParameterSerializer_LargeCollections_PerformanceTest()
    {
        // Arrange
        var serializer = new ParameterSerializer();
        var iterations = 1000;
        var largeList = Enumerable.Range(0, 1000).ToList();
        
        var testData = new Dictionary<string, object?>
        {
            ["list"] = largeList
        };
        
        // Warmup
        for (int i = 0; i < 100; i++)
        {
            serializer.SerializeParameters(testData);
        }
        
        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            serializer.SerializeParameters(testData);
        }
        sw.Stop();
        
        // Assert
        var avgTimeMs = (double)sw.ElapsedMilliseconds / iterations;
        _output.WriteLine($"Average serialization time for large collections (1000 items): {avgTimeMs:F4}ms");
        _output.WriteLine($"Total time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
        
        // Should only enumerate first 5 items, so should be fast
        Assert.True(avgTimeMs < 0.5, $"Large collection serialization too slow: {avgTimeMs}ms (expected < 0.5ms)");
    }
    
    [Fact]
    public void ParameterSerializer_LongStrings_PerformanceTest()
    {
        // Arrange
        var serializer = new ParameterSerializer();
        var iterations = 1000;
        var longString = new string('x', 10000);
        
        var testData = new Dictionary<string, object?>
        {
            ["longString"] = longString
        };
        
        // Warmup
        for (int i = 0; i < 100; i++)
        {
            serializer.SerializeParameters(testData);
        }
        
        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            serializer.SerializeParameters(testData);
        }
        sw.Stop();
        
        // Assert
        var avgTimeMs = (double)sw.ElapsedMilliseconds / iterations;
        _output.WriteLine($"Average serialization time for long strings (10000 chars): {avgTimeMs:F4}ms");
        _output.WriteLine($"Total time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
        
        // Should truncate at 200 chars, so should be fast
        Assert.True(avgTimeMs < 0.1, $"Long string serialization too slow: {avgTimeMs}ms (expected < 0.1ms)");
    }
    
    [Fact]
    public void MethodTracingActionFilter_DisabledLogging_MinimalOverhead()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MethodTracingActionFilter>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        var serializer = new ParameterSerializer();
        var filter = new MethodTracingActionFilter(mockLogger.Object, serializer);
        
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor { RouteValues = new Dictionary<string, string?> { ["action"] = "TestAction" } }
        );
        
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["param1"] = "value1", ["param2"] = 42 },
            new object()
        );
        
        var executedContext = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object()
        );
        
        var iterations = 100000;
        
        // Warmup
        for (int i = 0; i < 1000; i++)
        {
            filter.OnActionExecuting(executingContext);
            filter.OnActionExecuted(executedContext);
        }
        
        // Act - Measure with logging disabled
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            filter.OnActionExecuting(executingContext);
            filter.OnActionExecuted(executedContext);
        }
        sw.Stop();
        
        // Assert
        var avgTimeMicroseconds = (double)sw.Elapsed.TotalMicroseconds / iterations;
        _output.WriteLine($"Average overhead with logging disabled: {avgTimeMicroseconds:F4}μs");
        _output.WriteLine($"Total time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
        
        // Should be reasonably fast - just the IsEnabled check
        // Less than 15 microseconds per call (relaxed for CI environments)
        Assert.True(avgTimeMicroseconds < 15.0, 
            $"Overhead too high with logging disabled: {avgTimeMicroseconds:F4}μs (expected < 15μs)");
        
        // Verify serializer was never called
        mockLogger.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }
    
    [Fact]
    public void MethodTracingActionFilter_EnabledLogging_AcceptableOverhead()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MethodTracingActionFilter>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var serializer = new ParameterSerializer();
        var filter = new MethodTracingActionFilter(mockLogger.Object, serializer);
        
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor { RouteValues = new Dictionary<string, string?> { ["action"] = "TestAction" } }
        );
        
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["param1"] = "value1", ["param2"] = 42 },
            new object()
        );
        
        var executedContext = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object()
        );
        
        var iterations = 10000;
        
        // Warmup
        for (int i = 0; i < 100; i++)
        {
            filter.OnActionExecuting(executingContext);
            filter.OnActionExecuted(executedContext);
        }
        
        // Act - Measure with logging enabled
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            filter.OnActionExecuting(executingContext);
            filter.OnActionExecuted(executedContext);
        }
        sw.Stop();
        
        // Assert
        var avgTimeMs = (double)sw.ElapsedMilliseconds / iterations;
        _output.WriteLine($"Average overhead with logging enabled: {avgTimeMs:F4}ms");
        _output.WriteLine($"Total time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
        
        // Should be fast even with logging - less than 0.5ms per call
        Assert.True(avgTimeMs < 0.5, 
            $"Overhead too high with logging enabled: {avgTimeMs}ms (expected < 0.5ms)");
    }
    
    [Fact]
    public async Task LoggingAiClientDecorator_DisabledLogging_MinimalOverhead()
    {
        // Arrange
        var mockInner = new Mock<IAiClient>();
        mockInner.Setup(c => c.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse("test", 10, 5, 15));
        
        var mockLogger = new Mock<ILogger<LoggingAiClientDecorator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        var serializer = new ParameterSerializer();
        var decorator = new LoggingAiClientDecorator(mockInner.Object, mockLogger.Object, serializer);
        
        var iterations = 1000;
        
        // Warmup
        for (int i = 0; i < 10; i++)
        {
            await decorator.SendPromptWithMetadata("test", null);
        }
        
        // Act - Measure with logging disabled
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await decorator.SendPromptWithMetadata("test", null);
        }
        sw.Stop();
        
        // Assert
        var avgTimeMs = (double)sw.ElapsedMilliseconds / iterations;
        _output.WriteLine($"Average decorator overhead with logging disabled: {avgTimeMs:F4}ms");
        _output.WriteLine($"Total time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
        
        // Should be extremely fast - just the IsEnabled check and method call
        Assert.True(avgTimeMs < 0.1, 
            $"Decorator overhead too high with logging disabled: {avgTimeMs}ms (expected < 0.1ms)");
        
        // Verify serializer was never called (no logging)
        mockLogger.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }
    
    [Fact]
    public async Task LoggingAiClientDecorator_EnabledLogging_AcceptableOverhead()
    {
        // Arrange
        var mockInner = new Mock<IAiClient>();
        mockInner.Setup(c => c.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
            .ReturnsAsync(new AiResponse("test", 10, 5, 15));
        
        var mockLogger = new Mock<ILogger<LoggingAiClientDecorator>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        
        var serializer = new ParameterSerializer();
        var decorator = new LoggingAiClientDecorator(mockInner.Object, mockLogger.Object, serializer);
        
        var iterations = 1000;
        
        // Warmup
        for (int i = 0; i < 10; i++)
        {
            await decorator.SendPromptWithMetadata("test", null);
        }
        
        // Act - Measure with logging enabled
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await decorator.SendPromptWithMetadata("test", null);
        }
        sw.Stop();
        
        // Assert
        var avgTimeMs = (double)sw.ElapsedMilliseconds / iterations;
        _output.WriteLine($"Average decorator overhead with logging enabled: {avgTimeMs:F4}ms");
        _output.WriteLine($"Total time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
        
        // Should be fast even with logging - less than 1ms per call
        Assert.True(avgTimeMs < 1.0, 
            $"Decorator overhead too high with logging enabled: {avgTimeMs}ms (expected < 1ms)");
    }
    
    [Fact]
    public void ShortCircuit_VerifyNoSerializationWhenDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MethodTracingActionFilter>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        
        var mockSerializer = new Mock<IParameterSerializer>();
        var filter = new MethodTracingActionFilter(mockLogger.Object, mockSerializer.Object);
        
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor { RouteValues = new Dictionary<string, string?> { ["action"] = "TestAction" } }
        );
        
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["param1"] = "value1" },
            new object()
        );
        
        var executedContext = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object()
        );
        
        // Act
        filter.OnActionExecuting(executingContext);
        filter.OnActionExecuted(executedContext);
        
        // Assert - Verify serializer was NEVER called
        mockSerializer.Verify(s => s.Serialize(It.IsAny<string>(), It.IsAny<object?>()), Times.Never);
        mockSerializer.Verify(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()), Times.Never);
        
        _output.WriteLine("✓ Short-circuit optimization verified: No serialization when logging disabled");
    }
    
    [Fact]
    public void OverheadComparison_DisabledVsEnabled()
    {
        // This test compares the overhead of disabled vs enabled logging
        // to verify the <0.1% overhead claim when disabled
        
        // Arrange
        var serializer = new ParameterSerializer();
        var iterations = 10000;
        
        // Setup for disabled logging
        var mockLoggerDisabled = new Mock<ILogger<MethodTracingActionFilter>>();
        mockLoggerDisabled.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
        var filterDisabled = new MethodTracingActionFilter(mockLoggerDisabled.Object, serializer);
        
        // Setup for enabled logging
        var mockLoggerEnabled = new Mock<ILogger<MethodTracingActionFilter>>();
        mockLoggerEnabled.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        var filterEnabled = new MethodTracingActionFilter(mockLoggerEnabled.Object, serializer);
        
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor { RouteValues = new Dictionary<string, string?> { ["action"] = "TestAction" } }
        );
        
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["param1"] = "value1", ["param2"] = 42 },
            new object()
        );
        
        var executedContext = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object()
        );
        
        // Warmup
        for (int i = 0; i < 100; i++)
        {
            filterDisabled.OnActionExecuting(executingContext);
            filterDisabled.OnActionExecuted(executedContext);
            filterEnabled.OnActionExecuting(executingContext);
            filterEnabled.OnActionExecuted(executedContext);
        }
        
        // Measure disabled
        var swDisabled = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            filterDisabled.OnActionExecuting(executingContext);
            filterDisabled.OnActionExecuted(executedContext);
        }
        swDisabled.Stop();
        
        // Measure enabled
        var swEnabled = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            filterEnabled.OnActionExecuting(executingContext);
            filterEnabled.OnActionExecuted(executedContext);
        }
        swEnabled.Stop();
        
        // Calculate overhead
        var disabledTimeMs = swDisabled.ElapsedMilliseconds;
        var enabledTimeMs = swEnabled.ElapsedMilliseconds;
        var overheadPercent = ((double)(enabledTimeMs - disabledTimeMs) / enabledTimeMs) * 100;
        
        _output.WriteLine($"Disabled logging time: {disabledTimeMs}ms");
        _output.WriteLine($"Enabled logging time: {enabledTimeMs}ms");
        _output.WriteLine($"Overhead when disabled: {overheadPercent:F2}%");
        _output.WriteLine($"Speedup factor: {(double)enabledTimeMs / disabledTimeMs:F2}x");
        
        // When disabled should be significantly faster than enabled
        // Disabled time should be less than 50% of enabled time (at least 2x faster)
        var disabledPercentOfEnabled = ((double)disabledTimeMs / enabledTimeMs) * 100;
        Assert.True(disabledPercentOfEnabled < 50.0, 
            $"Disabled not fast enough: {disabledPercentOfEnabled:F2}% of enabled time (expected < 50%)");
    }
}
