namespace MineSharp.Items;

public readonly record struct ItemStack(ItemId ItemId, byte Count = 1, short Metadata = 0)
{
    public static readonly ItemStack Empty = new(ItemId.Empty, 0);
}