namespace MineSharp.Core;

public class NibbleArray
{
    private readonly ArraySegment<byte> _innerArray;

    public NibbleArray(byte[] innerArray, int offset, int count)
    {
        _innerArray = new ArraySegment<byte>(innerArray, offset, count);
    }

    public byte this[int index]
    {
        get => (byte) (_innerArray[index / 2] >> (index % 2 * 4) & 0xF);
        set
        {
            value &= 0xF;
            _innerArray[index / 2] &= (byte) ~(0xF << (index % 2 * 4));
            _innerArray[index / 2] |= (byte) (value << (index % 2 * 4));
        }
    }

    public byte[] ToArray() => _innerArray.ToArray();
}