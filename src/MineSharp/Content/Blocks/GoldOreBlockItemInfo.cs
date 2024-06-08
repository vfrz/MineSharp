using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class GoldOreBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.GoldOreBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Iron.Level)
            return [new ItemStack(ItemId.IronOreBlock)];
        return [];
    }
}