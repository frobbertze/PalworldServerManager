using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PalworldServerManager.Core.Tests;

/// <summary>
/// A minimal raw-socket HTTP/1.1 server used to test PalworldRestApiClient's requests
/// (path, headers, JSON body) and response handling, without needing HttpListener's Windows
/// URL-ACL/elevation requirements or a real Palworld server.
/// </summary>
public sealed class FakeHttpServer : IAsyncDisposable
{
    public sealed record RecordedRequest(string Method, string Path, string? AuthorizationHeader, string Body);

    private readonly TcpListener _listener;
    private readonly Func<RecordedRequest, (int StatusCode, string Body)> _handler;
    private readonly Task _acceptLoop;
    private readonly CancellationTokenSource _cts = new();

    public int Port { get; }
    public RecordedRequest? LastRequest { get; private set; }

    public FakeHttpServer(Func<RecordedRequest, (int StatusCode, string Body)> handler)
    {
        _handler = handler;
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
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var stream = client.GetStream();
        var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);

        var requestLine = await reader.ReadLineAsync(cancellationToken) ?? string.Empty;
        var parts = requestLine.Split(' ');
        var method = parts.Length > 0 ? parts[0] : string.Empty;
        var path = parts.Length > 1 ? parts[1] : string.Empty;

        string? authHeader = null;
        var contentLength = 0;
        var chunked = false;
        string? line;
        while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(cancellationToken)))
        {
            if (line.StartsWith("Authorization:", StringComparison.OrdinalIgnoreCase))
            {
                authHeader = line["Authorization:".Length..].Trim();
            }
            else if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                contentLength = int.Parse(line["Content-Length:".Length..].Trim());
            }
            else if (line.StartsWith("Transfer-Encoding:", StringComparison.OrdinalIgnoreCase) &&
                     line.Contains("chunked", StringComparison.OrdinalIgnoreCase))
            {
                chunked = true;
            }
        }

        var body = string.Empty;
        if (chunked)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var chunkSizeLine = await reader.ReadLineAsync(cancellationToken) ?? "0";
                var chunkSize = Convert.ToInt32(chunkSizeLine.Trim(), 16);
                if (chunkSize == 0)
                {
                    await reader.ReadLineAsync(cancellationToken); // trailing blank line after last chunk
                    break;
                }

                var buffer = new char[chunkSize];
                var read = 0;
                while (read < chunkSize)
                {
                    var n = await reader.ReadAsync(buffer.AsMemory(read, chunkSize - read), cancellationToken);
                    if (n == 0)
                    {
                        break;
                    }

                    read += n;
                }

                sb.Append(buffer, 0, read);
                await reader.ReadLineAsync(cancellationToken); // trailing \r\n after each chunk's data
            }

            body = sb.ToString();
        }
        else if (contentLength > 0)
        {
            var buffer = new char[contentLength];
            var read = 0;
            while (read < contentLength)
            {
                var n = await reader.ReadAsync(buffer.AsMemory(read, contentLength - read), cancellationToken);
                if (n == 0)
                {
                    break;
                }

                read += n;
            }

            body = new string(buffer, 0, read);
        }

        LastRequest = new RecordedRequest(method, path, authHeader, body);

        var (statusCode, responseBody) = _handler(LastRequest);
        var responseBodyBytes = Encoding.UTF8.GetBytes(responseBody);
        var reasonPhrase = statusCode switch
        {
            200 => "OK",
            401 => "Unauthorized",
            400 => "Bad Request",
            _ => "Unknown",
        };

        var header =
            $"HTTP/1.1 {statusCode} {reasonPhrase}\r\n" +
            $"Content-Type: application/json\r\n" +
            $"Content-Length: {responseBodyBytes.Length}\r\n" +
            "Connection: close\r\n\r\n";

        var headerBytes = Encoding.ASCII.GetBytes(header);
        await stream.WriteAsync(headerBytes, cancellationToken);
        await stream.WriteAsync(responseBodyBytes, cancellationToken);
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
