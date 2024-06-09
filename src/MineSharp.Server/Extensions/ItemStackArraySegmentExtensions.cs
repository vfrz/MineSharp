using MineSharp.Content;

namespace MineSharp.Extensions;

public static class ItemStackArraySegmentExtensions
{
    public static short? FirstEmptyIndex(this ArraySegment<ItemStack> array)
    {
        for (var i = 0; i < array.Count; i++)
        {
            if (array[i] == ItemStack.Empty)
                return (short) (i + array.Offset);
        }

        return null;
    }
}