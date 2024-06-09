using MineSharp.Core;

namespace MineSharp.Nbt.Tags;

public readonly struct ByteArrayNbtTag(string name, byte[] value) : INbtTag
{
    public string? Name { get; } = name;
    public byte[] Value { get; } = value;
    public int Offset { get; } = 0;
    public int Count { get; } = value.Length;

    public ByteArrayNbtTag(string name, ArraySegment<byte> arraySegment) : this(name, arraySegment.Array!)
    {
        Offset = arraySegment.Offset;
        Count = arraySegment.Count;
    }

    public ByteArrayNbtTag(string name, NibbleArray nibbleArray) : this(name, nibbleArray.Array!)
    {
        Offset = nibbleArray.Offset;
        Count = nibbleArray.Count;
    }
}