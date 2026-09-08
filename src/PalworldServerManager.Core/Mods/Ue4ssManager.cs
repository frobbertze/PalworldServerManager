namespace PalworldServerManager.Core.Mods;

/// <summary>
/// Detects and installs UE4SS (the Unreal Engine scripting mod loader) on a Palworld dedicated
/// server. This tool never downloads UE4SS itself — the release zip is something you source and
/// verify yourself; this just automates extracting it into the right place once you have it.
/// </summary>
public static class Ue4ssManager
{
    public static string GetWin64Path(string serverInstallPath) =>
        Path.Combine(serverInstallPath, "Pal", "Binaries", "Win64");

    private static string GetDwmapiPath(string serverInstallPath) =>
        Path.Combine(GetWin64Path(serverInstallPath), "dwmapi.dll");

    private static string GetUE4SSDllPath(string serverInstallPath) =>
        Path.Combine(GetWin64Path(serverInstallPath), "ue4ss", "UE4SS.dll");

    public static bool IsInstalled(string serverInstallPath) =>
        File.Exists(GetDwmapiPath(serverInstallPath)) && File.Exists(GetUE4SSDllPath(serverInstallPath));

    /// <summary>
    /// Extracts a UE4SS release zip (one whose entries already start with "Pal/Binaries/Win64/...",
    /// matching the official Palworld-specific release layout) directly into the server install root.
    /// </summary>
    public static void InstallFromZip(string serverInstallPath, string zipFilePath)
    {
        if (!ZipInstaller.ZipContainsEntry(zipFilePath, "Pal/Binaries/Win64/ue4ss/UE4SS.dll") ||
            !ZipInstaller.ZipContainsEntry(zipFilePath, "Pal/Binaries/Win64/dwmapi.dll"))
        {
            throw new InvalidOperationException(
                "This doesn't look like a UE4SS release zip — expected it to contain " +
                "Pal/Binaries/Win64/ue4ss/UE4SS.dll and Pal/Binaries/Win64/dwmapi.dll.");
        }

        ZipInstaller.ExtractTo(zipFilePath, serverInstallPath);
    }
}
