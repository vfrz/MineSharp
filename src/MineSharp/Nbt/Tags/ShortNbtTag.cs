namespace MineSharp.Nbt.Tags;

public class ShortNbtTag(string? name, short value) : INbtTag
{
    public string? Name { get; } = name;
    public short Value { get; } = value;
}