namespace PalworldServerManager.Core.Rcon;

/// <summary>Typed wrapper over <see cref="RconClient"/> for Palworld's specific RCON commands.</summary>
public sealed class PalworldRconClient : IAsyncDisposable
{
    private readonly RconClient _client;

    private PalworldRconClient(RconClient client)
    {
        _client = client;
    }

    public static async Task<PalworldRconClient> ConnectAsync(string host, int port, string password, CancellationToken cancellationToken = default)
    {
        var client = new RconClient();
        try
        {
            await client.ConnectAndAuthenticateAsync(host, port, password, cancellationToken);
        }
        catch
        {
            await client.DisposeAsync();
            throw;
        }

        return new PalworldRconClient(client);
    }

    public Task<string> GetInfoAsync(CancellationToken cancellationToken = default) =>
        _client.SendCommandAsync("Info", cancellationToken);

    public Task<string> SaveAsync(CancellationToken cancellationToken = default) =>
        _client.SendCommandAsync("Save", cancellationToken);

    public async Task<IReadOnlyList<PalworldPlayer>> ShowPlayersAsync(CancellationToken cancellationToken = default)
    {
        var raw = await _client.SendCommandAsync("ShowPlayers", cancellationToken);
        return ParsePlayers(raw);
    }

    /// <summary>Broadcasts a message to all players. Spaces are sent as underscores — a
    /// known Palworld RCON quirk where the command otherwise truncates at the first space.</summary>
    public Task BroadcastAsync(string message, CancellationToken cancellationToken = default) =>
        _client.SendCommandAsync($"Broadcast {EscapeMessage(message)}", cancellationToken);

    /// <summary>Asks the server to shut down after a countdown, saving the world first — the
    /// correct, safe way to stop a Palworld server (as opposed to killing the process).</summary>
    public Task<string> ShutdownAsync(int countdownSeconds, string message, CancellationToken cancellationToken = default) =>
        _client.SendCommandAsync($"Shutdown {countdownSeconds} {EscapeMessage(message)}", cancellationToken);

    public Task<string> KickPlayerAsync(string steamId, CancellationToken cancellationToken = default) =>
        _client.SendCommandAsync($"KickPlayer {steamId}", cancellationToken);

    public Task<string> BanPlayerAsync(string steamId, CancellationToken cancellationToken = default) =>
        _client.SendCommandAsync($"BanPlayer {steamId}", cancellationToken);

    public Task<string> UnbanPlayerAsync(string steamId, CancellationToken cancellationToken = default) =>
        _client.SendCommandAsync($"UnBanPlayer {steamId}", cancellationToken);

    public Task<string> ExecuteRawAsync(string command, CancellationToken cancellationToken = default) =>
        _client.SendCommandAsync(command, cancellationToken);

    private static string EscapeMessage(string message) => message.Replace(' ', '_');

    internal static IReadOnlyList<PalworldPlayer> ParsePlayers(string raw)
    {
        var players = new List<PalworldPlayer>();

        var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            if (line.Length == 0 || line.Equals("name,playeruid,steamid", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length < 3)
            {
                continue;
            }

            // A player name could itself contain a comma — treat the last two fields as
            // playeruid/steamid and rejoin everything before that as the name.
            var steamId = parts[^1].Trim();
            var playerUid = parts[^2].Trim();
            var name = string.Join(',', parts[..^2]).Trim();

            players.Add(new PalworldPlayer(name, playerUid, steamId));
        }

        return players;
    }

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}
