using System.Net.Sockets;

namespace MineSharp.Network;

public class SocketWrapper : IDisposable
{
    public Socket Socket { get; }

    public bool Connected => Socket.Connected;
    
    private NetworkStream NetworkStream { get; }
    private readonly SemaphoreSlim _semaphore;
    
    //TODO Remove this class and use Socket.SendAsync with a memorystream to write packet data beforehand
    //Then test the thread safety
    public SocketWrapper(Socket socket)
    {
        Socket = socket;
        NetworkStream = new NetworkStream(socket, true);
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public WritingSession StartWriting()
    {
        return new WritingSession(NetworkStream, _semaphore);
    }

    public async ValueTask CloseSocketConnectionAsync()
    {
        await Socket.DisconnectAsync(false);
    }
    
    public void Dispose()
    {
        _semaphore.Dispose();
        NetworkStream.Dispose();
    }
}