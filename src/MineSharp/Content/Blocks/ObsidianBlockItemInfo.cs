using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class ObsidianBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.ObsidianBlock;

    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
    {
        if (miningItemInfo is PickaxeItemInfo pickaxeItemInfo && pickaxeItemInfo.Material.Level >= ToolMaterial.Diamond.Level)
            return [new ItemStack(ItemId.ObsidianBlock)];

        return [];
    }
}