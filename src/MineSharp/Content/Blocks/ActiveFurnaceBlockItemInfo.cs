using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class ActiveFurnaceBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.ActiveFurnaceBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
            return [new ItemStack(ItemId.FurnaceBlock)];
        return [];
    }
}