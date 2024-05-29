using MineSharp.Extensions;

namespace MineSharp.Core;

public class LockableFileStream(string path, FileMode mode, FileAccess access) : Stream
{
    private readonly FileStream _fileStream = File.Open(path, mode, access);
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public async Task<IDisposable> EnterLockAsync() => await _semaphoreSlim.EnterLockAsync();

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        await _fileStream.WriteAsync(bytes, cancellationToken);
    }

    public override void Flush() => _fileStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _fileStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _fileStream.Seek(offset, origin);

    public override void SetLength(long value) => _fileStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _fileStream.Write(buffer, offset, count);

    public override bool CanRead => _fileStream.CanRead;
    public override bool CanSeek => _fileStream.CanSeek;
    public override bool CanWrite => _fileStream.CanWrite;
    public override long Length => _fileStream.Length;

    public override long Position
    {
        get => _fileStream.Position;
        set => _fileStream.Position = value;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _semaphoreSlim.Dispose();
    }
}