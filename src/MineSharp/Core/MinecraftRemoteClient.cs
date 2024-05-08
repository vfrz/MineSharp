using System.Net.Sockets;
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

    public PlayerEntity InitializePlayer()
    {
        if (Player is not null)
            throw new Exception($"Can't initialize player because it has already been initialized");
        var position = new Vector3(0, 50, 0);
        var player = new PlayerEntity(this)
        {
            Position = position,
            Stance = position.Y + 1.62,
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
        await using var writer = new PacketWriter();
        packet.Write(writer);
        var data = writer.ToByteArray();
        await SendAsync(data);
    }

    public async Task SendAsync(byte[] data)
    {
        await Socket.SendAsync(data);
    }

    public async Task DisconnectSocketAsync()
    {
        await Socket.DisconnectAsync(false);
    }

    public async Task SendMessageAsync(string message)
    {
        await SendPacketAsync(new ChatMessagePacket
        {
            Message = message
        });
    }

    //TODO optimize (remove LINQ)
    public async Task UpdateLoadedChunksAsync()
    {
        var visibleChunks = GetVisibleChunks();
        var chunksToLoad = visibleChunks.Except(_loadedChunks);
        var chunksToUnload = _loadedChunks.Except(visibleChunks);

        foreach (var chunkToLoad in chunksToLoad)
        {
            var chunk = Server.World.GetOrLoadChunk(chunkToLoad.X, chunkToLoad.Z);

            await SendPacketAsync(new PreChunkPacket
            {
                X = chunkToLoad.X,
                Z = chunkToLoad.Z,
                Mode = PreChunkPacket.LoadingMode.Load
            });

            await SendPacketAsync(new ChunkPacket
            {
                X = chunkToLoad.X * WorldChunk.Length,
                Y = 0,
                Z = chunkToLoad.Z * WorldChunk.Width,
                SizeX = WorldChunk.Length - 1,
                SizeY = WorldChunk.Height - 1,
                SizeZ = WorldChunk.Width - 1,
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

    public Vector2i GetCurrentChunk() => WorldChunk.WorldPositionToChunk(Player!.Position);

    public HashSet<Vector2i> GetVisibleChunks()
    {
        var chunks = new HashSet<Vector2i>();
        var distance = Server.Configuration.VisibleChunksDistance;
        var currentChunk = GetCurrentChunk();
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

    public void Dispose()
    {
        Socket.Dispose();
    }
}