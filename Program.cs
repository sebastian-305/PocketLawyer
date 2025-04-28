using Chatbot_in.Domain.Service;
using Microsoft.AspNetCore.Mvc;
using PocketLawyer.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IAiService, GeminiService>();
var secrets = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();





app.MapPost("/ai", async ([FromBody] string body) =>
{
    IAiService ai = new GeminiService(secrets, new Logger<GeminiService>(new LoggerFactory()));
    Console.WriteLine(body);
    var answer = await ai.SendPrompt("Give me a prime number");
    Console.WriteLine();
    return answer;

});

app.UseStaticFiles();
app.Run();

