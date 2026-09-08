using System.Diagnostics;

namespace PalworldServerManager.Core.ServerProcess;

public class ServerProcessManager : IServerProcessManager
{
    public string GetExecutablePath(string serverInstallPath) => Path.Combine(serverInstallPath, "PalServer.exe");

    public ServerProcessStatus GetStatus(string serverInstallPath)
    {
        var exePath = GetExecutablePath(serverInstallPath);

        foreach (var process in Process.GetProcessesByName("PalServer"))
        {
            using (process)
            {
                string? modulePath;
                try
                {
                    modulePath = process.MainModule?.FileName;
                }
                catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
                {
                    // Access denied (e.g. a process owned by another user/elevation level) — not one of ours.
                    continue;
                }

                if (!string.Equals(modulePath, exePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                DateTime? startTimeUtc;
                try
                {
                    startTimeUtc = process.StartTime.ToUniversalTime();
                }
                catch
                {
                    startTimeUtc = null;
                }

                return new ServerProcessStatus(true, process.Id, startTimeUtc);
            }
        }

        return new ServerProcessStatus(false, null, null);
    }

    public void Start(string serverInstallPath, string? launchArguments)
    {
        var exePath = GetExecutablePath(serverInstallPath);

        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"PalServer.exe not found at '{exePath}'.", exePath);
        }

        if (GetStatus(serverInstallPath).IsRunning)
        {
            throw new InvalidOperationException("The server is already running.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = serverInstallPath,
            Arguments = launchArguments ?? string.Empty,
            UseShellExecute = true,
        };

        Process.Start(startInfo);
    }

    public async Task StopAsync(string serverInstallPath, TimeSpan gracefulTimeout, CancellationToken cancellationToken = default)
    {
        var status = GetStatus(serverInstallPath);
        if (!status.IsRunning || status.ProcessId is not int processId)
        {
            return;
        }

        Process process;
        try
        {
            process = Process.GetProcessById(processId);
        }
        catch (ArgumentException)
        {
            // Already exited between GetStatus() and here.
            return;
        }

        using (process)
        {
            try
            {
                // Requests a clean shutdown (the server's console control handler can save
                // the world before exiting), rather than immediately force-killing it.
                process.CloseMainWindow();
            }
            catch
            {
                // No main window to close — fall through to waiting/force-kill below.
            }

            using var timeoutCts = new CancellationTokenSource(gracefulTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(linkedCts.Token);
                return;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Graceful timeout elapsed — fall through to force-kill.
            }

            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(cancellationToken);
            }
        }
    }
}
