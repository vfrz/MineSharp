using FluentAssertions;
using MineSharp.Core;
using MineSharp.World;

namespace MineSharp.Tests.World;

[TestClass]
public class RegionLocationTableTests
{
    [TestMethod]
    public void RegionLocationTable_SetAndGet()
    {
        var data = new byte[4096];
        var table = new RegionLocationTable(data);

        var chunkPosition = new Vector2i(6, 9);
        table.SetChunkLocation(chunkPosition, new RegionLocation(4269, 1));

        var regionLocation = table.GetChunkLocation(chunkPosition);
        regionLocation.Offset.Should().Be(4269);
        regionLocation.Size.Should().Be(1);
    }
}