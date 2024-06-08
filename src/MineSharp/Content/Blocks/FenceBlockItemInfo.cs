using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class FenceBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.FenceBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.FenceBlock)];
}