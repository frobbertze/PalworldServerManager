using System.IO.Compression;

namespace PalworldServerManager.Core.WorldData;

/// <summary>
/// Wipes a server's world save data so the next launch generates a fresh world, while keeping
/// PalWorldSettings.ini and everything else under Pal\Saved\Config untouched. Always backs up
/// the old save data as a zip before deleting it.
/// </summary>
public static class ServerWipeService
{
    public static string GetSaveGamesPath(string serverInstallPath) =>
        Path.Combine(serverInstallPath, "Pal", "Saved", "SaveGames");

    private static string GetBackupDirectory(string serverInstallPath) =>
        Path.Combine(serverInstallPath, "Pal", "Saved", "SaveGames_Backups");

    /// <summary>True if there's any save data to wipe.</summary>
    public static bool HasSaveData(string serverInstallPath)
    {
        var path = GetSaveGamesPath(serverInstallPath);
        return Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any();
    }

    /// <summary>
    /// Zips the current SaveGames folder into a timestamped backup, then deletes it.
    /// Returns the backup zip path, or null if there was no save data to wipe.
    /// </summary>
    public static async Task<string?> BackupAndWipeAsync(string serverInstallPath, CancellationToken cancellationToken = default)
    {
        var saveGamesPath = GetSaveGamesPath(serverInstallPath);
        if (!HasSaveData(serverInstallPath))
        {
            return null;
        }

        var backupDirectory = GetBackupDirectory(serverInstallPath);
        Directory.CreateDirectory(backupDirectory);

        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var backupPath = Path.Combine(backupDirectory, $"SaveGames-{DateTime.Now:yyyyMMdd-HHmmss}-{uniqueSuffix}.zip");

        await Task.Run(() => ZipFile.CreateFromDirectory(saveGamesPath, backupPath, CompressionLevel.Optimal, includeBaseDirectory: false), cancellationToken);

        Directory.Delete(saveGamesPath, recursive: true);

        return backupPath;
    }
}
