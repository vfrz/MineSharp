using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace MineSharp.Core.Packets;

public class PacketWriter : IDisposable, IAsyncDisposable
{
    private readonly MemoryStream _memoryStream;

    public PacketWriter()
    {
        _memoryStream = new MemoryStream();
    }
    
    public void WriteBytes(byte[] bytes)
    {
        _memoryStream.Write(bytes);
    }
    
    public void WriteByte(byte value)
    {
        _memoryStream.WriteByte(value);
    }

    public void WriteSByte(sbyte value) => WriteByte((byte) value);

    public void WriteInt(int value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(4);
        BinaryPrimitives.WriteInt32BigEndian(memoryOwner.Memory.Span, value);
        _memoryStream.Write(memoryOwner.Memory.Span[..4]);
    }
    
    public void WriteUShort(ushort value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(2);
        BinaryPrimitives.WriteUInt16BigEndian(memoryOwner.Memory.Span, value);
        _memoryStream.Write(memoryOwner.Memory.Span[..2]);
    }
    
    public void WriteString(string value)
    {
        var bytes = Encoding.BigEndianUnicode.GetBytes(value);
        WriteUShort((ushort) (bytes.Length / 2));
        if (bytes.Length > 0)
            WriteBytes(bytes);
    }

    public byte[] ToByteArray() => _memoryStream.ToArray();

    public void Dispose()
    {
        _memoryStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _memoryStream.DisposeAsync();
    }
}