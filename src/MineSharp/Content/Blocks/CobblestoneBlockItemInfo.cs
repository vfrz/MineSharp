using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class CobblestoneBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.CobblestoneBlock;

    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata)
    {
        var miningItemInfo = ItemInfoProvider.Get(miningItem);
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.CobblestoneBlock)];
        return [];
    }
}