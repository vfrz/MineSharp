using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace MineSharp.Extensions;

public static class SequenceReaderExtensions
{
    private const int SegmentBits = 0x7F;
    private const int ContinueBit = 0x80;

    public static ReadOnlySequence<byte> ReadBytes(ref this SequenceReader<byte> reader, int count)
    {
        if (reader.TryReadExact(count, out var bytes))
            return bytes;
        throw new Exception("Failed to read bytes");
    }

    public static byte[] ReadBytesArray(ref this SequenceReader<byte> reader, int count)
        => ReadBytes(ref reader, count).ToArray();

    public static int ReadVarInt(ref this SequenceReader<byte> reader)
    {
        if (TryReadVarInt(ref reader, out var value))
            return value;
        throw new Exception("Failed to read varint");
    }
    
    public static bool TryReadVarInt(ref this SequenceReader<byte> reader, out int output)
        => TryReadVarInt(ref reader, out output, out _);

    public static bool TryReadVarInt(ref this SequenceReader<byte> reader, out int output, out int bytesRead)
    {
        var value = 0;
        var position = 0;
        bytesRead = 0;

        while (true)
        {
            if (reader.TryRead(out var currentByte))
            {
                bytesRead++;
                value |= (currentByte & SegmentBits) << position;

                if ((currentByte & ContinueBit) == 0)
                    break;

                position += 7;

                if (position >= 32)
                {
                    output = default;
                    return false;
                }
            }
            else
            {
                output = default;
                return false;
            }
        }

        output = value;
        return true;
    }

    public static long ReadVarLong(ref this SequenceReader<byte> reader)
    {
        long value = 0;
        var position = 0;

        while (true)
        {
            if (!reader.TryRead(out var currentByte))
                throw new Exception("Failed to read byte");
                
            value |= (long) (currentByte & SegmentBits) << position;

            if ((currentByte & ContinueBit) == 0)
                break;

            position += 7;

            if (position >= 64)
                throw new Exception("VarLong is too big");
        }

        return value;
    }
    
    public static long ReadLong(ref this SequenceReader<byte> reader)
    {
        if (reader.TryReadBigEndian(out long value))
            return value;
        throw new Exception("Failed to read long");
    }
    
    public static ushort ReadUInt16(ref this SequenceReader<byte> reader)
    {
        var buffer = reader.ReadBytes(2);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer.FirstSpan);
    }

    public static string ReadString(ref this SequenceReader<byte> reader)
    {
        var size = reader.ReadVarInt();
        var bytes = reader.ReadBytes(size);
        return Encoding.UTF8.GetString(bytes);
    }
}