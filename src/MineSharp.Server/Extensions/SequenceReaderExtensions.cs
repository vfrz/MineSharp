using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace MineSharp.Extensions;

public static class SequenceReaderExtensions
{
    public static ReadOnlySequence<byte> ReadBytes(ref this SequenceReader<byte> reader, int count)
    {
        if (reader.TryReadExact(count, out var bytes))
            return bytes;
        throw new Exception("Failed to read bytes");
    }

    public static byte[] ReadBytesArray(ref this SequenceReader<byte> reader, int count)
        => ReadBytes(ref reader, count).ToArray();

    public static int ReadInt(ref this SequenceReader<byte> reader)
    {
        if (reader.TryReadBigEndian(out int value))
            return value;
        throw new Exception("Failed to read Int");
    }

    public static long ReadLong(ref this SequenceReader<byte> reader)
    {
        if (reader.TryReadBigEndian(out long value))
            return value;
        throw new Exception("Failed to read Long");
    }

    public static double ReadDouble(ref this SequenceReader<byte> reader)
    {
        var result = BinaryPrimitives.ReadDoubleBigEndian(reader.UnreadSpan);
        reader.Advance(sizeof(double));
        return result;
    }

    public static float ReadFloat(ref this SequenceReader<byte> reader)
    {
        var result = BinaryPrimitives.ReadSingleBigEndian(reader.UnreadSpan);
        reader.Advance(sizeof(float));
        return result;
    }

    public static ushort ReadUInt16(ref this SequenceReader<byte> reader)
    {
        var result = BinaryPrimitives.ReadUInt16BigEndian(reader.UnreadSpan);
        reader.Advance(sizeof(ushort));
        return result;
    }

    public static short ReadShort(ref this SequenceReader<byte> reader)
    {
        var result = BinaryPrimitives.ReadInt16BigEndian(reader.UnreadSpan);
        reader.Advance(sizeof(short));
        return result;
    }

    public static string ReadString(ref this SequenceReader<byte> reader)
    {
        var length = reader.ReadUInt16();
        if (length == 0)
            return string.Empty;
        var data = reader.ReadBytes(length * 2);
        return Encoding.BigEndianUnicode.GetString(data);
    }

    public static byte ReadByte(ref this SequenceReader<byte> reader)
    {
        if (reader.TryRead(out var output))
            return output;
        throw new Exception("Failed to read byte");
    }

    public static sbyte ReadSByte(ref this SequenceReader<byte> reader)
    {
        if (reader.TryRead(out var output))
            return (sbyte) output;
        throw new Exception("Failed to read byte");
    }

    public static bool ReadBool(ref this SequenceReader<byte> reader)
    {
        return reader.ReadByte() == 0x01;
    }
}