using PalworldServerManager.Core.Map;
using Xunit;

namespace PalworldServerManager.Core.Tests;

public class ResourceNodeDataTests
{
    [Fact]
    public void All_ContainsExpectedTotalCount()
    {
        Assert.Equal(58, ResourceNodeData.All.Count);
    }

    [Theory]
    [InlineData(ResourceType.Coal, 25)]
    [InlineData(ResourceType.Metal, 20)]
    [InlineData(ResourceType.Sulfur, 7)]
    [InlineData(ResourceType.Quartz, 6)]
    public void All_HasExpectedCountPerType(ResourceType type, int expectedCount)
    {
        var count = ResourceNodeData.All.Count(n => n.Type == type);
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void All_HasNoDuplicateIds()
    {
        var ids = ResourceNodeData.All.Select(n => n.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
