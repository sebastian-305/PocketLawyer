using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using Microsoft.Extensions.Options;

namespace PocketLawyer.Infrastructure.AiServices.Gemini;

public class ResponseLogger : IResponseLogger
{
    private readonly ILogger<ResponseLogger> _logger;
    private readonly PathOptions _paths;
    private readonly IWebHostEnvironment _environment;

    public ResponseLogger(
        ILogger<ResponseLogger> logger,
        IOptions<PathOptions> pathOptions,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _paths = pathOptions.Value;
        _environment = environment;
    }

    public async Task LogResponseAsync(string response)
    {
        _logger.LogDebug("Writing response to file");

        string folder = Path.Combine(
            _environment.ContentRootPath,
            _paths.ResponsesPath);

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
