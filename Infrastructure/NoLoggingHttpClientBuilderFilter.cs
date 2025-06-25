using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace PocketLawyer.Infrastructure
{
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
