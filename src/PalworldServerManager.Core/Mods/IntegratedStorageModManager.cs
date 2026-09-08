namespace PalworldServerManager.Core.Mods;

/// <summary>
/// Manages the "Integrate all camp storage" UE4SS mod
/// (https://www.nexusmods.com/palworld/mods/2028, source: https://github.com/Sarfflow/palworld-integrated-storage).
/// Purpose-built for this one mod rather than a generic mod manager — its zip is extracted
/// verbatim as a single "ModIntegratedStorageCpp" folder into UE4SS's Mods directory. The mod's
/// own "enabled.txt" file (present or absent) is what UE4SS uses to toggle it on/off.
/// </summary>
public static class IntegratedStorageModManager
{
    private const string ModFolderName = "ModIntegratedStorageCpp";

    public static string GetModsDirectory(string serverInstallPath) =>
        Path.Combine(Ue4ssManager.GetWin64Path(serverInstallPath), "ue4ss", "Mods");

    private static string GetModDirectory(string serverInstallPath) =>
        Path.Combine(GetModsDirectory(serverInstallPath), ModFolderName);

    private static string GetEnabledFilePath(string serverInstallPath) =>
        Path.Combine(GetModDirectory(serverInstallPath), "enabled.txt");

    private static string GetConfigFilePath(string serverInstallPath) =>
        Path.Combine(GetModDirectory(serverInstallPath), "config.txt");

    public static bool IsInstalled(string serverInstallPath) =>
        File.Exists(Path.Combine(GetModDirectory(serverInstallPath), "dlls", "main.dll"));

    public static bool IsEnabled(string serverInstallPath) =>
        File.Exists(GetEnabledFilePath(serverInstallPath));

    public static bool HasConfig(string serverInstallPath) =>
        File.Exists(GetConfigFilePath(serverInstallPath));

    public static void InstallFromZip(string serverInstallPath, string zipFilePath)
    {
        if (!ZipInstaller.ZipContainsEntry(zipFilePath, "ModIntegratedStorageCpp/dlls/main.dll"))
        {
            throw new InvalidOperationException(
                "This doesn't look like the Integrated Storage mod zip — expected it to contain " +
                "ModIntegratedStorageCpp/dlls/main.dll.");
        }

        ZipInstaller.ExtractTo(zipFilePath, GetModsDirectory(serverInstallPath));
    }

    public static void SetEnabled(string serverInstallPath, bool enabled)
    {
        var path = GetEnabledFilePath(serverInstallPath);
        if (enabled)
        {
            File.WriteAllText(path, string.Empty);
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static string ReadConfig(string serverInstallPath) =>
        File.ReadAllText(GetConfigFilePath(serverInstallPath));

    public static void WriteConfig(string serverInstallPath, string content) =>
        File.WriteAllText(GetConfigFilePath(serverInstallPath), content);
}
