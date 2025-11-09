using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure;
using PocketLawyer.Options;

namespace PocketLawyer.Tests.Infrastructure;

public class FileValidatorServiceTests
{
    private static FileValidatorService CreateService(Mock<IFileDecoderService> decoderMock, long maxSizeMb = 5)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new UploadOptions { MaxUploadSizeMB = maxSizeMb });
        return new FileValidatorService(NullLogger<FileValidatorService>.Instance, decoderMock.Object, options);
    }

    [Fact]
    public async Task ValidateAndDecodeAsync_ThrowsWhenExtensionNotAllowed()
    {
        // Dieser Test prüft, dass Dateien mit nicht erlaubten Endungen frühzeitig abgewiesen werden und eine FileValidationException entsteht.
        var decoderMock = new Mock<IFileDecoderService>();
        decoderMock.SetupGet(d => d.AllowedExtensions).Returns(new[] { "pdf" });
        decoderMock.SetupGet(d => d.AllowedExtensionsString).Returns("pdf");
        var sut = CreateService(decoderMock);
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        IFormFile file = new FormFile(stream, 0, stream.Length, "file", "contract.txt");

        await Assert.ThrowsAsync<FileValidationException>(() => sut.ValidateAndDecodeAsync(file));
    }

    [Fact]
    public async Task ValidateAndDecodeAsync_ThrowsWhenFileTooLarge()
    {
        // Dieser Test prüft, dass Dateien größer als die konfigurierte Upload-Grenze abgelehnt und mit einer FileValidationException quittiert werden.
        var decoderMock = new Mock<IFileDecoderService>();
        decoderMock.SetupGet(d => d.AllowedExtensions).Returns(new[] { "txt" });
        decoderMock.SetupGet(d => d.AllowedExtensionsString).Returns("txt");
        var sut = CreateService(decoderMock, maxSizeMb: 1);
        var largeBytes = new byte[2 * 1024 * 1024];
        await using var stream = new MemoryStream(largeBytes);
        IFormFile file = new FormFile(stream, 0, stream.Length, "file", "contract.txt");

        await Assert.ThrowsAsync<FileValidationException>(() => sut.ValidateAndDecodeAsync(file));
    }

    [Fact]
    public async Task ValidateAndDecodeAsync_WrapsDecoderExceptions()
    {
        // Dieser Test prüft, dass Decoder-Fehler protokolliert und als FileValidationException mit innerer Ausnahme weitergereicht werden.
        var decoderMock = new Mock<IFileDecoderService>();
        decoderMock.SetupGet(d => d.AllowedExtensions).Returns(new[] { "txt" });
        decoderMock.SetupGet(d => d.AllowedExtensionsString).Returns("txt");
        decoderMock.Setup(d => d.GetFileContentAsync(It.IsAny<string>(), It.IsAny<Stream>()))
            .ThrowsAsync(new InvalidOperationException("decode"));
        var sut = CreateService(decoderMock);
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        IFormFile file = new FormFile(stream, 0, stream.Length, "file", "contract.txt");

        var ex = await Assert.ThrowsAsync<FileValidationException>(() => sut.ValidateAndDecodeAsync(file));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public async Task ValidateAndDecodeAsync_ReturnsDecodedContent()
    {
        // Dieser Test prüft, dass bei gültiger Datei der Decoder aufgerufen wird und der zurückgegebene FileContentDto den erwarteten Inhalt enthält.
        var decoderMock = new Mock<IFileDecoderService>();
        decoderMock.SetupGet(d => d.AllowedExtensions).Returns(new[] { "txt" });
        decoderMock.SetupGet(d => d.AllowedExtensionsString).Returns("txt");
        decoderMock.Setup(d => d.GetFileContentAsync("contract.txt", It.IsAny<Stream>()))
            .ReturnsAsync("decoded content");
        var sut = CreateService(decoderMock);
        await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        IFormFile file = new FormFile(stream, 0, stream.Length, "file", "contract.txt");

        FileContentDto result = await sut.ValidateAndDecodeAsync(file);

        Assert.Equal("contract.txt", result.FileName);
        Assert.Equal("decoded content", result.FileContent);
    }
}
