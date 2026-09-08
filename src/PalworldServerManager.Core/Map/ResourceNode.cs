namespace PalworldServerManager.Core.Map;

public enum ResourceType
{
    Coal,
    Metal,
    Quartz,
    Sulfur,
}

/// <summary>A fixed, static resource node location on the map (loc values are the map-authoring
/// tool's own pre-scaled units — see PalworldMapCoordinateTransform.ToMapPercentFromPin).</summary>
public record ResourceNode(string Id, ResourceType Type, double LocX, double LocY);
