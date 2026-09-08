namespace PalworldServerManager.Core.Configuration;

public class AppConfig
{
    public string? ServerInstallPath { get; set; }

    /// <summary>Extra command-line arguments passed to PalServer.exe on launch (e.g. "-useperfthreads -NoAsyncLoadingThread").</summary>
    public string? LaunchArguments { get; set; }
}
