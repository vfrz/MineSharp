using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class LockedChestBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.LockedChestBlock;

    public override bool IsInstantDig(ItemInfo? miningItemInfo, byte blockMetadata) => true;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.LockedChestBlock)];
}