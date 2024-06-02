using System.Collections.Concurrent;
using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using MineSharp.Content;
using MineSharp.Core;
using MineSharp.Network.Packets;
using MineSharp.Saves;
using MineSharp.TileEntities;
using MineSharp.World.Generation;

namespace MineSharp.World;

public class MinecraftWorld : IDisposable
{
    public int Seed { get; }
    public bool Raining { get; private set; }

    private WorldTimer Timer { get; }
    public long Time => Timer.CurrentTime;

    public MinecraftServer Server { get; }
    public IWorldGenerator WorldGenerator { get; }

    private readonly ConcurrentDictionary<Vector2i, Region> _regions = new();
    private readonly AsyncKeyedLocker<Vector2i> _regionLoadLocker = new();

    private MinecraftWorld(MinecraftServer server, int seed)
    {
        Server = server;
        Seed = seed;
        WorldGenerator = new DefaultWorldGenerator(seed);
        //WorldGenerator = new DesertWorldGenerator(seed);
        //WorldGenerator = new FlatWorldGenerator();
        //WorldGenerator = new TestWorldGenerator(seed);
        Timer = new WorldTimer();
    }

    public static MinecraftWorld New(MinecraftServer server, int seed) => new(server, seed);

    public static MinecraftWorld FromSaveData(MinecraftServer server, WorldSaveData worldSaveData)
    {
        var world = new MinecraftWorld(server, worldSaveData.Seed)
        {
            Raining = worldSaveData.Raining
        };
        world.Timer.SetTime(worldSaveData.Time);
        return world;
    }

    public void Start()
    {
        Timer.Start();
    }

    public void Stop()
    {
        Timer.Stop();
    }

    public Task ProcessAsync(TimeSpan elapsed)
    {
        return Task.CompletedTask;
    }

    public async Task SetTimeAsync(long time)
    {
        Timer.SetTime(time);
        await BroadcastTimeUpdateAsync();
    }

    public async Task BroadcastTimeUpdateAsync(CancellationToken cancellationToken = default)
    {
        await Server.BroadcastPacketAsync(new TimeUpdatePacket
        {
            Time = Time
        }, readyClientsOnly: true);
    }

    public async Task SendTimeUpdateAsync(RemoteClient remoteClient)
    {
        await remoteClient.SendPacketAsync(new TimeUpdatePacket
        {
            Time = Time
        });
    }

    public async Task LoadInitialChunksAsync()
    {
        var initialChunks = Chunk.GetChunksAround(Vector2i.Zero, Server.Configuration.VisibleChunksDistance);
        Server.GetLogger<MinecraftWorld>().LogInformation("Generating world...");
        await Parallel.ForEachAsync(initialChunks, async (chunkPosition, _) => await GetOrCreateChunkAsync(chunkPosition));
    }

    public async Task<Chunk> GetOrCreateChunkAsync(Vector2i chunkPosition)
    {
        var regionPosition = Region.GetRegionPositionForChunkPosition(chunkPosition);
        var region = await GetRegionAsync(regionPosition);
        var chunk = await region.GetOrCreateChunkAsync(chunkPosition);
        return chunk;
    }

    public async Task<BlockId> GetBlockIdAsync(Vector3i worldPosition)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrCreateChunkAsync(chunkPosition);
        return chunk.GetBlockId(Chunk.WorldToLocal(worldPosition));
    }

    public async Task<Block> GetBlockAsync(Vector3i worldPosition)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrCreateChunkAsync(chunkPosition);
        return chunk.GetBlock(Chunk.WorldToLocal(worldPosition));
    }

    public async Task<int> GetHighestBlockHeightAsync(Vector2i worldPosition)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrCreateChunkAsync(chunkPosition);
        return chunk.GetHighestBlockHeight(Chunk.WorldToLocal(worldPosition));
    }

    public async Task SetBlockAsync(Vector3i worldPosition, BlockId blockId, byte metadata = 0)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrCreateChunkAsync(chunkPosition);

        var localPosition = Chunk.WorldToLocal(worldPosition);
        chunk.SetBlock(localPosition, blockId, metadata);

        var blockUpdatePacket = new BlockUpdatePacket
        {
            X = worldPosition.X,
            Y = (sbyte) worldPosition.Y,
            Z = worldPosition.Z,
            BlockId = blockId,
            Metadata = metadata
        };

        await Server.BroadcastPacketAsync(blockUpdatePacket);
    }

    public async Task SetTileEntityAsync(Vector3i worldPosition, TileEntity? tileEntity)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrCreateChunkAsync(chunkPosition);

        var localPosition = Chunk.WorldToLocal(worldPosition);
        chunk.SetTileEntity(localPosition, tileEntity);
    }

    public async Task<T?> GetTileEntityAsync<T>(Vector3i worldPosition) where T : TileEntity
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrCreateChunkAsync(chunkPosition);

        var localPosition = Chunk.WorldToLocal(worldPosition);
        return chunk.GetTileEntity<T>(localPosition);
    }

    public async Task StartRainAsync()
    {
        if (Raining)
            return;

        Raining = true;
        await Server.BroadcastPacketAsync(new NewStatePacket
        {
            Reason = NewStatePacket.ReasonType.BeginRaining
        });
    }

    public async Task StopRainAsync()
    {
        if (!Raining)
            return;

        Raining = false;
        await Server.BroadcastPacketAsync(new NewStatePacket
        {
            Reason = NewStatePacket.ReasonType.EndRaining
        });
    }

    public async Task SaveAsync()
    {
        SaveManager.SaveWorld(GetSaveData());

        foreach (var region in _regions.Values)
        {
            await region.SaveAsync();
        }
    }

    public WorldSaveData GetSaveData()
    {
        return new WorldSaveData
        {
            Seed = Seed,
            Time = Time,
            SpawnLocation = new Vector3i(0, 70, 0),
            Raining = Raining,
            RainTime = 0,
            Thundering = false,
            ThunderTime = 0,
            Version = 19132,
            LastPlayed = TimeProvider.System.GetUtcNow().ToUnixTimeMilliseconds(),
            LevelName = "world",
            SizeOnDisk = 0
        };
    }

    private async Task<Region> GetRegionAsync(Vector2i regionPosition)
    {
        using (await _regionLoadLocker.LockAsync(regionPosition))
        {
            if (_regions.TryGetValue(regionPosition, out var region))
                return region;

            region = await Region.LoadOrCreateAsync(regionPosition, this);
            _regions[regionPosition] = region;
            return region;
        }
    }

    public void Dispose()
    {
        foreach (var region in _regions.Values)
        {
            region.Dispose();
        }
    }
}