namespace MineSharp.Core;

public class ThreadSafeRandom(int? seed = null)
{
    private readonly Random _random = seed.HasValue ? new Random(seed.Value) : new Random();
    private readonly object _lock = new();

    public static ThreadSafeRandom Shared { get; } = new();

    public float Next()
    {
        lock (_lock)
        {
            return _random.NextSingle();
        }
    }

    public int Next(int min, int max)
    {
        lock (_lock)
        {
            return _random.Next(min, max);
        }
    }
}