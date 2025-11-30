using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;

namespace PocketLawyer.Infrastructure.Logging;

/// <summary>
/// Decorator that adds method tracing to IAiClient implementations.
/// Logs method entry, exit, duration, and exceptions at Verbose/Trace level.
/// </summary>
public class LoggingAiClientDecorator : IAiClient
{
    private readonly IAiClient _inner;
    private readonly ILogger<LoggingAiClientDecorator> _logger;
    private readonly IParameterSerializer _serializer;
    
    public LoggingAiClientDecorator(
        IAiClient inner,
        ILogger<LoggingAiClientDecorator> logger,
        IParameterSerializer serializer)
    {
        _inner = inner;
        _logger = logger;
        _serializer = serializer;
    }
    
    public async Task<string> SendPrompt(string prompt, object? outputSchema)
    {
        // Short-circuit if Trace logging is not enabled
        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            return await _inner.SendPrompt(prompt, outputSchema);
        }
        
        var serviceName = _inner.GetType().Name;
        var methodName = nameof(SendPrompt);
        
        // Serialize parameters
        var parameters = new Dictionary<string, object?>
        {
            ["prompt"] = prompt,
            ["outputSchema"] = outputSchema
        };
        var serializedParams = _serializer.SerializeParameters(parameters);
        
        // Log entry
        _logger.LogTrace(
            "Entering {ServiceName}.{MethodName} with parameters: {@Parameters}",
            serviceName,
            methodName,
            serializedParams);
        
        var sw = Stopwatch.StartNew();
        
        try
        {
            var result = await _inner.SendPrompt(prompt, outputSchema);
            
            sw.Stop();
            _logger.LogTrace(
                "Exiting {ServiceName}.{MethodName} in {Duration}ms",
                serviceName,
                methodName,
                sw.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogTrace(
                ex,
                "Exception in {ServiceName}.{MethodName} after {Duration}ms: {ExceptionType}",
                serviceName,
                methodName,
                sw.ElapsedMilliseconds,
                ex.GetType().Name);
            
            throw;
        }
    }
    
    public async Task<AiResponse> SendPromptWithMetadata(string prompt, object? outputSchema)
    {
        // Short-circuit if Trace logging is not enabled
        if (!_logger.IsEnabled(LogLevel.Trace))
        {
            return await _inner.SendPromptWithMetadata(prompt, outputSchema);
        }
        
        var serviceName = _inner.GetType().Name;
        var methodName = nameof(SendPromptWithMetadata);
        
        // Serialize parameters
        var parameters = new Dictionary<string, object?>
        {
            ["prompt"] = prompt,
            ["outputSchema"] = outputSchema
        };
        var serializedParams = _serializer.SerializeParameters(parameters);
        
        // Log entry
        _logger.LogTrace(
            "Entering {ServiceName}.{MethodName} with parameters: {@Parameters}",
            serviceName,
            methodName,
            serializedParams);
        
        var sw = Stopwatch.StartNew();
        
        try
        {
            var result = await _inner.SendPromptWithMetadata(prompt, outputSchema);
            
            sw.Stop();
            _logger.LogTrace(
                "Exiting {ServiceName}.{MethodName} in {Duration}ms",
                serviceName,
                methodName,
                sw.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogTrace(
                ex,
                "Exception in {ServiceName}.{MethodName} after {Duration}ms: {ExceptionType}",
                serviceName,
                methodName,
                sw.ElapsedMilliseconds,
                ex.GetType().Name);
            
            throw;
        }
    }
}
