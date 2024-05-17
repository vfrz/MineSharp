namespace MineSharp.Nbt.Tags;

public class LongNbtTag(string? name, long value) : INbtTag
{
    public string? Name { get; } = name;
    public long Value { get; } = value;
}