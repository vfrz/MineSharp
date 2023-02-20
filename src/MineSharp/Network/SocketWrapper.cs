using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using MineSharp.Extensions;

namespace MineSharp.Network;

public class SocketWrapper : IDisposable
{
    private const int SegmentBits = 0x7F;
    private const int ContinueBit = 0x80;
    
    public Socket Socket { get; }
    public NetworkStream NetworkStream { get; }
    
    public SocketWrapper(Socket socket)
    {
        Socket = socket;
        NetworkStream = new NetworkStream(socket, true);
    }

    public void WriteLong(long value)
    {
        Span<byte> span = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(span, value);
        NetworkStream.Write(span);
    }
    
    public async ValueTask WriteString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await WriteVarInt(bytes.Length);
        NetworkStream.Write(bytes, 0, bytes.Length);
    }

    public async ValueTask WriteVarInt(int value)
    {
        var bytes = value.ToVarInt();
        await NetworkStream.WriteAsync(bytes);
    }

    public void WriteBytes(byte[] bytes)
    {
        NetworkStream.Write(bytes, 0, bytes.Length);
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