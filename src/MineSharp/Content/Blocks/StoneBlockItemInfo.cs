using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class StoneBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.StoneBlock;

    public override ItemStack[] GetDroppedItems(ItemId miningItem, byte blockMetadata)
    {
        var miningItemInfo = ItemInfoProvider.Get(miningItem);
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.CobblestoneBlock)];
        return [];
    }
}