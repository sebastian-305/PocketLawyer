using Microsoft.AspNetCore.Http;
using PocketLawyer.Web;

namespace PocketLawyer.Application.Interfaces;

public interface IWebSocketAnalysisService
{
    Task HandleAsync(HttpContext context, AnalysisType type);
}
