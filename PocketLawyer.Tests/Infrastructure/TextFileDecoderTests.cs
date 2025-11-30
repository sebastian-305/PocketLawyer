using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using PocketLawyer.Infrastructure.FileDecoding.Strategies;

namespace PocketLawyer.Tests.Infrastructure;

public class TextFileDecoderTests
{
    private static TextFileDecoder CreateDecoder()
    {
        return new TextFileDecoder(NullLogger<TextFileDecoder>.Instance);
    }

    [Fact]
    public void CanDecode_ReturnsTrueForTxtExtension()
    {
        var decoder = CreateDecoder();

        bool result = decoder.CanDecode("txt");

        Assert.True(result);
    }

    [Fact]
    public void CanDecode_ReturnsTrueForMdExtension()
    {
        var decoder = CreateDecoder();

        bool result = decoder.CanDecode("md");

        Assert.True(result);
    }

    [Fact]
    public void CanDecode_IsCaseInsensitive()
    {
        var decoder = CreateDecoder();

        Assert.True(decoder.CanDecode("TXT"));
        Assert.True(decoder.CanDecode("MD"));
        Assert.True(decoder.CanDecode("Txt"));
        Assert.True(decoder.CanDecode("Md"));
    }

    [Fact]
    public void CanDecode_ReturnsFalseForOtherExtensions()
    {
        var decoder = CreateDecoder();

        Assert.False(decoder.CanDecode("pdf"));
        Assert.False(decoder.CanDecode("jpg"));
        Assert.False(decoder.CanDecode("png"));
        Assert.False(decoder.CanDecode("docx"));
    }

    [Fact]
    public async Task DecodeAsync_ReadsTextContent()
    {
        var decoder = CreateDecoder();
        var content = "This is test content";
        var bytes = Encoding.UTF8.GetBytes(content);
        await using var stream = new MemoryStream(bytes);

        string result = await decoder.DecodeAsync(stream);

        Assert.Equal(content, result);
    }

    [Fact]
    public async Task DecodeAsync_ReadsMarkdownContent()
    {
        var decoder = CreateDecoder();
        var content = "# Markdown Header\n\nThis is **bold** text.";
        var bytes = Encoding.UTF8.GetBytes(content);
        await using var stream = new MemoryStream(bytes);

        string result = await decoder.DecodeAsync(stream);

        Assert.Equal(content, result);
    }

    [Fact]
    public async Task DecodeAsync_HandlesEmptyContent()
    {
        var decoder = CreateDecoder();
        await using var stream = new MemoryStream();

        string result = await decoder.DecodeAsync(stream);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task DecodeAsync_HandlesUtf8Content()
    {
        var decoder = CreateDecoder();
        var content = "Österreich, Zürich, Müller";
        var bytes = Encoding.UTF8.GetBytes(content);
        await using var stream = new MemoryStream(bytes);

        string result = await decoder.DecodeAsync(stream);

        Assert.Equal(content, result);
    }
}
