namespace MineSharp.Extensions;

public static class SemaphoreSlimExtensions
{
    private readonly struct SemaphoreLock(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }

    public static async Task<IDisposable> EnterLockAsync(this SemaphoreSlim semaphoreSlim, CancellationToken cancellationToken = default)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        return new SemaphoreLock(semaphoreSlim);
    }
}