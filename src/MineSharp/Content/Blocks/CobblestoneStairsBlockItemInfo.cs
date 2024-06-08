using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class CobblestoneStairsBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.CobblestoneStairsBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.CobblestoneBlock)];
        return [];
    }
}