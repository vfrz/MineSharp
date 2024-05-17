namespace MineSharp.Nbt.Tags;

public class ByteArrayNbtTag(string name, byte[] value) : INbtTag
{
    public string? Name { get; } = name;
    public byte[] Value { get; } = value;
}