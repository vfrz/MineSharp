namespace MineSharp.Nbt.Tags;

public class ByteNbtTag(string? name, byte value) : INbtTag
{
    public string? Name { get; } = name;
    public byte Value { get; } = value;

    public bool ValueAsBool => Value == 1;

    public ByteNbtTag(string name, bool value) : this(name, (byte)(value ? 1 : 0))
    {
    }
}