using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class SnowLayerBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SnowLayerBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is ShovelItemInfo)
            return [new ItemStack(ItemId.Snowball)];
        return [];
    }
}