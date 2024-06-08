using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class WoodenStairsBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WoodenStairsBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.PlanksBlock)];
}