namespace MineSharp.Sdk.Core;

public readonly record struct ItemStack(ItemId ItemId, byte Count = 1, short Metadata = 0)
{
    public static readonly ItemStack Empty = new(ItemId.Empty, 0);

    public bool Match(ItemStack other, bool matchCount = false, bool matchMetadata = false)
    {
        if (ItemId != other.ItemId)
            return false;

        if (matchCount && Count != other.Count)
            return false;

        if (matchMetadata && Metadata != other.Metadata)
            return false;

        return true;
    }
}