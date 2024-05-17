namespace MineSharp.Nbt.Tags;

public class IntNbtTag(string? name, int value) : INbtTag
{
    public string? Name { get; } = name;
    public int Value { get; } = value;
}