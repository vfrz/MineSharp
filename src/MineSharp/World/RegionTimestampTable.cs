using MineSharp.Core;
using MineSharp.Sdk.Core;

namespace MineSharp.World;

public class RegionTimestampTable
{
    private readonly byte[] _data;

    public ReadOnlyMemory<byte> Data => _data;

    public RegionTimestampTable(byte[] data)
    {
        if (data.Length != Region.FileSectorSize)
            throw new Exception();
        _data = data;
    }

    public RegionTimestampTable() : this(new byte[Region.FileSectorSize])
    {
    }

    public int GetChunkTimestamp(Vector2<int> chunkPosition)
    {
        var index = GetTableIndexForChunkPosition(chunkPosition);
        var timestamp = (_data[index] << 24) | (_data[index + 1] << 16) | (_data[index + 2] << 8) | _data[index + 3];
        return timestamp;
    }

    public void SetChunkTimestamp(Vector2<int> chunkPosition, int timestamp)
    {
        var index = GetTableIndexForChunkPosition(chunkPosition);
        _data[index] = (byte) ((timestamp >> 24) & 0xFF);
        _data[index + 1] = (byte) ((timestamp >> 16) & 0xFF);
        _data[index + 2] = (byte) ((timestamp >> 8) & 0xFF);
        _data[index + 3] = (byte) (timestamp & 0xFF);
    }

    private static int GetTableIndexForChunkPosition(Vector2<int> chunkPosition)
        => (chunkPosition.X % 32 + chunkPosition.Z % 32 * 32) * 4;
}