using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class WoodenPressurePlateBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WoodenPressurePlateBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.WoodenPressurePlateBlock)];
}