using FluentAssertions;
using MineSharp.Core;
using MineSharp.World;

namespace MineSharp.Tests.World;

[TestClass]
public class RegionTimestampTableTests
{
    [TestMethod]
    public void RegionTimestampTable_SetAndGet()
    {
        var data = new byte[Region.FileSectorSize];
        var table = new RegionTimestampTable(data);

        var chunkPosition = new Vector2<int>(6, 9);
        table.SetChunkTimestamp(chunkPosition, 42694269);

        var chunkTimestamp = table.GetChunkTimestamp(chunkPosition);
        chunkTimestamp.Should().Be(42694269);
    }
}