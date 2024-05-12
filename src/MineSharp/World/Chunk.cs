using System.IO.Compression;
using System.Runtime.CompilerServices;
using MineSharp.Core;

namespace MineSharp.World;

public class Chunk : IBlockChunkData
{
    public const int ChunkWidth = 16;
    public const int ChunkHeight = 128;
    private const int ArraySize = ChunkWidth * ChunkWidth * ChunkHeight;

    private readonly byte[] _blocks;
    private readonly NibbleArray _metadata;
    private readonly NibbleArray _light;
    private readonly NibbleArray _skyLight;

    public Vector2i ChunkPosition { get; }

    public Chunk(Vector2i chunkPosition)
    {
        ChunkPosition = chunkPosition;
        _blocks = new byte[ArraySize + 3 * (ArraySize / 2)];
        _metadata = new NibbleArray(_blocks, ArraySize);
        _light = new NibbleArray(_blocks, ArraySize + ArraySize / 2);
        _skyLight = new NibbleArray(_blocks, ArraySize * 2);
    }

    public void SetBlock(Vector3i localPosition, byte blockId, byte metadata = 0)
    {
        //TODO Maybe should throw exception when world generation is reworked with multiple phases
        if (localPosition.X is < 0 or >= ChunkWidth
            || localPosition.Y is < 0 or >= ChunkHeight
            || localPosition.Z is < 0 or >= ChunkWidth)
            return;
        var index = LocalToIndex(localPosition);
        _blocks[index] = blockId;
        _metadata[index] = metadata;
    }

    public byte GetBlock(Vector3i localPosition, out byte metadata)
    {
        var index = LocalToIndex(localPosition);
        metadata = _metadata[index];
        return _blocks[index];
    }

    public void SetLight(Vector3i localPosition, byte light, byte skyLight)
    {
        var index = LocalToIndex(localPosition);
        _light[index] = light;
        _skyLight[index] = skyLight;
    }

    public int GetHighestBlockHeight(Vector2i localPosition)
    {
        for (var y = ChunkHeight - 1; y >= 0; y--)
        {
            if (GetBlock(new Vector3i(localPosition.X, y, localPosition.Z), out _) != 0)
                return y;
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocalToIndex(Vector3i localPosition)
    {
        return localPosition.Y + localPosition.Z * ChunkHeight + localPosition.X * ChunkHeight * ChunkWidth;
    }

    public Vector3i LocalToWorld(Vector3i position)
        => new(ChunkPosition.X * ChunkWidth + position.X, position.Y, ChunkPosition.Z * ChunkWidth + position.Z);

    public static Vector2i WorldToLocal(Vector2i position)
        => new((position.X % ChunkWidth + ChunkWidth) % ChunkWidth, (position.Z % ChunkWidth + ChunkWidth) % ChunkWidth);

    public static Vector3i WorldToLocal(Vector3i position)
        => new((position.X % ChunkWidth + ChunkWidth) % ChunkWidth, position.Y, (position.Z % ChunkWidth + ChunkWidth) % ChunkWidth);

    public static Vector2i GetChunkPositionForWorldPosition(Vector3d position)
    {
        var chunkX = (int) position.X / ChunkWidth - (position.X < 0 ? 1 : 0);
        var chunkZ = (int) position.Z / ChunkWidth - (position.Z < 0 ? 1 : 0);
        return new Vector2i(chunkX, chunkZ);
    }

    public static Vector2i GetChunkPositionForWorldPosition(Vector3i position)
    {
        var chunkX = position.X / ChunkWidth - (position.X < 0 ? 1 : 0);
        var chunkZ = position.Z / ChunkWidth - (position.Z < 0 ? 1 : 0);
        return new Vector2i(chunkX, chunkZ);
    }

    public static Vector2i GetChunkPositionForWorldPosition(Vector2i position)
    {
        var chunkX = position.X / ChunkWidth - (position.X < 0 ? 1 : 0);
        var chunkZ = position.Z / ChunkWidth - (position.Z < 0 ? 1 : 0);
        return new Vector2i(chunkX, chunkZ);
    }

    public async Task<byte[]> ToCompressedDataAsync()
    {
        await using var output = new MemoryStream();
        await using (var stream = new ZLibStream(output, CompressionMode.Compress))
        {
            await stream.WriteAsync(_blocks);
        }

        return output.ToArray();
    }
}