using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure;

// Liest Kollektivvertrag-Dateien vom Dateisystem
public class KvFileService : IKollektivvertragService
{
    private readonly ILogger<KvFileService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IFileDecoderService _fileDecoderService;

    // Logger, Konfiguration und Pfade werden injiziert
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


    // Liefert alle vorhandenen KV-Verzeichnisse zurueck
    public string[] GetKvsDirectories()
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
        return folders;
    }

    // Liest alle Dateien des angegebenen Kollektivvertrags und dekodiert sie
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

        // Jede Datei des KV-Ordners wird eingelesen
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
