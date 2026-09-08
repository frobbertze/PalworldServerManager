namespace PalworldServerManager.Core.Settings;

/// <summary>
/// Curated, friendly definitions for the most commonly-tweaked OptionSettings keys.
/// This is not exhaustive — every key (including ones not listed here) is still readable/editable
/// through the generic "Advanced" settings table, so nothing is inaccessible.
/// </summary>
public static class KnownSettings
{
    private const string CategoryRates = "Rates";
    private const string CategoryLimits = "Player & World Limits";
    private const string CategoryPvp = "PvP & Death";
    private const string CategoryServer = "Server Identity & Network";

    public static readonly IReadOnlyList<SettingDefinition> All =
    [
        // --- Rates ---
        new("DayTimeSpeedRate", "Daytime Speed", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f, Description: "How fast in-game daytime passes."),
        new("NightTimeSpeedRate", "Nighttime Speed", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f, Description: "How fast in-game nighttime passes."),
        new("ExpRate", "EXP Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 20f, 0.1f),
        new("PalCaptureRate", "Pal Capture Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PalSpawnNumRate", "Pal Spawn Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PalDamageRateAttack", "Pal Attack Damage Dealt", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PalDamageRateDefense", "Pal Damage Taken", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PlayerDamageRateAttack", "Player Attack Damage Dealt", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PlayerDamageRateDefense", "Player Damage Taken", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PlayerStomachDecreaceRate", "Player Hunger Depletion Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f, Description: "How fast the player's hunger meter drains."),
        new("PlayerStaminaDecreaceRate", "Player Stamina Depletion Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PlayerAutoHPRegeneRate", "Player HP Regen Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PlayerAutoHpRegeneRateInSleep", "Player HP Regen Rate (Sleeping)", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PalStomachDecreaceRate", "Pal Hunger Depletion Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f, Description: "How fast pals' hunger meters drain."),
        new("PalStaminaDecreaceRate", "Pal Stamina Depletion Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PalAutoHPRegeneRate", "Pal HP Regen Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("PalAutoHpRegeneRateInSleep", "Pal HP Regen Rate (In Palbox)", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("BuildObjectDamageRate", "Base Building Damage Taken", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("CollectionDropRate", "Gathering Drop Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("EnemyDropItemRate", "Enemy Drop Item Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("WorkSpeedRate", "Pal Work Speed Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f),
        new("ItemWeightRate", "Item Weight Rate", CategoryRates, SettingControlType.FloatSlider, 0f, 5f, 0.05f, Description: "Lower values make items weigh less in your inventory."),

        // --- Player & World Limits ---
        new("ServerPlayerMaxNum", "Max Players (Server)", CategoryLimits, SettingControlType.IntNumber, 1, 128, 1),
        new("CoopPlayerMaxNum", "Max Players (Per Co-op Party)", CategoryLimits, SettingControlType.IntNumber, 1, 8, 1),
        new("GuildPlayerMaxNum", "Max Players (Per Guild)", CategoryLimits, SettingControlType.IntNumber, 1, 100, 1),
        new("BaseCampMaxNum", "Max Base Camps (Server)", CategoryLimits, SettingControlType.IntNumber, 1, 500, 1),
        new("BaseCampWorkerMaxNum", "Max Pal Workers (Per Base)", CategoryLimits, SettingControlType.IntNumber, 1, 50, 1),
        new("BaseCampMaxNumInGuild", "Max Base Camps (Per Guild)", CategoryLimits, SettingControlType.IntNumber, 1, 20, 1),
        new("DropItemMaxNum", "Max Dropped Items (Server)", CategoryLimits, SettingControlType.IntNumber, 100, 10000, 100),

        // --- PvP & Death ---
        new("bIsPvP", "PvP Mode", CategoryPvp, SettingControlType.Toggle),
        new("bEnablePlayerToPlayerDamage", "Player-to-Player Damage", CategoryPvp, SettingControlType.Toggle),
        new("bEnableFriendlyFire", "Friendly Fire", CategoryPvp, SettingControlType.Toggle),
        new("bHardcore", "Hardcore Mode", CategoryPvp, SettingControlType.Toggle, Description: "Characters are deleted on death."),
        new("DeathPenalty", "Death Penalty", CategoryPvp, SettingControlType.Dropdown, DropdownOptions: ["None", "Item", "ItemAndEquipment", "All"]),

        // --- Server Identity & Network ---
        new("ServerName", "Server Name", CategoryServer, SettingControlType.Text),
        new("ServerDescription", "Server Description", CategoryServer, SettingControlType.Text),
        new("ServerPassword", "Server Password", CategoryServer, SettingControlType.Text, Description: "Leave blank for no password."),
        new("AdminPassword", "Admin Password", CategoryServer, SettingControlType.Text, Description: "Required for RCON and in-game admin commands."),
        new("PublicPort", "Public Port", CategoryServer, SettingControlType.IntNumber, 1024, 65535, 1),
        new("RCONEnabled", "RCON Enabled", CategoryServer, SettingControlType.Toggle),
        new("RCONPort", "RCON Port", CategoryServer, SettingControlType.IntNumber, 1024, 65535, 1),
    ];

    public static IEnumerable<IGrouping<string, SettingDefinition>> ByCategory() =>
        All.GroupBy(d => d.Category);

    private static readonly HashSet<string> KnownKeys = All.Select(d => d.Key).ToHashSet(StringComparer.Ordinal);

    public static bool IsKnown(string key) => KnownKeys.Contains(key);
}
