using MineSharp.Content.Items;
using MineSharp.Core;

namespace MineSharp.Content.Blocks;

public class GlowingRedstoneOreBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.GlowingRedstoneOreBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Iron.Level)
        {
            var quantity = (byte) ThreadSafeRandom.Shared.Next(4, 6);
            return [new ItemStack(ItemId.Redstone, quantity)];
        }

        return [];
    }
}