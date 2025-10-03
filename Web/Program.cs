using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Http;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.UseCases;
using PocketLawyer.Infrastructure;
using PocketLawyer.Infrastructure.AiServices.Claude;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure.AiServices.Nebius;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Options;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddOptions<GeminiOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Gemini"))
    .ValidateDataAnnotations()
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Gemini:Key is required")
    .ValidateOnStart();

builder.Services
    .AddOptions<NebiusOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Nebius"))
    .ValidateDataAnnotations()
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Nebius:Key is required")
    .ValidateOnStart();

builder.Services
    .AddOptions<ClaudeOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Claude"))
    .ValidateDataAnnotations()
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Claude:Key is required")
    .ValidateOnStart();

builder.Services
    .AddOptions<PathOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Paths"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<UploadOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Upload"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

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
builder.Services.AddKeyedScoped<IAiClient, NebiusService>("nebius");

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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

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



app.UseDefaultFiles();   // sucht index.html in wwwroot
app.UseStaticFiles();    // wwwroot/** wird serviert

app.MapControllers();


app.MapFallbackToFile("index.html");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Run();
