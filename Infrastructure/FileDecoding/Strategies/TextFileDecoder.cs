using System.Text;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;

namespace PocketLawyer.Infrastructure.FileDecoding.Strategies;

public class TextFileDecoder : IFileDecoder
{
    private readonly ILogger<TextFileDecoder> _logger;

    public TextFileDecoder(ILogger<TextFileDecoder> logger)
    {
        _logger = logger;
    }

    public bool CanDecode(string ext)
    {
        return string.Equals(ext, "txt", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(ext, "md", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> DecodeAsync(Stream input)
    {
        _logger.LogDebug("Decoding text stream");
        using var reader = new StreamReader(input, Encoding.UTF8, leaveOpen: true);
        string text = await reader.ReadToEndAsync();
        return text;
    }
}
