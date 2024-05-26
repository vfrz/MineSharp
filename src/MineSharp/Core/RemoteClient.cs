using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using MineSharp.Network;
using MineSharp.Network.Packets;
using MineSharp.Saves;
using MineSharp.World;

namespace MineSharp.Core;

public class RemoteClient : IDisposable
{
    public enum ClientState
    {
        Initial,
        Ready
    }

    public Player? Player { get; private set; }
    public string NetworkId { get; }

    public ClientState State { get; private set; }

    private Socket Socket { get; }
    private MinecraftServer Server { get; }

    private HashSet<Vector2i> _loadedChunks = new();

    public RemoteClient(Socket socket, MinecraftServer server)
    {
        Socket = socket;
        Server = server;
        State = ClientState.Initial;
        NetworkId = socket.RemoteEndPoint?.ToString() ?? throw new Exception();
    }

    public async Task<Player> InitializePlayerAsync(string username)
    {
        if (Player is not null)
            throw new Exception($"Can't initialize player {username} because it has already been initialized");

        if (SaveManager.IsPlayerSaved(username))
        {
            Player = await Player.LoadPlayerAsync(Server, this, username);
        }
        else
        {
            Player = await Player.NewPlayerAsync(Server, this, username);
        }

        Server.EntityManager.RegisterEntity(Player);
        return Player;
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
            Server.GetLogger<RemoteClient>().LogError(ex, "Failed to send data to client: {networkId}", NetworkId);
            return false;
        }
    }

    public async Task SendChatAsync(string message)
    {
        await SendPacketAsync(new ChatMessagePacket
        {
            Message = message
        });
    }

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

    public async Task LoadChunkAsync(Vector2i chunkToLoad)
    {
        if (_loadedChunks.Contains(chunkToLoad))
            return;

        var chunk = await Server.World.GetOrCreateChunkAsync(chunkToLoad);

        await SendPacketAsync(new PreChunkPacket
        {
            X = chunkToLoad.X,
            Z = chunkToLoad.Z,
            Mode = PreChunkPacket.LoadingMode.Load
        });

        await SendPacketAsync(new ChunkPacket
        {
            X = chunkToLoad.X * Chunk.ChunkWidth,
            Y = 0,
            Z = chunkToLoad.Z * Chunk.ChunkWidth,
            SizeX = Chunk.ChunkWidth - 1,
            SizeY = Chunk.ChunkHeight - 1,
            SizeZ = Chunk.ChunkWidth - 1,
            CompressedData = await chunk.ToCompressedDataAsync()
        });
    }

    public async Task UpdateLoadedChunksAsync()
    {
        var visibleChunks =
            Chunk.GetChunksAround(GetCurrentChunk(), Server.Configuration.VisibleChunksDistance);
        var chunksToLoad = visibleChunks.Except(_loadedChunks);
        var chunksToUnload = _loadedChunks.Except(visibleChunks);

        foreach (var chunkToLoad in chunksToLoad)
        {
            var chunk = await Server.World.GetOrCreateChunkAsync(chunkToLoad);

            await SendPacketAsync(new PreChunkPacket
            {
                X = chunkToLoad.X,
                Z = chunkToLoad.Z,
                Mode = PreChunkPacket.LoadingMode.Load
            });

            await SendPacketAsync(new ChunkPacket
            {
                X = chunkToLoad.X * Chunk.ChunkWidth,
                Y = 0,
                Z = chunkToLoad.Z * Chunk.ChunkWidth,
                SizeX = Chunk.ChunkWidth - 1,
                SizeY = Chunk.ChunkHeight - 1,
                SizeZ = Chunk.ChunkWidth - 1,
                CompressedData = await chunk.ToCompressedDataAsync()
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

    public async Task KickAsync(string reason)
    {
        await SendPacketAsync(new PlayerDisconnectPacket
        {
            Reason = reason
        });
        await DisconnectSocketAsync();
    }

    public async Task DisconnectSocketAsync()
    {
        await Socket.DisconnectAsync(false);
    }

    public void Dispose()
    {
        Socket.Dispose();
    }
}