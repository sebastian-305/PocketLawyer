using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using PocketLawyer.Infrastructure.KvDetection;
using PocketLawyer.Service;
using Xunit;

public class KvDetectionServiceTests
{
    [Fact]
    public async Task DetectKvAsync_UsesAiServiceAndReturnsResult()
    {
        // arrange
        var envDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var kvDir = Path.Combine(envDir, "Infrastructure", "KVS");
        Directory.CreateDirectory(Path.Combine(kvDir, "kv1"));
        Directory.CreateDirectory(Path.Combine(kvDir, "kv2"));

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.ContentRootPath).Returns(envDir);

        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.SendPrompt(It.IsAny<string>(), null, false))
              .ReturnsAsync("{\"kollektivvertrag\":\"kv1\"}");

        var logger = new Mock<ILogger<KvDetectionService>>();

        var service = new KvDetectionService(aiMock.Object, envMock.Object, logger.Object);

        // act
        var result = await service.DetectKvAsync("content");

        // assert
        Assert.Equal("{\"kollektivvertrag\":\"kv1\"}", result);
        aiMock.Verify(a => a.SendPrompt(It.Is<string>(s => s.Contains("content") && s.Contains("kv1") && s.Contains("kv2")), null, false), Times.Once);
    }

    [Fact]
    public async Task DetectKvAsync_ThrowsAiServiceExceptionWhenAiFails()
    {
        var envDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(Path.Combine(envDir, "Infrastructure", "KVS", "kv"));

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.ContentRootPath).Returns(envDir);

        var aiMock = new Mock<IAiService>();
        aiMock.Setup(a => a.SendPrompt(It.IsAny<string>(), null, false))
              .ThrowsAsync(new InvalidOperationException());

        var logger = new Mock<ILogger<KvDetectionService>>();

        var service = new KvDetectionService(aiMock.Object, envMock.Object, logger.Object);

        await Assert.ThrowsAsync<AiServiceException>(() => service.DetectKvAsync("text"));
    }
}
