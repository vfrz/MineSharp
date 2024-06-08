using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class RailBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.RailBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.RailBlock)];
}