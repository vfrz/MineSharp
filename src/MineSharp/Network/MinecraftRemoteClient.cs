using System.Net.Sockets;
using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network;

public class MinecraftRemoteClient : IDisposable
{
    public MinecraftPlayer? Player { get; private set; }
    public string? Username { get; set; }
    public string NetworkId { get; }

    private Socket Socket { get; }

    public MinecraftRemoteClient(Socket socket)
    {
        Socket = socket;
        NetworkId = socket.RemoteEndPoint?.ToString() ?? throw new Exception();
    }

    public void InitializePlayer(MinecraftPlayer player)
    {
        if (Player is not null)
            throw new Exception($"Can't initialize player because it has already been initialized");
        Player = player;
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