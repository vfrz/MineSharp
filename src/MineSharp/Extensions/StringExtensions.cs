using System.Text;

namespace MineSharp.Extensions;

public static class StringExtensions
{
    public static byte[] ToVarString(this string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var length = bytes.Length.ToVarInt().ToArray();
        var result = new byte[length.Length + bytes.Length];

        Buffer.BlockCopy(length, 0, result, 0, length.Length);
        Buffer.BlockCopy(bytes, 0, result, length.Length, bytes.Length);

        return result;
    }
}