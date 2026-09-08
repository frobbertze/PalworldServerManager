namespace PalworldServerManager.Core.ServerProcess;

public interface IServerProcessManager
{
    /// <summary>Full path to PalServer.exe under the given install folder.</summary>
    string GetExecutablePath(string serverInstallPath);

    /// <summary>
    /// Checks whether PalServer.exe from this install is currently running, by scanning
    /// running processes rather than relying on having started it ourselves — so it also
    /// detects a server someone started outside this tool, or before this app was last opened.
    /// </summary>
    ServerProcessStatus GetStatus(string serverInstallPath);

    /// <summary>Launches PalServer.exe. Throws if it's already running or the exe doesn't exist.</summary>
    void Start(string serverInstallPath, string? launchArguments);

    /// <summary>
    /// Asks the server to close (so it can save/shut down cleanly) and waits up to
    /// <paramref name="gracefulTimeout"/>; force-kills it if it hasn't exited by then.
    /// </summary>
    Task StopAsync(string serverInstallPath, TimeSpan gracefulTimeout, CancellationToken cancellationToken = default);
}
