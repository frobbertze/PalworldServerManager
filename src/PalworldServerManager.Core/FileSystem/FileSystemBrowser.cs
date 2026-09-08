using System.Security;

namespace PalworldServerManager.Core.FileSystem;

public class FileSystemBrowser : IFileSystemBrowser
{
    public IReadOnlyList<DirectoryEntry> GetRoots()
    {
        var roots = new List<DirectoryEntry>();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady)
            {
                continue;
            }

            roots.Add(new DirectoryEntry(drive.Name, drive.RootDirectory.FullName));
        }

        return roots;
    }

    public IReadOnlyList<DirectoryEntry> GetSubdirectories(string path)
    {
        try
        {
            return Directory.EnumerateDirectories(path)
                .Select(dir => new DirectoryEntry(Path.GetFileName(dir), dir))
                .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or SecurityException)
        {
            return [];
        }
    }

    public string? GetParent(string path)
    {
        var parent = Directory.GetParent(path);
        return parent?.FullName;
    }

    public bool LooksLikePalworldServerInstall(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        // Windows dedicated server, Linux dedicated server, and the settings folder
        // created the first time either one is launched.
        return File.Exists(Path.Combine(path, "PalServer.exe"))
            || File.Exists(Path.Combine(path, "PalServer.sh"))
            || Directory.Exists(Path.Combine(path, "Pal", "Saved", "Config"));
    }
}
