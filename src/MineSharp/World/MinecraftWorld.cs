using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using MineSharp.Blocks;
using MineSharp.Core;
using MineSharp.Network.Packets;
using MineSharp.Saves;
using MineSharp.World.Generation;

namespace MineSharp.World;

public class MinecraftWorld
{
    private ChunksContainer Chunks { get; }

    public bool Raining { get; private set; }
    private WorldTimer Timer { get; }

    public long Time => Timer.CurrentTime;

    private MinecraftServer Server { get; }
    private readonly ILogger<MinecraftWorld> _logger;

    public int Seed { get; }
    private IWorldGenerator WorldGenerator { get; }

    private readonly AsyncKeyedLocker<Vector2i> _chunkLoadLocker = new();

    private MinecraftWorld(MinecraftServer server, int seed)
    {
        Server = server;
        Seed = seed;
        WorldGenerator = new DefaultWorldGenerator(seed);
        //WorldGenerator = new DesertWorldGenerator(seed);
        //WorldGenerator = new FlatWorldGenerator();
        //WorldGenerator = new TestWorldGenerator(seed);
        _logger = server.GetLogger<MinecraftWorld>();
        Timer = new WorldTimer();
        Chunks = new ChunksContainer();
    }

    public static MinecraftWorld New(MinecraftServer server, int seed) => new MinecraftWorld(server, seed);

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
        await SendTimeUpdateAsync();
    }

    public async Task SendTimeUpdateAsync(CancellationToken cancellationToken = default)
    {
        await Server.BroadcastPacketAsync(new TimeUpdatePacket
        {
            Time = Time
        }, readyOnly: true);
    }

    public async Task LoadInitialChunksAsync()
    {
        var initialChunks = GetChunksAround(Vector2i.Zero, Server.Configuration.VisibleChunksDistance);
        _logger.LogInformation("Generating world...");
        await Parallel.ForEachAsync(initialChunks,
            async (chunkPosition, _) => await GetOrLoadChunkAsync(chunkPosition));
    }

    public async Task<BlockId> GetBlockIdAsync(Vector3i worldPosition)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrLoadChunkAsync(chunkPosition);
        return chunk.GetBlockId(Chunk.WorldToLocal(worldPosition));
    }

    public async Task<Block> GetBlockAsync(Vector3i worldPosition)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrLoadChunkAsync(chunkPosition);
        return chunk.GetBlock(Chunk.WorldToLocal(worldPosition));
    }

    public async Task<int> GetHighestBlockHeightAsync(Vector2i worldPosition)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrLoadChunkAsync(chunkPosition);
        return chunk.GetHighestBlockHeight(Chunk.WorldToLocal(worldPosition));
    }

    public async Task SetBlockAsync(Vector3i worldPosition, BlockId blockId, byte metadata = 0)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);

        var chunk = await GetOrLoadChunkAsync(chunkPosition);

        var localPosition = Chunk.WorldToLocal(worldPosition);
        chunk.SetBlock(localPosition, blockId, metadata);

        var blockUpdatePacket = new BlockUpdatePacket
        {
            X = worldPosition.X,
            Y = (sbyte)worldPosition.Y,
            Z = worldPosition.Z,
            BlockId = blockId,
            Metadata = metadata
        };

        await Server.BroadcastPacketAsync(blockUpdatePacket);
    }

    public async Task<Chunk> GetOrLoadChunkAsync(Vector2i chunkPosition)
    {
        using (await _chunkLoadLocker.LockAsync(chunkPosition))
        {
            var chunk = Chunks[chunkPosition];
            if (chunk is null)
            {
                chunk = new Chunk(chunkPosition);

                if (Server.SaveManager.IsChunkSaved(chunkPosition))
                {
                    var saveData = await Server.SaveManager.LoadChunkAsync(chunkPosition);
                    chunk.LoadFromSaveData(saveData);
                }
                else
                {
                    WorldGenerator.GenerateChunkTerrain(chunkPosition, chunk);
                    WorldGenerator.GenerateChunkDecorations(chunkPosition, chunk);

                    //TODO Move ligth calculation somewhere else
                    for (var x = 0; x < Chunk.ChunkWidth; x++)
                    for (var y = 0; y < Chunk.ChunkHeight; y++)
                    for (var z = 0; z < Chunk.ChunkWidth; z++)
                    {
                        chunk.SetLight(new Vector3i(x, y, z), 15, 15);
                    }

                    await SaveChunkAsync(chunk);
                }

                Chunks[chunkPosition] = chunk;
            }

            return chunk;
        }
    }

    public async Task StartRainAsync()
    {
        Raining = true;
        await Server.BroadcastPacketAsync(new NewStatePacket
        {
            Reason = NewStatePacket.ReasonType.BeginRaining
        });
    }

    public async Task StopRainAsync()
    {
        Raining = false;
        await Server.BroadcastPacketAsync(new NewStatePacket
        {
            Reason = NewStatePacket.ReasonType.EndRaining
        });
    }

    public async Task SaveAsync()
    {
        Server.SaveManager.SaveWorld(GetSaveData());

        foreach (var chunk in Chunks)
        {
            await SaveChunkAsync(chunk);
        }
    }

    public WorldSaveData GetSaveData()
    {
        return new WorldSaveData
        {
            Seed = Seed,
            Time = Time,
            Raining = Raining
        };
    }

    private async Task SaveChunkAsync(Chunk chunk)
    {
        var saveData = chunk.GetSaveData();
        await Server.SaveManager.SaveChunkAsync(chunk.ChunkPosition, saveData);
    }

    private static double Distance(Vector2i a, Vector2i b)
    {
        var dx = b.X - a.X;
        var dz = b.Z - a.Z;
        return Math.Sqrt(dx * dx + dz * dz);
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
                var distance = Distance(originChunk, new Vector2i(x, z));
                if (distance <= radius)
                {
                    chunks.Add(new Vector2i(x, z));
                }
            }
        }

        return chunks.OrderBy(c => Distance(originChunk, c)).ToHashSet();

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