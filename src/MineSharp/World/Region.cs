using System.Collections.Concurrent;
using MineSharp.Core;

namespace MineSharp.World;

public class Region
{
    public const int RegionWidth = 32;

    private readonly ConcurrentDictionary<Vector2i, Chunk> _chunks = new();
    public IEnumerable<Chunk> Chunks => _chunks.Values;

    public Vector2i RegionPosition { get; }

    public Region(Vector2i regionPosition)
    {
        RegionPosition = regionPosition;
    }
    
    public Chunk? this[Vector2i position]
    {
        get => _chunks.GetValueOrDefault(position);
        set
        {
            if (value is null)
                _chunks.Remove(position, out _);
            else
                _chunks.TryAdd(position, value);
        }
    }

    public static Vector2i GetRegionPositionForChunkPosition(Vector2i chunkPosition)
    {
        var regionX = chunkPosition.X / RegionWidth - (chunkPosition.X < 0 ? 1 : 0);
        var regionZ = chunkPosition.Z / RegionWidth - (chunkPosition.Z < 0 ? 1 : 0);
        return new Vector2i(regionX, regionZ);
    }
}