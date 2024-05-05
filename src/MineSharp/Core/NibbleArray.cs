namespace MineSharp.Core;

public class NibbleArray
{
    private readonly byte[] _innerArray;
    private readonly int _offset;

    public NibbleArray(byte[] innerArray, int offset)
    {
        _innerArray = innerArray;
        _offset = offset;
    }

    public byte this[int index]
    {
        get
        {
            var value = (byte) (_innerArray[_offset + index / 2] >> (index % 2 == 0 ? 4 : 0) & 0x0F);
            return value;
        }
        set
        {
            if (value > 0xF)
                throw new ArgumentOutOfRangeException(nameof(index), "Value must be between 0 and 15 (inclusive).");
            var shift = index % 2 == 0 ? 4 : 0;
            _innerArray[_offset + index / 2] &= (byte) (0xFF ^ (0x0F << shift));
            _innerArray[_offset + index / 2] |= (byte) (value << shift);
        }
    }
}