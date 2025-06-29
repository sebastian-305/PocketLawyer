using PocketLawyer.Infrastructure.AiServices.Claude;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Infrastructure;
using PocketLawyer.Service;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Http;
using System.Linq;

// Einstiegspunkt der ASP.NET-Anwendung.
// Hier wird die WebApplication konfiguriert und alle benoetigten
// Abhaengigkeiten dem Dependency-Injection-Container hinzugefuegt.
var builder = WebApplication.CreateBuilder(args);

// -------- Service-Registrierungen --------

// HTTP-Handler fuer Logging von API-Aufrufen
builder.Services.AddTransient<RedactingLoggingHandler>();
// Entfernt automatisch eingebautes Logging der HttpClients
builder.Services.AddSingleton<IHttpMessageHandlerBuilderFilter, NoLoggingHttpClientBuilderFilter>();
// HttpClient fuer das Verwenden des Kontextmanagers der Gemini-API
builder.Services
    .AddHttpClient<IGeminiContextManager, GeminiContextManager>()
    .AddHttpMessageHandler<RedactingLoggingHandler>();
// HttpClient fuer den eigentlichen Zugriff auf die Gemini-API
builder.Services
    .AddHttpClient<GeminiService>()
    .AddHttpMessageHandler<RedactingLoggingHandler>();
// Die Anwendung kann unterschiedliche KI-Dienste nutzen.
// Per Keyed-Service wird festgelegt, welche Implementierung spaeter verwendet wird.
builder.Services.AddKeyedScoped<IAiService, GeminiService>("gemini");
builder.Services.AddKeyedScoped<IAiService, ClaudeService>("claude");
builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.TextFileDecoder>();
builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.PdfFileDecoder>();
builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.ImageFileDecoder>();
// ueber IFileDecoderService koennen Dateien unabhaengig vom Format eingelesen werden
builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
builder.Services.AddScoped<IFileValidatorService, FileValidatorService>();
builder.Services.AddScoped<IKollektivvertragService, KvFileService>();
builder.Services.AddScoped<IKvDetectionService, KvAiDetectionService>();
builder.Services.AddScoped<IResponseLogger, ResponseLogger>();
// Hauptlogik fuer die Analyse des Vertrages
builder.Services.AddScoped<IContractAnalyzerService, ContractAnalyzerService>();
//var secrets = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Jetzt wird die Anwendung gebaut
var app = builder.Build();

// -------- Middleware-Pipeline --------
app.UseExceptionHandler(errorApp =>
{
    // Eigener Error-Handler, der alle ungefangenen Fehler
    // als JSON an den Client zurueckgibt
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

// Weiterleitung auf HTTPS und Aktivierung von WebSockets
app.UseHttpsRedirection();
app.UseWebSockets();

if (app.Environment.IsDevelopment())
{
    // Im Entwicklungsmodus API-Dokumentation aktivieren
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Routing der API-Controller und Ausliefern statischer Dateien
app.MapControllers();
app.UseStaticFiles();
// Startet den Webserver
app.Run();
