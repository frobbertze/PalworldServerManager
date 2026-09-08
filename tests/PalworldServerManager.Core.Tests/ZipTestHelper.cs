using System.IO.Compression;

namespace PalworldServerManager.Core.Tests;

/// <summary>Builds small synthetic zip files for tests, so they don't depend on real downloaded mod files.</summary>
public static class ZipTestHelper
{
    /// <param name="entries">Entry path (using the given separator) -> content.</param>
    public static string CreateZip(string directory, string fileName, IReadOnlyDictionary<string, string> entries)
    {
        var path = Path.Combine(directory, fileName);
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var (entryPath, content) in entries)
        {
            var entry = archive.CreateEntry(entryPath);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(content);
        }

        return path;
    }
}
