using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;

namespace PocketLawyer.Infrastructure;

/// <summary>
/// Middleware that converts custom domain exceptions into HTTP responses.
/// </summary>
public class ExceptionTranslatorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionTranslatorMiddleware> _logger;

    public ExceptionTranslatorMiddleware(RequestDelegate next, ILogger<ExceptionTranslatorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FileValidationException ex)
        {
            _logger.LogWarning(ex, "File validation error for {File}", ex.FileName);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (AnalysisException ex)
        {
            _logger.LogError(ex, "Analysis error at {Step}", ex.AnalysisStep);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }
}
