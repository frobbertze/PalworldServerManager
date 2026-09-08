using PalworldServerManager.Core.Mods;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class Ue4ssManagerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _installPath;

    public Ue4ssManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "Ue4ssManagerTests_" + Guid.NewGuid());
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

    [Fact]
    public void IsInstalled_NothingPresent_ReturnsFalse()
    {
        Assert.False(Ue4ssManager.IsInstalled(_installPath));
    }

    [Fact]
    public void InstallFromZip_ValidZip_ExtractsAndBecomesInstalled()
    {
        // Mirrors the real UE4SS release zip's structure: entries rooted at the server install path.
        var zipPath = ZipTestHelper.CreateZip(_tempDir, "ue4ss.zip", new Dictionary<string, string>
        {
            ["Pal/Binaries/Win64/dwmapi.dll"] = "fake-dwmapi",
            ["Pal/Binaries/Win64/ue4ss/UE4SS.dll"] = "fake-ue4ss-dll",
            ["Pal/Binaries/Win64/ue4ss/Mods/mods.txt"] = "BPModLoaderMod : 1",
        });

        Ue4ssManager.InstallFromZip(_installPath, zipPath);

        Assert.True(Ue4ssManager.IsInstalled(_installPath));
        Assert.True(File.Exists(Path.Combine(_installPath, "Pal", "Binaries", "Win64", "dwmapi.dll")));
        Assert.True(File.Exists(Path.Combine(_installPath, "Pal", "Binaries", "Win64", "ue4ss", "Mods", "mods.txt")));
    }

    [Fact]
    public void InstallFromZip_WrongZip_ThrowsAndDoesNotInstall()
    {
        var zipPath = ZipTestHelper.CreateZip(_tempDir, "wrong.zip", new Dictionary<string, string>
        {
            ["SomeOtherMod/readme.txt"] = "not ue4ss",
        });

        Assert.Throws<InvalidOperationException>(() => Ue4ssManager.InstallFromZip(_installPath, zipPath));
        Assert.False(Ue4ssManager.IsInstalled(_installPath));
    }

    [Fact]
    public void InstallFromZip_BackslashSeparatedEntries_StillWorks()
    {
        // Some zip tools write backslash-separated entry paths; the real Integrated Storage
        // zip does this, so verify UE4SS install logic tolerates it too, defensively.
        var zipPath = ZipTestHelper.CreateZip(_tempDir, "ue4ss-backslash.zip", new Dictionary<string, string>
        {
            [@"Pal\Binaries\Win64\dwmapi.dll"] = "fake-dwmapi",
            [@"Pal\Binaries\Win64\ue4ss\UE4SS.dll"] = "fake-ue4ss-dll",
        });

        Ue4ssManager.InstallFromZip(_installPath, zipPath);

        Assert.True(Ue4ssManager.IsInstalled(_installPath));
    }
}
