using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PocketLawyer.Infrastructure.AiServices.Gemini;

public class RedactingLoggingHandler : DelegatingHandler
{
    private readonly ILogger<RedactingLoggingHandler> _logger;

    public RedactingLoggingHandler(ILogger<RedactingLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sanitizedUri = Sanitize(request.RequestUri);
        _logger.LogInformation("Sending request: {Method} {Uri}", request.Method, sanitizedUri);

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("Received response {StatusCode} from {Uri}", (int)response.StatusCode, sanitizedUri);
        return response;
    }

    private static string Sanitize(Uri? uri)
    {
        if (uri == null)
        {
            return string.Empty;
        }

        return uri.GetLeftPart(UriPartial.Path);
    }
}
