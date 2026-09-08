namespace PalworldServerManager.Core.Map;

/// <summary>
/// Static locations of ore resource nodes (coal/metal/quartz/sulfur) on the map. This is a
/// small, filtered subset (coordinates + category only — no names, images, or other content)
/// extracted from the community-compiled dataset in the fa0311/palworld-map project
/// (public/pin_data.json, itself sourced from the GameWith wiki). Kept for personal, local-only
/// reference alongside the map image — see wwwroot/images/palworld-map.SOURCE.txt.
/// </summary>
public static class ResourceNodeData
{
    public static readonly IReadOnlyList<ResourceNode> All =
    [
        new("12002", ResourceType.Metal, -36.45, 6.39),
        new("12003", ResourceType.Metal, -47.43, 0.72),
        new("12004", ResourceType.Metal, -23.58, 8.37),
        new("12005", ResourceType.Metal, -28.26, -7.02),
        new("12006", ResourceType.Metal, -15.21, -2.88),
        new("12007", ResourceType.Metal, -7.38, 0),
        new("12008", ResourceType.Metal, -20.34, 24.3),
        new("12009", ResourceType.Metal, -8.37, 22.68),
        new("12010", ResourceType.Metal, -3.33, 27.99),
        new("12011", ResourceType.Metal, -3.6, 16.38),
        new("12012", ResourceType.Metal, -40.86, -22.23),
        new("12013", ResourceType.Metal, -32.31, -23.04),
        new("12014", ResourceType.Metal, -60.3, -68.94),
        new("12015", ResourceType.Metal, -44.73, -57.33),
        new("12016", ResourceType.Metal, -42.93, -48.24),
        new("12017", ResourceType.Metal, -54.27, -37.26),
        new("12018", ResourceType.Metal, -26.01, -60.57),
        new("12019", ResourceType.Sulfur, -36, -41.85),
        new("12020", ResourceType.Sulfur, -46.26, -45.27),
        new("12021", ResourceType.Sulfur, -47.79, -53.46),
        new("12022", ResourceType.Sulfur, -39.6, -67.86),
        new("12023", ResourceType.Sulfur, -30.87, -66.42),
        new("12024", ResourceType.Sulfur, -34.65, -44.37),
        new("12025", ResourceType.Sulfur, -45.99, -35.64),
        new("12026", ResourceType.Quartz, 8.73, 18.45),
        new("12027", ResourceType.Quartz, 22.59, -18.81),
        new("12028", ResourceType.Quartz, 35.37, -22.77),
        new("12029", ResourceType.Quartz, 42.57, -37.53),
        new("12030", ResourceType.Quartz, 52.02, -12.15),
        new("12031", ResourceType.Coal, -19.44, 21.15),
        new("12032", ResourceType.Coal, -10.26, 18.18),
        new("12033", ResourceType.Coal, -6.03, 14.04),
        new("12034", ResourceType.Coal, -3.33, 17.64),
        new("12035", ResourceType.Coal, 2.25, 9.18),
        new("12036", ResourceType.Coal, 5.4, 9.09),
        new("12037", ResourceType.Coal, -1.89, 26.1),
        new("12038", ResourceType.Coal, 19.08, 24.03),
        new("12039", ResourceType.Coal, 21.69, 26.19),
        new("12040", ResourceType.Coal, 45, 28.62),
        new("12041", ResourceType.Coal, 38.88, 39.15),
        new("12042", ResourceType.Coal, 28.17, 55.89),
        new("12043", ResourceType.Coal, 14.04, 53.1),
        new("12044", ResourceType.Coal, 5.94, 46.35),
        new("12045", ResourceType.Coal, -10.62, -8.55),
        new("12046", ResourceType.Coal, -7.56, -11.25),
        new("12047", ResourceType.Coal, -8.19, -14.13),
        new("12048", ResourceType.Coal, -61.02, -41.76),
        new("12049", ResourceType.Coal, -65.07, -45.99),
        new("12050", ResourceType.Coal, -58.14, -51.39),
        new("12051", ResourceType.Coal, -65.25, -54.36),
        new("12052", ResourceType.Coal, -57.33, -63.27),
        new("12053", ResourceType.Coal, -62.46, -66.24),
        new("12054", ResourceType.Quartz, 34.65, -37.62),
        new("12055", ResourceType.Metal, 35.37, -37.62),
        new("12056", ResourceType.Coal, 50.58, 30.24),
        new("12057", ResourceType.Coal, 46.89, 39.06),
        new("12058", ResourceType.Metal, -22.77, -30.6),
        new("12059", ResourceType.Metal, -19.17, -22.95),
    ];
}
