using Chatbot_in.Domain.Service;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddKeyedScoped<IAiService, GeminiService>("gemini");
builder.Services.AddKeyedScoped<IAiService, ClaudeService>("claude");
builder.Services.AddScoped<IFileDecoderService, FileDecoderService>();
//var secrets = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


app.MapControllers();
app.UseStaticFiles();
app.Run();
