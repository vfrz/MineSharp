using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class RedstoneWireBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.RedstoneWireBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.Redstone)];
}