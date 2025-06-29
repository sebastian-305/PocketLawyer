using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure;

public class KvFileService : IKollektivvertragService
{
    private readonly ILogger<KvFileService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IFileDecoderService _fileDecoderService;

    public KvFileService(ILogger<KvFileService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IFileDecoderService fileDecoderService)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
        _fileDecoderService = fileDecoderService;
    }


    public string[] GetKvsDirectories()
    {
        _logger.LogDebug("Listing KVS directories");
        string configuredPath = _configuration["KVSPath"] ?? "data/KVS";
        string kvsPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
        string[] folders = Directory.GetDirectories(kvsPath)
            .Select(name => Path.GetFileName(name) ?? string.Empty)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();
        return folders;
    }

    public async Task<string[]> GetKvFileContentsAsync(string kvName)
    {
        _logger.LogDebug("Reading KV files for {KvName}", kvName);

        string configuredPath = _configuration["KVSPath"] ?? "data/KVS";
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
