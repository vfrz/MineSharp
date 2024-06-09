namespace MineSharp.Core;

public class NibbleArray
{
    public int Offset => _innerArraySegment.Offset;

    public int Count => _innerArraySegment.Count;

    public byte[]? Array => _innerArraySegment.Array;

    private readonly ArraySegment<byte> _innerArraySegment;

    public NibbleArray(byte[] innerArray, int offset, int count)
    {
        _innerArraySegment = new ArraySegment<byte>(innerArray, offset, count);
    }

    public byte this[int index]
    {
        get => (byte) (_innerArraySegment[index / 2] >> (index % 2 * 4) & 0xF);
        set
        {
            value &= 0xF;
            _innerArraySegment[index / 2] &= (byte) ~(0xF << (index % 2 * 4));
            _innerArraySegment[index / 2] |= (byte) (value << (index % 2 * 4));
        }
    }
}