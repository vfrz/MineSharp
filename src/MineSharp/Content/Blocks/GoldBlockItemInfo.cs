using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class GoldBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.GoldBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Iron.Level)
            return [new ItemStack(ItemId.GoldBlock)];
        return [];
    }
}