using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using MineSharp.Core;
using MineSharp.Network.Packets;
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

    private readonly AsyncKeyedLocker<Vector2i> _chunkLoadingLocker = new();

    public MinecraftWorld(MinecraftServer server, int seed)
    {
        Server = server;
        Seed = seed;
        //WorldGenerator = new DefaultWorldGenerator(seed);
        //WorldGenerator = new DesertWorldGenerator(seed);
        WorldGenerator = new FlatWorldGenerator();
        //WorldGenerator = new TestWorldGenerator(seed);
        _logger = server.GetLogger<MinecraftWorld>();
        Timer = new WorldTimer();
        Chunks = new ChunksContainer();
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
        await Parallel.ForEachAsync(initialChunks, async (chunkPosition, _) => await GetOrLoadChunkAsync(chunkPosition));
    }

    public async Task<byte> GetBlockIdAsync(Vector3i worldPosition)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrLoadChunkAsync(chunkPosition);
        return chunk.Data.GetBlock(Chunk.WorldToLocal(worldPosition), out _);
    }

    public async Task<int> GetHighestBlockHeightAsync(Vector2i worldPosition)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);
        var chunk = await GetOrLoadChunkAsync(chunkPosition);
        return chunk.GetHighestBlockHeight(Chunk.WorldToLocal(worldPosition));
    }

    public async Task UpdateBlockAsync(Vector3i worldPosition, byte blockId, byte metadata = 0)
    {
        var chunkPosition = Chunk.GetChunkPositionForWorldPosition(worldPosition);

        var chunk = await GetOrLoadChunkAsync(chunkPosition);

        var localPosition = Chunk.WorldToLocal(worldPosition);
        chunk.UpdateBlock(localPosition, blockId, metadata);

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

    public async Task<Chunk> GetOrLoadChunkAsync(Vector2i chunkPosition)
    {
        using (await _chunkLoadingLocker.LockAsync(chunkPosition))
        {
            var chunk = Chunks[chunkPosition];
            if (chunk is null)
            {
                var chunkData = new ChunkData();
                WorldGenerator.GenerateChunkTerrain(chunkPosition, chunkData);
                WorldGenerator.GenerateChunkDecorations(chunkPosition, chunkData);

                //TODO Move ligth calculation somewhere else
                for (var x = 0; x < Chunk.Width; x++)
                for (var y = 0; y < Chunk.Height; y++)
                for (var z = 0; z < Chunk.Width; z++)
                {
                    chunkData.SetLight(new Vector3i(x, y, z), 15, 15);
                }

                chunk = new Chunk(chunkPosition.X, chunkPosition.Z, chunkData);
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

    public static HashSet<Vector2i> GetChunksAround(Vector2i originChunk, int distance)
    {
        var chunks = new HashSet<Vector2i>
        {
            originChunk
        };

        // Front
        for (var z = 1; z < distance; z++)
        {
            chunks.Add(new Vector2i(originChunk.X, originChunk.Z + z));
            for (var x = 1; x < distance - z; x++)
            {
                chunks.Add(new Vector2i(originChunk.X - x, originChunk.Z + z));
            }
        }

        // Right
        for (var x = 1; x < distance; x++)
        {
            chunks.Add(new Vector2i(originChunk.X - x, originChunk.Z));
            for (var z = 1; z < distance - x; z++)
            {
                chunks.Add(new Vector2i(originChunk.X - x, originChunk.Z - z));
            }
        }

        // Back
        for (var z = 1; z < distance; z++)
        {
            chunks.Add(new Vector2i(originChunk.X, originChunk.Z - z));
            for (var x = 1; x < distance - z; x++)
            {
                chunks.Add(new Vector2i(originChunk.X + x, originChunk.Z - z));
            }
        }

        // Left
        for (var x = 1; x < distance; x++)
        {
            chunks.Add(new Vector2i(originChunk.X + x, originChunk.Z));
            for (var z = 1; z < distance - x; z++)
            {
                chunks.Add(new Vector2i(originChunk.X + x, originChunk.Z + z));
            }
        }

        return chunks;
    }
}