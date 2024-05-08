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
        get => (byte) (_innerArray[_offset + index / 2] >> (index % 2 * 4) & 0xF);
        set
        {
            value &= 0xF;
            _innerArray[_offset + index / 2] &= (byte) ~(0xF << (index % 2 * 4));
            _innerArray[_offset + index / 2] |= (byte) (value << (index % 2 * 4));
        }
    }
}