namespace PocketLawyer.Service;

using Microsoft.AspNetCore.Http;

public interface IWebSocketAnalysisService
{
    Task HandleAsync(HttpContext context);
}
