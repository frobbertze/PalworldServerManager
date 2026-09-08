using PalworldServerManager.Core.Settings;

namespace PalworldServerManager.Core.Rcon;

public record RconConnectionInfo(bool Enabled, string Host, int Port, string Password)
{
    /// <summary>RCON always listens on the same machine the server runs on; this app only
    /// ever talks to a server it's pointed at locally, so "127.0.0.1" is always correct.</summary>
    public static RconConnectionInfo FromSettings(PalWorldOptionSettings settings) => new(
        Enabled: settings.GetBool("RCONEnabled"),
        Host: "127.0.0.1",
        Port: settings.GetInt("RCONPort"),
        Password: settings.GetString("AdminPassword"));
}
