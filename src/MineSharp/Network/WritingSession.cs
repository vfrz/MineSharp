using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace MineSharp.Network;

public readonly struct WritingSession : IDisposable
{
    private readonly NetworkStream _networkStream;
    private readonly SemaphoreSlim _semaphoreSlim;

    public WritingSession(NetworkStream networkStream, SemaphoreSlim semaphoreSlim)
    {
        _networkStream = networkStream;
        _semaphoreSlim = semaphoreSlim;
        _semaphoreSlim.Wait();
    }

    public async Task WriteLongAsync(long value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(sizeof(long));
        BinaryPrimitives.WriteInt64BigEndian(memoryOwner.Memory.Span, value);
        await _networkStream.WriteAsync(memoryOwner.Memory[..sizeof(long)]);
    }

    public async Task WriteShortAsync(short value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(2);
        BinaryPrimitives.WriteInt16BigEndian(memoryOwner.Memory.Span, value);
        await _networkStream.WriteAsync(memoryOwner.Memory[..2]);
    }
    
    public async Task WriteUInt16Async(ushort value)
    {
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(2);
        BinaryPrimitives.WriteUInt16BigEndian(memoryOwner.Memory.Span, value);
        await _networkStream.WriteAsync(memoryOwner.Memory[..2]);
    }

    public async ValueTask WriteStringAsync(string value)
    {
        var bytes = Encoding.BigEndianUnicode.GetBytes(value);
        await WriteUInt16Async((ushort) (bytes.Length / 2));
        if (bytes.Length > 0)
            await WriteBytesAsync(bytes);
    }

    public async ValueTask WriteIntAsync(int value)
    {
        var bytes = BitConverter.GetBytes(value).Reverse().ToArray();
        await _networkStream.WriteAsync(bytes);
    }

    public async ValueTask WriteDoubleAsync(double value)
    {
        var bytes = BitConverter.GetBytes(value).Reverse().ToArray();
        await _networkStream.WriteAsync(bytes);
    }

    public async ValueTask WriteFloatAsync(float value)
    {
        var bytes = BitConverter.GetBytes(value).Reverse().ToArray();
        await _networkStream.WriteAsync(bytes);
    }

    public void WriteByte(byte value)
    {
        _networkStream.WriteByte(value);
    }

    public void WriteSByte(sbyte value)
    {
        _networkStream.WriteByte((byte) value);
    }
    
    public async Task WriteBytesAsync(byte[] bytes)
    {
        await _networkStream.WriteAsync(bytes, 0, bytes.Length);
    }

    public void Dispose()
    {
        _semaphoreSlim.Release();
    }
}