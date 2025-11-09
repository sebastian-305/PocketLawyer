using System.Net.WebSockets;
using System.Text;
using PocketLawyer.Infrastructure;
using PocketLawyer.Tests.TestDoubles;

namespace PocketLawyer.Tests.Infrastructure;

public class WebSocketHelpersTests
{
    [Fact]
    public async Task SendErrorAsync_FormatsPayloadAsJson()
    {
        // Dieser Test prüft, dass Fehlernachrichten als JSON-Objekt mit dem Feld "error" versendet werden, sodass Frontend-Clients das Format zuverlässig parsen können.
        var socket = new TestWebSocket();

        await WebSocketHelpers.SendErrorAsync(socket, "Fehler", CancellationToken.None);

        Assert.Single(socket.SentMessages);
        var (data, type, endOfMessage) = socket.SentMessages[0];
        Assert.Equal(WebSocketMessageType.Text, type);
        Assert.True(endOfMessage);
        string payload = Encoding.UTF8.GetString(data);
        Assert.Equal("{\"error\":\"Fehler\"}", payload);
    }
}
