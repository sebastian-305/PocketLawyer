using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain.Dtos.Shared;
using PocketLawyer.Infrastructure;

namespace PocketLawyer.Tests.Infrastructure;

public class KvAiDetectionServiceTests
{
    private static KvAiDetectionService CreateService(Mock<IKollektivvertragService> kvServiceMock, Mock<IAiClient> aiClientMock)
    {
        return new KvAiDetectionService(
            NullLogger<KvAiDetectionService>.Instance,
            kvServiceMock.Object,
            aiClientMock.Object);
    }

    [Fact]
    public async Task DetectAsync_ReturnsAllEqualWhenAnswersMatch()
    {
        // Dieser Test prüft, dass identische KI-Antworten als einstimmig erkannt und als Ergebniswert zurückgegeben werden.
        var kvServiceMock = new Mock<IKollektivvertragService>();
        kvServiceMock.Setup(s => s.GetKvsDirectories()).Returns(new[] { "KV1" });
        var responses = new Queue<string>(new[]
        {
            "{\"kollektivvertrag\":\"KV Metall\"}",
            "{\"kollektivvertrag\":\"KV Metall\"}"
        });
        var aiClientMock = new Mock<IAiClient>();
        aiClientMock.Setup(a => a.SendPrompt(It.IsAny<string>(), null))
            .ReturnsAsync(() => responses.Dequeue());
        var sut = CreateService(kvServiceMock, aiClientMock);

        KvDetectionResult result = await sut.DetectAsync("Vertrag");

        Assert.True(result.AllEqual);
        Assert.Equal("KV Metall", result.KvName);
    }

    [Fact]
    public async Task DetectAsync_SelectsMajorityWhenAnswersDiffer()
    {
        // Dieser Test prüft, dass bei unterschiedlichen Antworten der häufigste KV-Name gewählt und das AllEqual-Flag deaktiviert wird.
        var kvServiceMock = new Mock<IKollektivvertragService>();
        kvServiceMock.Setup(s => s.GetKvsDirectories()).Returns(new[] { "KV1", "KV2" });
        var responses = new Queue<string>(new[]
        {
            "{\"kollektivvertrag\":\"KV Metall\"}",
            "{\"kollektivvertrag\":\"KV Handel\"}"
        });
        var aiClientMock = new Mock<IAiClient>();
        aiClientMock.Setup(a => a.SendPrompt(It.IsAny<string>(), null))
            .ReturnsAsync(() => responses.Dequeue());
        var sut = CreateService(kvServiceMock, aiClientMock);

        KvDetectionResult result = await sut.DetectAsync("Vertrag");

        Assert.False(result.AllEqual);
        Assert.True(result.KvName is "KV Metall" or "KV Handel");
    }

    [Fact]
    public async Task DetectAsync_ReturnsRawJsonWhenParsingFails()
    {
        // Dieser Test prüft, dass fehlerhafte JSON-Antworten nicht abstürzen, sondern als Rohtext in die Mehrheitsentscheidung eingehen.
        var kvServiceMock = new Mock<IKollektivvertragService>();
        kvServiceMock.Setup(s => s.GetKvsDirectories()).Returns(Array.Empty<string>());
        var responses = new Queue<string>(new[]
        {
            "not json",
            "{\"kollektivvertrag\":\"KV Handel\"}"
        });
        var aiClientMock = new Mock<IAiClient>();
        aiClientMock.Setup(a => a.SendPrompt(It.IsAny<string>(), null))
            .ReturnsAsync(() => responses.Dequeue());
        var sut = CreateService(kvServiceMock, aiClientMock);

        KvDetectionResult result = await sut.DetectAsync("Vertrag");

        Assert.False(result.AllEqual);
        Assert.Contains(result.KvName, new[] { "not json", "KV Handel" });
    }
}
