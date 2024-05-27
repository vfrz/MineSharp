using MineSharp.Core;

namespace MineSharp.World;

public class RegionLocationTable
{
    public const int Size = Region.RegionWidth * Region.RegionWidth * 4;

    private readonly byte[] _data;
    public ReadOnlyMemory<byte> Data => _data;

    private int _allocatedChunks;

    public RegionLocationTable(byte[] data)
    {
        if (data.Length != Size)
            throw new Exception();
        _data = data;

        for (var x = 0; x < Region.RegionWidth; x++)
        {
            for (var z = 0; z < Region.RegionWidth; z++)
            {
                var chunkLocation = GetChunkLocation(new Vector2i(x, z));
                if (!chunkLocation.IsEmpty)
                    _allocatedChunks++;
            }
        }
    }

    public RegionLocationTable() : this(new byte[Size])
    {
    }

    public RegionLocation AllocateNewChunk(Vector2i chunkPosition)
    {
        if (!GetChunkLocation(chunkPosition).IsEmpty)
            throw new Exception();
        //TODO Fix this fixed size of 8 sectors
        var regionLocation = new RegionLocation(_allocatedChunks * 8 + 2, 8);
        _allocatedChunks++;
        SetChunkLocation(chunkPosition, regionLocation);
        return regionLocation;
    }

    public RegionLocation GetChunkLocation(Vector2i chunkPosition)
    {
        var index = GetTableIndexForChunkPosition(chunkPosition);
        var offset = (_data[index] << 16) | (_data[index + 1] << 8) | _data[index + 2];
        var size = _data[index + 3];
        return new RegionLocation(offset, size);
    }

    public void SetChunkLocation(Vector2i chunkPosition, RegionLocation regionLocation)
    {
        var index = GetTableIndexForChunkPosition(chunkPosition);
        _data[index] = (byte) ((regionLocation.Offset >> 16) & 0xFF);
        _data[index + 1] = (byte) ((regionLocation.Offset >> 8) & 0xFF);
        _data[index + 2] = (byte) (regionLocation.Offset & 0xFF);
        _data[index + 3] = regionLocation.Size;
    }

    private static int GetTableIndexForChunkPosition(Vector2i chunkPosition)
        => (Math.Abs(chunkPosition.X % 32) + Math.Abs(chunkPosition.Z % 32) * 32) * 4;
}