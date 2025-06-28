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

    public async Task<string[]> GetKvFileContentsAsync(string folderName)
    {
        _logger.LogDebug("Reading KV file contents from {Folder}", folderName);

        string configuredPath = _configuration["KVSPath"] ?? "Infrastructure/KVS";
        string kvsPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
        string folderPath = Path.Combine(kvsPath, folderName);

        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"KVS folder not found: {folderName}");

        var files = Directory.GetFiles(folderPath);
        var contents = new List<string>(files.Length);

        foreach (var file in files)
        {
            await using var stream = File.OpenRead(file);
            string text = await _fileDecoderService.GetFileContentAsync(Path.GetFileName(file), stream);
            contents.Add(text);
        }

        return contents.ToArray();
    }
}
