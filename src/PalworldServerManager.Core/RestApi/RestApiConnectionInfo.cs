using PalworldServerManager.Core.Settings;

namespace PalworldServerManager.Core.RestApi;

public record RestApiConnectionInfo(bool Enabled, string Host, int Port, string Password)
{
    public static RestApiConnectionInfo FromSettings(PalWorldOptionSettings settings) => new(
        Enabled: settings.GetBool("RESTAPIEnabled"),
        Host: "127.0.0.1",
        Port: settings.GetInt("RESTAPIPort"),
        Password: settings.GetString("AdminPassword"));
}
