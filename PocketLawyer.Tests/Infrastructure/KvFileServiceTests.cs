using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure;
using PocketLawyer.Options;

namespace PocketLawyer.Tests.Infrastructure;

public class KvFileServiceTests
{
    private static KvFileService CreateService(string rootPath, string kvsPath, IFileDecoderService decoder)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new PathOptions { KVSPath = kvsPath });
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.SetupGet(e => e.ContentRootPath).Returns(rootPath);
        return new KvFileService(
            NullLogger<KvFileService>.Instance,
            options,
            envMock.Object,
            decoder);
    }

    [Fact]
    public void GetKvsDirectories_ResolvesRelativePath()
    {
        // Dieser Test prüft, dass relative KVS-Pfade anhand des ContentRoot korrekt aufgelöst und nur Verzeichnisnamen zurückgegeben werden.
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string kvsRelative = Path.Combine("data", "KVS");
        string kvsFull = Path.Combine(root, kvsRelative);
        Directory.CreateDirectory(Path.Combine(kvsFull, "Metall"));
        Directory.CreateDirectory(Path.Combine(kvsFull, "Handel"));
        var decoderMock = new Mock<IFileDecoderService>();
        var sut = CreateService(root, kvsRelative, decoderMock.Object);

        string[] result = sut.GetKvsDirectories();

        var expected = new[] { "Handel", "Metall" };
        Assert.Equal(expected, result.OrderBy(x => x));
    }

    [Fact]
    public async Task GetKvFileContentsAsync_ReturnsDecodedFilesAndSkipsFailures()
    {
        // Dieser Test prüft, dass KV-Dateien gelesen, über den Decoder in Text verwandelt und fehlerhafte Dateien stillschweigend übersprungen werden.
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string kvsRelative = Path.Combine("data", "KVS");
        string kvFolder = Path.Combine(root, kvsRelative, "Metall");
        Directory.CreateDirectory(kvFolder);
        string okFile = Path.Combine(kvFolder, "ok.txt");
        string badFile = Path.Combine(kvFolder, "bad.txt");
        await File.WriteAllTextAsync(okFile, "OK");
        await File.WriteAllTextAsync(badFile, "BAD");
        var decoderMock = new Mock<IFileDecoderService>();
        decoderMock.Setup(d => d.GetFileContentAsync("ok.txt", It.IsAny<Stream>()))
            .ReturnsAsync("decoded");
        decoderMock.Setup(d => d.GetFileContentAsync("bad.txt", It.IsAny<Stream>()))
            .ThrowsAsync(new InvalidOperationException());
        var sut = CreateService(root, kvsRelative, decoderMock.Object);

        string[] contents = await sut.GetKvFileContentsAsync("Metall");

        Assert.Single(contents);
        Assert.Equal("decoded", contents[0]);
    }
}
