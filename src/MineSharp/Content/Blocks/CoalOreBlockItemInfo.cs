using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class CoalOreBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.CoalOreBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.Coal)];
        return [];
    }
}