namespace MineSharp.Nbt.Tags;

public readonly struct StringNbtTag(string? name, string? value) : INbtTag
{
    public string? Name { get; } = name;
    public string? Value { get; } = value;
}