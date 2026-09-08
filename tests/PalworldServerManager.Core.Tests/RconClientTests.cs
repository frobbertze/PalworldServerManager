using PalworldServerManager.Core.Rcon;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class RconClientTests
{
    [Fact]
    public async Task ConnectAndAuthenticate_WithCorrectPassword_Succeeds()
    {
        await using var server = new FakeRconServer("correct-pw", _ => "ok");
        await using var client = new RconClient();

        // Should not throw.
        await client.ConnectAndAuthenticateAsync("127.0.0.1", server.Port, "correct-pw");
    }

    [Fact]
    public async Task ConnectAndAuthenticate_WithWrongPassword_ThrowsRconAuthenticationException()
    {
        await using var server = new FakeRconServer("correct-pw", _ => "ok");
        await using var client = new RconClient();

        await Assert.ThrowsAsync<RconAuthenticationException>(
            () => client.ConnectAndAuthenticateAsync("127.0.0.1", server.Port, "wrong-pw"));
    }

    [Fact]
    public async Task SendCommand_ReturnsServerResponse()
    {
        await using var server = new FakeRconServer("pw", command => command == "Info" ? "Palworld v1.0" : "unknown");
        await using var client = new RconClient();
        await client.ConnectAndAuthenticateAsync("127.0.0.1", server.Port, "pw");

        var response = await client.SendCommandAsync("Info");

        Assert.Equal("Palworld v1.0", response);
    }

    [Fact]
    public async Task SendCommand_WithLargeResponse_RoundTripsCorrectly()
    {
        // Bigger than a single typical TCP segment, to make sure ReadExactAsync loops
        // correctly instead of assuming one read fills the buffer.
        var largeResponse = new string('x', 50_000);
        await using var server = new FakeRconServer("pw", _ => largeResponse);
        await using var client = new RconClient();
        await client.ConnectAndAuthenticateAsync("127.0.0.1", server.Port, "pw");

        var response = await client.SendCommandAsync("Whatever");

        Assert.Equal(largeResponse, response);
    }

    [Fact]
    public async Task SendCommand_WithoutConnecting_Throws()
    {
        await using var client = new RconClient();

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendCommandAsync("Info"));
    }
}
