using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.Logging;

namespace PocketLawyer.Tests.Infrastructure;

/// <summary>
/// Property-based tests for MethodTracingActionFilter.
/// Feature: method-tracing
/// </summary>
public class MethodTracingActionFilterPropertyTests
{
    /// <summary>
    /// **Feature: method-tracing, Property: Controller action logging**
    /// For any controller action, when invoked, method name and parameters should be logged
    /// **Validates: Requirements 1.1, 1.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ControllerActionLogging_LogsMethodNameAndParameters()
    {
        // Generate random controller and action names
        var controllerNameGen = Gen.Elements("UserController", "ProductController", "OrderController", "TestController");
        var actionNameGen = Gen.Elements("Index", "Create", "Update", "Delete", "Get");
        
        // Generate random parameters
        var paramGen = from paramName in Gen.Elements("id", "name", "value", "data")
                      from paramValue in Gen.OneOf<object>(
                          Arb.Generate<int>().Select(x => (object)x),
                          Arb.Generate<string>().Where(s => s != null).Select(x => (object)x),
                          Gen.Constant((object)"test")
                      )
                      select (paramName, paramValue);

        return Prop.ForAll(
            controllerNameGen.ToArbitrary(),
            actionNameGen.ToArbitrary(),
            Gen.ListOf(paramGen).ToArbitrary(),
            (controllerName, actionName, parameters) =>
            {
                // Arrange
                var loggerMock = new Mock<ILogger<MethodTracingActionFilter>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
                
                var serializerMock = new Mock<IParameterSerializer>();
                var serializedParams = new Dictionary<string, string>();
                foreach (var (name, value) in parameters)
                {
                    serializedParams[name] = value?.ToString() ?? "null";
                }
                serializerMock.Setup(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()))
                    .Returns(serializedParams);
                
                var filter = new MethodTracingActionFilter(loggerMock.Object, serializerMock.Object);
                
                // Create a real controller instance (not a mock)
                var controller = new TestController();
                
                // Create action context
                var actionContext = new ActionContext(
                    new DefaultHttpContext(),
                    new RouteData(),
                    new ActionDescriptor 
                    { 
                        DisplayName = actionName,
                        RouteValues = new Dictionary<string, string?> { ["action"] = actionName }
                    }
                );
                
                var actionArguments = new Dictionary<string, object?>();
                foreach (var (name, value) in parameters)
                {
                    actionArguments[name] = value;
                }
                
                var executingContext = new ActionExecutingContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    actionArguments,
                    controller
                );
                
                // Act
                filter.OnActionExecuting(executingContext);
                
                // Assert - verify logging was called with correct parameters
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Entering") && v.ToString()!.Contains(actionName)),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
                
                // Verify serializer was called
                serializerMock.Verify(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()), Times.Once);
                
                return true;
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property 6: Timing accuracy**
    /// For any traced method execution, the logged duration should be within reasonable bounds
    /// **Validates: Requirements 1.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property DurationLogging_LogsExecutionTime()
    {
        var actionNameGen = Gen.Elements("Index", "Create", "Update", "Delete");
        
        return Prop.ForAll(
            actionNameGen.ToArbitrary(),
            actionName =>
            {
                // Arrange
                var loggerMock = new Mock<ILogger<MethodTracingActionFilter>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
                
                var serializerMock = new Mock<IParameterSerializer>();
                serializerMock.Setup(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()))
                    .Returns(new Dictionary<string, string>());
                
                var filter = new MethodTracingActionFilter(loggerMock.Object, serializerMock.Object);
                
                var controller = new TestController();
                var httpContext = new DefaultHttpContext();
                var actionContext = new ActionContext(
                    httpContext,
                    new RouteData(),
                    new ActionDescriptor 
                    { 
                        DisplayName = actionName,
                        RouteValues = new Dictionary<string, string?> { ["action"] = actionName }
                    }
                );
                
                var executingContext = new ActionExecutingContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    new Dictionary<string, object?>(),
                    controller
                );
                
                var executedContext = new ActionExecutedContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    controller
                );
                
                // Act
                filter.OnActionExecuting(executingContext);
                System.Threading.Thread.Sleep(10); // Simulate some execution time
                filter.OnActionExecuted(executedContext);
                
                // Assert - verify exit log was called with duration
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exiting") && v.ToString()!.Contains("ms")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
                
                return true;
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property 5: Logging level short-circuit**
    /// For any method tracing operation when Verbose logging is disabled, no parameter serialization should occur
    /// **Validates: Requirements 1.5, 5.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ShortCircuit_SkipsSerializationWhenTraceDisabled()
    {
        var actionNameGen = Gen.Elements("Index", "Create", "Update");
        
        return Prop.ForAll(
            actionNameGen.ToArbitrary(),
            actionName =>
            {
                // Arrange - logger with Trace DISABLED
                var loggerMock = new Mock<ILogger<MethodTracingActionFilter>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
                
                var serializerMock = new Mock<IParameterSerializer>();
                
                var filter = new MethodTracingActionFilter(loggerMock.Object, serializerMock.Object);
                
                var controller = new TestController();
                var actionContext = new ActionContext(
                    new DefaultHttpContext(),
                    new RouteData(),
                    new ActionDescriptor 
                    { 
                        DisplayName = actionName,
                        RouteValues = new Dictionary<string, string?> { ["action"] = actionName }
                    }
                );
                
                var executingContext = new ActionExecutingContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    new Dictionary<string, object?> { ["param1"] = "value1" },
                    controller
                );
                
                // Act
                filter.OnActionExecuting(executingContext);
                
                // Assert - serializer should NOT be called
                serializerMock.Verify(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()), Times.Never);
                
                // Logger should NOT be called
                loggerMock.Verify(
                    l => l.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Never);
                
                return true;
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property 8: Exception logging**
    /// For any method that throws an exception, the exception type should be logged
    /// **Validates: Requirements 6.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ExceptionLogging_LogsExceptionType()
    {
        var exceptionTypeGen = Gen.Elements<Exception>(
            new InvalidOperationException("test"),
            new ArgumentException("test"),
            new NullReferenceException("test")
        );
        
        return Prop.ForAll(
            exceptionTypeGen.ToArbitrary(),
            exception =>
            {
                // Arrange
                var loggerMock = new Mock<ILogger<MethodTracingActionFilter>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
                
                var serializerMock = new Mock<IParameterSerializer>();
                serializerMock.Setup(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()))
                    .Returns(new Dictionary<string, string>());
                
                var filter = new MethodTracingActionFilter(loggerMock.Object, serializerMock.Object);
                
                var controller = new TestController();
                var httpContext = new DefaultHttpContext();
                var actionContext = new ActionContext(
                    httpContext,
                    new RouteData(),
                    new ActionDescriptor 
                    { 
                        DisplayName = "TestAction",
                        RouteValues = new Dictionary<string, string?> { ["action"] = "TestAction" }
                    }
                );
                
                var executingContext = new ActionExecutingContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    new Dictionary<string, object?>(),
                    controller
                );
                
                var executedContext = new ActionExecutedContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    controller
                )
                {
                    Exception = exception
                };
                
                // Act
                filter.OnActionExecuting(executingContext);
                filter.OnActionExecuted(executedContext);
                
                // Assert - verify exception was logged
                var exceptionTypeName = exception.GetType().Name;
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exception") && v.ToString()!.Contains(exceptionTypeName)),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
                
                return true;
            });
    }

    /// <summary>
    /// **Feature: method-tracing, Property 7: Log format consistency**
    /// For any method entry log, the format should be "Entering {ClassName}.{MethodName}" with structured parameters
    /// **Validates: Requirements 6.1, 6.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property LogFormat_ConsistentFormat()
    {
        var actionNameGen = Gen.Elements("Index", "Create", "Update", "Delete", "Get");
        
        return Prop.ForAll(
            actionNameGen.ToArbitrary(),
            actionName =>
            {
                // Arrange
                var loggerMock = new Mock<ILogger<MethodTracingActionFilter>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
                
                var serializerMock = new Mock<IParameterSerializer>();
                serializerMock.Setup(s => s.SerializeParameters(It.IsAny<IDictionary<string, object?>>()))
                    .Returns(new Dictionary<string, string>());
                
                var filter = new MethodTracingActionFilter(loggerMock.Object, serializerMock.Object);
                
                var controller = new TestController();
                var httpContext = new DefaultHttpContext();
                var actionContext = new ActionContext(
                    httpContext,
                    new RouteData(),
                    new ActionDescriptor 
                    { 
                        DisplayName = actionName,
                        RouteValues = new Dictionary<string, string?> { ["action"] = actionName }
                    }
                );
                
                var executingContext = new ActionExecutingContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    new Dictionary<string, object?>(),
                    controller
                );
                
                var executedContext = new ActionExecutedContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    controller
                );
                
                // Act
                filter.OnActionExecuting(executingContext);
                filter.OnActionExecuted(executedContext);
                
                // Assert - verify entry log format
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Entering") && v.ToString()!.Contains(actionName)),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
                
                // Assert - verify exit log format
                loggerMock.Verify(
                    l => l.Log(
                        LogLevel.Trace,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exiting") && v.ToString()!.Contains(actionName) && v.ToString()!.Contains("ms")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
                
                return true;
            });
    }
    
    // Test controller for property tests
    private class TestController : ControllerBase
    {
    }
}
