using Microsoft.Extensions.Logging;
using MineSharp.Core;
using MineSharp.Network.Packets;
using MineSharp.World.Generation;

namespace MineSharp.World;

public class MinecraftWorld
{
    private TwoDimensionalArray<Chunk?> Chunks { get; }

    public bool Raining { get; private set; }
    private WorldTimer Timer { get; }

    public long Time => Timer.CurrentTime;

    public MinecraftServer Server { get; }
    private readonly ILogger<MinecraftWorld> _logger;

    private static readonly object Locker = new();

    public int Seed { get; }
    public IWorldGenerator WorldGenerator { get; }

    public MinecraftWorld(MinecraftServer server, int seed)
    {
        Server = server;
        Seed = seed;
        WorldGenerator = new DefaultWorldGenerator(seed);
        _logger = server.GetLogger<MinecraftWorld>();
        Timer = new WorldTimer();
        Chunks = new TwoDimensionalArray<Chunk?>(-500, 500, -500, 500);
    }

    public void Start()
    {
        Timer.Start();
    }

    public void Stop()
    {
        Timer.Stop();
    }

    public async Task ProcessAsync(TimeSpan elapsed)
    {
    }

    public async Task SetTimeAsync(long time)
    {
        Timer.SetTime(time);
        await SendTimeUpdateAsync();
    }

    public async Task SendTimeUpdateAsync()
    {
        await Server.BroadcastPacketAsync(new TimeUpdatePacket
        {
            Time = Time
        }, readyOnly: true);
    }

    public void GenerateInitialChunks()
    {
        foreach (var chunk in GetInitialChunks())
        {
            GetOrLoadChunk(chunk.X, chunk.Z);
        }
    }

    public HashSet<Vector2i> GetInitialChunks()
    {
        var chunks = new HashSet<Vector2i>();
        var distance = Server.Configuration.VisibleChunksDistance;
        var currentChunk = new Vector2i(0, 0);
        chunks.Add(currentChunk);

        // Front
        for (var z = 1; z < distance; z++)
        {
            chunks.Add(new Vector2i(currentChunk.X, currentChunk.Z + z));
            for (var x = 1; x < distance - z; x++)
            {
                chunks.Add(new Vector2i(currentChunk.X - x, currentChunk.Z + z));
            }
        }

        // Right
        for (var x = 1; x < distance; x++)
        {
            chunks.Add(new Vector2i(currentChunk.X - x, currentChunk.Z));
            for (var z = 1; z < distance - x; z++)
            {
                chunks.Add(new Vector2i(currentChunk.X - x, currentChunk.Z - z));
            }
        }

        // Back
        for (var z = 1; z < distance; z++)
        {
            chunks.Add(new Vector2i(currentChunk.X, currentChunk.Z - z));
            for (var x = 1; x < distance - z; x++)
            {
                chunks.Add(new Vector2i(currentChunk.X + x, currentChunk.Z - z));
            }
        }

        // Left
        for (var x = 1; x < distance; x++)
        {
            chunks.Add(new Vector2i(currentChunk.X + x, currentChunk.Z));
            for (var z = 1; z < distance - x; z++)
            {
                chunks.Add(new Vector2i(currentChunk.X + x, currentChunk.Z + z));
            }
        }

        return chunks;
    }

    public async Task UpdateBlockAsync(Vector3i worldPosition, byte blockId, byte metadata = 0)
    {
        var chunkX = worldPosition.X / Chunk.Width - (worldPosition.X < 0 ? 1 : 0);
        var chunkZ = worldPosition.Z / Chunk.Width - (worldPosition.Z < 0 ? 1 : 0);

        var chunk = Chunks[chunkX, chunkZ];
        if (chunk is null)
            chunk = GetOrLoadChunk(chunkX, chunkZ);

        chunk.UpdateBlock(worldPosition, blockId, metadata);

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

    public Chunk GetOrLoadChunk(int chunkX, int chunkZ)
    {
        //TODO Be careful about thread safety here
        var chunk = Chunks[chunkX, chunkZ];
        if (chunk is null)
        {
            var chunkData = new ChunkData();
            WorldGenerator.GenerateChunk(chunkX, chunkZ, chunkData);
            
            //TODO Move ligth calculation somewhere else
            for (var x = 0; x < Chunk.Width; x++)
            for (var y = 0; y < Chunk.Height; y++)
            for (var z = 0; z < Chunk.Width; z++)
            {
                chunkData.SetLight(new Vector3i(x, y, z), 15, 15);
            }
            
            chunk = new Chunk(chunkX, chunkZ, chunkData);
            Chunks[chunkX, chunkZ] = chunk;
        }

        return chunk;
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
}