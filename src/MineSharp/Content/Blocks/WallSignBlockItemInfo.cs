using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class WallSignBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WallSignBlock;

    public override bool HasTileEntity => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.Sign)];
}