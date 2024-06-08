using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class IronOreBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.IronOreBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Stone.Level)
            return [new ItemStack(ItemId.IronOreBlock)];
        return [];
    }
}