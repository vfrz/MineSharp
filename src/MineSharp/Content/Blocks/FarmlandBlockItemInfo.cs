using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class FarmlandBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.FarmlandBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.DirtBlock)];
}