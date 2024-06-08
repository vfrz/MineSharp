using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

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