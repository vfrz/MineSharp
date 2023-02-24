using System.Net.Sockets;
using MineSharp.Core;

namespace MineSharp.Network;

public class MinecraftClient : IDisposable
{
    public SocketWrapper SocketWrapper { get; }
    public MinecraftPlayer? Player { get; private set; }
    public string NetworkId { get; }

    public MinecraftClient(Socket socket)
    {
        SocketWrapper = new SocketWrapper(socket);
        NetworkId = socket.RemoteEndPoint?.ToString() ?? throw new Exception();
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