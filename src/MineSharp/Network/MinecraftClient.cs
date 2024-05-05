using MineSharp.Core;

namespace MineSharp.Network;

public class MinecraftClient : IDisposable
{
    public SocketWrapper SocketWrapper { get; }
    public MinecraftPlayer? Player { get; private set; }
    public string NetworkId { get; }
    public MinecraftClientState State { get; set; }
    public int? ProtocolVersion { get; set; }
    public string? Username { get; set; }
    public Guid? Id { get; set; }

    public MinecraftClient(SocketWrapper socketWrapper)
    {
        SocketWrapper = socketWrapper;
        NetworkId = socketWrapper.Socket.RemoteEndPoint?.ToString() ?? throw new Exception();
    }

    public void InitializePlayer()
    {
        Player = new MinecraftPlayer
        {
            Health = 20
        };
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