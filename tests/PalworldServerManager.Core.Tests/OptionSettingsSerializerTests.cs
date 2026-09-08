using PalworldServerManager.Core.Settings;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class OptionSettingsSerializerTests
{
    // Copied verbatim from a real PalWorldSettings.ini's OptionSettings line.
    private const string RealOptionSettingsValue =
        "(Difficulty=None,RandomizerType=None,RandomizerSeed=\"\",bIsRandomizerPalLevelRandom=False,DayTimeSpeedRate=1.000000,NightTimeSpeedRate=1.000000,ExpRate=1.000000,PalCaptureRate=1.000000,PalSpawnNumRate=1.000000,PalDamageRateAttack=1.000000,PalDamageRateDefense=1.000000,PlayerDamageRateAttack=1.000000,PlayerDamageRateDefense=1.000000,PlayerStomachDecreaceRate=1.000000,PlayerStaminaDecreaceRate=1.000000,PlayerAutoHPRegeneRate=1.000000,PlayerAutoHpRegeneRateInSleep=1.000000,PalStomachDecreaceRate=1.000000,PalStaminaDecreaceRate=1.000000,PalAutoHPRegeneRate=1.000000,PalAutoHpRegeneRateInSleep=1.000000,BuildObjectHpRate=1.000000,BuildObjectDamageRate=1.000000,BuildObjectDeteriorationDamageRate=1.000000,CollectionDropRate=1.000000,CollectionObjectHpRate=1.000000,CollectionObjectRespawnSpeedRate=1.000000,EnemyDropItemRate=1.000000,DeathPenalty=Item,bEnablePlayerToPlayerDamage=False,bEnableFriendlyFire=False,bEnableInvaderEnemy=True,bActiveUNKO=False,bEnableAimAssistPad=True,bEnableAimAssistKeyboard=False,DropItemMaxNum=3000,PhysicsActiveDropItemMaxNum=-1,DropItemMaxNum_UNKO=100,BaseCampMaxNum=128,BaseCampWorkerMaxNum=15,DropItemAliveMaxHours=1.000000,bAutoResetGuildNoOnlinePlayers=False,AutoResetGuildTimeNoOnlinePlayers=72.000000,GuildPlayerMaxNum=20,BaseCampMaxNumInGuild=4,PalEggDefaultHatchingTime=1.000000,WorkSpeedRate=1.000000,AutoSaveSpan=30.000000,bIsMultiplay=False,bIsPvP=False,bHardcore=False,bPalLost=False,bCharacterRecreateInHardcore=False,bCanPickupOtherGuildDeathPenaltyDrop=False,bEnableNonLoginPenalty=True,bEnableFastTravel=True,bEnableFastTravelOnlyBaseCamp=False,bIsStartLocationSelectByMap=False,bExistPlayerAfterLogout=False,bEnableDefenseOtherGuildPlayer=False,bInvisibleOtherGuildBaseCampAreaFX=False,bBuildAreaLimit=False,ItemWeightRate=1.000000,CoopPlayerMaxNum=4,ServerPlayerMaxNum=32,ServerName=\"Default Palworld Server\",ServerDescription=\"\",AdminPassword=\"bd07d182f400\",ServerPassword=\"\",bAllowClientMod=True,PublicPort=8211,PublicIP=\"\",RCONEnabled=False,RCONPort=25575,Region=\"\",bUseAuth=True,BanListURL=\"https://b.palworldgame.com/api/banlist.txt\",RESTAPIEnabled=True,RESTAPIPort=8213,bShowPlayerList=False,ChatPostLimitPerMinute=30,CrossplayPlatforms=(Steam,Xbox,PS5,Mac),bIsUseBackupSaveData=True,LogFormatType=Text,bIsShowJoinLeftMessage=True,SupplyDropSpan=180,EnablePredatorBossPal=True,MaxBuildingLimitNum=0,MaxBuildingLimitNumPerPlayer=0,ServerReplicatePawnCullDistance=15000.000000,bAllowGlobalPalboxExport=True,bAllowGlobalPalboxImport=False,EquipmentDurabilityDamageRate=1.000000,ItemContainerForceMarkDirtyInterval=1.000000,PlayerDataPalStorageUpdateCheckTickInterval=1.000000,ItemCorruptionMultiplier=1.000000,MonsterFarmActionSpeedRate=1.000000,FishingDifficultyRate=1.000000,DenyTechnologyList=,GuildRejoinCooldownMinutes=0,AutoTransferMasterCheckIntervalSeconds=3600.000000,AutoTransferMasterThresholdDays=14,MaxGuildsPerFrame=10,BlockRespawnTime=5.000000,RespawnPenaltyDurationThreshold=0.000000,RespawnPenaltyTimeScale=2.000000,bDisplayPvPItemNumOnWorldMap_BaseCamp=False,bDisplayPvPItemNumOnWorldMap_Player=False,AdditionalDropItemWhenPlayerKillingInPvPMode=\"PlayerDropItem\",AdditionalDropItemNumWhenPlayerKillingInPvPMode=1,bAdditionalDropItemWhenPlayerKillingInPvPMode=False,bEnableVoiceChat=False,VoiceChatMaxVolumeDistance=3000.000000,VoiceChatZeroVolumeDistance=15000.000000,bAllowEnhanceStat_Health=True,bAllowEnhanceStat_Attack=True,bAllowEnhanceStat_Stamina=True,bAllowEnhanceStat_Weight=True,bAllowEnhanceStat_WorkSpeed=True,bEnableBuildingPlayerUIdDisplay=False,BuildingNameDisplayCacheTTLSeconds=60,bAllowEnemyCampSpawnNearBaseCamp=False)";

    [Fact]
    public void Parse_ThenSerialize_RoundTripsExactly()
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        var reserialized = OptionSettingsSerializer.Serialize(settings);

        Assert.Equal(RealOptionSettingsValue, reserialized);
    }

    [Fact]
    public void Parse_ExtractsExpectedKeyCount()
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);

        // Sanity check: every top-level key from the real file should be present exactly once.
        Assert.Equal(122, settings.Keys.Count);
        Assert.Equal(settings.Keys.Count, settings.Keys.Distinct().Count());
    }

    [Theory]
    [InlineData("DayTimeSpeedRate", 1.000000f)]
    [InlineData("PalStomachDecreaceRate", 1.000000f)]
    [InlineData("ServerReplicatePawnCullDistance", 15000.000000f)]
    public void GetFloat_ReturnsParsedValue(string key, float expected)
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        Assert.Equal(expected, settings.GetFloat(key));
    }

    [Theory]
    [InlineData("bIsPvP", false)]
    [InlineData("bEnableInvaderEnemy", true)]
    [InlineData("RESTAPIEnabled", true)]
    public void GetBool_ReturnsParsedValue(string key, bool expected)
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        Assert.Equal(expected, settings.GetBool(key));
    }

    [Theory]
    [InlineData("ServerPlayerMaxNum", 32)]
    [InlineData("PublicPort", 8211)]
    [InlineData("PhysicsActiveDropItemMaxNum", -1)]
    public void GetInt_ReturnsParsedValue(string key, int expected)
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        Assert.Equal(expected, settings.GetInt(key));
    }

    [Fact]
    public void GetString_UnquotesServerName()
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        Assert.Equal("Default Palworld Server", settings.GetString("ServerName"));
    }

    [Fact]
    public void GetString_EmptyQuotedValue_ReturnsEmptyString()
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        Assert.Equal(string.Empty, settings.GetString("ServerDescription"));
    }

    [Fact]
    public void GetRaw_EmptyUnquotedValue_ReturnsEmptyString()
    {
        // DenyTechnologyList=,GuildRejoinCooldownMinutes=0 — a completely empty (not quoted) value.
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        Assert.Equal(string.Empty, settings.GetRaw("DenyTechnologyList"));
    }

    [Fact]
    public void GetRaw_NestedListValue_PreservesRawParens()
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        Assert.Equal("(Steam,Xbox,PS5,Mac)", settings.GetRaw("CrossplayPlatforms"));
    }

    [Fact]
    public void SetFloat_ThenSerialize_UsesGameNumberFormat()
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        settings.SetFloat("PalStomachDecreaceRate", 2.5f);

        Assert.Equal("2.500000", settings.GetRaw("PalStomachDecreaceRate"));
    }

    [Fact]
    public void SetString_ThenGetString_RoundTripsAndEscapesQuotes()
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        settings.SetString("ServerName", "Bob's \"Awesome\" Server");

        Assert.Equal("Bob's \"Awesome\" Server", settings.GetString("ServerName"));
        Assert.Equal("\"Bob's \\\"Awesome\\\" Server\"", settings.GetRaw("ServerName"));
    }

    [Fact]
    public void ChangingOneValue_DoesNotDisturbOtherKeysOrOrder()
    {
        var settings = OptionSettingsSerializer.Parse(RealOptionSettingsValue);
        settings.SetBool("bIsPvP", true);
        var reserialized = OptionSettingsSerializer.Serialize(settings);

        var expected = RealOptionSettingsValue.Replace("bIsPvP=False", "bIsPvP=True");
        Assert.Equal(expected, reserialized);
    }
}
