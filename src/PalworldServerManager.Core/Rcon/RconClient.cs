using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace PalworldServerManager.Core.Rcon;

/// <summary>
/// A minimal client for the Source RCON protocol (the same protocol Palworld's dedicated
/// server implements). See https://developer.valvesoftware.com/wiki/Source_RCON_Protocol.
/// </summary>
public sealed class RconClient : IAsyncDisposable
{
    private const int PacketTypeResponseValue = 0;
    private const int PacketTypeExecCommandOrAuthResponse = 2;
    private const int PacketTypeAuth = 3;

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private int _nextRequestId = 1;

    public async Task ConnectAndAuthenticateAsync(string host, int port, string password, CancellationToken cancellationToken = default)
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port, cancellationToken);
        _stream = _tcpClient.GetStream();

        const int authRequestId = 1;
        await SendPacketAsync(authRequestId, PacketTypeAuth, password, cancellationToken);

        // A well-behaved server may send an empty SERVERDATA_RESPONSE_VALUE packet before
        // the real auth response — skip any packets until we see the auth response itself.
        while (true)
        {
            var (id, type, _) = await ReadPacketAsync(cancellationToken);
            if (type != PacketTypeExecCommandOrAuthResponse)
            {
                continue;
            }

            if (id == -1)
            {
                throw new RconAuthenticationException("RCON authentication failed — check the admin password.");
            }

            return;
        }
    }

    public async Task<string> SendCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Not connected. Call ConnectAndAuthenticateAsync first.");
        }

        var requestId = _nextRequestId++;
        await SendPacketAsync(requestId, PacketTypeExecCommandOrAuthResponse, command, cancellationToken);

        var (_, _, body) = await ReadPacketAsync(cancellationToken);
        return body;
    }

    private async Task SendPacketAsync(int id, int type, string body, CancellationToken cancellationToken)
    {
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var payloadSize = 4 + 4 + bodyBytes.Length + 1 + 1; // id + type + body + body-terminator + packet-terminator
        var packet = new byte[4 + payloadSize];

        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(0, 4), payloadSize);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(4, 4), id);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(8, 4), type);
        bodyBytes.CopyTo(packet.AsSpan(12));
        // Last two bytes are already 0 (body terminator + empty-string packet terminator).

        await _stream!.WriteAsync(packet, cancellationToken);
    }

    private async Task<(int Id, int Type, string Body)> ReadPacketAsync(CancellationToken cancellationToken)
    {
        var sizeBuffer = new byte[4];
        await ReadExactAsync(sizeBuffer, cancellationToken);
        var size = BinaryPrimitives.ReadInt32LittleEndian(sizeBuffer);

        var payload = new byte[size];
        await ReadExactAsync(payload, cancellationToken);

        var id = BinaryPrimitives.ReadInt32LittleEndian(payload.AsSpan(0, 4));
        var type = BinaryPrimitives.ReadInt32LittleEndian(payload.AsSpan(4, 4));

        var bodyLength = size - 4 - 4 - 2;
        var body = bodyLength > 0 ? Encoding.UTF8.GetString(payload, 8, bodyLength) : string.Empty;

        return (id, type, body);
    }

    private async Task ReadExactAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await _stream!.ReadAsync(buffer.AsMemory(offset), cancellationToken);
            if (read == 0)
            {
                throw new IOException("RCON connection closed unexpectedly while reading a response.");
            }

            offset += read;
        }
    }

    public ValueTask DisposeAsync()
    {
        _stream?.Dispose();
        _tcpClient?.Dispose();
        return ValueTask.CompletedTask;
    }
}
