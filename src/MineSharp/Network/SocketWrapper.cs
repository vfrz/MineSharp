using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using MineSharp.Extensions;

namespace MineSharp.Network;

public class SocketWrapper : IDisposable
{
    public Socket Socket { get; }
    public NetworkStream NetworkStream { get; }
    
    public SocketWrapper(Socket socket)
    {
        Socket = socket;
        NetworkStream = new NetworkStream(socket, true);
    }

    public async Task WriteLongAsync(long value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(8);
        BinaryPrimitives.WriteInt64BigEndian(memoryOwner.Memory.Span, value);
        await NetworkStream.WriteAsync(memoryOwner.Memory[..8]);
    }
    
    public async ValueTask WriteStringAsync(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await WriteVarIntAsync(bytes.Length);
        NetworkStream.Write(bytes, 0, bytes.Length);
    }

    public async ValueTask WriteVarIntAsync(int value)
    {
        var bytes = value.ToVarInt();
        await NetworkStream.WriteAsync(bytes);
    }

    public async Task WriteBytesAsync(byte[] bytes)
    {
        await NetworkStream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async ValueTask CloseSocketConnectionAsync()
    {
        await Socket.DisconnectAsync(false);
    }
    
    public void Dispose()
    {
        NetworkStream.Dispose();
    }
}