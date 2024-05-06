namespace MineSharp.Core;

public class EntityIdGenerator
{
    private int _currentValue = 1;
    private readonly HashSet<int> _releasedIds = new();
    private readonly object _lockObject = new();

    public int Next()
    {
        lock (_lockObject)
        {
            if (_releasedIds.Count > 0)
            {
                var releasedId = _releasedIds.Min();
                _releasedIds.Remove(releasedId);
                return releasedId;
            }
            else
            {
                return _currentValue++;
            }
        }
    }

    public void Release(int id)
    {
        lock (_lockObject)
        {
            if (id < _currentValue)
            {
                _releasedIds.Add(id);
            }
            else
            {
                throw new ArgumentException("Id to release must be less than the current value.");
            }
        }
    }
}