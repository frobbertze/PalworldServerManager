using System.Text.Json.Serialization;

namespace PalworldServerManager.Core.RestApi;

public record PalworldServerInfo(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("servername")] string ServerName,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("worldguid")] string WorldGuid);

public record PalworldRestPlayer(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("accountName")] string AccountName,
    [property: JsonPropertyName("playerId")] string PlayerId,
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("ip")] string Ip,
    [property: JsonPropertyName("ping")] double Ping,
    [property: JsonPropertyName("location_x")] double LocationX,
    [property: JsonPropertyName("location_y")] double LocationY,
    [property: JsonPropertyName("level")] int Level,
    [property: JsonPropertyName("building_count")] int BuildingCount);

internal record PalworldPlayersResponse([property: JsonPropertyName("players")] List<PalworldRestPlayer> Players);

public record PalworldMetrics(
    [property: JsonPropertyName("serverfps")] int ServerFps,
    [property: JsonPropertyName("currentplayernum")] int CurrentPlayerNum,
    [property: JsonPropertyName("serverframetime")] double ServerFrameTime,
    [property: JsonPropertyName("maxplayernum")] int MaxPlayerNum,
    [property: JsonPropertyName("uptime")] int Uptime,
    [property: JsonPropertyName("basecampnum")] int BaseCampNum,
    [property: JsonPropertyName("days")] int Days);
