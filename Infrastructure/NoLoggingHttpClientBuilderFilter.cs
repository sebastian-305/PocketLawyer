using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace PocketLawyer.Infrastructure
{
    // Entfernt automatisch hinzugefügte Logging-Handler von HttpClient-
    // Instanzen, damit keine großen Anfrage- oder Antwort-Bodies ins Log
    // geschrieben werden.
    public class NoLoggingHttpClientBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        // Wird vom Framework aufgerufen, wenn ein HttpClient erzeugt wird.
        // Entfernt alle Logging-Handler, die von anderen Komponenten
        // hinzugefügt wurden, sodass nur unser eigener Handler übrig bleibt.
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
