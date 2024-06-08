using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class WoodBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.WoodBlock;
    
    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.WoodBlock, Metadata: blockMetadata)];
}