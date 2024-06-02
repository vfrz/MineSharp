namespace MineSharp.Content.Blocks;

public class WallSignBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WallSignBlock;

    public override bool HasTileEntity => true;

    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata) => [new ItemStack(ItemId.Sign)];
}