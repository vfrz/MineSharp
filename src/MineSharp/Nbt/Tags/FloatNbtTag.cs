namespace MineSharp.Nbt.Tags;

public readonly struct FloatNbtTag(string? name, float value) : INbtTag
{
    public string? Name { get; } = name;
    public float Value { get; } = value;
}