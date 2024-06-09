using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class ButtonBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.ButtonBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.ButtonBlock)];
}