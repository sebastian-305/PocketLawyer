using PocketLawyer;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using PocketLawyer.Service;

namespace PocketLawyer.Infrastructure;

public class ContractAnalyzerService : IContractAnalyzerService
{
    private readonly ILogger<ContractAnalyzerService> _logger;
    private readonly IAiService _aiService;
    private readonly IFileDecoderService _fileDecoderService;
    private readonly IConfiguration _configuration;
    private readonly IKollektivvertragService _kvService;
    private readonly IKvDetectionService _kvDetectionService;

    public ContractAnalyzerService(
        ILogger<ContractAnalyzerService> logger,
        [FromKeyedServices("gemini")] IAiService aiService,
        IFileDecoderService fileDecoderService,
        IConfiguration configuration,
        IKollektivvertragService kvService,
        IKvDetectionService kvDetectionService)
    {
        _logger = logger;
        _aiService = aiService;
        _fileDecoderService = fileDecoderService;
        _configuration = configuration;
        _kvService = kvService;
        _kvDetectionService = kvDetectionService;
    }

    public async Task<T> ProcessContractRequest<T>(
        IFormFile file,
        Func<string, string, string> promptBuilder,
        object schema,
        bool useAltKey,
        bool detectKv = false,
        string? kvName = null)
    {
        FileContentDto requestContent = await PreValidate(file);

        string prompt = promptBuilder(requestContent.FileName, requestContent.FileContent);

        if (!string.IsNullOrWhiteSpace(kvName) || detectKv)
        {
            try
            {
                if (detectKv && string.IsNullOrWhiteSpace(kvName))
                {
                    KvDetectionResult kvResult = await _kvDetectionService.DetectAsync(requestContent.FileContent);
                    _logger.LogInformation("KV detection result: {Result}", kvResult.KvName);

                    if (kvResult.AllEqual && kvResult.KvName != "Keiner" && kvResult.KvName != "Multiple")
                    {
                        kvName = kvResult.KvName;
                    }
                }

                if (!string.IsNullOrWhiteSpace(kvName))
                {
                    string[] kvFiles = await _kvService.GetKvFileContentsAsync(kvName);
                    var sbPrompt = new StringBuilder(prompt);
                    sbPrompt.AppendLine();
                    sbPrompt.AppendLine($"Der folgende Text enthält den dazugehörigen Kollektivvertrag \"{kvName}\" und mögliche Anhänge. " +
                        $"Diese Informationen sind laut Gesetz zwingend Grundlage der Vertragsanalyse. Wenn der Vertrag in einem Punkt so etwas schreibt wie \"laut Kollektivvertrag\" soll die entsprechende Regelung" +
                        $"aus dem Kollektivvetrag angegeben werden mit dem Zusatz \"(lt. Kollektivvertrag)\"");
                    sbPrompt.AppendLine(string.Join("\n\n", kvFiles));
                    prompt = sbPrompt.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kollektivvertrag detection failed");
            }
        }

        string answer = await _aiService.SendPrompt(prompt, schema, useAltKey);

        T? dto;
        try
        {
            dto = JsonSerializer.Deserialize<T>(answer);
        }
        catch (JsonException ex)
        {
            string truncated = answer.Length > 200 ? answer[..200] + "..." : answer;
            _logger.LogError(ex, "Failed to parse AI response: {Snippet}", truncated);
            throw new InvalidOperationException($"Fehler beim Interpretieren der KI-Antwort: {truncated}", ex);
        }
        if (dto == null)
            throw new InvalidOperationException("Invalid AI response");

        if (!string.IsNullOrWhiteSpace(kvName))
        {
            switch (dto)
            {
                case ContractValidationDto v:
                    v.Kollektivvertrag = kvName;
                    break;
                case ContractAnalysisDto a:
                    a.Kollektivvertrag = kvName;
                    break;
            }
        }

        return dto;
    }

    private async Task<FileContentDto> PreValidate(IFormFile file)
    {
        _logger.LogDebug("Entering {Method}", nameof(PreValidate));

        string filename = file.FileName;
        string fileContent;

        var extension = Path.GetExtension(filename).TrimStart('.').ToLowerInvariant();
        if (!_fileDecoderService.AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Fileextension not allowed: {FileName}", filename);
            throw new BadHttpRequestException($"Fileextension not recognised! Recognised extensions are {_fileDecoderService.AllowedExtensionsString}.");
        }

        long maxUploadSizeMb = _configuration.GetValue<long>("MaxUploadSizeMB");
        long maxBytes = maxUploadSizeMb * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new BadHttpRequestException($"File too large. Maximum {maxUploadSizeMb} MB allowed.");
        }

        await using var stream = file.OpenReadStream();

        try
        {
            fileContent = await _fileDecoderService.GetFileContentAsync(filename, stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding file {FileName}", filename);
            throw new BadHttpRequestException(ex.Message);
        }

        return new FileContentDto(filename, fileContent);
    }
}
