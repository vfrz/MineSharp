using System.Buffers.Binary;
using System.Collections.Concurrent;
using AsyncKeyedLock;
using MineSharp.Core;
using MineSharp.Extensions;
using MineSharp.Nbt;
using MineSharp.Numerics;
using MineSharp.Saves;

namespace MineSharp.World;

public class Region : IDisposable
{
    public const int RegionWidth = 32;
    public const int FileSectorSize = 4096;

    private readonly ConcurrentDictionary<Vector2<int>, Chunk> _chunks = new();
    public IEnumerable<Chunk> Chunks => _chunks.Values;

    private readonly AsyncKeyedLocker<Vector2<int>> _chunkLoadLocker = new();

    public Vector2<int> RegionPosition { get; }

    private MinecraftWorld World { get; }

    private readonly RegionLocationTable _regionLocationTable;
    private readonly RegionTimestampTable _regionTimestampTable;

    private readonly LockableFileStream _fileStream;

    private Region(Vector2<int> regionPosition,
        MinecraftWorld world,
        RegionLocationTable regionLocationTable,
        RegionTimestampTable regionTimestampTable,
        LockableFileStream fileStream)
    {
        RegionPosition = regionPosition;
        World = world;
        _regionLocationTable = regionLocationTable;
        _regionTimestampTable = regionTimestampTable;
        _fileStream = fileStream;
    }

    public static async Task<Region> LoadOrCreateAsync(Vector2<int> regionPosition, MinecraftWorld world)
    {
        if (SaveManager.IsRegionSaved(regionPosition))
        {
            var regionFileStream = new LockableFileStream(SaveManager.GetRegionFilePath(regionPosition), FileMode.Open, FileAccess.ReadWrite);

            var locationBytes = new byte[FileSectorSize];
            await regionFileStream.ReadExactlyAsync(locationBytes);
            var locationTable = new RegionLocationTable(locationBytes);

            var timestampBytes = new byte[FileSectorSize];
            await regionFileStream.ReadExactlyAsync(timestampBytes);
            var timestampTable = new RegionTimestampTable(timestampBytes);

            return new Region(regionPosition, world, locationTable, timestampTable, regionFileStream);
        }
        else
        {
            var regionFileStream = new LockableFileStream(SaveManager.GetRegionFilePath(regionPosition), FileMode.Create, FileAccess.ReadWrite);

            var locationTable = new RegionLocationTable();
            var timestampTable = new RegionTimestampTable();
            var region = new Region(regionPosition, world, locationTable, timestampTable, regionFileStream);
            await region.SaveAsync();
            return region;
        }
    }

    public async Task<Chunk> GetOrCreateChunkAsync(Vector2<int> chunkPosition)
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

                    chunk.CalculateLight();

                    _regionLocationTable.AllocateNewChunk(chunkPosition);

                    _chunks.TryAdd(chunkPosition, chunk);

                    await SaveChunkAsync(chunk);
                }
                else
                {
                    using (await _fileStream.EnterLockAsync())
                    {
                        _fileStream.Seek(location.Offset * FileSectorSize, SeekOrigin.Begin);

                        var lengthBytes = new byte[4];
                        _fileStream.ReadExactly(lengthBytes);
                        var length = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);

                        var nbtCompression = (NbtCompression) _fileStream.ReadByte();

                        var nbtBytes = new byte[length];

                        await _fileStream.ReadExactlyAsync(nbtBytes);

                        if (nbtCompression is NbtCompression.Gzip)
                        {
                            nbtBytes = nbtBytes.GZipDecompress();
                        }
                        else if (nbtCompression is NbtCompression.Zlib)
                        {
                            nbtBytes = nbtBytes.ZLibDecompress();
                        }

                        var chunkNbt = NbtSerializer.Deserialize(nbtBytes);
                        chunk = Chunk.CreateFromNbt(chunkNbt);
                        _chunks.TryAdd(chunkPosition, chunk);
                    }
                }
            }

            return chunk;
        }
    }

    private async Task SaveChunkAsync(Chunk chunk)
    {
        using (await _fileStream.EnterLockAsync())
        {
            var location = _regionLocationTable.GetChunkLocation(chunk.ChunkPosition);
            if (location.IsEmpty)
                throw new Exception();
            var nbt = chunk.ToNbt();
            var nbtBytes = NbtSerializer.Serialize(nbt).ZLibCompress();
            var seekOffset = location.Offset * FileSectorSize;
            _fileStream.Seek(seekOffset, SeekOrigin.Begin);
            var lengthBytes = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(lengthBytes, nbtBytes.Length);
            await _fileStream.WriteAsync(lengthBytes);
            _fileStream.WriteByte((byte) NbtCompression.Zlib);
            await _fileStream.WriteAsync(nbtBytes);
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        using (await _fileStream.EnterLockAsync())
        {
            _fileStream.Seek(0, SeekOrigin.Begin);
            await _fileStream.WriteAsync(_regionLocationTable.Data, cancellationToken);
            await _fileStream.WriteAsync(_regionTimestampTable.Data, cancellationToken);

            foreach (var chunk in Chunks)
            {
                var location = _regionLocationTable.GetChunkLocation(chunk.ChunkPosition);
                if (location.IsEmpty)
                    throw new Exception();
                var nbt = chunk.ToNbt();
                var nbtBytes = NbtSerializer.Serialize(nbt).ZLibCompress();
                var seekOffset = location.Offset * FileSectorSize;
                _fileStream.Seek(seekOffset, SeekOrigin.Begin);
                var lengthBytes = new byte[4];
                BinaryPrimitives.WriteInt32BigEndian(lengthBytes, nbtBytes.Length);
                await _fileStream.WriteAsync(lengthBytes, cancellationToken);
                _fileStream.WriteByte((byte) NbtCompression.Zlib);
                await _fileStream.WriteAsync(nbtBytes, cancellationToken);
            }
        }
    }

    public static Vector2<int> GetRegionPositionForChunkPosition(Vector2<int> chunkPosition)
    {
        var regionX = chunkPosition.X / RegionWidth;
        var regionZ = chunkPosition.Z / RegionWidth;
        if (chunkPosition.X < 0 && chunkPosition.X % RegionWidth != 0)
            regionX--;
        if (chunkPosition.Z < 0 && chunkPosition.Z % RegionWidth != 0)
            regionZ--;
        return new Vector2<int>(regionX, regionZ);
    }

    public void Dispose()
    {
        _fileStream.Dispose();
    }
}