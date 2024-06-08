using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class DoubleSlabBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DoubleSlabBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.SlabBlock, 2, Metadata: blockMetadata)];
        return [];
    }
}