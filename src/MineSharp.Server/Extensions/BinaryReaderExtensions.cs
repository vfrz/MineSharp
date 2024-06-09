using System.Buffers.Binary;
using System.Text;

namespace MineSharp.Extensions;

public static class BinaryReaderExtensions
{
    public static short ReadBigEndianShort(this BinaryReader reader)
    {
        var value = reader.ReadBytes(sizeof(short));
        return BinaryPrimitives.ReadInt16BigEndian(value);
    }

    public static int ReadBigEndianInt(this BinaryReader reader)
    {
        var value = reader.ReadBytes(sizeof(int));
        return BinaryPrimitives.ReadInt32BigEndian(value);
    }

    public static long ReadBigEndianLong(this BinaryReader reader)
    {
        var value = reader.ReadBytes(sizeof(long));
        return BinaryPrimitives.ReadInt64BigEndian(value);
    }

    public static float ReadBigEndianFloat(this BinaryReader reader)
    {
        var value = reader.ReadBytes(sizeof(float));
        return BinaryPrimitives.ReadSingleBigEndian(value);
    }

    public static double ReadBigEndianDouble(this BinaryReader reader)
    {
        var value = reader.ReadBytes(sizeof(double));
        return BinaryPrimitives.ReadDoubleBigEndian(value);
    }

    public static string? ReadNbtString(this BinaryReader reader)
    {
        var length = reader.ReadBigEndianShort();
        if (length == 0)
            return null;
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }
}