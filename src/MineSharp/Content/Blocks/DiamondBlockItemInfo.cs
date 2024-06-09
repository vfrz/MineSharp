using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class DiamondBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DiamondBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Iron.Level)
            return [new ItemStack(ItemId.DiamondBlock)];
        return [];
    }
}