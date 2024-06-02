namespace MineSharp.Content.Blocks;

public class CactusBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.CactusBlock;

    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata) => [new ItemStack(ItemId.CactusBlock)];
}