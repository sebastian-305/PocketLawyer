using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure;

public class KollektivvertragService : IKollektivvertragService
{
    private readonly ILogger<KollektivvertragService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public KollektivvertragService(ILogger<KollektivvertragService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<string> GetSampleContractAsync()
    {
        _logger.LogDebug("Reading sample contract");
        string configuredPath = _configuration["SampleContractPath"] ?? "Samples/int.txt";
        string path = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
        return await File.ReadAllTextAsync(path);
    }

    public Task<string[]> GetKvsDirectoriesAsync()
    {
        _logger.LogDebug("Listing KVS directories");
        string configuredPath = _configuration["KVSPath"] ?? "Infrastructure/KVS";
        string kvsPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
        string[] folders = Directory.GetDirectories(kvsPath)
            .Select(name => Path.GetFileName(name) ?? string.Empty)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();
        return Task.FromResult(folders);
    }
}
