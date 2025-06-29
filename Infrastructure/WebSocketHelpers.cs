using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace PocketLawyer.Infrastructure;

public static class WebSocketHelpers
{
    public static Task SendStringAsync(WebSocket socket, string data, CancellationToken token)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
    }

    public static Task SendErrorAsync(WebSocket socket, string message, CancellationToken token)
    {
        var payload = JsonSerializer.Serialize(new { error = message });
        return SendStringAsync(socket, payload, token);
    }
}
