using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure.AiServices.Gemini;

public class ResponseLogger : IResponseLogger
{
    private readonly ILogger<ResponseLogger> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public ResponseLogger(
        ILogger<ResponseLogger> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task LogResponseAsync(string response)
    {
        _logger.LogDebug("Writing response to file");

        string folder = Path.Combine(
            _environment.ContentRootPath,
            _configuration["ResponsesPath"] ?? "data/Responses");

        try
        {
            Directory.CreateDirectory(folder);
            string filePath = Path.Combine(
                folder,
                $"{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");

            _logger.LogDebug(
                "Filename: {Name} Content Length: {Length}",
                Path.GetFileName(filePath),
                response.Length);

            await File.WriteAllTextAsync(filePath, response);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to write response to file");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Failed to write response due to permission issues");
        }
    }
}
