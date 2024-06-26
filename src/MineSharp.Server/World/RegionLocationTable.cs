using MineSharp.Numerics;

namespace MineSharp.World;

public class RegionLocationTable
{
    private readonly byte[] _data;
    public ReadOnlyMemory<byte> Data => _data;

    private int _allocatedChunks;

    public RegionLocationTable(byte[] data)
    {
        if (data.Length != Region.FileSectorSize)
            throw new Exception();
        _data = data;

        for (var x = 0; x < Region.RegionWidth; x++)
        {
            for (var z = 0; z < Region.RegionWidth; z++)
            {
                var chunkLocation = GetChunkLocation(new Vector2<int>(x, z));
                if (!chunkLocation.IsEmpty)
                    _allocatedChunks++;
            }
        }
    }

    public RegionLocationTable() : this(new byte[Region.FileSectorSize])
    {
    }

    public RegionLocation AllocateNewChunk(Vector2<int> chunkPosition)
    {
        if (!GetChunkLocation(chunkPosition).IsEmpty)
            throw new Exception();
        //TODO Fix this fixed size of 8 sectors
        var regionLocation = new RegionLocation(_allocatedChunks * 8 + 2, 8);
        _allocatedChunks++;
        SetChunkLocation(chunkPosition, regionLocation);
        return regionLocation;
    }

    public RegionLocation GetChunkLocation(Vector2<int> chunkPosition)
    {
        var index = GetTableIndexForChunkPosition(chunkPosition);
        var offset = (_data[index] << 16) | (_data[index + 1] << 8) | _data[index + 2];
        var size = _data[index + 3];
        return new RegionLocation(offset, size);
    }

    public void SetChunkLocation(Vector2<int> chunkPosition, RegionLocation regionLocation)
    {
        var index = GetTableIndexForChunkPosition(chunkPosition);
        _data[index] = (byte) ((regionLocation.Offset >> 16) & 0xFF);
        _data[index + 1] = (byte) ((regionLocation.Offset >> 8) & 0xFF);
        _data[index + 2] = (byte) (regionLocation.Offset & 0xFF);
        _data[index + 3] = regionLocation.Size;
    }

    private static int GetTableIndexForChunkPosition(Vector2<int> chunkPosition)
        => (Math.Abs(chunkPosition.X % 32) + Math.Abs(chunkPosition.Z % 32) * 32) * 4;
}