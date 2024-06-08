using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class StonePressurePlateBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.StonePressurePlateBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.StonePressurePlateBlock)];
        return [];
    }
}