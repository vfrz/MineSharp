using MineSharp.Content.Items;
using MineSharp.Core;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class RedstoneOreBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.RedstoneOreBlock;

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