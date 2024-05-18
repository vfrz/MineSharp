namespace MineSharp.Core;

public class ThreadSafeRandom
{
    private readonly Random _random = new();
    private readonly object _lock = new();

    public float Next()
    {
        lock (_lock)
        {
            return _random.NextSingle();
        }
    }
}