using PalworldServerManager.Core.Rcon;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class PalworldRconClientTests
{
    [Fact]
    public void ParsePlayers_ParsesHeaderAndRows()
    {
        var raw = "name,playeruid,steamid\nAlice,111,76561190000000001\nBob,222,76561190000000002\n";

        var players = PalworldRconClient.ParsePlayers(raw);

        Assert.Equal(2, players.Count);
        Assert.Equal(new PalworldPlayer("Alice", "111", "76561190000000001"), players[0]);
        Assert.Equal(new PalworldPlayer("Bob", "222", "76561190000000002"), players[1]);
    }

    [Fact]
    public void ParsePlayers_EmptyServer_ReturnsEmptyList()
    {
        var raw = "name,playeruid,steamid\n";

        var players = PalworldRconClient.ParsePlayers(raw);

        Assert.Empty(players);
    }

    [Fact]
    public void ParsePlayers_NameContainingComma_IsHandled()
    {
        var raw = "name,playeruid,steamid\nSmith, John,111,76561190000000001\n";

        var players = PalworldRconClient.ParsePlayers(raw);

        Assert.Single(players);
        Assert.Equal("Smith, John", players[0].Name);
        Assert.Equal("111", players[0].PlayerUid);
        Assert.Equal("76561190000000001", players[0].SteamId);
    }

    [Fact]
    public async Task ShowPlayersAsync_ParsesRealServerResponse()
    {
        const string canned = "name,playeruid,steamid\nAlice,111,76561190000000001\n";
        await using var server = new FakeRconServer("pw", command => command == "ShowPlayers" ? canned : string.Empty);
        await using var rcon = await PalworldRconClient.ConnectAsync("127.0.0.1", server.Port, "pw");

        var players = await rcon.ShowPlayersAsync();

        Assert.Single(players);
        Assert.Equal("Alice", players[0].Name);
    }

    [Fact]
    public async Task BroadcastAsync_ReplacesSpacesWithUnderscores()
    {
        string? receivedCommand = null;
        await using var server = new FakeRconServer("pw", command =>
        {
            receivedCommand = command;
            return string.Empty;
        });
        await using var rcon = await PalworldRconClient.ConnectAsync("127.0.0.1", server.Port, "pw");

        await rcon.BroadcastAsync("Server restarting soon");

        Assert.Equal("Broadcast Server_restarting_soon", receivedCommand);
    }
}
