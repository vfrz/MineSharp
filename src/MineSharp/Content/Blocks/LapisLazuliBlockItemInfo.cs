using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class LapisLazuliBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.LapisLazuliBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Stone.Level)
            return [new ItemStack(ItemId.LapisLazuliBlock)];

        return [];
    }
}