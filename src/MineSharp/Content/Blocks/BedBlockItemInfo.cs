using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class BedBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.BedBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.Bed)];
}