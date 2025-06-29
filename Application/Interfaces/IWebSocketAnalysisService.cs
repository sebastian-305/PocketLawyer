using Microsoft.AspNetCore.Http;

namespace PocketLawyer.Application.Interfaces;

public interface IWebSocketAnalysisService
{
    Task HandleAsync(HttpContext context);
}
