using MineSharp.Content.Items;
using MineSharp.Core;

namespace MineSharp.Content.Blocks;

public class GlowstoneBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.GlowstoneBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo)
        {
            var quantity = (byte) ThreadSafeRandom.Shared.Next(2, 5);
            return [new ItemStack(ItemId.Glowstone, quantity)];
        }

        return [];
    }
}