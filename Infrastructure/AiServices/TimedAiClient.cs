namespace PocketLawyer.Infrastructure.AiServices;

using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
using System.Diagnostics;

public sealed class TimedAiClient : IAiClient
{
    private readonly IAiClient _inner;
    private readonly ILogger<TimedAiClient> _log;

    public TimedAiClient(IAiClient inner, ILogger<TimedAiClient> log)
    {
        _inner = inner;
        _log = log;
    }

    public async Task<string> SendPrompt(string prompt, object? outputSchema)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await _inner.SendPrompt(prompt, outputSchema);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogError(ex,
                "IAiClient.SendPrompt (impl: {Impl}) failed after {Ms} ms, OutputSchema: {out}",
                _inner.GetType().Name, sw.ElapsedMilliseconds, SchemaNameDetectionService.GetName(outputSchema));
            throw;
        }
        finally
        {
            if (sw.IsRunning) sw.Stop();
            _log.LogInformation(
                "IAiClient.SendPrompt (impl: {Impl}) took {Ms} ms, OutputSchma: {out}",
                _inner.GetType().Name, sw.ElapsedMilliseconds, SchemaNameDetectionService.GetName(outputSchema));
        }
    }

    public async Task<AiResponse> SendPromptWithMetadata(string prompt, object? outputSchema)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await _inner.SendPromptWithMetadata(prompt, outputSchema);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogError(ex,
                "IAiClient.SendPromptWithMetadata (impl: {Impl}) failed after {Ms} ms, OutputSchema: {out}",
                _inner.GetType().Name, sw.ElapsedMilliseconds, SchemaNameDetectionService.GetName(outputSchema));
            throw;
        }
        finally
        {
            if (sw.IsRunning) sw.Stop();
            _log.LogInformation(
                "IAiClient.SendPromptWithMetadata (impl: {Impl}) took {Ms} ms, OutputSchma: {out}",
                _inner.GetType().Name, sw.ElapsedMilliseconds, SchemaNameDetectionService.GetName(outputSchema));
        }
    }
}
