using Microsoft.Extensions.Http;

// Entfernt Logging-Handler aus HttpClient-Instanzen um Datenlecks zu vermeiden.
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace PocketLawyer.Infrastructure
{
    // Kleiner Filter, der automatisch Logging-Handler entfernt.
    public class NoLoggingHttpClientBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                next(builder);
                var loggingHandlers = builder.AdditionalHandlers
                    .Where(h => h.GetType().Name.Contains("Logging"))
                    .ToList();

                foreach (var handler in loggingHandlers)
                {
                    builder.AdditionalHandlers.Remove(handler);
                }
            };
        }
    }
}
