using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class GravelBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.GravelBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.GravelBlock)];
}