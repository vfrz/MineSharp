namespace MineSharp.Content.Blocks;

public class GrassBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.GrassBlock;
    
    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata)
        => [new ItemStack(ItemId.DirtBlock)];
}