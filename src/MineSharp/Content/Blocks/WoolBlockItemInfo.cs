using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class WoolBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WoolBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.WoolBlock, Metadata: blockMetadata)];
}