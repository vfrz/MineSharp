using MineSharp.Core;
using MineSharp.Core.Packets;

namespace MineSharp.Network;

public class MinecraftRemoteClient : IDisposable
{
    public SocketWrapper SocketWrapper { get; }
    public MinecraftPlayer? Player { get; private set; }
    public string NetworkId { get; }
    public string? Username { get; set; }

    public MinecraftRemoteClient(SocketWrapper socketWrapper)
    {
        SocketWrapper = socketWrapper;
        NetworkId = socketWrapper.Socket.RemoteEndPoint?.ToString() ?? throw new Exception();
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
        await SocketWrapper.Socket.SendAsync(data);
    }

    public async Task DisconnectAsync()
    {
        await SocketWrapper.CloseSocketConnectionAsync();
    }

    public void Dispose()
    {
        SocketWrapper.Dispose();
    }
}