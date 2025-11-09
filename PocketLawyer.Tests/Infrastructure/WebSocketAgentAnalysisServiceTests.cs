using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PocketLawyer.Application.Exceptions;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Domain.Dtos;
using PocketLawyer.Infrastructure;
using PocketLawyer.Tests.TestDoubles;

namespace PocketLawyer.Tests.Infrastructure;

public class WebSocketAgentAnalysisServiceTests
{
    private static Task<(byte[] fileBytes, string fileName)> InvokeReceiveFileAsync(WebSocketAgentAnalysisService sut, WebSocket socket)
    {
        var method = typeof(WebSocketAgentAnalysisService)
            .GetMethod("ReceiveFileAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (Task<(byte[] fileBytes, string fileName)>)method.Invoke(sut, new object[] { socket })!;
    }

    private static WebSocketAgentAnalysisService CreateService()
    {
        var validatorMock = new Mock<IFileValidatorService>();
        return new WebSocketAgentAnalysisService(
            NullLogger<WebSocketAgentAnalysisService>.Instance,
            validatorMock.Object);
    }

    [Fact]
    public async Task ReceiveFileAsync_ThrowsWhenSocketClosesEarly()
    {
        // Dieser Test prüft, dass ein vorzeitig geschlossenes WebSocket während des Uploads eine FileValidationException auslöst.
        var sut = CreateService();
        var socket = new TestWebSocket();
        socket.EnqueueReceive(Array.Empty<byte>(), messageType: WebSocketMessageType.Close);

        await Assert.ThrowsAsync<FileValidationException>(() => InvokeReceiveFileAsync(sut, socket));
    }

    [Fact]
    public async Task ReceiveFileAsync_ThrowsWhenPayloadInvalid()
    {
        // Dieser Test prüft, dass ungültige JSON-Payloads als fehlerhafte Uploads erkannt und mit einer FileValidationException beantwortet werden.
        var sut = CreateService();
        var socket = new TestWebSocket();
        socket.EnqueueReceive("{ invalid json }");

        await Assert.ThrowsAsync<FileValidationException>(() => InvokeReceiveFileAsync(sut, socket));
    }

    [Fact]
    public async Task ReceiveFileAsync_ReassemblesAndDecodesBase64Payload()
    {
        // Dieser Test prüft, dass mehrteilige Textnachrichten korrekt zusammengesetzt, deserialisiert und in Byte-Content plus Dateinamen umgewandelt werden.
        var sut = CreateService();
        var socket = new TestWebSocket();
        var jsonStart = "{\"fileName\":\"contract.txt\",\"data\":\"";
        var jsonEnd = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello")) + "\"}";
        socket.EnqueueReceive(jsonStart, endOfMessage: false);
        socket.EnqueueReceive(jsonEnd, endOfMessage: true);

        var (bytes, fileName) = await InvokeReceiveFileAsync(sut, socket);

        Assert.Equal("contract.txt", fileName);
        Assert.Equal("hello", Encoding.UTF8.GetString(bytes));
    }
}
