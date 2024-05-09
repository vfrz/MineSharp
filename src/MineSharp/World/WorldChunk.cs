using System.Runtime.CompilerServices;
using MineSharp.Core;

namespace MineSharp.World;

public class WorldChunk
{
    public const int Width = 16;
    public const int Height = 128;

    public ChunkData Data { get; }

    public int ChunkX { get; }
    public int ChunkZ { get; }

    public WorldChunk(int chunkX, int chunkZ, ChunkData data)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
        Data = data;
    }

    public void UpdateBlock(Vector3i worldPosition, byte blockId, byte metadata = 0)
    {
        Data.SetBlock(WorldToLocal(worldPosition), blockId, metadata);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CoordinatesToIndex(int chunkX, int chunkY, int chunkZ)
    {
        return chunkY + chunkZ * Height + chunkX * Height * Width;
    }

    public static Vector2i GetChunkPositionForWorldPosition(Vector3d position)
    {
        var chunkX = (int) position.X / Width - (position.X < 0 ? 1 : 0);
        var chunkZ = (int) position.Z / Width - (position.Z < 0 ? 1 : 0);
        return new Vector2i(chunkX, chunkZ);
    }

    public Vector3i LocalToWorld(Vector3i position)
        => new(ChunkX * Width + position.X, position.Y, ChunkZ * Width + position.Z);

    public static Vector2i WorldToLocal(Vector2i position)
        => new((position.X % Width + Width) % Width, (position.Z % Width + Width) % Width);
    
    public static Vector3i WorldToLocal(Vector3i position)
        => new((position.X % Width + Width) % Width, position.Y, (position.Z % Width + Width) % Width);
}