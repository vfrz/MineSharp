using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class SandstoneBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SandstoneBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.SandstoneBlock)];
        return [];
    }
}