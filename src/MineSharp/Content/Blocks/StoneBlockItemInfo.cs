using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class StoneBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.StoneBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.CobblestoneBlock)];
        return [];
    }
}