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

    public void WriteBool(bool value) => WriteByte((byte) (value ? 1 : 0));

    public void WriteInt(int value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(sizeof(int));
        BinaryPrimitives.WriteInt32BigEndian(memoryOwner.Memory.Span, value);
        _memoryStream.Write(memoryOwner.Memory.Span[..sizeof(int)]);
    }

    public void WriteUShort(ushort value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(sizeof(ushort));
        BinaryPrimitives.WriteUInt16BigEndian(memoryOwner.Memory.Span, value);
        _memoryStream.Write(memoryOwner.Memory.Span[..sizeof(ushort)]);
    }
    
    public void WriteShort(short value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(sizeof(short));
        BinaryPrimitives.WriteInt16BigEndian(memoryOwner.Memory.Span, value);
        _memoryStream.Write(memoryOwner.Memory.Span[..sizeof(short)]);
    }

    public void WriteFloat(float value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(sizeof(float));
        BinaryPrimitives.WriteSingleBigEndian(memoryOwner.Memory.Span, value);
        _memoryStream.Write(memoryOwner.Memory.Span[..sizeof(float)]);
    }
    
    public void WriteDouble(double value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(sizeof(double));
        BinaryPrimitives.WriteDoubleBigEndian(memoryOwner.Memory.Span, value);
        _memoryStream.Write(memoryOwner.Memory.Span[..sizeof(double)]);
    }
    
    public void WriteLong(long value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(sizeof(long));
        BinaryPrimitives.WriteInt64BigEndian(memoryOwner.Memory.Span, value);
        _memoryStream.Write(memoryOwner.Memory.Span[..sizeof(long)]);
    }

    public void WriteString(string value)
    {
        WriteUShort((ushort) value.Length);
        var bytes = Encoding.BigEndianUnicode.GetBytes(value);
        if (bytes.Length > 0)
            WriteBytes(bytes);
    }
    
    public void WriteString8(string value)
    {
        WriteUShort((ushort) value.Length);
        var bytes = Encoding.UTF8.GetBytes(value);
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