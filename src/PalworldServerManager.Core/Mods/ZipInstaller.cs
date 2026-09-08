using System.IO.Compression;

namespace PalworldServerManager.Core.Mods;

/// <summary>Small shared helper for "does this zip look right, then extract it" installers.</summary>
public static class ZipInstaller
{
    /// <summary>True if any entry's path ends with <paramref name="entryPathSuffix"/>, comparing
    /// with '/' and '\' both normalized (zips in the wild use either as a directory separator).</summary>
    public static bool ZipContainsEntry(string zipFilePath, string entryPathSuffix)
    {
        using var archive = ZipFile.OpenRead(zipFilePath);
        var normalizedSuffix = Normalize(entryPathSuffix);
        return archive.Entries.Any(e => Normalize(e.FullName).EndsWith(normalizedSuffix, StringComparison.OrdinalIgnoreCase));
    }

    public static void ExtractTo(string zipFilePath, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory, overwriteFiles: true);
    }

    private static string Normalize(string path) => path.Replace('\\', '/');
}
