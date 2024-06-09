using System.Buffers.Binary;
using System.Text;

namespace MineSharp.Extensions;

public static class BinaryWriterExtensions
{
    public static void WriteBigEndianShort(this BinaryWriter writer, short value)
    {
        var bytes = new byte[sizeof(short)];
        BinaryPrimitives.WriteInt16BigEndian(bytes, value);
        writer.Write(bytes);
    }

    public static void WriteBigEndianInt(this BinaryWriter writer, int value)
    {
        var bytes = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        writer.Write(bytes);
    }

    public static void WriteBigEndianLong(this BinaryWriter writer, long value)
    {
        var bytes = new byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, value);
        writer.Write(bytes);
    }

    public static void WriteBigEndianFloat(this BinaryWriter writer, float value)
    {
        var bytes = new byte[sizeof(float)];
        BinaryPrimitives.WriteSingleBigEndian(bytes, value);
        writer.Write(bytes);
    }

    public static void WriteBigEndianDouble(this BinaryWriter writer, double value)
    {
        var bytes = new byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleBigEndian(bytes, value);
        writer.Write(bytes);
    }

    public static void WriteNbtString(this BinaryWriter writer, string? value)
    {
        if (value is null)
        {
            writer.WriteBigEndianShort(0);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        writer.WriteBigEndianShort((short)bytes.Length);
        writer.Write(bytes);
    }
}