using Chatbot_in.Domain.Service;
using PocketLawyer.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IAiService, GeminiService>();
var secrets = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();





app.MapGet("/ai", async () =>
{
    IAiService ai = new GeminiService(secrets, new Logger<GeminiService>(new LoggerFactory()));

    var answer = await ai.SendPrompt("Give me a prime number");

    return answer;

});

app.UseStaticFiles();
app.Run();

