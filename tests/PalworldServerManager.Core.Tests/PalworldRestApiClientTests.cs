using PalworldServerManager.Core.RestApi;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class PalworldRestApiClientTests
{
    [Fact]
    public async Task GetInfoAsync_SendsBasicAuthAndParsesResponse()
    {
        await using var server = new FakeHttpServer(_ => (200, """{"version":"v1.0.4.102642","servername":"Test Server","description":"desc","worldguid":"ABC123"}"""));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "secret-pw");

        var info = await client.GetInfoAsync();

        Assert.Equal("v1.0.4.102642", info.Version);
        Assert.Equal("Test Server", info.ServerName);
        Assert.Equal("ABC123", info.WorldGuid);

        Assert.Equal("/v1/api/info", server.LastRequest!.Path);
        Assert.NotNull(server.LastRequest.AuthorizationHeader);
        Assert.StartsWith("Basic ", server.LastRequest.AuthorizationHeader);

        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(server.LastRequest.AuthorizationHeader["Basic ".Length..]));
        Assert.Equal("admin:secret-pw", decoded);
    }

    [Fact]
    public async Task GetPlayersAsync_ParsesLocationAndOtherFields()
    {
        const string body = """
        {"players":[{"name":"Alice","accountName":"alice","playerId":"P1","userId":"steam_123","ip":"1.2.3.4","ping":12.5,"location_x":100.5,"location_y":-200.25,"level":10,"building_count":5}]}
        """;
        await using var server = new FakeHttpServer(_ => (200, body));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "pw");

        var players = await client.GetPlayersAsync();

        Assert.Single(players);
        var player = players[0];
        Assert.Equal("Alice", player.Name);
        Assert.Equal("steam_123", player.UserId);
        Assert.Equal(100.5, player.LocationX);
        Assert.Equal(-200.25, player.LocationY);
        Assert.Equal(10, player.Level);
        Assert.Equal(5, player.BuildingCount);
    }

    [Fact]
    public async Task GetMetricsAsync_ParsesAllFields()
    {
        const string body = """{"serverfps":60,"currentplayernum":3,"serverframetime":16.7,"maxplayernum":32,"uptime":3600,"basecampnum":2,"days":5}""";
        await using var server = new FakeHttpServer(_ => (200, body));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "pw");

        var metrics = await client.GetMetricsAsync();

        Assert.Equal(60, metrics.ServerFps);
        Assert.Equal(3, metrics.CurrentPlayerNum);
        Assert.Equal(32, metrics.MaxPlayerNum);
        Assert.Equal(5, metrics.Days);
    }

    [Fact]
    public async Task AnnounceAsync_PostsMessageBodyToCorrectPath()
    {
        await using var server = new FakeHttpServer(_ => (200, "{}"));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "pw");

        await client.AnnounceAsync("Hello everyone");

        Assert.Equal("POST", server.LastRequest!.Method);
        Assert.Equal("/v1/api/announce", server.LastRequest.Path);
        Assert.Contains("\"message\":\"Hello everyone\"", server.LastRequest.Body);
    }

    [Fact]
    public async Task KickAsync_PostsUserIdAndMessage()
    {
        await using var server = new FakeHttpServer(_ => (200, "{}"));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "pw");

        await client.KickAsync("steam_123", "rule violation");

        Assert.Equal("/v1/api/kick", server.LastRequest!.Path);
        Assert.Contains("\"userid\":\"steam_123\"", server.LastRequest.Body);
        Assert.Contains("\"message\":\"rule violation\"", server.LastRequest.Body);
    }

    [Fact]
    public async Task ShutdownAsync_PostsWaitTimeAndMessage()
    {
        await using var server = new FakeHttpServer(_ => (200, "{}"));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "pw");

        await client.ShutdownAsync(30, "Restarting");

        Assert.Equal("/v1/api/shutdown", server.LastRequest!.Path);
        Assert.Contains("\"waittime\":30", server.LastRequest.Body);
        Assert.Contains("\"message\":\"Restarting\"", server.LastRequest.Body);
    }

    [Fact]
    public async Task SaveAsync_PostsToSavePath()
    {
        await using var server = new FakeHttpServer(_ => (200, "{}"));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "pw");

        await client.SaveAsync();

        Assert.Equal("/v1/api/save", server.LastRequest!.Path);
        Assert.Equal("POST", server.LastRequest.Method);
    }

    [Fact]
    public async Task UnauthorizedResponse_ThrowsWithClearMessage()
    {
        await using var server = new FakeHttpServer(_ => (401, """{"error":"unauthorized"}"""));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "wrong-pw");

        var ex = await Assert.ThrowsAsync<PalworldRestApiException>(() => client.GetInfoAsync());
        Assert.Contains("authentication failed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ErrorResponse_ThrowsWithStatusCodeAndBody()
    {
        await using var server = new FakeHttpServer(_ => (400, """{"error":"bad request"}"""));
        using var client = new PalworldRestApiClient("127.0.0.1", server.Port, "pw");

        var ex = await Assert.ThrowsAsync<PalworldRestApiException>(() => client.SaveAsync());
        Assert.Contains("400", ex.Message);
    }
}
