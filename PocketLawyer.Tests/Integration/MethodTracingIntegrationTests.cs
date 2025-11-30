using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using PocketLawyer.Infrastructure.Logging;
using Serilog;
using Serilog.Events;

namespace PocketLawyer.Tests.Integration;

/// <summary>
/// Integration tests for method tracing functionality.
/// Verifies end-to-end controller action tracing and service method tracing.
/// **Validates: Requirements 1.1, 1.2, 1.4, 3.2, 3.3**
/// </summary>
public class MethodTracingIntegrationTests : IDisposable
{
    private readonly string _testLogPath;
    private readonly List<LogEvent> _logEvents;

    public MethodTracingIntegrationTests()
    {
        _testLogPath = Path.Combine(Path.GetTempPath(), $"method-tracing-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testLogPath);
        _logEvents = new List<LogEvent>();
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
        
        if (Directory.Exists(_testLogPath))
        {
            try
            {
                Directory.Delete(_testLogPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void ControllerActionFilter_WithVerboseEnabled_LogsEntryAndExit()
    {
        // Arrange
        _logEvents.Clear();
        var logger = CreateInMemoryLogger<MethodTracingActionFilter>(verboseEnabled: true);
        var serializer = new ParameterSerializer();
        var filter = new MethodTracingActionFilter(logger, serializer);

        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.RouteValues["action"] = "TestAction";
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            actionDescriptor);
        
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["param1"] = "value1" },
            new object());

        var executedContext = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object());

        // Act
        filter.OnActionExecuting(executingContext);
        filter.OnActionExecuted(executedContext);

        // Assert
        // Verify entry log
        var entryLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Entering") && 
            e.MessageTemplate.Text.Contains("{ActionName}"));
        Assert.NotNull(entryLog);
        Assert.Equal(LogEventLevel.Verbose, entryLog.Level);
        
        // Verify the action name is in the properties
        Assert.True(entryLog.Properties.ContainsKey("ActionName"));
        Assert.Equal("TestAction", entryLog.Properties["ActionName"].ToString().Trim('"'));

        // Verify exit log
        var exitLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Exiting") && 
            e.MessageTemplate.Text.Contains("{ActionName}"));
        Assert.NotNull(exitLog);
        Assert.Equal(LogEventLevel.Verbose, exitLog.Level);
        
        // Verify the action name is in the properties
        Assert.True(exitLog.Properties.ContainsKey("ActionName"));
        Assert.Equal("TestAction", exitLog.Properties["ActionName"].ToString().Trim('"'));
    }

    [Fact]
    public void ControllerActionFilter_WithVerboseDisabled_DoesNotLogTrace()
    {
        // Arrange
        _logEvents.Clear();
        var logger = CreateInMemoryLogger<MethodTracingActionFilter>(verboseEnabled: false);
        var serializer = new ParameterSerializer();
        var filter = new MethodTracingActionFilter(logger, serializer);

        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.RouteValues["action"] = "TestAction";
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            actionDescriptor);
        
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["param1"] = "value1" },
            new object());

        var executedContext = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object());

        // Act
        filter.OnActionExecuting(executingContext);
        filter.OnActionExecuted(executedContext);

        // Assert
        // Verify no trace logs
        var traceLogs = _logEvents.Where(e => 
            e.Level == LogEventLevel.Verbose && 
            (e.MessageTemplate.Text.Contains("Entering") || e.MessageTemplate.Text.Contains("Exiting")));
        Assert.Empty(traceLogs);
    }

    [Fact]
    public void ControllerActionFilter_LogsParameters()
    {
        // Arrange
        _logEvents.Clear();
        var logger = CreateInMemoryLogger<MethodTracingActionFilter>(verboseEnabled: true);
        var serializer = new ParameterSerializer();
        var filter = new MethodTracingActionFilter(logger, serializer);

        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.RouteValues["action"] = "TestAction";
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            actionDescriptor);
        
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["param1"] = "value1", ["param2"] = 42 },
            new object());

        // Act
        filter.OnActionExecuting(executingContext);

        // Assert
        // Verify parameters are logged
        var entryLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Entering") && 
            e.MessageTemplate.Text.Contains("parameters"));
        Assert.NotNull(entryLog);
        
        var parametersProperty = entryLog.Properties.FirstOrDefault(p => p.Key == "Parameters");
        Assert.NotEqual(default, parametersProperty);
    }

    [Fact]
    public void ControllerActionFilter_LogsDuration()
    {
        // Arrange
        _logEvents.Clear();
        var logger = CreateInMemoryLogger<MethodTracingActionFilter>(verboseEnabled: true);
        var serializer = new ParameterSerializer();
        var filter = new MethodTracingActionFilter(logger, serializer);

        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.RouteValues["action"] = "TestAction";
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            actionDescriptor);
        
        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        var executedContext = new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object());

        // Act
        filter.OnActionExecuting(executingContext);
        System.Threading.Thread.Sleep(10); // Ensure some time passes
        filter.OnActionExecuted(executedContext);

        // Assert
        // Verify duration is logged
        var exitLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Exiting") && 
            e.MessageTemplate.Text.Contains("Duration"));
        Assert.NotNull(exitLog);
        
        var durationProperty = exitLog.Properties.FirstOrDefault(p => p.Key == "Duration");
        Assert.NotEqual(default, durationProperty);
    }

    [Fact]
    public async Task ServiceMethod_WithVerboseEnabled_LogsEntryAndExit()
    {
        // Arrange
        _logEvents.Clear();
        var mockInnerClient = new Mock<IAiClient>();
        mockInnerClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new AiResponse("test response", 10, 5, 15));

        var logger = CreateInMemoryLogger<LoggingAiClientDecorator>(verboseEnabled: true);
        var serializer = new ParameterSerializer();
        var decorator = new LoggingAiClientDecorator(mockInnerClient.Object, logger, serializer);

        // Act
        var result = await decorator.SendPromptWithMetadata("test prompt", null);

        // Assert
        Assert.NotNull(result);

        // Wait a bit for logs to be written
        await Task.Delay(50);

        // Verify entry log
        var entryLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Entering") && 
            e.MessageTemplate.Text.Contains("{MethodName}"));
        Assert.NotNull(entryLog);
        Assert.Equal(LogEventLevel.Verbose, entryLog.Level);

        // Verify exit log
        var exitLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Exiting") && 
            e.MessageTemplate.Text.Contains("{MethodName}"));
        Assert.NotNull(exitLog);
        Assert.Equal(LogEventLevel.Verbose, exitLog.Level);
    }

    [Fact]
    public async Task ServiceMethod_WithVerboseDisabled_DoesNotLogTrace()
    {
        // Arrange
        _logEvents.Clear();
        var mockInnerClient = new Mock<IAiClient>();
        mockInnerClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new AiResponse("test response", 10, 5, 15));

        var logger = CreateInMemoryLogger<LoggingAiClientDecorator>(verboseEnabled: false);
        var serializer = new ParameterSerializer();
        var decorator = new LoggingAiClientDecorator(mockInnerClient.Object, logger, serializer);

        // Act
        var result = await decorator.SendPromptWithMetadata("test prompt", null);

        // Assert
        Assert.NotNull(result);

        // Wait a bit for logs to be written
        await Task.Delay(50);

        // Verify no trace logs
        var traceLogs = _logEvents.Where(e => 
            e.Level == LogEventLevel.Verbose && 
            (e.MessageTemplate.Text.Contains("Entering") || e.MessageTemplate.Text.Contains("Exiting")));
        Assert.Empty(traceLogs);
    }

    [Fact]
    public async Task ServiceMethod_LogsParameters()
    {
        // Arrange
        _logEvents.Clear();
        var mockInnerClient = new Mock<IAiClient>();
        mockInnerClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new AiResponse("test response", 10, 5, 15));

        var logger = CreateInMemoryLogger<LoggingAiClientDecorator>(verboseEnabled: true);
        var serializer = new ParameterSerializer();
        var decorator = new LoggingAiClientDecorator(mockInnerClient.Object, logger, serializer);

        // Act
        var result = await decorator.SendPromptWithMetadata("test prompt", null);

        // Assert
        Assert.NotNull(result);

        // Wait a bit for logs to be written
        await Task.Delay(50);

        // Verify parameters are logged
        var entryLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Entering") && 
            e.MessageTemplate.Text.Contains("parameters"));
        Assert.NotNull(entryLog);
        
        var parametersProperty = entryLog.Properties.FirstOrDefault(p => p.Key == "Parameters");
        Assert.NotEqual(default, parametersProperty);
    }

    [Fact]
    public async Task ServiceMethod_LogsDuration()
    {
        // Arrange
        _logEvents.Clear();
        var mockInnerClient = new Mock<IAiClient>();
        mockInnerClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new AiResponse("test response", 10, 5, 15));

        var logger = CreateInMemoryLogger<LoggingAiClientDecorator>(verboseEnabled: true);
        var serializer = new ParameterSerializer();
        var decorator = new LoggingAiClientDecorator(mockInnerClient.Object, logger, serializer);

        // Act
        var result = await decorator.SendPromptWithMetadata("test prompt", null);

        // Assert
        Assert.NotNull(result);

        // Wait a bit for logs to be written
        await Task.Delay(50);

        // Verify duration is logged
        var exitLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Exiting") && 
            e.MessageTemplate.Text.Contains("Duration"));
        Assert.NotNull(exitLog);
        
        var durationProperty = exitLog.Properties.FirstOrDefault(p => p.Key == "Duration");
        Assert.NotEqual(default, durationProperty);
    }

    [Fact]
    public async Task ServiceMethod_WhenExceptionThrown_LogsException()
    {
        // Arrange
        _logEvents.Clear();
        var mockInnerClient = new Mock<IAiClient>();
        mockInnerClient
            .Setup(x => x.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var logger = CreateInMemoryLogger<LoggingAiClientDecorator>(verboseEnabled: true);
        var serializer = new ParameterSerializer();
        var decorator = new LoggingAiClientDecorator(mockInnerClient.Object, logger, serializer);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await decorator.SendPromptWithMetadata("test prompt", null));

        // Wait a bit for logs to be written
        await Task.Delay(50);

        // Verify exception log
        var exceptionLog = _logEvents.FirstOrDefault(e => 
            e.MessageTemplate.Text.Contains("Exception in") && 
            e.MessageTemplate.Text.Contains("{MethodName}"));
        Assert.NotNull(exceptionLog);
        Assert.Equal(LogEventLevel.Verbose, exceptionLog.Level);
        Assert.Contains("InvalidOperationException", exceptionLog.RenderMessage());
    }

    #region Helper Methods

    private ILogger<T> CreateInMemoryLogger<T>(bool verboseEnabled)
    {
        var minimumLevel = verboseEnabled ? LogEventLevel.Verbose : LogEventLevel.Information;
        
        // Create a new logger configuration for each test
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .Enrich.FromLogContext()
            .WriteTo.Sink(new InMemoryLogSink(_logEvents));

        var serilogLogger = loggerConfig.CreateLogger();
        
        // Set as global logger to ensure it's used
        Log.Logger = serilogLogger;

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(verboseEnabled ? Microsoft.Extensions.Logging.LogLevel.Trace : Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddSerilog(serilogLogger, dispose: false);
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ILogger<T>>();
    }

    #endregion

    #region InMemoryLogSink

    private class InMemoryLogSink : Serilog.Core.ILogEventSink
    {
        private readonly List<LogEvent> _logEvents;

        public InMemoryLogSink(List<LogEvent> logEvents)
        {
            _logEvents = logEvents;
        }

        public void Emit(LogEvent logEvent)
        {
            _logEvents.Add(logEvent);
        }
    }

    #endregion
}
