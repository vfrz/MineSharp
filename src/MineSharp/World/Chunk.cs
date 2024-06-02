using System.Collections.Concurrent;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using MineSharp.Content;
using MineSharp.Core;
using MineSharp.Nbt;
using MineSharp.Nbt.Tags;
using MineSharp.TileEntities;

namespace MineSharp.World;

public class Chunk : IBlockChunkData
{
    public const int ChunkWidth = 16;
    public const int ChunkHeight = 128;
    private const int ArraySize = ChunkWidth * ChunkWidth * ChunkHeight;

    public ReadOnlySpan<byte> Data => _data;

    private readonly byte[] _data;
    private readonly ArraySegment<byte> _blocks;
    private readonly NibbleArray _metadata;
    private readonly NibbleArray _light;
    private readonly NibbleArray _skyLight;

    public Vector2i ChunkPosition { get; }

    public Vector2i ChunkLocalPosition => new(ChunkPosition.X % Region.RegionWidth, ChunkPosition.Z % Region.RegionWidth);

    private readonly ConcurrentDictionary<Vector3i, TileEntity> _tileEntities;
    public ICollection<TileEntity> TileEntities => _tileEntities.Values;

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

    public static Chunk CreateFromNbt(INbtTag tag)
    {
        var level = (CompoundNbtTag) tag;

        var chunkX = level.Get<IntNbtTag>("xPos").Value;
        var chunkZ = level.Get<IntNbtTag>("zPos").Value;

        var chunk = new Chunk(new Vector2i(chunkX, chunkZ));

        var blocks = level.Get<ByteArrayNbtTag>("Blocks").Value;
        var metadata = level.Get<ByteArrayNbtTag>("Data").Value;
        var light = level.Get<ByteArrayNbtTag>("BlockLight").Value;
        var skyLight = level.Get<ByteArrayNbtTag>("SkyLight").Value;

        Array.Copy(blocks, 0, chunk._data, chunk._blocks.Offset, blocks.Length);
        Array.Copy(metadata, 0, chunk._data, chunk._metadata.Offset, metadata.Length);
        Array.Copy(light, 0, chunk._data, chunk._light.Offset, light.Length);
        Array.Copy(skyLight, 0, chunk._data, chunk._skyLight.Offset, skyLight.Length);

        var tileEntityTags = level.Get<ListNbtTag>("TileEntities").Tags;
        foreach (var tileEntityTag in tileEntityTags.Cast<CompoundNbtTag>())
        {
            var tileEntityId = tileEntityTag.Get<StringNbtTag>("id").Value;

            //TODO Handle other TileEntity types
            var tileEntity = tileEntityId switch
            {
                SignTileEntity.Id => new SignTileEntity(),
                _ => throw new Exception($"Unknown TileEntityId: {tileEntityId}")
            };

            tileEntity.LoadFromNbt(tileEntityTag);
            chunk._tileEntities[tileEntity.LocalPosition] = tileEntity;
        }

        return chunk;
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

    public void SetTileEntity(Vector3i localPosition, TileEntity? tileEntity)
    {
        if (tileEntity is null)
        {
            _tileEntities.Remove(localPosition, out _);
        }
        else
        {
            _tileEntities[localPosition] = tileEntity;
        }
    }

    public TileEntity? GetTileEntity(Vector3i localPosition)
    {
        return _tileEntities.GetValueOrDefault(localPosition);
    }

    public T GetTileEntity<T>(Vector3i localPosition) where T : TileEntity
    {
        if (_tileEntities.TryGetValue(localPosition, out var tileEntity))
            return (T) tileEntity;
        throw new KeyNotFoundException();
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

    public static Vector2i GetChunkPositionForWorldPosition(Vector3d position) => GetChunkPositionForWorldPosition(position.ToVector3i());

    public static Vector2i GetChunkPositionForWorldPosition(Vector3i position)
    {
        var chunkX = position.X / ChunkWidth;
        var chunkZ = position.Z / ChunkWidth;
        if (position.X < 0 && position.X % ChunkWidth != 0)
            chunkX--;
        if (position.Z < 0 && position.Z % ChunkWidth != 0)
            chunkZ--;
        return new Vector2i(chunkX, chunkZ);
    }

    public static Vector2i GetChunkPositionForWorldPosition(Vector2i position)
    {
        var chunkX = position.X / ChunkWidth;
        var chunkZ = position.Z / ChunkWidth;
        if (position.X < 0 && position.X % ChunkWidth != 0)
            chunkX--;
        if (position.Z < 0 && position.Z % ChunkWidth != 0)
            chunkZ--;
        return new Vector2i(chunkX, chunkZ);
    }

    public INbtTag ToNbt()
    {
        var tileEntities = _tileEntities.Values
            .Select(tileEntity => tileEntity.ToNbt())
            .ToList();

        //TODO Add missing tags
        var nbt = new CompoundNbtTag("Level")
            .AddTag(new ByteArrayNbtTag("Blocks", _blocks))
            .AddTag(new ByteArrayNbtTag("Data", _metadata))
            .AddTag(new ByteArrayNbtTag("SkyLight", _skyLight))
            .AddTag(new ByteArrayNbtTag("BlockLight", _light))
            .AddTag(new ListNbtTag("TileEntities", TagType.Compound, tileEntities))
            .AddTag(new IntNbtTag("xPos", ChunkPosition.X))
            .AddTag(new IntNbtTag("zPos", ChunkPosition.Z));
        return nbt;
    }

    public async Task<byte[]> ToCompressedDataAsync()
    {
        //TODO Keep an updated compressed version to avoid compress it everytime
        await using var output = new MemoryStream();
        await using (var stream = new ZLibStream(output, CompressionMode.Compress))
        {
            await stream.WriteAsync(_data);
        }

        return output.ToArray();
    }

    public static HashSet<Vector2i> GetChunksAround(Vector2i originChunk, int radius)
    {
        // Circle
        var chunks = new HashSet<Vector2i>
        {
            originChunk
        };

        for (var x = originChunk.X - radius; x <= originChunk.X + radius; x++)
        {
            for (var z = originChunk.Z - radius; z <= originChunk.Z + radius; z++)
            {
                var distance = originChunk.DistanceTo(new Vector2i(x, z));
                if (distance <= radius)
                {
                    chunks.Add(new Vector2i(x, z));
                }
            }
        }

        return chunks.OrderBy(originChunk.DistanceTo).ToHashSet();

        // Diamond
        /*
        var chunks = new HashSet<Vector2i>
        {
            originChunk
        };

        // Front
        for (var z = 1; z < radius; z++)
        {
            chunks.Add(new Vector2i(originChunk.X, originChunk.Z + z));
            for (var x = 1; x < radius - z; x++)
            {
                chunks.Add(new Vector2i(originChunk.X - x, originChunk.Z + z));
            }
        }

        // Right
        for (var x = 1; x < radius; x++)
        {
            chunks.Add(new Vector2i(originChunk.X - x, originChunk.Z));
            for (var z = 1; z < radius - x; z++)
            {
                chunks.Add(new Vector2i(originChunk.X - x, originChunk.Z - z));
            }
        }

        // Back
        for (var z = 1; z < radius; z++)
        {
            chunks.Add(new Vector2i(originChunk.X, originChunk.Z - z));
            for (var x = 1; x < radius - z; x++)
            {
                chunks.Add(new Vector2i(originChunk.X + x, originChunk.Z - z));
            }
        }

        // Left
        for (var x = 1; x < radius; x++)
        {
            chunks.Add(new Vector2i(originChunk.X + x, originChunk.Z));
            for (var z = 1; z < radius - x; z++)
            {
                chunks.Add(new Vector2i(originChunk.X + x, originChunk.Z + z));
            }
        }

        return chunks;*/
    }
}