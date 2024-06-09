using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class IronBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.IronBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Stone.Level)
            return [new ItemStack(ItemId.IronBlock)];
        return [];
    }
}