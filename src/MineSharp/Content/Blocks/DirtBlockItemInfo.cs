using MineSharp.Content.Items;
using MineSharp.Sdk.Core;

namespace MineSharp.Content.Blocks;

public class DirtBlockItemInfo : BlockItemInfo
{
    public override ItemId ItemId => ItemId.DirtBlock;
    
    public override ItemStack[] GetDroppedItems(ItemInfo? miningItemInfo, byte blockMetadata)
        => [new ItemStack(ItemId.DirtBlock)];
}