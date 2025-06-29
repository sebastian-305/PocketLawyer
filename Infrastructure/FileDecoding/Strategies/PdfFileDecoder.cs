using System.Text;
using Microsoft.Extensions.Logging;
using PocketLawyer.Application.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PocketLawyer.Infrastructure.FileDecoding.Strategies;

public class PdfFileDecoder : IFileDecoder
{
    private readonly ILogger<PdfFileDecoder> _logger;

    public PdfFileDecoder(ILogger<PdfFileDecoder> logger)
    {
        _logger = logger;
    }

    public bool CanDecode(string ext)
    {
        return string.Equals(ext, "pdf", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> DecodeAsync(Stream input)
    {
        _logger.LogDebug("Decoding pdf stream");
        using PdfDocument document = PdfDocument.Open(input);
        var contentBuilder = new StringBuilder();
        foreach (Page page in document.GetPages())
            contentBuilder.Append(page.Text);
        return Task.FromResult(contentBuilder.ToString());
    }
}
