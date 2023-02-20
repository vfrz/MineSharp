using System.Buffers.Binary;
using System.Text;

namespace MineSharp.Extensions;

public static class BinaryReaderExtensions
{
    private const int SegmentBits = 0x7F;
    private const int ContinueBit = 0x80;

    public static long ReadLong(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(8);
        return BinaryPrimitives.ReadInt64BigEndian(bytes);
    }

    public static string ReadVarString(this BinaryReader reader)
    {
        var size = reader.ReadVarInt();
        var bytes = reader.ReadBytes(size);
        return Encoding.UTF8.GetString(bytes);
    }

    public static int ReadVarInt(this BinaryReader reader) => reader.ReadVarInt(out _);

    public static int ReadVarInt(this BinaryReader reader, out int bytesRead)
    {
        var value = 0;
        var position = 0;
        bytesRead = 0;

        while (true)
        {
            var currentByte = reader.ReadByte();
            bytesRead++;
            value |= (currentByte & SegmentBits) << position;

            if ((currentByte & ContinueBit) == 0)
                break;

            position += 7;

            if (position >= 32)
                throw new Exception("VarInt is too big");
        }

        return value;
    }


}