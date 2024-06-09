using MineSharp.Content.Items;

namespace MineSharp.Content.Blocks;

public class WoodBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WoodBlock;
    
    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.WoodBlock, Metadata: blockMetadata)];
}