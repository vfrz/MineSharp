using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class LeavesBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.LeavesBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo?.ItemId is ItemId.Shears)
            return true;
        return false;
    }

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo?.ItemId is ItemId.Shears)
            return [new ItemStack(ItemId.LeavesBlock, Metadata: blockMetadata)];
        return [];
    }

    public override byte Opacity => 1;
}