using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class PumpkinBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.PumpkinBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.PumpkinBlock)];
}