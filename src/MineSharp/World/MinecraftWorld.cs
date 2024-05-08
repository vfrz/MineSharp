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

    public MinecraftWorld(MinecraftServer server)
    {
        Server = server;
        _logger = server.GetLogger<MinecraftWorld>();
        Timer = new WorldTimer();
        Chunks = new TwoDimensionalArray<WorldChunk?>(-1, 2, -1, 2);
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

    public void InitializeDefault()
    {
        for (var x = Chunks.LowerBoundX; x < Chunks.UpperBoundX; x++)
        for (var z = Chunks.LowerBoundZ; z < Chunks.UpperBoundZ; z++)
            LoadChunk(x, z);
    }

    public async Task SetBlockAsync(Vector3i worldPosition, byte blockId)
    {
        var chunkX = worldPosition.X / WorldChunk.Length - (worldPosition.X < 0 ? 1 : 0);
        var chunkZ = worldPosition.Z / WorldChunk.Width - (worldPosition.Z < 0 ? 1 : 0);

        var chunk = Chunks[chunkX, chunkZ];
        if (chunk is null)
            chunk = LoadChunk(chunkX, chunkZ);

        await chunk.SetBlockAsync(worldPosition.X, worldPosition.Y, worldPosition.Z, blockId);
    }

    private WorldChunk LoadChunk(int chunkX, int chunkZ)
    {
        var chunk = new WorldChunk(this, chunkX, chunkZ);
        chunk.FillDefault();
        Chunks[chunkX, chunkZ] = chunk;
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