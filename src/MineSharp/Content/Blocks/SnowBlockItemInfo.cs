using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class SnowBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.SnowBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is ShovelItemInfo)
            return [new ItemStack(ItemId.Snowball, 4)];
        return [];
    }
}