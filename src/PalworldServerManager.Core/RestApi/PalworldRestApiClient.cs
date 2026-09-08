using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PalworldServerManager.Core.RestApi;

/// <summary>
/// Client for Palworld's official REST admin API (https://docs.palworldgame.com/category/rest-api/).
/// Compared to RCON, this gives structured JSON, richer player data (location, level, ping,
/// building count) and server metrics — but only the fixed set of endpoints below; there's no
/// arbitrary "run any command" endpoint the way RCON has.
/// </summary>
public sealed class PalworldRestApiClient : IDisposable
{
    private readonly HttpClient _http;

    public PalworldRestApiClient(string host, int port, string password)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri($"http://{host}:{port}/v1/api/"),
            Timeout = TimeSpan.FromSeconds(10),
        };

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"admin:{password}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public Task<PalworldServerInfo> GetInfoAsync(CancellationToken cancellationToken = default) =>
        GetAsync<PalworldServerInfo>("info", cancellationToken);

    public async Task<IReadOnlyList<PalworldRestPlayer>> GetPlayersAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<PalworldPlayersResponse>("players", cancellationToken);
        return response.Players;
    }

    public Task<PalworldMetrics> GetMetricsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<PalworldMetrics>("metrics", cancellationToken);

    public Task AnnounceAsync(string message, CancellationToken cancellationToken = default) =>
        PostAsync("announce", new { message }, cancellationToken);

    public Task KickAsync(string userId, string? message = null, CancellationToken cancellationToken = default) =>
        PostAsync("kick", new { userid = userId, message }, cancellationToken);

    public Task BanAsync(string userId, string? message = null, CancellationToken cancellationToken = default) =>
        PostAsync("ban", new { userid = userId, message }, cancellationToken);

    public Task UnbanAsync(string userId, CancellationToken cancellationToken = default) =>
        PostAsync("unban", new { userid = userId }, cancellationToken);

    public Task SaveAsync(CancellationToken cancellationToken = default) =>
        PostAsync("save", new { }, cancellationToken);

    public Task ShutdownAsync(int waitTimeSeconds, string? message = null, CancellationToken cancellationToken = default) =>
        PostAsync("shutdown", new { waittime = waitTimeSeconds, message }, cancellationToken);

    /// <summary>Immediately force-stops the server — no save, no countdown. Prefer ShutdownAsync.</summary>
    public Task ForceStopAsync(CancellationToken cancellationToken = default) =>
        PostAsync("stop", new { }, cancellationToken);

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        return result ?? throw new PalworldRestApiException($"Empty response from '{path}'.");
    }

    private async Task PostAsync<TBody>(string path, TBody body, CancellationToken cancellationToken)
    {
        // Palworld's embedded HTTP server rejects chunked transfer-encoding (returns 411
        // Length Required), which is what PostAsJsonAsync/JsonContent send by default —
        // so serialize up front into a StringContent, which always sets Content-Length.
        var json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _http.PostAsync(path, content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new PalworldRestApiException("REST API authentication failed — check the admin password.");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new PalworldRestApiException($"REST API request failed ({(int)response.StatusCode} {response.StatusCode}): {body}");
    }

    public void Dispose() => _http.Dispose();
}
