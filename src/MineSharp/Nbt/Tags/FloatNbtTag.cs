namespace MineSharp.Nbt.Tags;

public class FloatNbtTag(string? name, float value) : INbtTag
{
    public string? Name { get; } = name;
    public float Value { get; } = value;
}