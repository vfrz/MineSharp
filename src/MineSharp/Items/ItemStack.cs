namespace MineSharp.Items;

public readonly record struct ItemStack(ItemId ItemId, byte Count, short Metadata)
{
    public static readonly ItemStack Empty = new(ItemId.Empty, 0, 0);
}