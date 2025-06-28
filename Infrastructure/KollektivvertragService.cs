using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PocketLawyer.Service;
using System.Collections.Generic;
using System.IO;

namespace PocketLawyer.Infrastructure;

public class KollektivvertragService : IKollektivvertragService
{
    private readonly ILogger<KollektivvertragService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IFileDecoderService _fileDecoderService;

    public KollektivvertragService(ILogger<KollektivvertragService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IFileDecoderService fileDecoderService)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
        _fileDecoderService = fileDecoderService;
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

    public async Task<string[]> GetKvFileContentsAsync(string kvName)
    {
        _logger.LogDebug("Reading KV files for {KvName}", kvName);

        string configuredPath = _configuration["KVSPath"] ?? "Infrastructure/KVS";
        string kvsPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);

        string kvPath = Path.Combine(kvsPath, kvName);
        if (!Directory.Exists(kvPath))
        {
            _logger.LogWarning("KV folder not found: {KvPath}", kvPath);
            return Array.Empty<string>();
        }

        var files = Directory.GetFiles(kvPath);
        List<string> contents = new();

        foreach (var file in files)
        {
            try
            {
                await using var stream = File.OpenRead(file);
                string text = await _fileDecoderService.GetFileContentAsync(Path.GetFileName(file), stream);
                contents.Add(text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read KV file {File}", file);
            }
        }

        return contents.ToArray();
    }
}
