namespace MineSharp.Nbt.Tags;

public readonly struct DoubleNbtTag(string? name, double value) : INbtTag
{
    public string? Name { get; } = name;
    public double Value { get; } = value;
}