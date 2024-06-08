using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class FloorSignBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.FloorSignBlock;

    public override bool HasTileEntity => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.Sign)];
}