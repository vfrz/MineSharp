using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class SlabBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SlabBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.SlabBlock, Metadata: blockMetadata)];
        return [];
    }
}