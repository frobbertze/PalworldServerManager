namespace PalworldServerManager.Core.Settings;

/// <summary>
/// Locates and reads/writes a server install's PalWorldSettings.ini — specifically the single
/// "OptionSettings=(...)" line inside it, which is where every server-tunable setting lives.
/// </summary>
public class PalWorldSettingsFile
{
    private const string OptionSettingsPrefix = "OptionSettings=";

    /// <summary>
    /// Finds PalWorldSettings.ini under &lt;serverInstallPath&gt;\Pal\Saved\Config\&lt;Platform&gt;Server.
    /// Checks WindowsServer and LinuxServer (a Linux-hosted server can still be pointed at from
    /// this app), preferring whichever already exists; defaults to WindowsServer if neither does yet.
    /// </summary>
    public static string ResolvePath(string serverInstallPath)
    {
        var configRoot = Path.Combine(serverInstallPath, "Pal", "Saved", "Config");
        var windowsPath = Path.Combine(configRoot, "WindowsServer", "PalWorldSettings.ini");
        var linuxPath = Path.Combine(configRoot, "LinuxServer", "PalWorldSettings.ini");

        if (File.Exists(windowsPath))
        {
            return windowsPath;
        }

        if (File.Exists(linuxPath))
        {
            return linuxPath;
        }

        return windowsPath;
    }

    public static bool Exists(string serverInstallPath) => File.Exists(ResolvePath(serverInstallPath));

    public static async Task<PalWorldOptionSettings> LoadAsync(string serverInstallPath, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(serverInstallPath);
        var lines = await File.ReadAllLinesAsync(path, cancellationToken);

        foreach (var line in lines)
        {
            if (line.StartsWith(OptionSettingsPrefix, StringComparison.Ordinal))
            {
                var value = line[OptionSettingsPrefix.Length..];
                return OptionSettingsSerializer.Parse(value);
            }
        }

        throw new InvalidDataException($"No OptionSettings line found in '{path}'.");
    }

    /// <summary>
    /// Writes the settings back to the ini file, replacing only the OptionSettings line so any
    /// other lines/sections stay untouched. A timestamped backup of the previous file is kept
    /// alongside it before it's overwritten.
    /// </summary>
    public static async Task SaveAsync(string serverInstallPath, PalWorldOptionSettings settings, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(serverInstallPath);
        var lines = (await File.ReadAllLinesAsync(path, cancellationToken)).ToList();

        var newLine = OptionSettingsPrefix + OptionSettingsSerializer.Serialize(settings);
        var replaced = false;

        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith(OptionSettingsPrefix, StringComparison.Ordinal))
            {
                lines[i] = newLine;
                replaced = true;
                break;
            }
        }

        if (!replaced)
        {
            lines.Add(newLine);
        }

        var backupPath = path + $".{DateTime.Now:yyyyMMdd-HHmmss}.bak";
        File.Copy(path, backupPath, overwrite: false);

        await File.WriteAllLinesAsync(path, lines, cancellationToken);
    }
}
