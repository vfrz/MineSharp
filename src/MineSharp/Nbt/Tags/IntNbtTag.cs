namespace MineSharp.Nbt.Tags;

public readonly struct IntNbtTag(string? name, int value) : INbtTag
{
    public string? Name { get; } = name;
    public int Value { get; } = value;
}