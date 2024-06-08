using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class DiamondOreBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DiamondOreBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Iron.Level)
            return [new ItemStack(ItemId.Diamond)];
        return [];
    }
}