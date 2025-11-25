using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Http;
using PocketLawyer.Application.Agents;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.UseCases;
using PocketLawyer.Infrastructure;
using PocketLawyer.Infrastructure.AiServices;
using PocketLawyer.Infrastructure.AiServices.Claude;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure.AiServices.Nebius;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Infrastructure.SchemaProviders;
using PocketLawyer.Options;
using Serilog;
using Serilog.Events;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var fileLoggingOptions = builder.Configuration.GetSection("Logging:File").Get<FileLoggingOptions>() ?? new FileLoggingOptions();

builder.Host.UseSerilog((context, configuration) =>
{
    var isDevelopment = context.HostingEnvironment.IsDevelopment();
    var consoleLogLevel = isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information;
    var fileMinimumLevel = Enum.TryParse(fileLoggingOptions.MinimumLevel, true, out LogEventLevel parsedFileLevel)
        ? parsedFileLevel
        : LogEventLevel.Information;

    configuration
        .ReadFrom.Configuration(context.Configuration);

    if (isDevelopment)
    {
        configuration.MinimumLevel.Is(LogEventLevel.Debug);
    }

    configuration
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Extensions.Http", LogEventLevel.Warning)
        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(restrictedToMinimumLevel: consoleLogLevel);

    // Add file logging if enabled - always at Information level
    if (fileLoggingOptions.Enabled)
    {
        configuration.WriteTo.File(
            path: System.IO.Path.Combine(fileLoggingOptions.Path, "log-.txt"),
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: fileMinimumLevel,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
    }
});

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

// inner (echte Implementierung)
builder.Services.AddKeyedScoped<IAiClient, GeminiService>("gemini:inner");

// outer (Decorator) – Factory hat (sp, key)!
builder.Services.AddKeyedScoped<IAiClient>("gemini", (sp, key) =>
    new TimedAiClient(
        sp.GetRequiredKeyedService<IAiClient>("gemini:inner"),
        sp.GetRequiredService<ILogger<TimedAiClient>>()));

// inner (echte Implementierung)
builder.Services.AddKeyedScoped<IAiClient, ClaudeService>("claude:inner");

// outer (Decorator) – Factory hat (sp, key)!
builder.Services.AddKeyedScoped<IAiClient>("claude", (sp, key) =>
    new TimedAiClient(
        sp.GetRequiredKeyedService<IAiClient>("claude:inner"),
        sp.GetRequiredService<ILogger<TimedAiClient>>()));

// inner (echte Implementierung)
builder.Services.AddKeyedScoped<IAiClient, NebiusService>("nebius:inner");

// outer (Decorator) – Factory hat (sp, key)!
builder.Services.AddKeyedScoped<IAiClient>("nebius", (sp, key) =>
    new TimedAiClient(
        sp.GetRequiredKeyedService<IAiClient>("nebius:inner"),
        sp.GetRequiredService<ILogger<TimedAiClient>>()));

builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.TextFileDecoder>();
builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.PdfFileDecoder>();
builder.Services.AddScoped<IFileDecoder, PocketLawyer.Infrastructure.FileDecoding.Strategies.ImageFileDecoder>();
builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
builder.Services.AddScoped<IFileValidatorService, FileValidatorService>();
builder.Services.AddScoped<IKollektivvertragService, KvFileService>();
builder.Services.AddScoped<IKvDetectionService, KvAiDetectionService>();
builder.Services.AddScoped<IResponseLogger, ResponseLogger>();

// Schema Provider and Validator (Singletons for performance)
builder.Services.AddSingleton<ISchemaProvider, PocketLawyer.Infrastructure.SchemaProviders.SchemaProvider>();
builder.Services.AddSingleton<ISchemaValidator, PocketLawyer.Infrastructure.SchemaValidator>();

// New Contract Analysis Agents (Scoped - both keyed and direct registration)
builder.Services.AddScoped<RentalContractAgent>();
builder.Services.AddScoped<EmploymentContractAgent>();
builder.Services.AddKeyedScoped<IContractAnalysisAgent, RentalContractAgent>("rental");
builder.Services.AddKeyedScoped<IContractAnalysisAgent, EmploymentContractAgent>("employment");

// WebSocket Analysis Handler
builder.Services.AddScoped<WebSocketAnalysisHandler>();
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
