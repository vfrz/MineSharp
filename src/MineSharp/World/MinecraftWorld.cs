using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using MineSharp.Core;
using MineSharp.Network.Packets;

namespace MineSharp.World;

public class MinecraftWorld
{
    public TwoDimensionalArray<WorldChunk?> Chunks { get; }

    public bool Raining { get; private set; }
    private WorldTimer Timer { get; }

    public long Time => Timer.CurrentTime;

    public MinecraftServer Server { get; }
    private readonly ILogger<MinecraftWorld> _logger;

    private static readonly object Locker = new();

    public int Seed { get; }
    public FastNoiseLite Noise { get; }
    public FastNoiseLite OtherNoise { get; }
    public Random Random { get; }

    public MinecraftWorld(MinecraftServer server, int seed)
    {
        Server = server;
        Seed = seed;

        Random = new Random(Seed);
        
        Noise = new FastNoiseLite(Seed);
        Noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        // Terrain parameters
        Noise.SetFrequency(0.008f); // Adjust the frequency to change the terrain detail
        Noise.SetFractalOctaves(16); // Adjust the number of octaves for more complexity
        Noise.SetFractalLacunarity(2.0f); // Adjust the lacunarity for variation
        Noise.SetFractalGain(0.5f); // Adjust the gain for smoothness
        
        OtherNoise = new FastNoiseLite(Seed);
        OtherNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        OtherNoise.SetFrequency(0.5f); // Adjust the frequency to change the terrain detail
        OtherNoise.SetFractalOctaves(1); // Adjust the number of octaves for more complexity
        OtherNoise.SetFractalLacunarity(2.0f); // Adjust the lacunarity for variation
        OtherNoise.SetFractalGain(0.5f); // Adjust the gain for smoothness

        _logger = server.GetLogger<MinecraftWorld>();
        Timer = new WorldTimer();
        Chunks = new TwoDimensionalArray<WorldChunk?>(-500, 500, -500, 500);
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
        Parallel.ForEach(GetInitialChunks(), chunk =>
        {
            GetOrLoadChunk(chunk.X, chunk.Z);
        });
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

    public async Task SetBlockAsync(Vector3i worldPosition, byte blockId)
    {
        var chunkX = worldPosition.X / WorldChunk.Length - (worldPosition.X < 0 ? 1 : 0);
        var chunkZ = worldPosition.Z / WorldChunk.Width - (worldPosition.Z < 0 ? 1 : 0);

        var chunk = Chunks[chunkX, chunkZ];
        if (chunk is null)
            chunk = GetOrLoadChunk(chunkX, chunkZ);

        await chunk.SetBlockAsync(worldPosition.X, worldPosition.Y, worldPosition.Z, blockId);
    }

    public WorldChunk GetOrLoadChunk(int chunkX, int chunkZ)
    {
        //TODO Be careful about thread safety here
        var chunk = Chunks[chunkX, chunkZ];
        if (chunk is null)
        {
            chunk = new WorldChunk(this, chunkX, chunkZ);
            Chunks[chunkX, chunkZ] = chunk;
            chunk.Generate();
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