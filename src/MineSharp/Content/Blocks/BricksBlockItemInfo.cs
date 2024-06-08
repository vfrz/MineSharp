using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class BricksBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.BricksBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.BricksBlock)];
        return [];
    }
}