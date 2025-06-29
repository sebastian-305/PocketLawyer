using Microsoft.AspNetCore.Http;

namespace PocketLawyer.Application.Interfaces;

// Behandelt eine Analyseanfrage über WebSockets.

// Definiert die zentrale Methode zur Bearbeitung über Sockets.
public interface IWebSocketAnalysisService
{
    Task HandleAsync(HttpContext context);
}
