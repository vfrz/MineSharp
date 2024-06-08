using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class LadderBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.LadderBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.LadderBlock)];
}