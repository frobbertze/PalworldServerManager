using System.IO.Compression;
using PalworldServerManager.Core.WorldData;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class ServerWipeServiceTests : IDisposable
{
    private readonly string _installPath;

    public ServerWipeServiceTests()
    {
        _installPath = Path.Combine(Path.GetTempPath(), "PalworldServerManagerTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_installPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_installPath))
        {
            Directory.Delete(_installPath, recursive: true);
        }
    }

    private string CreateFakeSaveData()
    {
        var saveGamesPath = ServerWipeService.GetSaveGamesPath(_installPath);
        var worldDir = Path.Combine(saveGamesPath, "0", "SOME-WORLD-GUID");
        Directory.CreateDirectory(worldDir);
        File.WriteAllText(Path.Combine(worldDir, "Level.sav"), "fake save data");
        File.WriteAllText(Path.Combine(worldDir, "LevelMeta.sav"), "fake meta");
        return saveGamesPath;
    }

    [Fact]
    public void HasSaveData_NoSaveGamesFolder_ReturnsFalse()
    {
        Assert.False(ServerWipeService.HasSaveData(_installPath));
    }

    [Fact]
    public void HasSaveData_WithSaveData_ReturnsTrue()
    {
        CreateFakeSaveData();
        Assert.True(ServerWipeService.HasSaveData(_installPath));
    }

    [Fact]
    public async Task BackupAndWipeAsync_NoSaveData_ReturnsNull()
    {
        var result = await ServerWipeService.BackupAndWipeAsync(_installPath);
        Assert.Null(result);
    }

    [Fact]
    public async Task BackupAndWipeAsync_DeletesSaveGamesFolder()
    {
        var saveGamesPath = CreateFakeSaveData();

        await ServerWipeService.BackupAndWipeAsync(_installPath);

        Assert.False(Directory.Exists(saveGamesPath));
        Assert.False(ServerWipeService.HasSaveData(_installPath));
    }

    [Fact]
    public async Task BackupAndWipeAsync_CreatesZipContainingOriginalFiles()
    {
        CreateFakeSaveData();

        var backupPath = await ServerWipeService.BackupAndWipeAsync(_installPath);

        Assert.NotNull(backupPath);
        Assert.True(File.Exists(backupPath));

        using var archive = ZipFile.OpenRead(backupPath!);
        var entryNames = archive.Entries.Select(e => e.FullName.Replace('\\', '/')).ToList();

        Assert.Contains("0/SOME-WORLD-GUID/Level.sav", entryNames);
        Assert.Contains("0/SOME-WORLD-GUID/LevelMeta.sav", entryNames);
    }

    [Fact]
    public async Task BackupAndWipeAsync_TwiceInSuccession_CreatesSeparateBackups()
    {
        CreateFakeSaveData();
        var firstBackup = await ServerWipeService.BackupAndWipeAsync(_installPath);

        CreateFakeSaveData();
        var secondBackup = await ServerWipeService.BackupAndWipeAsync(_installPath);

        Assert.NotEqual(firstBackup, secondBackup);
        Assert.True(File.Exists(firstBackup));
        Assert.True(File.Exists(secondBackup));
    }
}
