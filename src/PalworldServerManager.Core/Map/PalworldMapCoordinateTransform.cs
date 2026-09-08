namespace PalworldServerManager.Core.Map;

/// <summary>
/// Converts a player's raw Unreal Engine world coordinates (as returned by the REST API's
/// location_x/location_y) into a percentage position (0-100, 0-100) on the bundled map image.
///
/// This is a direct port of the coordinate math from the fa0311/palworld-map project's Leaflet
/// map (src/app/leaflet.tsx), which is what actually produced wwwroot/images/palworld-map.jpg —
/// so these constants are calibrated specifically to that image.
///
/// If the map image is ever replaced with a different one, this is the one place that needs
/// recalibrating: check a real player's in-game position against where their dot lands here,
/// and adjust the constants below until they line up. Nothing else (the page, the CSS overlay)
/// needs to change, since dots are positioned by percentage, not pixels.
/// </summary>
public static class PalworldMapCoordinateTransform
{
    private const double OriginX = 122500;
    private const double OriginY = -158100;
    private const double GameRatio = 458.355;
    private const double MapRatio = 7.8;
    private const double MapSize = 256; // the Leaflet CRS.Simple map-unit span the source project used

    public static (double PercentX, double PercentY) ToMapPercent(double locationX, double locationY)
    {
        var (gameX, gameY) = ToGamePosition(locationX + OriginX, locationY + OriginY);
        return MarkerToPercent(ToMarkerPosition(gameX, gameY));
    }

    /// <summary>
    /// Converts a static point-of-interest coordinate (as stored in ResourceNodeData — units
    /// from the map-authoring tool that produced pin_data.json) into a map percentage position.
    /// This is a different input space than player world coordinates, so it skips ToGamePosition
    /// and applies that tool's own pin-scale factor directly, per its source (fromPinPossition
    /// in src/app/leaflet.tsx).
    /// </summary>
    public static (double PercentX, double PercentY) ToMapPercentFromPin(double pinLocX, double pinLocY)
    {
        const double PinScale = 11;
        return MarkerToPercent(ToMarkerPosition(pinLocX * PinScale, pinLocY * PinScale));
    }

    private static (double PercentX, double PercentY) MarkerToPercent((double Lat, double Lng) marker) =>
        (marker.Lng / MapSize * 100.0, -marker.Lat / MapSize * 100.0);

    private static (double X, double Y) ToGamePosition(double x, double y)
    {
        var signX = x > 0 ? 0 : 1;
        var signY = y > 0 ? 0 : 1;
        return (x / GameRatio + signX, y / GameRatio + signY);
    }

    private static (double Lat, double Lng) ToMarkerPosition(double gameX, double gameY)
    {
        var signX = gameX > 0 ? 0 : 1;
        var signY = gameY > 0 ? 0 : 1;
        var makerX = (gameX - signX) / MapRatio - MapSize / 2;
        var makerY = (gameY - signY) / MapRatio + MapSize / 2;
        return (makerX, makerY);
    }
}
