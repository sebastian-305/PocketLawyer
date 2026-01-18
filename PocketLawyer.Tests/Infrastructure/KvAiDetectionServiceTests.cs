using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Application.Models;
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
        
        var response1 = new AiResponse("{\"kollektivvertrag\":\"KV1\",\"begruendung\":\"Test\"}", 100, 50, 150);
        var response2 = new AiResponse("{\"kollektivvertrag\":\"KV1\",\"begruendung\":\"Test\"}", 100, 50, 150);
        var responses = new Queue<AiResponse>(new[] { response1, response2 });
        
        var aiClientMock = new Mock<IAiClient>();
        aiClientMock.Setup(a => a.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(() => responses.Dequeue());
        var sut = CreateService(kvServiceMock, aiClientMock);

        KvDetectionResult result = await sut.DetectAsync("Vertrag");

        Assert.True(result.AllEqual);
        Assert.Equal("KV1", result.KvName);
    }

    [Fact]
    public async Task DetectAsync_SelectsMajorityWhenAnswersDiffer()
    {
        // Dieser Test prüft, dass bei unterschiedlichen Antworten der häufigste KV-Name gewählt und das AllEqual-Flag deaktiviert wird.
        var kvServiceMock = new Mock<IKollektivvertragService>();
        kvServiceMock.Setup(s => s.GetKvsDirectories()).Returns(new[] { "KV1", "KV2" });
        
        var response1 = new AiResponse("{\"kollektivvertrag\":\"KV1\",\"begruendung\":\"Test\"}", 100, 50, 150);
        var response2 = new AiResponse("{\"kollektivvertrag\":\"KV2\",\"begruendung\":\"Test\"}", 100, 50, 150);
        var responses = new Queue<AiResponse>(new[] { response1, response2 });
        
        var aiClientMock = new Mock<IAiClient>();
        aiClientMock.Setup(a => a.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(() => responses.Dequeue());
        var sut = CreateService(kvServiceMock, aiClientMock);

        KvDetectionResult result = await sut.DetectAsync("Vertrag");

        Assert.False(result.AllEqual);
        Assert.True(result.KvName is "KV1" or "KV2");
    }

    [Fact]
    public async Task DetectAsync_ReturnsRawJsonWhenParsingFails()
    {
        // Dieser Test prüft, dass fehlerhafte JSON-Antworten nicht abstürzen, sondern als Rohtext in die Mehrheitsentscheidung eingehen.
        // Wenn der KV-Name nicht in der Liste ist, wird er durch "Keiner" ersetzt, was zu einem leeren String führt.
        var kvServiceMock = new Mock<IKollektivvertragService>();
        kvServiceMock.Setup(s => s.GetKvsDirectories()).Returns(Array.Empty<string>());
        
        var response1 = new AiResponse("not json", 100, 50, 150);
        var response2 = new AiResponse("not json", 100, 50, 150);
        var responses = new Queue<AiResponse>(new[] { response1, response2 });
        
        var aiClientMock = new Mock<IAiClient>();
        aiClientMock.Setup(a => a.SendPromptWithMetadata(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(() => responses.Dequeue());
        var sut = CreateService(kvServiceMock, aiClientMock);

        KvDetectionResult result = await sut.DetectAsync("Vertrag");

        // When parsing fails and the result is not in the KV list, it gets replaced with "Keiner" -> empty string
        Assert.True(result.AllEqual);  // Both answers are the same
        Assert.Equal(string.Empty, result.KvName);  // Invalid KV names are replaced with empty string
        Assert.Contains("FEHLER", result.Begruendungen[0]);  // Error message should be in the reasoning
    }
}
