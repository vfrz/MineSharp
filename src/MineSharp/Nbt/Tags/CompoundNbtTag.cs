using System.Collections;

namespace MineSharp.Nbt.Tags;

public class CompoundNbtTag(string? name) : INbtTag, IEnumerable<INbtTag>
{
    private readonly Dictionary<string, INbtTag> _values = new();

    public string? Name { get; } = name;

    public INbtTag this[string name]
    {
        get => _values[name];
        set => _values[name] = value;
    }

    public T Get<T>(string name) where T : INbtTag => (T)this[name];

    public CompoundNbtTag AddTag(INbtTag tag)
    {
        this[tag.Name!] = tag;
        return this;
    }

    public IEnumerator<INbtTag> GetEnumerator()
    {
        return _values.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}