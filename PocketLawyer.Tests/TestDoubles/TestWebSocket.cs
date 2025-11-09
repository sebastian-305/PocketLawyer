using System.Net.WebSockets;
using System.Text;

namespace PocketLawyer.Tests.TestDoubles;

public class TestWebSocket : WebSocket
{
    private readonly Queue<(byte[] Data, WebSocketMessageType Type, bool EndOfMessage)> _receiveQueue = new();
    private readonly List<(byte[] Data, WebSocketMessageType Type, bool EndOfMessage)> _sentMessages = new();
    private WebSocketState _state = WebSocketState.Open;
    private WebSocketCloseStatus? _closeStatus;
    private string? _closeStatusDescription;

    public override WebSocketCloseStatus? CloseStatus => _closeStatus;
    public override string? CloseStatusDescription => _closeStatusDescription;
    public override WebSocketState State => _state;
    public override string? SubProtocol => null;

    public IReadOnlyList<(byte[] Data, WebSocketMessageType Type, bool EndOfMessage)> SentMessages => _sentMessages;

    public void EnqueueReceive(string text, bool endOfMessage = true, WebSocketMessageType messageType = WebSocketMessageType.Text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        _receiveQueue.Enqueue((bytes, messageType, endOfMessage));
    }

    public void EnqueueReceive(byte[] data, bool endOfMessage = true, WebSocketMessageType messageType = WebSocketMessageType.Text)
    {
        _receiveQueue.Enqueue((data, messageType, endOfMessage));
    }

    public override void Abort()
    {
        _state = WebSocketState.Aborted;
    }

    public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        _closeStatus = closeStatus;
        _closeStatusDescription = statusDescription;
        _state = WebSocketState.Closed;
        return Task.CompletedTask;
    }

    public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        return CloseAsync(closeStatus, statusDescription, cancellationToken);
    }

    public override void Dispose()
    {
        _state = WebSocketState.Closed;
    }

    public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        if (_receiveQueue.Count == 0)
        {
            throw new InvalidOperationException("No receive data enqueued.");
        }

        var (data, type, endOfMessage) = _receiveQueue.Dequeue();
        if (buffer.Array is null)
        {
            throw new ArgumentException("Buffer array is null.");
        }

        if (data.Length > buffer.Count)
        {
            throw new InvalidOperationException("Buffer too small for queued data.");
        }

        Array.Copy(data, 0, buffer.Array, buffer.Offset, data.Length);
        return Task.FromResult(new WebSocketReceiveResult(data.Length, type, endOfMessage));
    }

    public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        if (buffer.Array is null)
        {
            throw new ArgumentException("Buffer array is null.");
        }

        var data = new byte[buffer.Count];
        Array.Copy(buffer.Array, buffer.Offset, data, 0, buffer.Count);
        _sentMessages.Add((data, messageType, endOfMessage));
        return Task.CompletedTask;
    }
}
