using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class TrapdoorBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.TrapdoorBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.TrapdoorBlock)];
}