using PalworldServerManager.Core.Mods;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class IntegratedStorageModManagerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _installPath;

    public IntegratedStorageModManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "IntegratedStorageModManagerTests_" + Guid.NewGuid());
        _installPath = Path.Combine(_tempDir, "PalServer");
        Directory.CreateDirectory(_installPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    private string CreateRealisticModZip()
    {
        // Mirrors the real mod zip's exact structure (backslash-separated, as confirmed
        // by inspecting the actual downloaded file).
        return ZipTestHelper.CreateZip(_tempDir, "mod.zip", new Dictionary<string, string>
        {
            [@"ModIntegratedStorageCpp\config.txt"] = "verbose = true\nreconcile_interval_ms = 8000\n",
            [@"ModIntegratedStorageCpp\enabled.txt"] = "",
            [@"ModIntegratedStorageCpp\dlls\main.dll"] = "fake-dll-bytes",
        });
    }

    [Fact]
    public void IsInstalled_NothingPresent_ReturnsFalse()
    {
        Assert.False(IntegratedStorageModManager.IsInstalled(_installPath));
    }

    [Fact]
    public void InstallFromZip_RealisticZip_InstallsEnabledWithConfig()
    {
        var zipPath = CreateRealisticModZip();

        IntegratedStorageModManager.InstallFromZip(_installPath, zipPath);

        Assert.True(IntegratedStorageModManager.IsInstalled(_installPath));
        Assert.True(IntegratedStorageModManager.IsEnabled(_installPath));
        Assert.True(IntegratedStorageModManager.HasConfig(_installPath));
        Assert.Contains("reconcile_interval_ms", IntegratedStorageModManager.ReadConfig(_installPath));
    }

    [Fact]
    public void InstallFromZip_WrongZip_Throws()
    {
        var zipPath = ZipTestHelper.CreateZip(_tempDir, "wrong.zip", new Dictionary<string, string>
        {
            ["SomeOtherThing/file.txt"] = "not the mod",
        });

        Assert.Throws<InvalidOperationException>(() => IntegratedStorageModManager.InstallFromZip(_installPath, zipPath));
        Assert.False(IntegratedStorageModManager.IsInstalled(_installPath));
    }

    [Fact]
    public void SetEnabled_False_RemovesEnabledFile_ThenTrue_RecreatesIt()
    {
        IntegratedStorageModManager.InstallFromZip(_installPath, CreateRealisticModZip());
        Assert.True(IntegratedStorageModManager.IsEnabled(_installPath));

        IntegratedStorageModManager.SetEnabled(_installPath, false);
        Assert.False(IntegratedStorageModManager.IsEnabled(_installPath));

        IntegratedStorageModManager.SetEnabled(_installPath, true);
        Assert.True(IntegratedStorageModManager.IsEnabled(_installPath));
    }

    [Fact]
    public void WriteConfig_ThenReadConfig_RoundTrips()
    {
        IntegratedStorageModManager.InstallFromZip(_installPath, CreateRealisticModZip());

        IntegratedStorageModManager.WriteConfig(_installPath, "verbose = false\n");

        Assert.Equal("verbose = false\n", IntegratedStorageModManager.ReadConfig(_installPath));
    }
}
