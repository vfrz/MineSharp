using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class PoweredRailBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.PoweredRailBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.PoweredRailBlock)];
}