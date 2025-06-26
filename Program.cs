// Die folgenden using-Direktiven binden unsere internen Services ein,
// die wir innerhalb der ASP.NET-Anwendung verwenden.
using PocketLawyer.Infrastructure.AiServices.Claude;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Infrastructure;
using PocketLawyer.Service;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Http;
using System.Linq;

// Der WebApplicationBuilder richtet die grundlegende ASP.NET-Anwendung ein.
var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------
//  Registrierung aller benötigten Services im Dependency-Injection
//  Container. Dadurch können sie später bequem im Code angefordert
//  werden. Dieser Abschnitt ist das Herzstück des Startvorgangs.
// -------------------------------------------------------------

builder.Services.AddTransient<RedactingLoggingHandler>();
builder.Services.AddSingleton<IHttpMessageHandlerBuilderFilter, NoLoggingHttpClientBuilderFilter>();
builder.Services
    .AddHttpClient<GeminiService>()
    .AddHttpMessageHandler<RedactingLoggingHandler>();
builder.Services.AddKeyedScoped<IAiService, GeminiService>("gemini");
builder.Services.AddKeyedScoped<IAiService, ClaudeService>("claude");
builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
// Optional: Hier könnten User-Secrets eingebunden werden, falls
// sensible Daten außerhalb der Konfigurationsdatei gespeichert sind.
// var secrets = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Nachdem alle Services registriert sind, wird die App-Instanz erstellt.
var app = builder.Build();

// -------------------------------------------------------------
//  Ab hier wird die HTTP-Pipeline konfiguriert. Mittels Middleware
//  wird festgelegt, wie eingehende Requests verarbeitet werden.
// -------------------------------------------------------------
app.UseExceptionHandler(errorApp =>
{
    // Diese Middleware fängt unbehandelte Exceptions ab und
    // gibt eine einheitliche Fehlermeldung zurück.
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception occurred");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es erneut." });
    });
});
app.UseStatusCodePages();

// Leitet HTTP-Zugriffe automatisch auf HTTPS um.
app.UseHttpsRedirection();

// Im Entwicklungsmodus wird Swagger eingebunden,
// damit wir die API direkt testen können.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Aktiviert die Routen zu unseren Controllern.
app.MapControllers();
// Stellt Dateien aus dem wwwroot-Ordner bereit (z.B. JS, CSS).
app.UseStaticFiles();

// Startet den HTTP-Server und wartet auf Anfragen.
app.Run();
