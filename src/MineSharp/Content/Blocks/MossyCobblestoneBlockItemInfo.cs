using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class MossyCobblestoneBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.MossyCobblestoneBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.MossyCobblestoneBlock)];
        return [];
    }
}