namespace MineSharp.Entities;

public class ThreadSafeIdGenerator
{
    private int _currentValue;
    private readonly object _lockObject = new();

    public ThreadSafeIdGenerator(int initialValue = 0)
    {
        _currentValue = initialValue;
    }
    
    public int NextId()
    {
        lock (_lockObject)
        {
            return _currentValue++;
        }
    }
}