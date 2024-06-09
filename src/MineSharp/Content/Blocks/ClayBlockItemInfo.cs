using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class ClayBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.ClayBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.Clay, 4)];
}