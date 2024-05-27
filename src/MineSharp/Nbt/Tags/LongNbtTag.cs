namespace MineSharp.Nbt.Tags;

public readonly struct LongNbtTag(string? name, long value) : INbtTag
{
    public string? Name { get; } = name;
    public long Value { get; } = value;
}