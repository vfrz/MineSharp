using System.Buffers.Binary;
using System.Collections.Concurrent;
using AsyncKeyedLock;
using MineSharp.Core;
using MineSharp.Nbt;
using MineSharp.Saves;

namespace MineSharp.World;

public class Region
{
    public const int RegionWidth = 32;

    private readonly ConcurrentDictionary<Vector2i, Chunk> _chunks = new();
    public IEnumerable<Chunk> Chunks => _chunks.Values;

    private readonly AsyncKeyedLocker<Vector2i> _chunkLoadLocker = new();

    public Vector2i RegionPosition { get; }

    private MinecraftWorld World { get; }

    private readonly RegionLocationTable _regionLocationTable;
    private readonly RegionTimestampTable _regionTimestampTable;

    private Region(Vector2i regionPosition,
        MinecraftWorld world,
        RegionLocationTable regionLocationTable,
        RegionTimestampTable regionTimestampTable)
    {
        RegionPosition = regionPosition;
        World = world;
        _regionLocationTable = regionLocationTable;
        _regionTimestampTable = regionTimestampTable;
    }

    public static async Task<Region> LoadOrCreateAsync(Vector2i regionPosition, MinecraftWorld world)
    {
        if (SaveManager.IsRegionSaved(regionPosition))
        {
            await using var regionFileStream = File.OpenRead(SaveManager.GetRegionFilePath(regionPosition));

            var locationBytes = new byte[RegionLocationTable.Size];
            await regionFileStream.ReadExactlyAsync(locationBytes);
            var locationTable = new RegionLocationTable(locationBytes);

            var timestampBytes = new byte[RegionTimestampTable.Size];
            await regionFileStream.ReadExactlyAsync(timestampBytes);
            var timestampTable = new RegionTimestampTable(timestampBytes);

            return new Region(regionPosition, world, locationTable, timestampTable);
        }
        else
        {
            var locationTable = new RegionLocationTable();
            var timestampTable = new RegionTimestampTable();
            var region = new Region(regionPosition, world, locationTable, timestampTable);
            await region.SaveAsync();
            return region;
        }
    }

    public async Task<Chunk> GetOrCreateChunkAsync(Vector2i chunkPosition)
    {
        using (await _chunkLoadLocker.LockAsync(chunkPosition))
        {
            var chunk = _chunks.GetValueOrDefault(chunkPosition);

            if (chunk is null)
            {
                var location = _regionLocationTable.GetChunkLocation(chunkPosition);

                if (location.IsEmpty)
                {
                    chunk = new Chunk(chunkPosition);

                    World.WorldGenerator.GenerateChunkTerrain(chunkPosition, chunk);
                    World.WorldGenerator.GenerateChunkDecorations(chunkPosition, chunk);

                    //TODO Move light calculation somewhere else
                    for (var x = 0; x < Chunk.ChunkWidth; x++)
                    for (var y = 0; y < Chunk.ChunkHeight; y++)
                    for (var z = 0; z < Chunk.ChunkWidth; z++)
                    {
                        chunk.SetLight(new Vector3i(x, y, z), 15, 15);
                    }

                    _regionLocationTable.AllocateNewChunk(chunkPosition);

                    _chunks.TryAdd(chunkPosition, chunk);

                    //TODO Avoid saving them all again and again
                    await SaveAsync();
                }
                else
                {
                    await using var regionFileStream = File.OpenRead(SaveManager.GetRegionFilePath(RegionPosition));
                    regionFileStream.Seek(location.Offset * 4096, SeekOrigin.Begin);

                    var lengthBytes = new byte[4];
                    regionFileStream.ReadExactly(lengthBytes);
                    var length = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);
                    //TODO Implement compression
                    var compressionTypeByte = (byte) regionFileStream.ReadByte();

                    var nbtBytes = new byte[length];
                    await regionFileStream.ReadExactlyAsync(nbtBytes);
                    var chunkNbt = NbtSerializer.Deserialize(nbtBytes);

                    chunk = Chunk.CreateFromNbt(chunkNbt);

                    _chunks.TryAdd(chunkPosition, chunk);
                }
            }

            return chunk;
        }
    }

    public async Task SaveAsync()
    {
        await using var regionFileStream = File.OpenWrite(SaveManager.GetRegionFilePath(RegionPosition));
        await regionFileStream.WriteAsync(_regionLocationTable.Data);
        await regionFileStream.WriteAsync(_regionTimestampTable.Data);

        foreach (var chunk in Chunks)
        {
            var location = _regionLocationTable.GetChunkLocation(chunk.ChunkPosition);
            if (location.IsEmpty)
                throw new Exception();
            var nbt = chunk.ToNbt();
            var nbtBytes = NbtSerializer.Serialize(nbt);

            var seekOffset = location.Offset * 4096;
            var result = regionFileStream.Seek(seekOffset, SeekOrigin.Begin);
            var lengthBytes = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(lengthBytes, nbtBytes.Length);
            await regionFileStream.WriteAsync(lengthBytes);
            //TODO Implement compression
            regionFileStream.WriteByte(0);
            await regionFileStream.WriteAsync(nbtBytes);
        }
    }

    public static Vector2i GetRegionPositionForWorldPosition(Vector2i worldPosition)
    {
        var regionX = worldPosition.X / (RegionWidth * Chunk.ChunkWidth) - (worldPosition.X < 0 ? 1 : 0);
        var regionZ = worldPosition.Z / (RegionWidth * Chunk.ChunkWidth) - (worldPosition.Z < 0 ? 1 : 0);
        return new Vector2i(regionX, regionZ);
    }

    public static Vector2i GetRegionPositionForChunkPosition(Vector2i chunkPosition)
    {
        var regionX = chunkPosition.X / RegionWidth - (chunkPosition.X < 0 ? 1 : 0);
        var regionZ = chunkPosition.Z / RegionWidth - (chunkPosition.Z < 0 ? 1 : 0);
        return new Vector2i(regionX, regionZ);
    }
}