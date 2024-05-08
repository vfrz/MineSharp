using System.Net.Sockets;
using MineSharp.Core.Packets;

namespace MineSharp.Core;

public class MinecraftRemoteClient : IDisposable
{
    public enum ClientState
    {
        Initial,
        Ready
    }
    
    public MinecraftPlayer? Player { get; private set; }
    public string? Username { get; set; }
    public string NetworkId { get; }
    
    public ClientState State { get; private set; }

    private Socket Socket { get; }
    private MinecraftServer Server { get; }

    public MinecraftRemoteClient(Socket socket, MinecraftServer server)
    {
        Socket = socket;
        Server = server;
        State = ClientState.Initial;
        NetworkId = socket.RemoteEndPoint?.ToString() ?? throw new Exception();
    }

    public MinecraftPlayer InitializePlayer()
    {
        if (Player is not null)
            throw new Exception($"Can't initialize player because it has already been initialized");
        var player = new MinecraftPlayer(this)
        {
            X = 0,
            Y = 5,
            Z = 0,
            Stance = 5 + 1.62,
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

    public void Dispose()
    {
        Socket.Dispose();
    }
}