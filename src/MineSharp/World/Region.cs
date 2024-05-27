using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.IO.Compression;
using AsyncKeyedLock;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Nbt;
using MineSharp.Saves;

namespace MineSharp.World;

public class Region : IDisposable
{
    public const int RegionWidth = 32;

    private readonly ConcurrentDictionary<Vector2i, Chunk> _chunks = new();
    public IEnumerable<Chunk> Chunks => _chunks.Values;

    private readonly AsyncKeyedLocker<Vector2i> _chunkLoadLocker = new();
    private readonly SemaphoreSlim _fileLocker = new(1, 1);

    public Vector2i RegionPosition { get; }

    private MinecraftWorld World { get; }

    private readonly RegionLocationTable _regionLocationTable;
    private readonly RegionTimestampTable _regionTimestampTable;

    private readonly FileStream _fileStream;

    private Region(Vector2i regionPosition,
        MinecraftWorld world,
        RegionLocationTable regionLocationTable,
        RegionTimestampTable regionTimestampTable,
        FileStream fileStream)
    {
        RegionPosition = regionPosition;
        World = world;
        _regionLocationTable = regionLocationTable;
        _regionTimestampTable = regionTimestampTable;
        _fileStream = fileStream;
    }

    public static async Task<Region> LoadOrCreateAsync(Vector2i regionPosition, MinecraftWorld world)
    {
        if (SaveManager.IsRegionSaved(regionPosition))
        {
            var regionFileStream = File.Open(SaveManager.GetRegionFilePath(regionPosition), FileMode.Open, FileAccess.ReadWrite);

            var locationBytes = new byte[RegionLocationTable.Size];
            await regionFileStream.ReadExactlyAsync(locationBytes);
            var locationTable = new RegionLocationTable(locationBytes);

            var timestampBytes = new byte[RegionTimestampTable.Size];
            await regionFileStream.ReadExactlyAsync(timestampBytes);
            var timestampTable = new RegionTimestampTable(timestampBytes);

            return new Region(regionPosition, world, locationTable, timestampTable, regionFileStream);
        }
        else
        {
            var regionFileStream = File.Open(SaveManager.GetRegionFilePath(regionPosition), FileMode.Create, FileAccess.ReadWrite);

            var locationTable = new RegionLocationTable();
            var timestampTable = new RegionTimestampTable();
            var region = new Region(regionPosition, world, locationTable, timestampTable, regionFileStream);
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

                    await SaveChunkAsync(chunk);
                }
                else
                {
                    try
                    {
                        await _fileLocker.WaitAsync();
                        _fileStream.Seek(location.Offset * 4096, SeekOrigin.Begin);

                        var lengthBytes = new byte[4];
                        _fileStream.ReadExactly(lengthBytes);
                        var length = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);

                        var compressionTypeByte = (byte) _fileStream.ReadByte();

                        var nbtBytes = new byte[length];

                        await _fileStream.ReadExactlyAsync(nbtBytes);

                        if (compressionTypeByte == 1)
                        {
                            nbtBytes = nbtBytes.GZipDecompress();
                        }
                        else if (compressionTypeByte == 2)
                        {
                            nbtBytes = nbtBytes.ZLibDecompress();
                        }

                        var chunkNbt = NbtSerializer.Deserialize(nbtBytes);
                        chunk = Chunk.CreateFromNbt(chunkNbt);
                        _chunks.TryAdd(chunkPosition, chunk);
                    }
                    finally
                    {
                        _fileLocker.Release();
                    }
                }
            }

            return chunk;
        }
    }

    private async Task SaveChunkAsync(Chunk chunk)
    {
        try
        {
            await _fileLocker.WaitAsync();

            var location = _regionLocationTable.GetChunkLocation(chunk.ChunkPosition);
            if (location.IsEmpty)
                throw new Exception();
            var nbt = chunk.ToNbt();
            var nbtBytes = NbtSerializer.Serialize(nbt).ZLibCompress();
            var seekOffset = location.Offset * 4096;
            _fileStream.Seek(seekOffset, SeekOrigin.Begin);
            var lengthBytes = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(lengthBytes, nbtBytes.Length);
            await _fileStream.WriteAsync(lengthBytes);
            //TODO Implement compression
            _fileStream.WriteByte(2);
            await _fileStream.WriteAsync(nbtBytes);
        }
        finally
        {
            _fileLocker.Release();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            await _fileLocker.WaitAsync();
            _fileStream.Seek(0, SeekOrigin.Begin);
            await _fileStream.WriteAsync(_regionLocationTable.Data);
            await _fileStream.WriteAsync(_regionTimestampTable.Data);

            foreach (var chunk in Chunks)
            {
                var location = _regionLocationTable.GetChunkLocation(chunk.ChunkPosition);
                if (location.IsEmpty)
                    throw new Exception();
                var nbt = chunk.ToNbt();
                var nbtBytes = NbtSerializer.Serialize(nbt).ZLibCompress();
                var seekOffset = location.Offset * 4096;
                _fileStream.Seek(seekOffset, SeekOrigin.Begin);
                var lengthBytes = new byte[4];
                BinaryPrimitives.WriteInt32BigEndian(lengthBytes, nbtBytes.Length);
                await _fileStream.WriteAsync(lengthBytes);
                //TODO Implement compression
                _fileStream.WriteByte(2);
                await _fileStream.WriteAsync(nbtBytes);
            }
        }
        finally
        {
            _fileLocker.Release();
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

    public void Dispose()
    {
        _fileStream.Dispose();
    }
}