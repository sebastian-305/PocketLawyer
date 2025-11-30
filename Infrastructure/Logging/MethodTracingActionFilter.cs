using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using PocketLawyer.Application.Interfaces;

namespace PocketLawyer.Infrastructure.Logging;

/// <summary>
/// Action filter that automatically logs controller method invocations with parameters at Trace level.
/// Measures execution duration and logs exceptions.
/// </summary>
public class MethodTracingActionFilter : IActionFilter
{
    private readonly ILogger<MethodTracingActionFilter> _logger;
    private readonly IParameterSerializer _serializer;
    private const string StopwatchKey = "MethodTracing_Stopwatch";
    
    public MethodTracingActionFilter(
        ILogger<MethodTracingActionFilter> logger,
        IParameterSerializer serializer)
    {
        _logger = logger;
        _serializer = serializer;
    }
    
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Short-circuit if Trace logging is not enabled
        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }
        
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var action) 
            ? action?.ToString() 
            : context.ActionDescriptor.DisplayName ?? "Unknown";
        
        // Serialize parameters
        var parameters = _serializer.SerializeParameters(context.ActionArguments);
        
        // Start timing
        var stopwatch = Stopwatch.StartNew();
        context.HttpContext.Items[StopwatchKey] = stopwatch;
        
        _logger.LogTrace(
            "Entering {ControllerName}.{ActionName} with parameters: {@Parameters}",
            controllerName,
            actionName,
            parameters);
    }
    
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Short-circuit if Trace logging is not enabled
        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            return;
        }
        
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var action) 
            ? action?.ToString() 
            : context.ActionDescriptor.DisplayName ?? "Unknown";
        
        // Get timing
        var stopwatch = context.HttpContext.Items[StopwatchKey] as Stopwatch;
        var duration = stopwatch?.ElapsedMilliseconds ?? 0;
        
        if (context.Exception != null)
        {
            _logger.LogTrace(
                "Exception in {ControllerName}.{ActionName} after {Duration}ms: {ExceptionType}",
                controllerName,
                actionName,
                duration,
                context.Exception.GetType().Name);
        }
        else
        {
            _logger.LogTrace(
                "Exiting {ControllerName}.{ActionName} in {Duration}ms",
                controllerName,
                actionName,
                duration);
        }
    }
}
