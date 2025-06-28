using PocketLawyer.Infrastructure.AiServices.Claude;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Infrastructure;
using PocketLawyer.Service;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Http;
using System.Linq;

// Aufbau des ASP.NET-Core-Hosts (inklusive Konfiguration und DI-Container)
var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Registrierung der Anwendungsdienste, die per Dependency Injection
// in den Controllern und Services verwendet werden.
// ------------------------------------------------------------

builder.Services.AddTransient<RedactingLoggingHandler>();
builder.Services.AddSingleton<IHttpMessageHandlerBuilderFilter, NoLoggingHttpClientBuilderFilter>();
builder.Services
    .AddHttpClient<GeminiService>()
    .AddHttpMessageHandler<RedactingLoggingHandler>();
builder.Services.AddKeyedScoped<IAiService, GeminiService>("gemini");
// Alternativer KI-Dienst, der mit dem Key "claude" aufgelöst wird
builder.Services.AddKeyedScoped<IAiService, ClaudeService>("claude");
builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
builder.Services.AddScoped<IKollektivvertragService, KollektivvertragService>();
builder.Services.AddScoped<IKvDetectionService, KvDetectionService>();
//var secrets = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Anwendung mit den konfigurierten Diensten erstellen
var app = builder.Build();

// ------------------------------------------------------------
// Konfiguration der Middleware-Pipeline, die eingehende HTTP-Requests
// verarbeitet und Antworten erzeugt.
// ------------------------------------------------------------
app.UseExceptionHandler(errorApp =>
{
    // Eigener Handler, der unerwartete Exceptions protokolliert und dem
    // Client ein einfaches JSON-Fehlerobjekt zurückgibt.
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
// Gibt bei HTTP-Fehlercodes einfachen Text zurück, damit das Frontend
// ein einheitliches Antwortformat erhält.
app.UseStatusCodePages();

// Erzwingt HTTPS, um übertragene Daten zu schützen
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Aktiviert die Controller mit Attributrouting
app.MapControllers();
// Stellt die Dateien im Ordner wwwroot bereit
app.UseStaticFiles();
// Startet den Webserver
app.Run();
