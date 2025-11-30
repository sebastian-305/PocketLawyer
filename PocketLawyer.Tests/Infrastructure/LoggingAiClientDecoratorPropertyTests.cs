using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using PocketLawyer.Infrastructure.Logging;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Property-based tests for LoggingAiClientDecorator.
/// Feature: method-tracing
/// </summary>
public class LoggingAiClientDecoratorPropertyTests
{
    /// <summary>
    /// **Feature: method-tracing, Property: Service method logging**
    /// For any decorated service method, when invoked, method name and parameters should be logged
    /// **Validates: Requirements 3.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ServiceMethodLogging_LogsMethodNameAndParameters()
    {
        // Generate random prompts and schemas
        var promptGen = Arb.Generate<NonEmptyString>().Select(s => s.Get);
        var schemaGen = Gen.OneOf(
            Gen.Constant<object?>(null),
            Gen.Constant<object?>(new { type = "object" }),
            Gen.Constant<object?>(new { type = "string" })
        );
        
        return Prop.ForAll(
            promptGen.ToArbitrary(),
            schemaGen.ToArbitrary(),
            (prompt, schema) =>
            {
                // Arrange
                var innerMock = new Mock<IAiClient>();
                var expectedResponse = new AiResponse("test response", 10, 20, 30);
                innerMock.Setup(c => c.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
                    .ReturnsAsync(expectedResponse);
                
                var loggerMock = new Mock<ILogger<LoggingAiClientDecorator>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
                
                var serializerMock = new Mock<IParameterSerializer>();
                serializerMock.Setup(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()))
                    .Returns(new Dictionary<string, string> 
                    { 
                        ["prompt"] = prompt.Substring(0, Math.Min(50, prompt.Length)),
                        ["outputSchema"] = schema?.ToString() ?? "null"
                    });
                
                var decorator = new LoggingAiClientDecorator(
                    innerMock.Object,
                    loggerMock.Object,
                    serializerMock.Object);
                
                // Act
                var task = decorator.SendPromptWithMetadata(prompt, schema);
                task.Wait();
                
                // Assert - verify logging was called with method name
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => 
                            v.ToString()!.Contains("Entering") && 
                            v.ToString()!.Contains("SendPromptWithMetadata")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
                
                // Verify serializer was called with parameters
                serializerMock.Verify(
                    s => s.SerializeParameters(It.Is<IDictionary<string, object?>>(d => 
                        d.ContainsKey("prompt") && d.ContainsKey("outputSchema"))),
                    Times.Once);
                
                // Verify inner service was called
                innerMock.Verify(c => c.SendPromptWithMetadata(prompt, schema), Times.Once);
                
                return true;
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property: Service duration logging**
    /// For any decorated service method, execution duration should be logged
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ServiceDurationLogging_LogsExecutionTime()
    {
        var promptGen = Arb.Generate<NonEmptyString>().Select(s => s.Get);
        
        return Prop.ForAll(
            promptGen.ToArbitrary(),
            prompt =>
            {
                // Arrange
                var innerMock = new Mock<IAiClient>();
                var expectedResponse = new AiResponse("test response", 10, 20, 30);
                innerMock.Setup(c => c.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
                    .ReturnsAsync(expectedResponse);
                
                var loggerMock = new Mock<ILogger<LoggingAiClientDecorator>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
                
                var serializerMock = new Mock<IParameterSerializer>();
                serializerMock.Setup(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()))
                    .Returns(new Dictionary<string, string>());
                
                var decorator = new LoggingAiClientDecorator(
                    innerMock.Object,
                    loggerMock.Object,
                    serializerMock.Object);
                
                // Act
                var task = decorator.SendPromptWithMetadata(prompt, null);
                task.Wait();
                
                // Assert - verify exit log was called with duration
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => 
                            v.ToString()!.Contains("Exiting") && 
                            v.ToString()!.Contains("ms")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
                
                return true;
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property: Service exception logging**
    /// For any decorated service method that throws, exception should be logged before re-throw
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ServiceExceptionLogging_LogsExceptionBeforeRethrow()
    {
        var promptGen = Arb.Generate<NonEmptyString>().Select(s => s.Get);
        var exceptionGen = Gen.Elements<Exception>(
            new InvalidOperationException("test"),
            new ArgumentException("test"),
            new TimeoutException("test")
        );
        
        return Prop.ForAll(
            promptGen.ToArbitrary(),
            exceptionGen.ToArbitrary(),
            (prompt, exception) =>
            {
                // Arrange
                var innerMock = new Mock<IAiClient>();
                innerMock.Setup(c => c.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
                    .ThrowsAsync(exception);
                
                var loggerMock = new Mock<ILogger<LoggingAiClientDecorator>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
                
                var serializerMock = new Mock<IParameterSerializer>();
                serializerMock.Setup(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()))
                    .Returns(new Dictionary<string, string>());
                
                var decorator = new LoggingAiClientDecorator(
                    innerMock.Object,
                    loggerMock.Object,
                    serializerMock.Object);
                
                // Act & Assert - verify exception is re-thrown
                Exception? thrownException = null;
                try
                {
                    var task = decorator.SendPromptWithMetadata(prompt, null);
                    task.Wait();
                }
                catch (AggregateException ae)
                {
                    thrownException = ae.InnerException;
                }
                
                // Verify exception was re-thrown
                if (thrownException == null || thrownException.GetType() != exception.GetType())
                {
                    return false;
                }
                
                // Verify exception was logged
                var exceptionTypeName = exception.GetType().Name;
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => 
                            v.ToString()!.Contains("Exception") && 
                            v.ToString()!.Contains(exceptionTypeName) &&
                            v.ToString()!.Contains("ms")),
                        It.Is<Exception>(e => e == exception),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
                
                return true;
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property: Short-circuit optimization**
    /// For any decorated service method when Trace logging is disabled, no serialization should occur
    /// **Validates: Requirements 5.1, 5.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ShortCircuit_SkipsSerializationWhenTraceDisabled()
    {
        var promptGen = Arb.Generate<NonEmptyString>().Select(s => s.Get);
        
        return Prop.ForAll(
            promptGen.ToArbitrary(),
            prompt =>
            {
                // Arrange - logger with Trace DISABLED
                var innerMock = new Mock<IAiClient>();
                var expectedResponse = new AiResponse("test response", 10, 20, 30);
                innerMock.Setup(c => c.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object?>()))
                    .ReturnsAsync(expectedResponse);
                
                var loggerMock = new Mock<ILogger<LoggingAiClientDecorator>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
                
                var serializerMock = new Mock<IParameterSerializer>();
                
                var decorator = new LoggingAiClientDecorator(
                    innerMock.Object,
                    loggerMock.Object,
                    serializerMock.Object);
                
                // Act
                var task = decorator.SendPromptWithMetadata(prompt, null);
                task.Wait();
                
                // Assert - serializer should NOT be called
                serializerMock.Verify(
                    s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()), 
                    Times.Never);
                
                // Logger should NOT be called (except for IsEnabled check)
                loggerMock.Verify(
                    l => l.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Never);
                
                // Inner service should still be called
                innerMock.Verify(c => c.SendPromptWithMetadata(prompt, null), Times.Once);
                
                return true;
            });
    }
}
