namespace MineSharp.Nbt.Tags;

public class StringNbtTag(string? name, string? value) : INbtTag
{
    public string? Name { get; } = name;
    public string? Value { get; } = value;
}