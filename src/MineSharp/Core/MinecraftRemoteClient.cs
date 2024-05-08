using System.Net.Sockets;
using MineSharp.Core.Packets;
using MineSharp.Network.Packets;

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
        var player = new PlayerEntity(this)
        {
            Position = new Vector3(0, 5, 0),
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

    public async Task SendMessageAsync(string message)
    {
        await SendPacketAsync(new ChatMessagePacket
        {
            Message = message
        });
    }
    
    public void Dispose()
    {
        Socket.Dispose();
    }
}