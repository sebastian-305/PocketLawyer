using PocketLawyer.Infrastructure.AiServices.Claude;

// Einstiegspunkt der Webanwendung. Registriert alle Services und Middleware.
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Infrastructure;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using PocketLawyer.Application.UseCases;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.Configure<PathOptions>(builder.Configuration.GetSection("Paths"));
builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection("Upload"));
builder.Services.Configure<ClaudeOptions>(builder.Configuration.GetSection("Claude"));

builder.Services.AddTransient<RedactingLoggingHandler>();
builder.Services.AddSingleton<IHttpMessageHandlerBuilderFilter, NoLoggingHttpClientBuilderFilter>();
builder.Services
    .AddHttpClient<IGeminiContextManager, GeminiContextManager>()
    .AddHttpMessageHandler<RedactingLoggingHandler>(); // IWebHostEnvironment injected
builder.Services
    .AddHttpClient<GeminiService>()
    .AddHttpMessageHandler<RedactingLoggingHandler>();
builder.Services.AddKeyedScoped<IAiClient, GeminiService>("gemini");
builder.Services.AddKeyedScoped<IAiClient, ClaudeService>("claude");
builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.TextFileDecoder>();
builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.PdfFileDecoder>();
builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.ImageFileDecoder>();
builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
builder.Services.AddScoped<IFileValidatorService, FileValidatorService>();
builder.Services.AddScoped<IKollektivvertragService, KvFileService>();
builder.Services.AddScoped<IKvDetectionService, KvAiDetectionService>();
builder.Services.AddScoped<IResponseLogger, ResponseLogger>();
builder.Services.AddScoped<IContractAnalyzerService, AnalyzeContractUseCase>();
builder.Services.AddScoped<IWebSocketAnalysisService, WebSocketAnalysisService>();
//var secrets = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionTranslatorMiddleware>();
app.UseExceptionHandler(errorApp =>
{
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

app.UseHttpsRedirection();
app.UseWebSockets();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
app.UseStaticFiles();
app.Run();
