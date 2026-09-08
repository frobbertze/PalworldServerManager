using PalworldServerManager.Core.Map;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class PalworldMapCoordinateTransformTests
{
    [Fact]
    public void ToMapPercent_AtWorldOrigin_MatchesHandComputedValue()
    {
        // Hand-computed by walking the same steps as the ported code, to pin down exact
        // expected behavior and catch any future regression in the arithmetic.
        var (percentX, percentY) = PalworldMapCoordinateTransform.ToMapPercent(0, 0);

        Assert.Equal(32.72, percentX, precision: 1);
        Assert.Equal(36.61, percentY, precision: 1);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100000, 100000)]
    [InlineData(-100000, -100000)]
    [InlineData(300000, -300000)]
    [InlineData(-300000, 300000)]
    public void ToMapPercent_TypicalWorldCoordinates_StaysWithinPlausibleRange(double x, double y)
    {
        // Not a correctness guarantee (that needs live in-game calibration), just a sanity
        // check that the math doesn't blow up or produce wildly out-of-range values for
        // coordinates within Palworld's actual explorable world bounds.
        var (percentX, percentY) = PalworldMapCoordinateTransform.ToMapPercent(x, y);

        Assert.InRange(percentX, -50, 150);
        Assert.InRange(percentY, -50, 150);
    }

    [Fact]
    public void ToMapPercent_DifferentInputs_ProduceDifferentPositions()
    {
        var a = PalworldMapCoordinateTransform.ToMapPercent(0, 0);
        var b = PalworldMapCoordinateTransform.ToMapPercent(50000, -20000);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToMapPercentFromPin_AtOrigin_MatchesHandComputedValue()
    {
        var (percentX, percentY) = PalworldMapCoordinateTransform.ToMapPercentFromPin(0, 0);

        Assert.Equal(49.9499, percentX, precision: 3);
        Assert.Equal(50.0500, percentY, precision: 3);
    }

    [Theory]
    [InlineData(-36.45, 6.39)]  // a real metal node from ResourceNodeData
    [InlineData(52.02, -12.15)] // a real quartz node from ResourceNodeData
    public void ToMapPercentFromPin_RealResourceNodeCoordinates_StayWithinPlausibleRange(double x, double y)
    {
        var (percentX, percentY) = PalworldMapCoordinateTransform.ToMapPercentFromPin(x, y);

        Assert.InRange(percentX, 0, 100);
        Assert.InRange(percentY, 0, 100);
    }
}
