using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using MineSharp.Extensions;

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
        using var memoryOwner = MemoryPool<byte>.Shared.Rent(8);
        BinaryPrimitives.WriteInt64BigEndian(memoryOwner.Memory.Span, value);
        await _networkStream.WriteAsync(memoryOwner.Memory[..8]);
    }

    public async ValueTask WriteStringAsync(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await WriteVarIntAsync(bytes.Length);
        _networkStream.Write(bytes, 0, bytes.Length);
    }

    public async ValueTask WriteVarIntAsync(int value)
    {
        var bytes = value.ToVarInt();
        await _networkStream.WriteAsync(bytes);
    }

    public async Task WriteBytesAsync(byte[] bytes)
    {
        await _networkStream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task WriteGuidAsync(Guid guid)
    {
        await WriteBytesAsync(guid.ToByteArray());
    }

    public void Dispose()
    {
        _semaphoreSlim.Release();
    }
}