using System.Collections.Concurrent;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using MineSharp.Content;
using MineSharp.Core;
using MineSharp.Nbt.Tags;
using MineSharp.Saves;
using MineSharp.TileEntities;

namespace MineSharp.World;

public class Chunk : IBlockChunkData
{
    public const int ChunkWidth = 16;
    public const int ChunkHeight = 128;
    private const int ArraySize = ChunkWidth * ChunkWidth * ChunkHeight;

    private const int SaveRegionSize = 32;

    public ReadOnlySpan<byte> Data => _data;

    private readonly byte[] _data;
    private readonly ArraySegment<byte> _blocks;
    private readonly NibbleArray _metadata;
    private readonly NibbleArray _light;
    private readonly NibbleArray _skyLight;

    public Vector2i ChunkPosition { get; }

    private readonly ConcurrentDictionary<Vector3i, TileEntity> _tileEntities;

    public Chunk(Vector2i chunkPosition)
    {
        ChunkPosition = chunkPosition;
        _data = new byte[ArraySize + 3 * (ArraySize / 2)];
        _blocks = new ArraySegment<byte>(_data, 0, ArraySize);
        _metadata = new NibbleArray(_data, ArraySize, ArraySize / 2);
        _light = new NibbleArray(_data, ArraySize + ArraySize / 2, ArraySize / 2);
        _skyLight = new NibbleArray(_data, ArraySize * 2, ArraySize / 2);

        _tileEntities = new ConcurrentDictionary<Vector3i, TileEntity>();
    }

    public void LoadFromSaveData(ChunkSaveData saveData)
    {
        Array.Copy(saveData.Data, _data, saveData.Data.Length);
    }

    public ChunkSaveData GetSaveData()
    {
        return new ChunkSaveData
        {
            Data = _data.ToArray()
        };
    }

    public void SetBlock(Vector3i localPosition, BlockId blockId, byte metadata = 0)
    {
        //TODO Maybe should throw exception when world generation is reworked with multiple phases
        if (localPosition.X is < 0 or >= ChunkWidth
            || localPosition.Y is < 0 or >= ChunkHeight
            || localPosition.Z is < 0 or >= ChunkWidth)
            return;
        var index = LocalToIndex(localPosition);
        _blocks[index] = (byte) blockId;
        _metadata[index] = metadata;
    }

    public BlockId GetBlockId(Vector3i localPosition)
    {
        var index = LocalToIndex(localPosition);
        return (BlockId) _blocks[index];
    }

    public Block GetBlock(Vector3i localPosition)
    {
        var index = LocalToIndex(localPosition);
        return new Block((BlockId) _blocks[index], _metadata[index], _light[index], _skyLight[index]);
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
            if (GetBlock(new Vector3i(localPosition.X, y, localPosition.Z)).BlockId != BlockId.Air)
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
        => new((position.X % ChunkWidth + ChunkWidth) % ChunkWidth,
            (position.Z % ChunkWidth + ChunkWidth) % ChunkWidth);

    public static Vector3i WorldToLocal(Vector3i position)
        => new((position.X % ChunkWidth + ChunkWidth) % ChunkWidth, position.Y,
            (position.Z % ChunkWidth + ChunkWidth) % ChunkWidth);

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

    public static int GetSaveFileIndex(Vector2i chunkPosition)
    {
        var regionX = chunkPosition.X / SaveRegionSize - (chunkPosition.X < 0 ? 1 : 0);
        var regionZ = chunkPosition.Z / SaveRegionSize - (chunkPosition.Z < 0 ? 1 : 0);
        return regionX + regionZ * SaveRegionSize;
    }

    public INbtTag ToNbt()
    {
        //TODO Check if we can avoid .ToArray() on ArraySegment
        //TODO Add missing tags
        var nbt = new CompoundNbtTag("Level")
            .AddTag(new ByteArrayNbtTag("Blocks ", _blocks.ToArray()))
            .AddTag(new ByteArrayNbtTag("Data", _metadata.ToArray()))
            .AddTag(new ByteArrayNbtTag("SkyLight", _skyLight.ToArray()))
            .AddTag(new ByteArrayNbtTag("BlockLight", _light.ToArray()))
            .AddTag(new IntNbtTag("xPos", ChunkPosition.X))
            .AddTag(new IntNbtTag("zPos", ChunkPosition.Z));
        
        return nbt;
    }
    
    public async Task<byte[]> ToCompressedDataAsync()
    {
        await using var output = new MemoryStream();
        await using (var stream = new ZLibStream(output, CompressionMode.Compress))
        {
            await stream.WriteAsync(_data);
        }

        return output.ToArray();
    }
}