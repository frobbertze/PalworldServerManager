using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PalworldServerManager.Core.Tests;

/// <summary>
/// A minimal Source RCON server used only to exercise RconClient's wire format end-to-end,
/// implemented independently of RconClient itself so the test isn't just checking the code
/// against itself.
/// </summary>
public sealed class FakeRconServer : IAsyncDisposable
{
    private const int PacketTypeResponseValue = 0;
    private const int PacketTypeExecCommandOrAuthResponse = 2;
    private const int PacketTypeAuth = 3;

    private readonly TcpListener _listener;
    private readonly string _expectedPassword;
    private readonly Func<string, string> _commandHandler;
    private readonly Task _acceptLoop;
    private readonly CancellationTokenSource _cts = new();

    public int Port { get; }

    public FakeRconServer(string expectedPassword, Func<string, string> commandHandler)
    {
        _expectedPassword = expectedPassword;
        _commandHandler = commandHandler;

        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

        _acceptLoop = Task.Run(() => AcceptLoopAsync(_cts.Token));
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                await HandleClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var stream = client.GetStream();

        var (authId, authType, authBody) = await ReadPacketAsync(stream, cancellationToken);
        if (authType != PacketTypeAuth)
        {
            return;
        }

        var success = authBody == _expectedPassword;
        await WritePacketAsync(stream, success ? authId : -1, PacketTypeExecCommandOrAuthResponse, string.Empty, cancellationToken);

        if (!success)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            (int id, int type, string body) packet;
            try
            {
                packet = await ReadPacketAsync(stream, cancellationToken);
            }
            catch (IOException)
            {
                return;
            }

            if (packet.type != PacketTypeExecCommandOrAuthResponse)
            {
                continue;
            }

            var response = _commandHandler(packet.body);
            await WritePacketAsync(stream, packet.id, PacketTypeResponseValue, response, cancellationToken);
        }
    }

    private static async Task WritePacketAsync(NetworkStream stream, int id, int type, string body, CancellationToken cancellationToken)
    {
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var payloadSize = 4 + 4 + bodyBytes.Length + 1 + 1;
        var packet = new byte[4 + payloadSize];

        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(0, 4), payloadSize);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(4, 4), id);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(8, 4), type);
        bodyBytes.CopyTo(packet.AsSpan(12));

        await stream.WriteAsync(packet, cancellationToken);
    }

    private static async Task<(int Id, int Type, string Body)> ReadPacketAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var sizeBuffer = new byte[4];
        await ReadExactAsync(stream, sizeBuffer, cancellationToken);
        var size = BinaryPrimitives.ReadInt32LittleEndian(sizeBuffer);

        var payload = new byte[size];
        await ReadExactAsync(stream, payload, cancellationToken);

        var id = BinaryPrimitives.ReadInt32LittleEndian(payload.AsSpan(0, 4));
        var type = BinaryPrimitives.ReadInt32LittleEndian(payload.AsSpan(4, 4));
        var bodyLength = size - 4 - 4 - 2;
        var body = bodyLength > 0 ? Encoding.UTF8.GetString(payload, 8, bodyLength) : string.Empty;

        return (id, type, body);
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset), cancellationToken);
            if (read == 0)
            {
                throw new IOException("Connection closed.");
            }

            offset += read;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _listener.Stop();
        try
        {
            await _acceptLoop;
        }
        catch
        {
        }

        _cts.Dispose();
    }
}
