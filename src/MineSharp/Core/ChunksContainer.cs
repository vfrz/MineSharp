using System.Collections;
using System.Collections.Concurrent;
using MineSharp.World;

namespace MineSharp.Core;

public class ChunksContainer : IEnumerable<Chunk>
{
    private readonly ConcurrentDictionary<Vector2i, Chunk> _data = new();

    public Chunk? this[Vector2i position]
    {
        get => _data.GetValueOrDefault(position);
        set
        {
            if (value is null)
                _data.Remove(position, out _);
            else
                _data.TryAdd(position, value);
        }
    }

    public IEnumerator<Chunk> GetEnumerator()
    {
        return _data.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}