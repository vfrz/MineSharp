using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using MineSharp.Core.Packets;
using MineSharp.Network.Packets;
using MineSharp.World;

namespace MineSharp.Core;

public class MinecraftRemoteClient : IDisposable
{
    public enum ClientState
    {
        Initial,
        Ready
    }

    public PlayerEntity? Player { get; private set; }
    public string? Username { get; set; }
    public string NetworkId { get; }

    public ClientState State { get; private set; }

    private Socket Socket { get; }
    private MinecraftServer Server { get; }

    private HashSet<Vector2i> _loadedChunks = new();

    public MinecraftRemoteClient(Socket socket, MinecraftServer server)
    {
        Socket = socket;
        Server = server;
        State = ClientState.Initial;
        NetworkId = socket.RemoteEndPoint?.ToString() ?? throw new Exception();
    }

    public PlayerEntity InitializePlayer(Vector3d position)
    {
        if (Player is not null)
            throw new Exception($"Can't initialize player because it has already been initialized");
        var player = new PlayerEntity(this)
        {
            Position = position,
            Stance = position.Y + PlayerEntity.Height,
            OnGround = false, //TODO Change that when spawning correctly
            Pitch = 0,
            Yaw = 0,
            PositionDirty = false
        };
        Server.EntityManager.RegisterEntity(player);
        return Player = player;
    }

    public void SetReady()
    {
        State = ClientState.Ready;
    }

    public async Task SendPacketAsync(IServerPacket packet)
    {
        await using var writer = new PacketWriter(packet.PacketId);
        packet.Write(writer);
        var data = writer.ToByteArray();
        await TrySendAsync(data);
    }

    public async Task<bool> TrySendAsync(byte[] data)
    {
        if (!Socket.Connected)
            return false;
        try
        {
            await Socket.SendAsync(data);
            return true;
        }
        catch (Exception ex)
        {
            Server.GetLogger<MinecraftRemoteClient>().LogError(ex, "Failed to send data to client: {networkId}", NetworkId);
            return false;
        }
    }

    public async Task DisconnectSocketAsync()
    {
        await Socket.DisconnectAsync(false);
    }

    public async Task SendChatAsync(string message)
    {
        await SendPacketAsync(new ChatMessagePacket
        {
            Message = message
        });
    }

    //TODO optimize (remove LINQ)

    public async Task UnloadChunksAsync()
    {
        foreach (var chunkToUnload in _loadedChunks)
        {
            await SendPacketAsync(new PreChunkPacket
            {
                X = chunkToUnload.X,
                Z = chunkToUnload.Z,
                Mode = PreChunkPacket.LoadingMode.Unload
            });
        }

        _loadedChunks.Clear();
    }

    public async Task UpdateLoadedChunksAsync()
    {
        var visibleChunks = MinecraftWorld.GetChunksAround(GetCurrentChunk(), Server.Configuration.VisibleChunksDistance);
        var chunksToLoad = visibleChunks.Except(_loadedChunks);
        var chunksToUnload = _loadedChunks.Except(visibleChunks);

        //TODO Maybe parallelize this when network is using batching with channels
        foreach (var chunkToLoad in chunksToLoad)
        {
            var chunk = Server.World.GetOrLoadChunk(chunkToLoad);

            await SendPacketAsync(new PreChunkPacket
            {
                X = chunkToLoad.X,
                Z = chunkToLoad.Z,
                Mode = PreChunkPacket.LoadingMode.Load
            });

            await SendPacketAsync(new ChunkPacket
            {
                X = chunkToLoad.X * Chunk.Width,
                Y = 0,
                Z = chunkToLoad.Z * Chunk.Width,
                SizeX = Chunk.Width - 1,
                SizeY = Chunk.Height - 1,
                SizeZ = Chunk.Width - 1,
                CompressedData = await chunk.Data.ToCompressedDataAsync()
            });
        }

        foreach (var chunkToUnload in chunksToUnload)
        {
            await SendPacketAsync(new PreChunkPacket
            {
                X = chunkToUnload.X,
                Z = chunkToUnload.Z,
                Mode = PreChunkPacket.LoadingMode.Unload
            });
        }

        _loadedChunks = visibleChunks;
    }

    public Vector2i GetCurrentChunk() => Chunk.GetChunkPositionForWorldPosition(Player!.Position);

    public void Dispose()
    {
        Socket.Dispose();
    }
}