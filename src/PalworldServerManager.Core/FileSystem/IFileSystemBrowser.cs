namespace PalworldServerManager.Core.FileSystem;

public interface IFileSystemBrowser
{
    /// <summary>Top-level entries to start browsing from (drives on Windows, "/" on Linux/macOS).</summary>
    IReadOnlyList<DirectoryEntry> GetRoots();

    /// <summary>Immediate subdirectories of <paramref name="path"/>, sorted by name. Inaccessible entries are silently skipped.</summary>
    IReadOnlyList<DirectoryEntry> GetSubdirectories(string path);

    /// <summary>The parent directory of <paramref name="path"/>, or null if it's already a root.</summary>
    string? GetParent(string path);

    /// <summary>Heuristic check for whether a folder looks like a Palworld dedicated server install.</summary>
    bool LooksLikePalworldServerInstall(string path);
}
