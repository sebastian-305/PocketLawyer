using Microsoft.Extensions.Logging.Abstractions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.FileDecoding;
using PocketLawyer.Infrastructure.FileDecoding.Strategies;

namespace PocketLawyer.Tests.Infrastructure;

public class FileDecoderServiceTests
{
    private static FileDecoderService CreateService()
    {
        var decoders = new IFileDecoder[]
        {
            new TextFileDecoder(NullLogger<TextFileDecoder>.Instance)
        };
        return new FileDecoderService(NullLogger<FileDecoderService>.Instance, decoders);
    }

    [Fact]
    public void AllowedExtensions_IncludesTxt()
    {
        var service = CreateService();

        Assert.Contains("txt", service.AllowedExtensions);
    }

    [Fact]
    public void AllowedExtensions_IncludesMd()
    {
        var service = CreateService();

        Assert.Contains("md", service.AllowedExtensions);
    }

    [Fact]
    public void AllowedExtensions_IncludesPdf()
    {
        var service = CreateService();

        Assert.Contains("pdf", service.AllowedExtensions);
    }

    [Fact]
    public void AllowedExtensionsString_ContainsMd()
    {
        var service = CreateService();

        Assert.Contains("md", service.AllowedExtensionsString);
    }

    [Fact]
    public async Task GetFileContentAsync_DecodesMarkdownFile()
    {
        var service = CreateService();
        var content = "# Test Markdown\n\nThis is a test.";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        await using var stream = new MemoryStream(bytes);

        string result = await service.GetFileContentAsync("test.md", stream);

        Assert.Equal(content, result);
    }

    [Fact]
    public async Task GetFileContentAsync_DecodesTextFile()
    {
        var service = CreateService();
        var content = "This is plain text.";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        await using var stream = new MemoryStream(bytes);

        string result = await service.GetFileContentAsync("test.txt", stream);

        Assert.Equal(content, result);
    }
}
