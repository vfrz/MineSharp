namespace MineSharp.Content.Blocks;

public class BedBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.BedBlock;

    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata) => [new ItemStack(ItemId.Bed)];
}