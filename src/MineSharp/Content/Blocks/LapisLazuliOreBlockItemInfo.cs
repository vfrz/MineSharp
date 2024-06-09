using MineSharp.Content.Items;
using MineSharp.Core;

namespace MineSharp.Content.Blocks;

public class LapisLazuliOreBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.LapisLazuliOreBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Stone.Level)
        {
            var quantity = (byte) ThreadSafeRandom.Shared.Next(4, 10);
            return [new ItemStack(ItemId.Dye, quantity, Metadata: (byte) Dye.LapisLazuli)];
        }

        return [];
    }
}