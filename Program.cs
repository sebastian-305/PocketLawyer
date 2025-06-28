using PocketLawyer.Infrastructure.AiServices.Claude;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Infrastructure.KvDetection;
using PocketLawyer.Infrastructure;
using PocketLawyer.Service;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Http;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddTransient<RedactingLoggingHandler>();
builder.Services.AddSingleton<IHttpMessageHandlerBuilderFilter, NoLoggingHttpClientBuilderFilter>();
builder.Services
    .AddHttpClient<GeminiService>()
    .AddHttpMessageHandler<RedactingLoggingHandler>();
builder.Services.AddKeyedScoped<IAiService, GeminiService>("gemini");
builder.Services.AddKeyedScoped<IAiService, ClaudeService>("claude");
builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
builder.Services.AddScoped<IKvDetectionService, KvDetectionService>();
//var secrets = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
app.UseStaticFiles();
app.Run();
