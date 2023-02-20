using System.Buffers;

namespace MineSharp.Extensions;

public static class IntExtensions
{
    private const int SegmentBits = 0x7F;
    private const int ContinueBit = 0x80;
    
    public static Memory<byte> ToVarInt(this int value)
    {
        var memoryOwner = MemoryPool<byte>.Shared.Rent(5);
        var span = memoryOwner.Memory.Span;

        var length = 0;
        
        while (true)
        {
            if ((value & ~SegmentBits) == 0)
            {
                span[length] = (byte) value;
                return memoryOwner.Memory.Slice(0, length + 1);
            }

            span[length] = (byte) ((value & SegmentBits) | ContinueBit);
            length++;

            value >>>= 7;
        }
    }
}