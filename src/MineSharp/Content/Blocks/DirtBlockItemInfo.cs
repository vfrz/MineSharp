namespace MineSharp.Content.Blocks;

public class DirtBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DirtBlock;
    
    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata)
        => [new ItemStack(ItemId.DirtBlock)];
}